using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

#if UNITY_PS4
public class SonyNpMatching : IScreen
{
    MenuLayout m_MenuMatching;
    MenuLayout m_MenuInRoom;

    bool matchingInitialized = false;
    Sony.NP.Matching.Room currentRoom = null;
    Sony.NP.Matching.Room foundRoom = null;
    UInt16 lastMemberToJoin = 0;
    Sony.NP.Matching.SessionImage sessionImage = new Sony.NP.Matching.SessionImage();
    int nextMaxBoost = 10;

    public SonyNpMatching()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuMatching;
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void Process(MenuStack stack)
    {
        if (currentRoom == null)
        {
            MenuSetupMatching(stack);
        }
        else
        {
            MenuInRoom(stack);
        }
    }

    public void Initialize()
    {
        m_MenuMatching = new MenuLayout(this, 450, 20);
        m_MenuInRoom = new MenuLayout(this, 450, 20);

        CreateAttributeMetaData();

        // Must define an image for the rooms session otherwise it can't be created.
        // Also streamingAssetsPath needs to be called on the main thread. 
        sessionImage.SessionImgPath = Application.streamingAssetsPath + "/PS4SessionImage.jpg";
    }

    public void MenuSetupMatching(MenuStack menuStack)
    {
        m_MenuMatching.Update();

        if (m_MenuMatching.AddItem("Set Init Configuration", "Set the initial configuration for the matching system.", matchingInitialized == false))
        {
            SetInitConfiguration(true);
        }

        if (m_MenuMatching.AddItem("Get Worlds", "Get the worlds configured on DevNet.", matchingInitialized))
        {
            GetWorlds();
        }

        if (m_MenuMatching.AddItem("Create Room", "Create and join a new room.", matchingInitialized && currentRoom == null))
        {
            CreateRoom();
        }

        if (m_MenuMatching.AddItem("Search Rooms", "Search for rooms", matchingInitialized))
        {
            SearchRooms(true);
        }

        if (m_MenuMatching.AddItem("Search Rooms (No NAT filter)", "Search for rooms. Don't apply a NAT filter. Rooms behind a NAT Type 3 will be visible.", matchingInitialized))
        {
            SearchRooms(false);
        }

        if (m_MenuMatching.AddItem("Join 1st Found Room", "Join the first found room.", matchingInitialized && currentRoom == null && foundRoom != null))
        {
            JoinSearchRoom();
        }

        if (m_MenuMatching.AddItem("Terminate Matching Service", "Terminate the matching service. This maybe required if a user signs out while joined to a room. The service needs resetting to allow room joining to work again.", matchingInitialized))
        {
            TerminateMatchingService();
            matchingInitialized = false;
        }

        if (m_MenuMatching.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void MenuInRoom(MenuStack menuStack)
    {
        m_MenuInRoom.Update();

        if (m_MenuInRoom.AddItem("Leave Room", "Leave the current room.", matchingInitialized && currentRoom != null))
        {
            LeaveRoom();
        }

        if (m_MenuInRoom.AddItem("Get Room Ping Time", "Return the ping time for the room.", matchingInitialized && currentRoom != null))
        {
            GetRoomPingTime();
        }

        if (m_MenuInRoom.AddItem("Kick Last Member", "Kick out the last member to join the room.", matchingInitialized && currentRoom != null && lastMemberToJoin > 0))
        {
            KickOutRoomMember();
        }

        if (m_MenuInRoom.AddItem("Send Invitation", "Send invitation.", matchingInitialized && currentRoom != null))
        {
            SendInvitation();
        }

        if (m_MenuInRoom.AddItem("Send Room Message", "Send a room message to all members of the current room.", matchingInitialized && currentRoom != null))
        {
            SendRoomMessage();
        }

        if (m_MenuInRoom.AddItem("Set Room Info (Various)", "Update the current room info. Each press of this button calls SetInfoData with different parameters to update the different types of data in the room.", matchingInitialized && currentRoom != null))
        {
            SetRoomInfo();
        }

        if (m_MenuInRoom.AddItem("Get Attributes (Various)", "Get the search attributes for the current room. Each press of this button calls GetAttributes with different parameters.", matchingInitialized && currentRoom != null))
        {
            GetAttributes();
        }

        if (m_MenuInRoom.AddItem("Get Data", "Get data from the current room.", matchingInitialized && currentRoom != null))
        {
            GetData();
        }

        //if (m_MenuInRoom.AddItem("Set Members As Recently Met", "Set Members in the room as recently met", matchingInitialized && currentRoom != null && currentRoom.CurrentMembers.Length > 1))
        //{
        //    SetMembersAsRecentlyMet();
        //}

        //if (m_MenuInRoom.AddItem("Broadcast To All", "Broadcast data to all other members, including local client."))
        //{
        //    NetworkManager.BroadcastToAllMembers();
        //}

        //if (m_MenuInRoom.AddItem("Send To Host", "Send data to the host only.", NetworkManager.IsClient))
        //{
        //    NetworkManager.SendToHost();
        //}

        //if (m_MenuInRoom.AddItem("Output Network Manager Info", "Output the current network manager details. This includes a list of members and machines with known IP addresses."))
        //{
        //    NetworkManager.OutputInfo();
        //}

        if (m_MenuInRoom.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    Sony.NP.Matching.AttributeMetadata[] attributesMetadata;

    public void CreateAttributeMetaData()
    {
        //attributesMetadata = new Sony.NP.Matching.AttributeMetadata[1];
        //attributesMetadata[0] = Sony.NP.Matching.AttributeMetadata.CreateBinaryAttribute("NPC_NAME", Sony.NP.Matching.AttributeScope.Room, Sony.NP.Matching.RoomAttributeVisibility.Search, 64);

        attributesMetadata = new Sony.NP.Matching.AttributeMetadata[9];

        // Member attributes
        attributesMetadata[0] = Sony.NP.Matching.AttributeMetadata.CreateBinaryAttribute("CURRENT_STATE", Sony.NP.Matching.AttributeScope.Member, Sony.NP.Matching.RoomAttributeVisibility.Invalid, 32);
        attributesMetadata[1] = Sony.NP.Matching.AttributeMetadata.CreateIntegerAttribute("MAX_BOOSTS", Sony.NP.Matching.AttributeScope.Member, Sony.NP.Matching.RoomAttributeVisibility.Invalid);
        attributesMetadata[2] = Sony.NP.Matching.AttributeMetadata.CreateIntegerAttribute("MAX_TEAMS", Sony.NP.Matching.AttributeScope.Member, Sony.NP.Matching.RoomAttributeVisibility.Invalid);

        // Room search attributes
        attributesMetadata[3] = Sony.NP.Matching.AttributeMetadata.CreateBinaryAttribute("NPC_NAME", Sony.NP.Matching.AttributeScope.Room, Sony.NP.Matching.RoomAttributeVisibility.Search, 64);
        attributesMetadata[4] = Sony.NP.Matching.AttributeMetadata.CreateIntegerAttribute("TRACK_ID", Sony.NP.Matching.AttributeScope.Room, Sony.NP.Matching.RoomAttributeVisibility.Search);
        attributesMetadata[5] = Sony.NP.Matching.AttributeMetadata.CreateIntegerAttribute("COUNTRY_ID", Sony.NP.Matching.AttributeScope.Room, Sony.NP.Matching.RoomAttributeVisibility.Search);
        attributesMetadata[6] = Sony.NP.Matching.AttributeMetadata.CreateIntegerAttribute("SEASON_ID", Sony.NP.Matching.AttributeScope.Room, Sony.NP.Matching.RoomAttributeVisibility.Search);

        // Internal Room attributes
        attributesMetadata[7] = Sony.NP.Matching.AttributeMetadata.CreateIntegerAttribute("HOST_NAME", Sony.NP.Matching.AttributeScope.Room, Sony.NP.Matching.RoomAttributeVisibility.Internal);

        // External Room attributes
        attributesMetadata[8] = Sony.NP.Matching.AttributeMetadata.CreateBinaryAttribute("SPECIAL_MESSAGE", Sony.NP.Matching.AttributeScope.Room, Sony.NP.Matching.RoomAttributeVisibility.External, 128);

    }

    public Sony.NP.Matching.Attribute[] CreateDefaultAttributeValues()
    {
        string testBinary = "Some test binary data";
        byte[] data = System.Text.Encoding.ASCII.GetBytes(testBinary);

        //Sony.NP.Matching.Attribute[] attributes = new Sony.NP.Matching.Attribute[1];
        //attributes[0] = Sony.NP.Matching.Attribute.CreateBinaryAttribute(FindAttributeMetaData("NPC_NAME"), data);     

        Sony.NP.Matching.Attribute[] attributes = new Sony.NP.Matching.Attribute[9];

        // Member attributes
        attributes[0] = Sony.NP.Matching.Attribute.CreateBinaryAttribute(FindAttributeMetaData("CURRENT_STATE"), data);
        attributes[1] = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("MAX_BOOSTS"), nextMaxBoost);
        attributes[2] = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("MAX_TEAMS"), 3);

        // Room search attributes
        attributes[3] = Sony.NP.Matching.Attribute.CreateBinaryAttribute(FindAttributeMetaData("NPC_NAME"), data);
        attributes[4] = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("TRACK_ID"), 1);
        attributes[5] = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("COUNTRY_ID"), 2);
        attributes[6] = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("SEASON_ID"), 3);

