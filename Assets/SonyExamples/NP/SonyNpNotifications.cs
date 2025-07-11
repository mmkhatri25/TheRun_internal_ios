using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS4
class SonyNpNotifications
{
    public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {

        if (callbackEvent.Service == Sony.NP.ServiceTypes.Notification)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.NotificationAborted:
                    OnScreenLog.AddWarning("Request has been aborted (" + callbackEvent.NpRequestId + ")");
                    break;
                case Sony.NP.FunctionTypes.NotificationDialogOpened:
                    break;
                case Sony.NP.FunctionTypes.NotificationDialogClosed:
                    break;
                case Sony.NP.FunctionTypes.NotificationUpdateFriendsList:
                    OutputFriendListUpdate(callbackEvent.Response as Sony.NP.Friends.FriendListUpdateResponse);
                    break;
                case Sony.NP.FunctionTypes.NotificationUpdateFriendPresence:
                    OutputPresenceUpdate(callbackEvent.Response as Sony.NP.Presence.PresenceUpdateResponse);
                    break;

                case Sony.NP.FunctionTypes.NotificationUserStateChange:
                    OutputUserStateChange(callbackEvent.Response as Sony.NP.NpUtils.UserStateChangeResponse);
                    break;

                case Sony.NP.FunctionTypes.NotificationNetStateChange:
                    OutputNetStateChange(callbackEvent.Response as Sony.NP.NetworkUtils.NetStateChangeResponse);
                    break;

                case Sony.NP.FunctionTypes.NotificationRefreshRoom:
                    OutputRefreshRoom(callbackEvent.Response as Sony.NP.Matching.RefreshRoomResponse);
                    break;

                case Sony.NP.FunctionTypes.NotificationNewInvitation:
                    OutputInvitationReceived(callbackEvent.Response as Sony.NP.Matching.InvitationReceivedResponse);
                    break;

                case Sony.NP.FunctionTypes.NotificationNewRoomMessage:
                    OutputNewRoomMessage(callbackEvent.Response as Sony.NP.Matching.NewRoomMessageResponse);
                    break;

                case Sony.NP.FunctionTypes.NotificationSessionInvitationEvent:
                    OutputSessionInvitationEvent(callbackEvent.Response as Sony.NP.Matching.SessionInvitationEventResponse);
                    break;

                case Sony.NP.FunctionTypes.NotificationPlayTogetherHostEvent:
                    OutputPlayTogetherHostEvent(callbackEvent.Response as Sony.NP.Matching.PlayTogetherHostEventResponse);
                    break;

                case Sony.NP.FunctionTypes.NotificationNewInGameMessage:
                    OutputNewInGameMessage(callbackEvent.Response as Sony.NP.Messaging.NewInGameMessageResponse);
                    break;

                case Sony.NP.FunctionTypes.NotificationNewGameDataMessage:
                    OutputNewGameDataMessage(callbackEvent.Response as Sony.NP.Messaging.NewGameDataMessageResponse);
                    break;

                case Sony.NP.FunctionTypes.NotificationGameCustomDataEvent:
                    OutputGameCustomDataEvent(callbackEvent.Response as Sony.NP.Messaging.GameCustomDataEventResponse);
                    break;
            }
        }
    }

    private void OutputFriendListUpdate(Sony.NP.Friends.FriendListUpdateResponse friendListUpdateResponse)
    {
        if (friendListUpdateResponse == null) return;

        OnScreenLog.Add("Update Friends List");

        if (friendListUpdateResponse.Locked == false)
        {
            OnScreenLog.Add("Local Updated User : " + friendListUpdateResponse.LocalUpdatedUser);
            OnScreenLog.Add("Remote User : " + friendListUpdateResponse.RemoteUser);
            OnScreenLog.Add("User Id : " + friendListUpdateResponse.UserId);
            OnScreenLog.Add("FriendlistUpdateEvent : " + friendListUpdateResponse.EventType);
        }

        OnScreenLog.AddNewLine();
    }

    private void OutputPresenceUpdate(Sony.NP.Presence.PresenceUpdateResponse presenceUpdateResponse)
    {
        if (presenceUpdateResponse == null) return;

        OnScreenLog.Add("Presence Update");

        if (presenceUpdateResponse.Locked == false)
        {
            OnScreenLog.Add("Local Updated User : " + presenceUpdateResponse.LocalUpdatedUser);
            OnScreenLog.Add("Remote User : " + presenceUpdateResponse.RemoteUser);
            OnScreenLog.Add("User Id : " + presenceUpdateResponse.UserId);
            OnScreenLog.Add("PresenceUpdateType : " + presenceUpdateResponse.UpdateType);

            if (presenceUpdateResponse.UpdateType == Sony.NP.Presence.PresenceUpdateType.gameStatus)
            {
                OnScreenLog.Add("Game Status : " + presenceUpdateResponse.GameStatus);
            }
            else if (presenceUpdateResponse.UpdateType == Sony.NP.Presence.PresenceUpdateType.gameData)
            {
                string gameData = "";
                if (presenceUpdateResponse.BinaryGameData != null)
                {
                    for (int i = 0; i < presenceUpdateResponse.BinaryGameData.Length; i++)
                    {
                        gameData += presenceUpdateResponse.BinaryGameData[i] + " ";
                    }
                }

                OnScreenLog.Add("Binary Game Data : " + gameData);
            }

            OnScreenLog.Add("Platform : " + presenceUpdateResponse.Platform);
        }

        OnScreenLog.AddNewLine();
    }

    private void OutputUserStateChange(Sony.NP.NpUtils.UserStateChangeResponse userStateChangeResponse)
    {
        if (userStateChangeResponse == null) return;

        OnScreenLog.Add("User State Change");

        if (userStateChangeResponse.Locked == false)
        {
            OnScreenLog.Add("User Id : " + userStateChangeResponse.UserId);
            OnScreenLog.Add("Current SignIn State : " + userStateChangeResponse.CurrentSignInState);
            OnScreenLog.Add("Current LogIn state: " + userStateChangeResponse.CurrentLogInState);
            OnScreenLog.Add("State Changed : " + userStateChangeResponse.StateChanged);
        }

        OnScreenLog.AddNewLine();
    }

    private void OutputNetStateChange(Sony.NP.NetworkUtils.NetStateChangeResponse netStateChangeResponse)
    {
        if (netStateChangeResponse == null) return;

        OnScreenLog.Add("Net State Change");

        if (netStateChangeResponse.Locked == false)
        {
            OnScreenLog.Add("Event : " + netStateChangeResponse.NetEvent); 
        }

        OnScreenLog.AddNewLine();
    }

    static public void OutputRefreshRoom(Sony.NP.Matching.RefreshRoomResponse refreshRoomResponse)
    {
        if (refreshRoomResponse == null) return;

        OnScreenLog.Add("Refresh Room");

        if (refreshRoomResponse.Locked == false)
        {
            OnScreenLog.Add("RoomId : " + refreshRoomResponse.RoomId);
            OnScreenLog.Add("notificationFromMember : " + refreshRoomResponse.NotificationFromMember.DataAsString);
            OnScreenLog.Add("Cause : " + refreshRoomResponse.Cause);
            OnScreenLog.Add("Reason : " + refreshRoomResponse.Reason);

            if (refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.MemberJoined ||
                refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.MemberLeft ||
                refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.MemberSignalingUpdate ||
                refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.MemberInfoUpdated)
            {
                Sony.NP.Matching.Member member = refreshRoomResponse.MemberInfo;

                if (member != null)
                {
                    string output = "     Member : " + member.OnlineUser;

                    output += "\n          Attributes = : ";

                    for (int a = 0; a < member.MemberAttributes.Length; a++)
                    {
                        output += "\n               " + SonyNpMatching.BuildAttributeString(member.MemberAttributes[a]);
                        //output += "\n";
                    }

                    output += "\n          JoinedDate : " + member.JoinedDate;

                    output += "\n          SignalingInformation : " + SonyNpMatching.BuildSignalingInformationString(member.SignalingInformation);

                    output += "\n          Platform : " + member.Platform;
                    output += "\n          RoomMemberId : " + member.RoomMemberId;
                    output += "\n          IsOwner : " + member.IsOwner;
                    output += "\n          IsMe : " + member.IsMe;

                    OnScreenLog.Add(output, true);
                }
                else
                {
                    OnScreenLog.AddError("Expecting MemberInfo when reason is " + refreshRoomResponse.Reason);
                }
            }
            else if (refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.OwnerChanged)
            {
                Sony.NP.Matching.RefreshRoomResponse.OwnerInformation ownerInfo = refreshRoomResponse.OwnerInfo;

                if (ownerInfo != null)
                {
                    string output = "     OwnerInfo : ";
                    output += "\n          Password = : " + ownerInfo.Password;
                    for (int i = 0; i < ownerInfo.OldAndNewOwners.Length; i++)
                    {
                        output += "\n          Member Id = : " + ownerInfo.OldAndNewOwners[i];
                    }

                    OnScreenLog.Add(output, true);
                }
                else
                {
                    OnScreenLog.AddError("Expecting OwnerInfo when reason is " + refreshRoomResponse.Reason);
                }
            }
            else if (refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.RoomDestroyed ||
                     refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.RoomKickedOut)
            {
                string roomLeftError = "0x" + refreshRoomResponse.RoomLeftError.ToString("X16");
                OnScreenLog.Add("     RoomLeftError : " + roomLeftError);
            }
            else if (refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.RoomExternalInfoUpdated)
            {
                Sony.NP.Matching.RefreshRoomResponse.RoomExternalInformation roomExternalInfo = refreshRoomResponse.RoomExternalInfo;

                if (roomExternalInfo != null)
                {
                    string output = "     RoomExternalInfo : ";
                    output += "\n          Attributes = : ";
                    for (int i = 0; i < roomExternalInfo.Attributes.Length; i++)
                    {
                        output += "\n              " + SonyNpMatching.BuildAttributeString(roomExternalInfo.Attributes[i]);
                    }

                    OnScreenLog.Add(output, true);
                }
                else
                {
                    OnScreenLog.AddError("Expecting RoomExternalInfo when reason is " + refreshRoomResponse.Reason);
                }
            }
            else if (refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.RoomInternalInfoUpdated)
            {
                Sony.NP.Matching.RefreshRoomResponse.RoomInternalInformation roomInternalInfo = refreshRoomResponse.RoomInternalInfo;

                if (roomInternalInfo != null)
                {
                    string output = "     RoomInternalInfo : ";
                    output += "\n          Attributes = : ";
                    for (int i = 0; i < roomInternalInfo.Attributes.Length; i++)
                    {
                        output += "\n              " + SonyNpMatching.BuildAttributeString(roomInternalInfo.Attributes[i]);
                    }

                    output += "\n          AllowBlockedUsersOfMembers : " + roomInternalInfo.AllowBlockedUsersOfMembers;
                    output += "\n          JoinAllLocalUsers : " + roomInternalInfo.JoinAllLocalUsers;
                    output += "\n          IsNatRestricted : " + roomInternalInfo.IsNatRestricted;
                    output += "\n          NumReservedSlots : " + roomInternalInfo.NumReservedSlots;

                    output += "\n          Visibility : " + roomInternalInfo.Visibility;
                    output += "\n          CloseRoom : " + roomInternalInfo.CloseRoom;

                    OnScreenLog.Add(output, true);
                }
                else
                {
                    OnScreenLog.AddError("Expecting RoomInternalInfo when reason is " + refreshRoomResponse.Reason);
                }
            }
            else if (refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.RoomSessionInfoUpdated)
            {
                Sony.NP.Matching.RefreshRoomResponse.RoomSessionInformation roomSessionInfo = refreshRoomResponse.RoomSessionInfo;

                if (roomSessionInfo != null)
                {
                    string output = "     RoomSessionInfo : ";
                    output += "\n          DisplayOnSystem : " + roomSessionInfo.DisplayOnSystem;
                    output += "\n          IsSystemJoinable : " + roomSessionInfo.IsSystemJoinable;
                    output += "\n          HasChangeableData : " + roomSessionInfo.HasChangeableData;
                    output += "\n          BoundSessionId : " + roomSessionInfo.BoundSessionId;
                    OnScreenLog.Add(output, true);
                }
                else
                {
                    OnScreenLog.AddError("Expecting RoomSessionInfo when reason is " + refreshRoomResponse.Reason);
                }
            }
            else if (refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.RoomTopologyUpdated)
            {
                OnScreenLog.Add("     Topology :" + refreshRoomResponse.RoomTopology);
            }
        }

        OnScreenLog.AddNewLine();
    }

    private void OutputInvitationReceived(Sony.NP.Matching.InvitationReceivedResponse invitationReceivedResponse)
    {
        if (invitationReceivedResponse == null) return;

        OnScreenLog.Add("Invitation Received");

        if (invitationReceivedResponse.Locked == false)
        {
            OnScreenLog.Add("LocalUpdatedUser : " + invitationReceivedResponse.LocalUpdatedUser);
            OnScreenLog.Add("RemoteUser : " + invitationReceivedResponse.RemoteUser);
            OnScreenLog.Add("Platform : " + invitationReceivedResponse.Platform);
        }

        OnScreenLog.AddNewLine();
    }

    private void OutputNewRoomMessage(Sony.NP.Matching.NewRoomMessageResponse newRoomMessage)
    {
        if (newRoomMessage == null) return;

        OnScreenLog.Add("New Room Message");

        if (newRoomMessage.Locked == false)
        {
            OnScreenLog.Add("RoomId : " + newRoomMessage.RoomId);
            OnScreenLog.Add("Sender : " + newRoomMessage.Sender);
            OnScreenLog.Add("IsChatMsg : " + newRoomMessage.IsChatMsg);
            OnScreenLog.Add("IsFiltered : " + newRoomMessage.IsFiltered);

            if (newRoomMessage.IsChatMsg)
            {
                OnScreenLog.Add("DataAsString : " + newRoomMessage.DataAsString);
            }
            else
            {
                string data = "";
                if (newRoomMessage.Data != null)
                {
                    for (int i = 0; i < newRoomMessage.Data.Length; i++)
                    {
                        data += newRoomMessage.Data[i] + ", ";
                    }
                }

                OnScreenLog.Add("Binary Data : " + data);
            }
        }

        OnScreenLog.AddNewLine();
    }

    private void OutputSessionInvitationEvent(Sony.NP.Matching.SessionInvitationEventResponse sessionInvitationEvent)
    {
        if (sessionInvitationEvent == null) return;

        OnScreenLog.Add("Session Invitation Event ");

        if (sessionInvitationEvent.Locked == false)
        {
            OnScreenLog.Add("SessionId : " + sessionInvitationEvent.SessionId);
            OnScreenLog.Add("InvitationId : " + sessionInvitationEvent.InvitationId);
            OnScreenLog.Add("AcceptedInvite : " + sessionInvitationEvent.AcceptedInvite);
            OnScreenLog.Add("OnlineId : " + sessionInvitationEvent.OnlineId);
            OnScreenLog.Add("UserId : " + sessionInvitationEvent.UserId);
            OnScreenLog.Add("ReferralOnlineId : " + sessionInvitationEvent.ReferralOnlineId);
            OnScreenLog.Add("ReferralAccountId : " + sessionInvitationEvent.ReferralAccountId);
        }

        OnScreenLog.AddNewLine();
    }

    private void OutputPlayTogetherHostEvent(Sony.NP.Matching.PlayTogetherHostEventResponse playTogetherHostEvent)
    {
        if (playTogetherHostEvent == null) return;

        OnScreenLog.Add("Play Together Host Event ");

        if (playTogetherHostEvent.Locked == false)
        {
            OnScreenLog.Add("Host UserId : " + playTogetherHostEvent.UserId);
            OnScreenLog.Add("   Invitees : ");

            if (playTogetherHostEvent.Invitees != null)
            {
                for (int i = 0; i < playTogetherHostEvent.Invitees.Length; i++)
                {
                    OnScreenLog.Add("           : " + playTogetherHostEvent.Invitees[i].AccountId + " : " + playTogetherHostEvent.Invitees[i].OnlineId);
                }
            }
        }

        OnScreenLog.AddNewLine();
    }

    public void OutputData(byte[] data)
    {
        string output = "";
        string outputLine = "";
        for (int i = 0; i < data.Length; i++)
        {
            outputLine += data[i] + ", ";

            if (outputLine.Length > 160)
            {
                output += outputLine + "\n      ";
                outputLine = "";
            }
        }

        output += outputLine;

        OnScreenLog.Add(output, true);
        OnScreenLog.AddNewLine();
    }

    private void OutputNewInGameMessage(Sony.NP.Messaging.NewInGameMessageResponse newInGameMessage)
    {
        if (newInGameMessage == null) return;

        OnScreenLog.Add("New In Game Message ");

        if (newInGameMessage.Locked == false)
        {
            OnScreenLog.Add("Message data:");
            OutputData(newInGameMessage.Message);
            OnScreenLog.Add("Sender :" + newInGameMessage.Sender);
            OnScreenLog.Add("SenderPlatformType :" + newInGameMessage.SenderPlatformType);
            OnScreenLog.Add("Recipient :" + newInGameMessage.Recipient);
            OnScreenLog.Add("RecipientPlatformType :" + newInGameMessage.RecipientPlatformType);
        }

        OnScreenLog.AddNewLine();
    }

    private void OutputNewGameDataMessage(Sony.NP.Messaging.NewGameDataMessageResponse newGameDataMessage)
    {
        if (newGameDataMessage == null) return;

        OnScreenLog.Add("New Game Data Message ");

        if (newGameDataMessage.Locked == false)
        {
            OnScreenLog.Add("To :" + newGameDataMessage.To);
            OnScreenLog.Add("From :" + newGameDataMessage.From);
        }

        OnScreenLog.AddNewLine();
    }

    private void OutputGameCustomDataEvent(Sony.NP.Messaging.GameCustomDataEventResponse gameCustomDataEventResponse)
    {
        if (gameCustomDataEventResponse == null) return;

        OnScreenLog.Add("Game Custom Data Event");

        if (gameCustomDataEventResponse.Locked == false)
        {
            OnScreenLog.Add("ItemId :" + gameCustomDataEventResponse.ItemId);
            OnScreenLog.Add("OnlineId :" + gameCustomDataEventResponse.OnlineId);
            OnScreenLog.Add("UserId :" + gameCustomDataEventResponse.UserId);
        }

        OnScreenLog.AddNewLine();
    }

}
#endif