        // Internal Room attributes
        attributes[7] = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("HOST_NAME"), 3);

        // External Room attributes
        attributes[8] = Sony.NP.Matching.Attribute.CreateBinaryAttribute(FindAttributeMetaData("SPECIAL_MESSAGE"), data);

        nextMaxBoost++;

        return attributes;
    }

    public Sony.NP.Matching.AttributeMetadata FindAttributeMetaData(string name)
    {
        for (int i = 0; i < attributesMetadata.Length; i++)
        {
            if (attributesMetadata[i].Name == name)
            {
                return attributesMetadata[i];
            }
        }

        OnScreenLog.AddError("FindAttributeMetaData : Can't find attribute metadata with name : " + name);
        return new Sony.NP.Matching.AttributeMetadata();
    }

    public void SetInitConfiguration(bool async)
    {
        try
        {
            Sony.NP.Matching.SetInitConfigurationRequest request = new Sony.NP.Matching.SetInitConfigurationRequest();

            request.UserId = User.GetActiveUserId;

            request.Attributes = attributesMetadata;

            request.Async = async;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Matching.SetInitConfiguration(request, response);
            if (async == true)
            {
                OnScreenLog.Add("SetInitConfiguration Async : Request Id = " + requestId);
            }
            else
            {
                if (response.ReturnCode == Sony.NP.Core.ReturnCodes.SUCCESS) // Success
                {
                    matchingInitialized = true;
                }
            }
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetWorlds()
    {
        try
        {
            Sony.NP.Matching.GetWorldsRequest request = new Sony.NP.Matching.GetWorldsRequest();
            request.UserId = User.GetActiveUserId;

            Sony.NP.Matching.WorldsResponse response = new Sony.NP.Matching.WorldsResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Matching.GetWorlds(request, response);
            OnScreenLog.Add("GetWorlds Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public Sony.NP.Matching.CreateRoomRequest CreateRoomRequest()
    {
        Sony.NP.Matching.CreateRoomRequest request = new Sony.NP.Matching.CreateRoomRequest();
        request.UserId = User.GetActiveUserId;

        Sony.NP.Matching.Attribute[] attributes = CreateDefaultAttributeValues();

        request.Attributes = attributes;

        request.Name = "Room : Created Frame " + OnScreenLog.FrameCount;
        request.Status = "Room was created on frame " + OnScreenLog.FrameCount;

        Sony.NP.Matching.LocalizedSessionInfo[] localisedInfo = new Sony.NP.Matching.LocalizedSessionInfo[2];
        localisedInfo[0] = new Sony.NP.Matching.LocalizedSessionInfo("German session text", "German text session created on frame " + OnScreenLog.FrameCount, "de");
        localisedInfo[1] = new Sony.NP.Matching.LocalizedSessionInfo("French session text", "French text session created on frame " + OnScreenLog.FrameCount, "fr");

        request.MaxNumMembers = 8;
        request.OwnershipMigration = Sony.NP.Matching.RoomMigrationType.OwnerBind; // OwnerMigration means session will be deleted when joined users reaches 0
        request.Topology = Sony.NP.Matching.TopologyType.Mesh;
        request.Visibility = Sony.NP.Matching.RoomVisibility.PublicRoom;
        request.WorldNumber = 1;
        request.JoinAllLocalUsers = true;

        // Must define an image for the rooms session otherwise it can't be created.
        //Sony.NP.Matching.SessionImage image = new Sony.NP.Matching.SessionImage();
        //image.SessionImgPath = Application.streamingAssetsPath + "/PS4SessionImage.jpg";
        //request.Image = image;

        request.Image = sessionImage;

        request.FixedData = System.Text.Encoding.ASCII.GetBytes("Fixed room data test");
        request.ChangeableData = System.Text.Encoding.ASCII.GetBytes("Changeable room data test setup on frame " + OnScreenLog.FrameCount);

        return request;
    }


    public void CreateRoom()
    {
        try
        {
            Sony.NP.Matching.CreateRoomRequest request = CreateRoomRequest();
            request.JoinAllLocalUsers = true;

            Sony.NP.Matching.RoomResponse response = new Sony.NP.Matching.RoomResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Matching.CreateRoom(request, response);
            OnScreenLog.Add("CreateRoom Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void LeaveRoom()
    {
        try
        {
            Sony.NP.Matching.LeaveRoomRequest request = new Sony.NP.Matching.LeaveRoomRequest();
            request.UserId = User.GetActiveUserId;

            request.RoomId = currentRoom.RoomId;

            Sony.NP.Matching.PresenceOptionData optiondata = new Sony.NP.Matching.PresenceOptionData();
            optiondata.DataAsString = "I'm out of here.";

            request.NotificationDataToMembers = optiondata;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Matching.LeaveRoom(request, response);
            OnScreenLog.Add("LeaveRoom Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SearchRooms(bool natFilter)
    {
        try
        {
            Sony.NP.Matching.SearchRoomsRequest request = new Sony.NP.Matching.SearchRoomsRequest();
            request.UserId = User.GetActiveUserId;

            Sony.NP.Matching.SearchClause[] searchClauses = new Sony.NP.Matching.SearchClause[1];

            searchClauses[0].AttributeToCompare = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("TRACK_ID"), 1);
            searchClauses[0].OperatorType = Sony.NP.Matching.SearchOperatorTypes.Equals;

            request.SearchClauses = searchClauses;

            request.ApplyNatTypeFilter = natFilter;

            Sony.NP.Matching.RoomsResponse response = new Sony.NP.Matching.RoomsResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Matching.SearchRooms(request, response);
            OnScreenLog.Add("SearchRooms Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void JoinSearchRoom()
    {
        try
        {
            Sony.NP.Matching.JoinRoomRequest request = new Sony.NP.Matching.JoinRoomRequest();
            request.UserId = User.GetActiveUserId;

            request.IdentifyRoomBy = Sony.NP.Matching.RoomJoiningType.Room;
            request.RoomId = foundRoom.RoomId;
            request.JoinAllLocalUsers = true;

            Sony.NP.Matching.RoomResponse response = new Sony.NP.Matching.RoomResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Matching.JoinRoom(request, response);
            OnScreenLog.Add("JoinRoom Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void TerminateMatchingService()
    {
        try
        {
            Sony.NP.Core.TerminateServiceRequest request = new Sony.NP.Core.TerminateServiceRequest();
            request.UserId = User.GetActiveUserId;

            request.Service = Sony.NP.ServiceTypes.Matching;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Core.TerminateService(request, response);
            OnScreenLog.Add("TerminateService Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetRoomPingTime()
    {
        try
        {
            Sony.NP.Matching.GetRoomPingTimeRequest request = new Sony.NP.Matching.GetRoomPingTimeRequest();
            request.UserId = User.GetActiveUserId;

            request.RoomId = currentRoom.RoomId;

            Sony.NP.Matching.GetRoomPingTimeResponse response = new Sony.NP.Matching.GetRoomPingTimeResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Matching.GetRoomPingTime(request, response);
            OnScreenLog.Add("GetRoomPingTime Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void KickOutRoomMember()
    {
        try
        {
            Sony.NP.Matching.KickOutRoomMemberRequest request = new Sony.NP.Matching.KickOutRoomMemberRequest();
            request.UserId = User.GetActiveUserId;

            request.RoomId = currentRoom.RoomId;
            request.MemberId = lastMemberToJoin;

            Sony.NP.Matching.PresenceOptionData optiondata = new Sony.NP.Matching.PresenceOptionData();
            optiondata.DataAsString = "Kicked out.";

            request.NotificationDataToMembers = optiondata;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Matching.KickOutRoomMember(request, response);
            OnScreenLog.Add("KickOutRoomMember Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SendRoomMessage()
    {
        try
        {
            Sony.NP.Matching.SendRoomMessageRequest request = new Sony.NP.Matching.SendRoomMessageRequest();
            request.UserId = User.GetActiveUserId;

            request.RoomId = currentRoom.RoomId;
            request.DataAsString = "Hello, this is a fucking crap bullshit message that tests the vulgarity filter.";
            request.IsChatMsg = true;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Matching.SendRoomMessage(request, response);
            OnScreenLog.Add("SendRoomMessage Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void ApplyMemberUpdate(Sony.NP.Matching.SetRoomInfoRequest request)
    {
        Sony.NP.Core.NpAccountId accountId = SonyNpUserProfiles.GetLocalAccountId(User.GetActiveUserId);

        request.RoomInfoType = Sony.NP.Matching.SetRoomInfoType.MemberInfo;

        Sony.NP.Matching.SetRoomInfoRequest.MemberInformation memberInfo = new Sony.NP.Matching.SetRoomInfoRequest.MemberInformation();
        memberInfo.MemberId = currentRoom.FindRoomMemberId(accountId);

        Sony.NP.Matching.Attribute[] attributes = new Sony.NP.Matching.Attribute[1];
        attributes[0] = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("MAX_BOOSTS"), nextMaxBoost);
        memberInfo.MemberAttributes = attributes;

        request.MemberInfo = memberInfo;

        nextMaxBoost++;
    }

    public void ApplyExternalRoomUpdate(Sony.NP.Matching.SetRoomInfoRequest request)
    {
        request.RoomInfoType = Sony.NP.Matching.SetRoomInfoType.RoomExternalInfo;

        Sony.NP.Matching.SetRoomInfoRequest.ExternalRoomInformation externalRoomInfo = new Sony.NP.Matching.SetRoomInfoRequest.ExternalRoomInformation();

        Sony.NP.Matching.Attribute[] externalAttributes = new Sony.NP.Matching.Attribute[1];
        externalAttributes[0] = Sony.NP.Matching.Attribute.CreateBinaryAttribute(FindAttributeMetaData("SPECIAL_MESSAGE"), System.Text.Encoding.ASCII.GetBytes("Updating external room attribute on frame " + OnScreenLog.FrameCount));
        externalRoomInfo.ExternalAttributes = externalAttributes;

        Sony.NP.Matching.Attribute[] searchAttributes = new Sony.NP.Matching.Attribute[1];
        searchAttributes[0] = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("SEASON_ID"), OnScreenLog.FrameCount);
        externalRoomInfo.SearchAttributes = searchAttributes;

        request.ExternalRoomInfo = externalRoomInfo;
    }

    public void ApplyInternalRoomUpdate(Sony.NP.Matching.SetRoomInfoRequest request)
    {
        request.RoomInfoType = Sony.NP.Matching.SetRoomInfoType.RoomInternalInfo;

        Sony.NP.Matching.SetRoomInfoRequest.InternalRoomInformation internalRoomInfo = new Sony.NP.Matching.SetRoomInfoRequest.InternalRoomInformation();

        Sony.NP.Matching.Attribute[] internalAttributes = new Sony.NP.Matching.Attribute[1];
        internalAttributes[0] = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("HOST_NAME"), OnScreenLog.FrameCount);
        internalRoomInfo.InternalAttributes = internalAttributes;

        internalRoomInfo.Visibility = Sony.NP.Matching.RoomVisibility.PrivateRoom;
        internalRoomInfo.CloseRoom = Sony.NP.Core.OptionalBoolean.setTrue;

        request.InternalRoomInfo = internalRoomInfo;
    }

    public void ApplySessionRoomUpdate(Sony.NP.Matching.SetRoomInfoRequest request)
    {
        request.RoomInfoType = Sony.NP.Matching.SetRoomInfoType.RoomSessionInfo;

        Sony.NP.Matching.SetRoomInfoRequest.RoomSessionInformation sessionRoomInfo = new Sony.NP.Matching.SetRoomInfoRequest.RoomSessionInformation();

        sessionRoomInfo.Status = "Room was updated on frame " + OnScreenLog.FrameCount;
        sessionRoomInfo.ChangeableData = System.Text.Encoding.ASCII.GetBytes("Changeable room data test was changed on frame " + OnScreenLog.FrameCount);
        sessionRoomInfo.DisplayOnSystem = Sony.NP.Core.OptionalBoolean.setFalse;

        request.RoomSessionInfo = sessionRoomInfo;
    }

    public void ApplyTopologyRoomUpdate(Sony.NP.Matching.SetRoomInfoRequest request)
    {
        request.RoomInfoType = Sony.NP.Matching.SetRoomInfoType.RoomTopology;

        request.RoomTopology = Sony.NP.Matching.TopologyType.Star;
    }

    Sony.NP.Matching.SetRoomInfoType nextSetRoomType = Sony.NP.Matching.SetRoomInfoType.MemberInfo;

    public void SetRoomInfo()
    {
        try
        {
            Sony.NP.Matching.SetRoomInfoRequest request = new Sony.NP.Matching.SetRoomInfoRequest();
            request.UserId = User.GetActiveUserId;

            request.RoomId = currentRoom.RoomId;

            if (nextSetRoomType == Sony.NP.Matching.SetRoomInfoType.MemberInfo)
            {
                ApplyMemberUpdate(request);
            }
            else if (nextSetRoomType == Sony.NP.Matching.SetRoomInfoType.RoomExternalInfo)
            {
                ApplyExternalRoomUpdate(request);
            }
            else if (nextSetRoomType == Sony.NP.Matching.SetRoomInfoType.RoomInternalInfo)
            {
                ApplyInternalRoomUpdate(request);
            }
            else if (nextSetRoomType == Sony.NP.Matching.SetRoomInfoType.RoomSessionInfo)
            {
                ApplySessionRoomUpdate(request);
            }
            else if (nextSetRoomType == Sony.NP.Matching.SetRoomInfoType.RoomTopology)
            {
                ApplyTopologyRoomUpdate(request);
            }

            nextSetRoomType++;

            if (nextSetRoomType > Sony.NP.Matching.SetRoomInfoType.RoomTopology)
            {
                nextSetRoomType = Sony.NP.Matching.SetRoomInfoType.MemberInfo;
            }

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Matching.SetRoomInfo(request, response);
            OnScreenLog.Add("SetRoomInfo Async : Request Id = " + requestId);

            OnScreenLog.Add("Updating : " + request.RoomInfoType);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    Sony.NP.Matching.RoomAttributeVisibility nextRoomAttributeVisibility = Sony.NP.Matching.RoomAttributeVisibility.Internal;

    public void GetAttributes()
    {
        try
        {
            Sony.NP.Matching.GetAttributesRequest request = new Sony.NP.Matching.GetAttributesRequest();
            request.UserId = User.GetActiveUserId;

            request.RoomId = currentRoom.RoomId;
            request.RoomAttributeVisibility = nextRoomAttributeVisibility;

            nextRoomAttributeVisibility++;

            if (nextRoomAttributeVisibility > Sony.NP.Matching.RoomAttributeVisibility.Search)
            {
                nextRoomAttributeVisibility = Sony.NP.Matching.RoomAttributeVisibility.Internal;
            }

            Sony.NP.Matching.RefreshRoomResponse response = new Sony.NP.Matching.RefreshRoomResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Matching.GetAttributes(request, response);
            OnScreenLog.Add("GetAttributes Async : Request Id = " + requestId);

            OnScreenLog.Add("Fetching : " + request.RoomAttributeVisibility);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetData()
    {
        try
        {
            Sony.NP.Matching.GetDataRequest request = new Sony.NP.Matching.GetDataRequest();
            request.UserId = User.GetActiveUserId;

            request.RoomId = currentRoom.RoomId;
            request.Type = Sony.NP.Matching.DataType.Changeable;

            Sony.NP.Matching.GetDataResponse response = new Sony.NP.Matching.GetDataResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Matching.GetData(request, response);
            OnScreenLog.Add("GetData Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    //public void SetMembersAsRecentlyMet()
    //{
    //    try
    //    {
    //        Sony.NP.Matching.SetMembersAsRecentlyMetRequest request = new Sony.NP.Matching.SetMembersAsRecentlyMetRequest();
    //        request.UserId = User.GetActiveUserId;

    //        request.RoomId = currentRoom.RoomId;
    //        request.Text = "Played NpToolkit2 together on frame " + OnScreenLog.FrameCount;

    //        // Get a list of member ids to mark as recently met
    //        request.Members = NetworkManager.GetMemberIds(Sony.NP.Matching.SetMembersAsRecentlyMetRequest.NUM_RECENTLY_MET_MAX_LEN);

    //        Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

    //        // Make the async call which will return the Request Id 
    //        int requestId = Sony.NP.Matching.SetMembersAsRecentlyMet(request, response);
    //        OnScreenLog.Add("SetMembersAsRecentlyMet Async : Request Id = " + requestId);
    //    }
    //    catch (Sony.NP.NpToolkitException e)
    //    {
    //        OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
    //    }
    //}

    public void SendInvitation()
    {
        try
        {
            Sony.NP.Matching.SendInvitationRequest request = new Sony.NP.Matching.SendInvitationRequest();
            request.UserId = User.GetActiveUserId;

            request.RoomId = currentRoom.RoomId;
            request.UserMessage = "Do you want to join a room in the NpToolkit2 sample?";
            request.MaxNumberRecipientsToAdd = 1;
            request.RecipientsEditableByUser = true;
            request.EnableDialog = true;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Matching.SendInvitation(request, response);
            OnScreenLog.Add("SendInvitation Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void HandleSessionInvitationEvent(Sony.NP.Matching.SessionInvitationEventResponse sessionInvitationEvent)
    {
        Thread thread = new Thread(new ParameterizedThreadStart(DoSessionInvitationEvent));
        thread.Start(sessionInvitationEvent);
    }

    public void DoSessionInvitationEvent(object sessionInvitationObject)
    {
        Sony.NP.Matching.SessionInvitationEventResponse sessionInvitationEvent = (Sony.NP.Matching.SessionInvitationEventResponse)sessionInvitationObject;

        // Must wait until there is an active gamepad.
        while (GamePad.activeGamePad == null)
        {

        }

        if (matchingInitialized == false)
        {
            OnScreenLog.Add("Initializing Np Matching..");
            // If the matching system has been initialised.
            // Note it is OK to call these asynchronously as the SetInitConfiguration and JoinRoom will be executed in order on the NPToolkit2 thread.
            SetInitConfiguration(false);
            OnScreenLog.Add("Matching system initialized");
        }

        try
        {
            Sony.NP.Matching.JoinRoomRequest request = new Sony.NP.Matching.JoinRoomRequest();
            request.UserId = User.GetActiveUserId;
            request.Async = false;

            request.IdentifyRoomBy = Sony.NP.Matching.RoomJoiningType.BoundSessionId;
            request.BoundSessionId = sessionInvitationEvent.SessionId;

            Sony.NP.Matching.RoomResponse response = new Sony.NP.Matching.RoomResponse();

            Sony.NP.Matching.JoinRoom(request, response);
            OnScreenLog.Add("Invite JoinRoom");

            OutputJoinRoom(response);

            if (response != null && response.ReturnCode == Sony.NP.Core.ReturnCodes.SUCCESS)
            {
                currentRoom = response.Room;
     //           NetworkManager.HandleJoinRoom(response);
            }
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }

    }

    public void HandlePlayTogetherHostEvent(Sony.NP.Matching.PlayTogetherHostEventResponse playTogetherHostEvent)
    {
        Thread thread = new Thread(new ParameterizedThreadStart(DoPlayTogetherHostEvent));
        thread.Start(playTogetherHostEvent);
    }

    public void DoPlayTogetherHostEvent(object playTogetherObject)
    {
        Sony.NP.Matching.PlayTogetherHostEventResponse playTogetherHostEvent = (Sony.NP.Matching.PlayTogetherHostEventResponse)playTogetherObject;

        // Must wait until there is an active gamepad.
        while (GamePad.activeGamePad == null)
        {

        }

        if (matchingInitialized == false)
        {
            OnScreenLog.Add("Initializing Np Matching..");
            // If the matching system has been initialised.
            // This needs to be called synchronously as the matching system needs to be initialised, then the room needs to be created and finally invites need to be sent to all the invitees.
            SetInitConfiguration(false);

            OnScreenLog.Add("Matching system initialized");
        }

        // Create a room and then invite all play together invitees to it.
        try
        {
            Sony.NP.Matching.CreateRoomRequest request = CreateRoomRequest();
            request.Async = false;
            request.JoinAllLocalUsers = false;

            Sony.NP.Matching.RoomResponse response = new Sony.NP.Matching.RoomResponse();

            // Make the async call which will return the Request Id 
            Sony.NP.Matching.CreateRoom(request, response);
            OnScreenLog.Add("Play Together CreateRoom");

            OutputCreateRoom(response);

            currentRoom = response.Room;

          //  NetworkManager.HandleCreateRoom(response);

            if ( response.ReturnCode == Sony.NP.Core.ReturnCodes.SUCCESS && playTogetherHostEvent.Invitees != null && playTogetherHostEvent.Invitees.Length > 0)
            {
                // Now send out invitiations
                OnScreenLog.AddNewLine();
                OnScreenLog.Add("Sending invitations");

                Sony.NP.Matching.SendInvitationRequest inviteRequest = new Sony.NP.Matching.SendInvitationRequest();
                inviteRequest.UserId = User.GetActiveUserId;
                inviteRequest.Async = false;

                inviteRequest.RoomId = currentRoom.RoomId;
                inviteRequest.UserMessage = "Do you want to join a Play Together room in the NpToolkit2 sample?";
                inviteRequest.RecipientsEditableByUser = false;
                inviteRequest.EnableDialog = false;

                Sony.NP.Core.NpAccountId[] accountIds = new Sony.NP.Core.NpAccountId[playTogetherHostEvent.Invitees.Length];

                for(int i = 0; i < accountIds.Length; i++)
                {
                    OnScreenLog.Add("     Invitee: " + playTogetherHostEvent.Invitees[i].AccountId);
                    accountIds[i] = playTogetherHostEvent.Invitees[i].AccountId;
                }

                inviteRequest.Recipients = accountIds;

                Sony.NP.Core.EmptyResponse emptyResponse = new Sony.NP.Core.EmptyResponse();

                // Make the async call which will return the Request Id 
                Sony.NP.Matching.SendInvitation(inviteRequest, emptyResponse);
            }
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void HandleRefreshRoom(Sony.NP.Matching.RefreshRoomResponse refreshRoomResponse)
    {
        if (refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.RoomDestroyed)
        {
            OnScreenLog.Add("HandleRefreshRoom : Room has been destroyed.", Color.green);
            // Reset the rooms
            currentRoom = null;
            foundRoom = null;
        }
        else if (refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.RoomKickedOut)
        {
            OnScreenLog.Add("HandleRefreshRoom : Kicked out of room.");
            currentRoom = null;
            foundRoom = null;
        }
        else if (refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.MemberJoined)
        {
            lastMemberToJoin = refreshRoomResponse.MemberInfo.RoomMemberId;
        }
        else if (refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.MemberLeft)
        {
            if (refreshRoomResponse.MemberInfo.RoomMemberId == lastMemberToJoin)
            {
                lastMemberToJoin = 0;
            }
        }
    }

    public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.Service == Sony.NP.ServiceTypes.Matching)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.MatchingSetInitConfiguration:

                    if (callbackEvent.Response.ReturnCode == Sony.NP.Core.ReturnCodes.SUCCESS) // Success
                    {
                        matchingInitialized = true;
                    }

                    OutputSetInitConfiguration(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.MatchingGetWorlds:
                    OutputGetWorlds(callbackEvent.Response as Sony.NP.Matching.WorldsResponse);
                    break;
                case Sony.NP.FunctionTypes.MatchingCreateRoom:
                    {
                        Sony.NP.Matching.RoomResponse cr = callbackEvent.Response as Sony.NP.Matching.RoomResponse;
                        if (cr != null && cr.ReturnCode == Sony.NP.Core.ReturnCodes.SUCCESS)
                        {
                            currentRoom = cr.Room;
                        }
                        OutputCreateRoom(callbackEvent.Response as Sony.NP.Matching.RoomResponse);
                    }
                    break;
                case Sony.NP.FunctionTypes.MatchingLeaveRoom:
                    {
                        OutputLeaveRoom(callbackEvent.Response as Sony.NP.Core.EmptyResponse);

                        Sony.NP.Core.NpAccountId accountId = SonyNpUserProfiles.GetLocalAccountId(callbackEvent.Request.UserId.Id);
                        ushort memberId = currentRoom.FindRoomMemberId(accountId);
                        //if (NetworkManager.IsLocalOwner(memberId) ||
                        //    (NetworkManager.IsLocalMember(memberId) == true && NetworkManager.CurrentRoomNumLocalUsers <= 1))
                        //{
                        //    // Local owner of the room is about to leave and there is only 1 of them left. The room is no longer required.
                        //    // If this is the host then the room will be destoryed, otherwise if this is a remote connection the room is no longer
                        //    // required on this console.
                        //    currentRoom = null;
                        //    foundRoom = null;
                        //}
                    }
                    break;
                case Sony.NP.FunctionTypes.MatchingSearchRooms:
                    {
                        Sony.NP.Matching.RoomsResponse cr = callbackEvent.Response as Sony.NP.Matching.RoomsResponse;
                        if (cr != null && cr.ReturnCode == Sony.NP.Core.ReturnCodes.SUCCESS)
                        {
                            if (cr.Rooms != null && cr.Rooms.Length > 0)
                            {
                                foundRoom = cr.Rooms[0];
                            }
                        }

                        OutputSearchRooms(callbackEvent.Response as Sony.NP.Matching.RoomsResponse);
                    }
                    break;
                case Sony.NP.FunctionTypes.MatchingJoinRoom:
                    {
                        Sony.NP.Matching.RoomResponse cr = callbackEvent.Response as Sony.NP.Matching.RoomResponse;
                        if (cr != null && cr.ReturnCode == Sony.NP.Core.ReturnCodes.SUCCESS)
                        {
                            currentRoom = cr.Room;
                        }
                        OutputJoinRoom(callbackEvent.Response as Sony.NP.Matching.RoomResponse);
                    }
                    break;
                case Sony.NP.FunctionTypes.MatchingGetRoomPingTime:
                    OutputGetRoomPingTime(callbackEvent.Response as Sony.NP.Matching.GetRoomPingTimeResponse);
                    break;
                case Sony.NP.FunctionTypes.MatchingSendRoomMessage:
                    OutputSendRoomMessage(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.MatchingGetAttributes:
                    OutputGetAttributes(callbackEvent.Response as Sony.NP.Matching.RefreshRoomResponse);
                    break;
                case Sony.NP.FunctionTypes.MatchingGetData:
                    OutputGetData(callbackEvent.Response as Sony.NP.Matching.GetDataResponse);
                    break;
                case Sony.NP.FunctionTypes.MatchingSendInvitation:
                    OutputSendInvitation(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.MatchingSetRoomInfo:
                    OutputSetRoomInfo(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.MatchingSetMembersAsRecentlyMet:
                    OutputSetMembersAsRecentlyMet(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                default:
                    break;
            }
        }

        // Notifications
        if (callbackEvent.Service == Sony.NP.ServiceTypes.Notification)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.NotificationSessionInvitationEvent:
                    HandleSessionInvitationEvent(callbackEvent.Response as Sony.NP.Matching.SessionInvitationEventResponse);
                    break;
                case Sony.NP.FunctionTypes.NotificationPlayTogetherHostEvent:
                    HandlePlayTogetherHostEvent(callbackEvent.Response as Sony.NP.Matching.PlayTogetherHostEventResponse);
                    break;
                case Sony.NP.FunctionTypes.NotificationRefreshRoom:
                    HandleRefreshRoom(callbackEvent.Response as Sony.NP.Matching.RefreshRoomResponse);
                    break;
                default:
                    break;
            }
        }
    }




    private void OutputSetInitConfiguration(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputSetInitConfiguration Empty Response");

        if (response.Locked == false)
        {

        }
    }

    private void OutputGetWorlds(Sony.NP.Matching.WorldsResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("GetWorlds Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("NumWorlds : " + response.Worlds.Length);

            for(int i = 0; i < response.Worlds.Length; i++)
            {
                OnScreenLog.Add("World Id = " + response.Worlds[i].WorldId.Id + " : Num Rooms = " + response.Worlds[i].CurrentNumberOfRooms + " : Num Members = " + response.Worlds[i].CurrentNumberOfMembers + " : World Num = " + response.Worlds[i].WorldNumber.Num);
            }
        }
    }

    private void OutputCreateRoom(Sony.NP.Matching.RoomResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("CreateRoom Response");

        if (response.Locked == false)
        {
            if (response.ReturnCode == Sony.NP.Core.ReturnCodes.ERROR_MATCHING_USER_IS_ALREADY_IN_A_ROOM)
            {
                OnScreenLog.Add("CreateRoom Response : User already in a room.");
            }
            else if (response.ReturnCode == Sony.NP.Core.ReturnCodes.NP_MATCHING2_ERROR_CONTEXT_NOT_STARTED)
            {
                OnScreenLog.Add("CreateRoom Response : Content not started. Probably another user has already use matching methods.");
            }
            else
            {
                OutputRoom(response.Room);
            }
        }
    }

    private void OutputJoinRoom(Sony.NP.Matching.RoomResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("JoinRoom Response");

        if (response.Locked == false)
        {
            if (response.ReturnCode == Sony.NP.Core.ReturnCodes.NP_MATCHING2_ERROR_CONTEXT_NOT_STARTED)
            {
                OnScreenLog.Add("JoinRoom Response : Content not started. Probably another user has already use matching methods.");
            }
            else
            {
                if (response.ReturnCode == Sony.NP.Core.ReturnCodes.ERROR_MATCHING_USER_IS_ALREADY_IN_A_ROOM)
                {
                    OnScreenLog.Add("JoinRoom Response : User already in a room.");
                }
                OutputRoom(response.Room);
            }
        }
    }

    private void OutputLeaveRoom(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputLeaveRoom Empty Response");

        if (response.Locked == false)
        {

        }
    }

    private void OutputSearchRooms(Sony.NP.Matching.RoomsResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputSearchRooms Response");

        if (response.Locked == false)
        {
            Sony.NP.Matching.Room[] rooms = response.Rooms;

            for(int i = 0; i < rooms.Length; i++)
            {
                OutputRoomLite(rooms[i]);
            }
        }
    }

    private void OutputGetRoomPingTime(Sony.NP.Matching.GetRoomPingTimeResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputGetRoomPingTime Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("RoundTripTime : " + response.RoundTripTime);
        }
    }

    private void OutputSendInvitation(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputSendInvitation Empty Response");

        if (response.Locked == false)
        {

        }
    }

    public static string BuildAttributeString(Sony.NP.Matching.Attribute attribute)
    {
        string output = "Attribute : " + attribute.Metadata.Name;

        if (attribute.Metadata.Type == Sony.NP.Matching.AttributeType.Integer)
        {
            output += " : Value = " + attribute.IntValue;
        }
        else if (attribute.Metadata.Type == Sony.NP.Matching.AttributeType.Binary)
        {
            string text = System.Text.Encoding.ASCII.GetString(attribute.BinValue);
            output += " : String = " + text;
        }

        return output;
    }

    public static string BuildSignalingInformationString(Sony.NP.Matching.MemberSignalingInformation signalingInformation)
    {
        string output = "NatType = " + signalingInformation.NatType + " : Status = " + signalingInformation.Status +
                        " : RoundTripTime = " + signalingInformation.RoundTripTime + " : ipAddress = " + signalingInformation.IpAddress +
                        " : Port = " + signalingInformation.Port;

        return output;
    }

    // single line output for a room
    private void OutputRoomLite(Sony.NP.Matching.Room room)
    {
        string output = "RoomId = " + room.RoomId;

        output += " : Name = " + room.Name;

        Sony.NP.Matching.Member[] members = room.CurrentMembers;

        if (members != null)
        {
            output += " : # Members = " + room.CurrentMembers.Length;

            for (int i = 0; i < members.Length; i++)
            {
                if (members[i].IsOwner == true)
                {
                    output += " : Owner = " + members[i].OnlineUser.OnlineID;
                }
            }
        }
        else
        {
            output += " : # Members = 0";
        }

        OnScreenLog.Add(output);
    }

    private void OutputRoom(Sony.NP.Matching.Room room)
    {
        OnScreenLog.Add("MatchingContext : " + room.MatchingContext);
        OnScreenLog.Add("ServerId : " + room.ServerId);
        OnScreenLog.Add("WorldId : " + room.WorldId);
        OnScreenLog.Add("RoomId : " + room.RoomId);

        Sony.NP.Matching.Attribute[] attributes = room.Attributes;
        OnScreenLog.Add("Num Attributes : " + attributes.Length);

        for (int i = 0; i < attributes.Length; i++)
        {
            string output = "     " + BuildAttributeString(attributes[i]);
            OnScreenLog.Add(output);
        }

        OnScreenLog.Add("Name : " + room.Name);

        Sony.NP.Matching.Member[] members = room.CurrentMembers;

        OnScreenLog.Add("Num Memebers : " + members.Length);

        for (int i = 0; i < members.Length; i++)
        {
            string output = "     Member : " + members[i].OnlineUser;

            output += "\n          Attributes = : ";

            for (int a = 0; a < members[i].MemberAttributes.Length; a++)
            {
                output += "\n               " + BuildAttributeString(members[i].MemberAttributes[a]);
                output += "\n";
            }

            output += "\n          JoinedDate : " + members[i].JoinedDate;

            output += "\n          SignalingInformation : " + BuildSignalingInformationString(members[i].SignalingInformation);

            output += "\n          Platform : " + members[i].Platform;
            output += "\n          RoomMemberId : " + members[i].RoomMemberId;
            output += "\n          IsOwner : " + members[i].IsOwner;
            output += "\n          IsMe : " + members[i].IsMe;

            OnScreenLog.Add(output, true);
        }

        OnScreenLog.Add("NumMaxMembers : " + room.NumMaxMembers);
        OnScreenLog.Add("Topology : " + room.Topology);
        OnScreenLog.Add("NumReservedSlots : " + room.NumReservedSlots);

        OnScreenLog.Add("IsNatRestricted : " + room.IsNatRestricted);
        OnScreenLog.Add("AllowBlockedUsersOfOwner : " + room.AllowBlockedUsersOfOwner);
        OnScreenLog.Add("AllowBlockedUsersOfMembers : " + room.AllowBlockedUsersOfMembers);
        OnScreenLog.Add("JoinAllLocalUsers : " + room.JoinAllLocalUsers);

        OnScreenLog.Add("OwnershipMigration : " + room.OwnershipMigration);
        OnScreenLog.Add("Visibility : " + room.Visibility);

        OnScreenLog.Add("Password : " + room.Password);
        OnScreenLog.Add("BoundSessionId : " + room.BoundSessionId);

        OnScreenLog.Add("IsSystemJoinable : " + room.IsSystemJoinable);
        OnScreenLog.Add("DisplayOnSystem : " + room.DisplayOnSystem);
        OnScreenLog.Add("HasChangeableData : " + room.HasChangeableData);
        OnScreenLog.Add("HasFixedData : " + room.HasFixedData);
        OnScreenLog.Add("IsCrossplatform : " + room.IsCrossplatform);
        OnScreenLog.Add("IsClosed : " + room.IsClosed);
    }

    private void OutputSendRoomMessage(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputSendRoomMessage Empty Response");

        if (response.Locked == false)
        {

        }
    }

    private void OutputSetRoomInfo(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputSetRoomInfo Response");

        if (response.Locked == false)
        {

        }
    }

    private void OutputSetMembersAsRecentlyMet(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputSetMembersAsRecentlyMet Response");

        if (response.Locked == false)
        {

        }
    }

    private void OutputGetAttributes(Sony.NP.Matching.RefreshRoomResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputGetAttributes Response");

        if (response.Locked == false)
        {
            SonyNpNotifications.OutputRefreshRoom(response);
        }
    }

    private void OutputGetData(Sony.NP.Matching.GetDataResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputGetData Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("Type : " + response.Type);

            string data = "";
            if (response.Data != null)
            {
                for (int i = 0; i < response.Data.Length; i++)
                {
                    data += response.Data[i] + ", ";
                }
            }

            OnScreenLog.Add("Binary Data : " + data);
            OnScreenLog.Add("Data As String : " + System.Text.Encoding.ASCII.GetString(response.Data));
        }
    }

}
#endif
