using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS4
public class SonyNpTus : IScreen
{
    MenuLayout m_MenuTus;

    string virtualUserOnlineID = "_ERGVirtualUser1";

    public SonyNpTus()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuTus;
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void Process(MenuStack stack)
    {
        MenuUserProfiles(stack);
    }

    public void Initialize()
    {
        m_MenuTus = new MenuLayout(this, 450, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuTus.Update();

        if (m_MenuTus.AddItem("Set Variables", "Set the TUS variables."))
        {
            SetVariables();
        }

        if (m_MenuTus.AddItem("Get Variables", "Get the TUS variables."))
        {
            GetVariables();
        }

        if (m_MenuTus.AddItem("Set Variables (Virtual User)", "Set the TUS variables for the virtual user."))
        {
            SetVariablesVirtualUser();
        }

        if (m_MenuTus.AddItem("Get Variables (Virtual User)", "Get the TUS variables for the virtual user."))
        {
            GetVariablesVirtualUser();
        }

        if (m_MenuTus.AddItem("Add To And Get Varaiable", "Atomic add and get variable."))
        {
            AddToAndGetVariable();
        }

        if (m_MenuTus.AddItem("Set Data", "Set TUS data."))
        {
            SetData();
        }

        if (m_MenuTus.AddItem("Get Data", "Get the TUS data."))
        {
            GetData();
        }

        if (m_MenuTus.AddItem("Delete Data", "Delete TUS data."))
        {
            DeleteData();
        }

        if (m_MenuTus.AddItem("Try And Set Variable", "Try and set a TUS variable"))
        {
            TryAndSetVariable();
        }

        if (m_MenuTus.AddItem("Get Friends Variable", "Get friends TUS slot values"))
        {
            GetFriendsVariable();
        }

        Sony.NP.Friends.FriendsResponse friendsResponse = SonyNpFriends.latestFriendsResponse;
        bool enableGetUsersVariable = false;
        if ( friendsResponse != null && friendsResponse.Friends != null && friendsResponse.Friends.Length > 0)
        {
            enableGetUsersVariable = true;
        }

        if (m_MenuTus.AddItem("Get Users Variable", "Get Users variable from a list of user. Use 'Request All Friends' to fetch a list of users Id's to populate the request.", enableGetUsersVariable))
        {
            GetUsersVariable();
        }

        if (m_MenuTus.AddItem("Get Users Data Status", "Get User Data statuses from a list of user. Use 'Request All Friends' to fetch a list of users Id's to populate the request.", enableGetUsersVariable))
        {
            GetUsersDataStatus();
        }

        if (m_MenuTus.AddItem("Get Friends Data Status", "Get user data statuses of friends."))
        {
            GetFriendsDataStatus();
        }

        if (m_MenuTus.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void SetVariables()
    {
        try
        {
            Sony.NP.Tus.SetVariablesRequest request = new Sony.NP.Tus.SetVariablesRequest();

            Sony.NP.Core.NpAccountId accountId = SonyNpUserProfiles.GetLocalAccountId(User.GetActiveUserId);

            request.TusUser = new Sony.NP.Tus.UserInput(accountId);

            request.UserId = User.GetActiveUserId;

            Sony.NP.Tus.VariableInput[] variables = new Sony.NP.Tus.VariableInput[1];
            variables[0].Value = OnScreenLog.FrameCount;
            variables[0].SlotId = 1;

            request.Vars = variables;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Tus.SetVariables(request, response);
            OnScreenLog.Add("Tus SetVariables Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SetVariablesVirtualUser()
    {
        try
        {
            Sony.NP.Tus.SetVariablesRequest request = new Sony.NP.Tus.SetVariablesRequest();

            Sony.NP.Tus.VirtualUserID id = new Sony.NP.Tus.VirtualUserID();
            id.Name = virtualUserOnlineID;
            request.TusUser = new Sony.NP.Tus.UserInput(id);

            request.UserId = User.GetActiveUserId;

            Sony.NP.Tus.VariableInput[] variables = new Sony.NP.Tus.VariableInput[1];
            variables[0].Value = OnScreenLog.FrameCount;
            variables[0].SlotId = 5; // Slot 5 is configured for the virtual user

            request.Vars = variables;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Tus.SetVariables(request, response);
            OnScreenLog.Add("Tus SetVariables Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetVariables()
    {
        try
        {
            Sony.NP.Tus.GetVariablesRequest request = new Sony.NP.Tus.GetVariablesRequest();

            Sony.NP.Core.NpAccountId accountId = SonyNpUserProfiles.GetLocalAccountId(User.GetActiveUserId);

            request.TusUser = new Sony.NP.Tus.UserInput(accountId);

            request.UserId = User.GetActiveUserId;

            Int32[] slotIds = new Int32[4];
            slotIds[0] = 1;
            slotIds[1] = 2;
            slotIds[2] = 3;
            slotIds[3] = 4;

            request.SlotIds = slotIds;

            Sony.NP.Tus.VariablesResponse response = new Sony.NP.Tus.VariablesResponse();

            int requestId = Sony.NP.Tus.GetVariables(request, response);
            OnScreenLog.Add("Tus GetVariables Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetVariablesVirtualUser()
    {
        try
        {
            Sony.NP.Tus.GetVariablesRequest request = new Sony.NP.Tus.GetVariablesRequest();

            Sony.NP.Tus.VirtualUserID id = new Sony.NP.Tus.VirtualUserID();
            id.Name = virtualUserOnlineID;
            request.TusUser = new Sony.NP.Tus.UserInput(id);

            request.UserId = User.GetActiveUserId;

            Int32[] slotIds = new Int32[1];
            slotIds[0] = 5;

            request.SlotIds = slotIds;

            Sony.NP.Tus.VariablesResponse response = new Sony.NP.Tus.VariablesResponse();

            int requestId = Sony.NP.Tus.GetVariables(request, response);
            OnScreenLog.Add("Tus GetVariables Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void AddToAndGetVariable()
    {
        try
        {
            Sony.NP.Tus.AddToAndGetVariableRequest request = new Sony.NP.Tus.AddToAndGetVariableRequest();

            Sony.NP.Core.NpAccountId accountId = SonyNpUserProfiles.GetLocalAccountId(User.GetActiveUserId);

            request.TusUser = new Sony.NP.Tus.UserInput(accountId);

            request.UserId = User.GetActiveUserId;

            Sony.NP.Tus.VariableInput var = new Sony.NP.Tus.VariableInput();
            var.Value = 100;
            var.SlotId = 1;

            request.Var = var;

            //Sony.NP.Tus.AtomicAddToAndGetVariableResponse response = new Sony.NP.Tus.AtomicAddToAndGetVariableResponse();
            Sony.NP.Tus.VariablesResponse response = new Sony.NP.Tus.VariablesResponse();

            int requestId = Sony.NP.Tus.AddToAndGetVariable(request, response);
            OnScreenLog.Add("Tus AddToAndGetVariable Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public byte[] GenerateRandomGameData(int size)
    {
        Random rand = new Random();

        byte[] gameData = new byte[size];

        rand.NextBytes(gameData);

        return gameData;
    }

    public void OutputData(byte[] data)
    {
        if (data == null) return;

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

    public void SetData()
    {
        try
        {
            Sony.NP.Tus.SetDataRequest request = new Sony.NP.Tus.SetDataRequest();

            Sony.NP.Core.NpAccountId accountId = SonyNpUserProfiles.GetLocalAccountId(User.GetActiveUserId);

            request.TusUser = new Sony.NP.Tus.UserInput(accountId);

            byte[] gameData = GenerateRandomGameData(228);

            OnScreenLog.Add("Random Data to Set:");
            OutputData(gameData);

            request.Data = gameData;

            byte[] supplementaryInfo = GenerateRandomGameData(32);
            OnScreenLog.Add("Supplementary Info to Set:");
            OutputData(supplementaryInfo);

            request.SupplementaryInfo = supplementaryInfo;

            request.SlotId = 1;

            request.UserId = User.GetActiveUserId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Tus.SetData(request, response);
            OnScreenLog.Add("Tus SetData Async : Request Id = " + requestId);
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
            Sony.NP.Tus.GetDataRequest request = new Sony.NP.Tus.GetDataRequest();

            Sony.NP.Core.NpAccountId accountId = SonyNpUserProfiles.GetLocalAccountId(User.GetActiveUserId);

            request.TusUser = new Sony.NP.Tus.UserInput(accountId);
            request.SlotId = 1;

            request.UserId = User.GetActiveUserId;

            Sony.NP.Tus.GetDataResponse response = new Sony.NP.Tus.GetDataResponse();

            int requestId = Sony.NP.Tus.GetData(request, response);
            OnScreenLog.Add("Tus GetData Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void DeleteData()
    {
        try
        {
            Sony.NP.Tus.DeleteDataRequest request = new Sony.NP.Tus.DeleteDataRequest();

            Sony.NP.Core.NpAccountId accountId = SonyNpUserProfiles.GetLocalAccountId(User.GetActiveUserId);

            request.TusUser = new Sony.NP.Tus.UserInput(accountId);

            Int32[] slotIds = new Int32[1];
            slotIds[0] = 1;

            request.SlotIds = slotIds;

            request.UserId = User.GetActiveUserId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Tus.DeleteData(request, response);
            OnScreenLog.Add("Tus DeleteData Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void TryAndSetVariable()
    {
        try
        {
            Sony.NP.Tus.TryAndSetVariableRequest request = new Sony.NP.Tus.TryAndSetVariableRequest();
            request.UserId = User.GetActiveUserId;

            Sony.NP.Core.NpAccountId accountId = SonyNpUserProfiles.GetLocalAccountId(User.GetActiveUserId);
            request.TusUser = new Sony.NP.Tus.UserInput(accountId);

            Sony.NP.Tus.VariableInput varToUpdate = new Sony.NP.Tus.VariableInput();
            varToUpdate.Value = 201;
            varToUpdate.SlotId = 1;

            request.VarToUpdate = varToUpdate;

            request.CompareValue = 201;
            request.CompareOperator = Sony.NP.Tus.TryAndSetCompareOperator.LessThan;

            Sony.NP.Tus.VariablesResponse response = new Sony.NP.Tus.VariablesResponse();

            int requestId = Sony.NP.Tus.TryAndSetVariable(request, response);
            OnScreenLog.Add("Tus TryAndSetVariable Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetFriendsVariable()
    {
        try
        {
            Sony.NP.Tus.GetFriendsVariableRequest request = new Sony.NP.Tus.GetFriendsVariableRequest();
            request.UserId = User.GetActiveUserId;

            request.SortingOrder = Sony.NP.Tus.FriendsVariableSortingOrder.DescDate;
            request.StartIndex = 0;
            request.SlotId = 1;
            request.IncludeMeIfFound = true;

            Sony.NP.Tus.FriendsVariablesResponse response = new Sony.NP.Tus.FriendsVariablesResponse();

            int requestId = Sony.NP.Tus.GetFriendsVariable(request, response);
            OnScreenLog.Add("Tus GetFriendsVariable Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetUsersVariable()
    {
        try
        {
            Sony.NP.Friends.FriendsResponse friendsResponse = SonyNpFriends.latestFriendsResponse;

            int numberAccounts = friendsResponse.Friends.Length;
            numberAccounts = Math.Min(numberAccounts, Sony.NP.Tus.GetUsersVariableRequest.MAX_NUM_USERS);

            Sony.NP.Core.NpAccountId[] acountIds = new Sony.NP.Core.NpAccountId[numberAccounts];

            for (int i = 0; i < numberAccounts; i++)
            {
                acountIds[i] = friendsResponse.Friends[i].Profile.OnlineUser.AccountId;
            }


            Sony.NP.Tus.GetUsersVariableRequest request = new Sony.NP.Tus.GetUsersVariableRequest();
            request.UserId = User.GetActiveUserId;

            request.RealUsersIds = acountIds;

            request.SlotId = 1;

            Sony.NP.Tus.VariablesResponse response = new Sony.NP.Tus.VariablesResponse();

            int requestId = Sony.NP.Tus.GetUsersVariable(request, response);
            OnScreenLog.Add("Tus GetUsersVariable Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetUsersDataStatus()
    {
        try
        {
            Sony.NP.Friends.FriendsResponse friendsResponse = SonyNpFriends.latestFriendsResponse;

            int numberAccounts = friendsResponse.Friends.Length;
            numberAccounts = Math.Min(numberAccounts, Sony.NP.Tus.GetUsersVariableRequest.MAX_NUM_USERS);

            Sony.NP.Core.NpAccountId[] acountIds = new Sony.NP.Core.NpAccountId[numberAccounts];

            for (int i = 0; i < numberAccounts; i++)
            {
                acountIds[i] = friendsResponse.Friends[i].Profile.OnlineUser.AccountId;
            }


            Sony.NP.Tus.GetUsersDataStatusRequest request = new Sony.NP.Tus.GetUsersDataStatusRequest();
            request.UserId = User.GetActiveUserId;

            request.RealUsersIds = acountIds;

            request.SlotId = 1;

            Sony.NP.Tus.DataStatusesResponse response = new Sony.NP.Tus.DataStatusesResponse();

            int requestId = Sony.NP.Tus.GetUsersDataStatus(request, response);
            OnScreenLog.Add("Tus GetUsersDataStatus Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetFriendsDataStatus()
    {
        try
        {
            Sony.NP.Tus.GetFriendsDataStatusRequest request = new Sony.NP.Tus.GetFriendsDataStatusRequest();
            request.UserId = User.GetActiveUserId;

            request.SlotId = 1;

            Sony.NP.Tus.FriendsDataStatusesResponse response = new Sony.NP.Tus.FriendsDataStatusesResponse();

            int requestId = Sony.NP.Tus.GetFriendsDataStatus(request, response);
            OnScreenLog.Add("Tus GetFriendsDataStatus Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.Service == Sony.NP.ServiceTypes.Tus)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.TusSetVariables:
                    OutputTusSetVariables(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.TusGetVariables:
                    OutputTusVariables(callbackEvent.Response as Sony.NP.Tus.VariablesResponse);
                    break;
                case Sony.NP.FunctionTypes.TusAddToAndGetVariable:
                    //OutputTusAddToAndGetVariable(callbackEvent.Response as Sony.NP.Tus.AtomicAddToAndGetVariableResponse);
                    OutputTusVariables(callbackEvent.Response as Sony.NP.Tus.VariablesResponse);
                    break;
                case Sony.NP.FunctionTypes.TusSetData:
                    OutputTusSetData(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.TusGetData:
                    OutputTusGetData(callbackEvent.Response as Sony.NP.Tus.GetDataResponse);
                    break;
                case Sony.NP.FunctionTypes.TusDeleteData:
                    OutputTusDeleteData(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.TusTryAndSetVariable:
                    OutputTusTryAndSetVariable(callbackEvent.Response as Sony.NP.Tus.VariablesResponse);
                    break;
                case Sony.NP.FunctionTypes.TusGetFriendsVariable:
                    OutputTusGetFriendsVariable(callbackEvent.Response as Sony.NP.Tus.FriendsVariablesResponse);
                    break;
                case Sony.NP.FunctionTypes.TusGetUsersVariable:
                    OutputTusGetUsersVariable(callbackEvent.Response as Sony.NP.Tus.VariablesResponse);
                    break;
                case Sony.NP.FunctionTypes.TusGetUsersDataStatus:
                    OutputTusGetUsersDataStatus(callbackEvent.Response as Sony.NP.Tus.DataStatusesResponse);
                    break;
                case Sony.NP.FunctionTypes.TusGetFriendsDataStatus:
                    OutputTusGetFriendsDataStatus(callbackEvent.Response as Sony.NP.Tus.FriendsDataStatusesResponse);
                    break;
                default:
                    break;
            }
        }
    }

    private void OutputTusSetVariables(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Tus SetVariables Empty Response");

        if (response.Locked == false)
        {
        
        }
    }

    private void OutputTusVariables(Sony.NP.Tus.VariablesResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Tus Variables Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("ForCrossSave : " + response.ForCrossSave);

            if (response.ForCrossSave == true )
            {
                if (response.VarsForCrossSave != null)
                {
                    for (int i = 0; i < response.VarsForCrossSave.Length; i++)
                    {
                        OnScreenLog.Add("VarsForCrossSave : ");
                        OnScreenLog.Add("     OwnerId : " + response.VarsForCrossSave[i].OwnerId);
                        OnScreenLog.Add("     HasData : " + response.VarsForCrossSave[i].HasData);
                        OnScreenLog.Add("     LastChangedDate : " + response.VarsForCrossSave[i].LastChangedDate);
                        OnScreenLog.Add("     LastChangedAuthorId : " + response.VarsForCrossSave[i].LastChangedAuthorId);
                        OnScreenLog.Add("     Variable : " + response.VarsForCrossSave[i].Variable);
                        OnScreenLog.Add("     OldVariable : " + response.VarsForCrossSave[i].OldVariable);
                        OnScreenLog.Add("     OwnerAccountId : " + response.VarsForCrossSave[i].OwnerAccountId);
                        OnScreenLog.Add("     LastChangedAuthorAccountId : " + response.VarsForCrossSave[i].LastChangedAuthorAccountId);
                    }
                }
            }
            else
            {
                if (response.Vars != null)
                {
                    for (int i = 0; i < response.Vars.Length; i++)
                    {
                        OnScreenLog.Add("Vars : ");
                        OnScreenLog.Add("     OwnerId : " + response.Vars[i].OwnerId);
                        OnScreenLog.Add("     HasData : " + response.Vars[i].HasData);
                        OnScreenLog.Add("     LastChangedDate : " + response.Vars[i].LastChangedDate);
                        OnScreenLog.Add("     LastChangedAuthorId : " + response.Vars[i].LastChangedAuthorId);
                        OnScreenLog.Add("     Variable : " + response.Vars[i].Variable);
                        OnScreenLog.Add("     OldVariable : " + response.Vars[i].OldVariable);
                        OnScreenLog.Add("     OwnerAccountId : " + response.Vars[i].OwnerAccountId);
                        OnScreenLog.Add("     LastChangedAuthorAccountId : " + response.Vars[i].LastChangedAuthorAccountId);
                    }
                }
            }
            OnScreenLog.AddNewLine();
        }
    }

    private void OutputTusSetData(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Tus SetData Empty Response");

        if (response.Locked == false)
        {

        }
    }

    private void OutputTusGetData(Sony.NP.Tus.GetDataResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Tus GetData Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("Data : ");
            OutputData(response.Data);

            OnScreenLog.Add("ForCrossSave : " + response.ForCrossSave);


            if (response.ForCrossSave == true)
            {
                OnScreenLog.Add("DataStatusForCrossSave : ");
                OnScreenLog.Add("     OwnerAccountId : " + response.DataStatusForCrossSave.OwnerAccountId);
                OnScreenLog.Add("     LastChangedByAccountId : " + response.DataStatusForCrossSave.LastChangedByAccountId);
                OnScreenLog.Add("     OwnerNpId : " + response.DataStatusForCrossSave.OwnerNpId);
                OnScreenLog.Add("     LastChangedByNpId : " + response.DataStatusForCrossSave.LastChangedByNpId);

                OnScreenLog.Add("     HasData : " + response.DataStatusForCrossSave.HasData);
                OnScreenLog.Add("     LastChangedDate : " + response.DataStatusForCrossSave.LastChangedDate);
                OnScreenLog.Add("SupplementaryInfo : ");
                OutputData(response.DataStatusForCrossSave.SupplementaryInfo);
            }
            else
            {
                OnScreenLog.Add("DataStatus : ");

                OnScreenLog.Add("     Owner : " + response.DataStatus.Owner);
                OnScreenLog.Add("     LastChangedBy : " + response.DataStatus.LastChangedBy);

                OnScreenLog.Add("     HasData : " + response.DataStatus.HasData);
                OnScreenLog.Add("     LastChangedDate : " + response.DataStatus.LastChangedDate);

                OnScreenLog.Add("SupplementaryInfo : ");
                OutputData(response.DataStatus.SupplementaryInfo);
            }

            OnScreenLog.AddNewLine();
        }
    }

    private void OutputTusDeleteData(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Tus DeleteData Empty Response");

        if (response.Locked == false)
        {

        }
    }

    private void OutputTusTryAndSetVariable(Sony.NP.Tus.VariablesResponse response)
    {
        OutputTusVariables(response);

        if(response.ReturnCode == Sony.NP.Core.ReturnCodes.SUCCESS)
        {
            // This should always return a variable even if the comparison failed. 
            // Check the 
            if ( response.Vars != null && response.Vars.Length > 0 )
            {
                Sony.NP.Tus.NpVariable var = response.Vars[0];

                if (var.OldVariable != var.Variable)
                {
                    OnScreenLog.Add("Variable was changed");
                }
                else
                {
                    OnScreenLog.Add("Variable wasn't changed so the compare condition failed");
                }
            }
            else
            {
                OnScreenLog.AddError("Tus TryAndSetVariable didn't return any variables");
            }
        }
    }

    private void OutputTusGetFriendsVariable(Sony.NP.Tus.FriendsVariablesResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Tus Friends Variables Response");

        if ( response.ReturnCode == Sony.NP.Core.ReturnCodes.NP_COMMUNITY_SERVER_ERROR_FORBIDDEN )
        {
            OnScreenLog.AddError("NP_COMMUNITY_SERVER_ERROR_FORBIDDEN : This happens if the slot doesn't exist or the slot doesn't have the correct permisions.");
            OnScreenLog.AddError("Use the Title User Storage Tool on DevNet to configure the slot. \"PS4 OthersPermission\" must be set to \"3 : Allow read/write\" for the slot to work.");        
        }

        if (response.Locked == false)
        {
            OnScreenLog.Add("TotalFriends : " + response.TotalFriends);
            OnScreenLog.Add("ForCrossSave : " + response.ForCrossSave);

            if (response.ForCrossSave == true)
            {
                if (response.VarsForCrossSave != null)
                {
                    for (int i = 0; i < response.VarsForCrossSave.Length; i++)
                    {
                        OnScreenLog.Add("VarsForCrossSave : ");
                        OnScreenLog.Add("     OwnerId : " + response.VarsForCrossSave[i].OwnerId);
                        OnScreenLog.Add("     HasData : " + response.VarsForCrossSave[i].HasData);
                        OnScreenLog.Add("     LastChangedDate : " + response.VarsForCrossSave[i].LastChangedDate);
                        OnScreenLog.Add("     LastChangedAuthorId : " + response.VarsForCrossSave[i].LastChangedAuthorId);
                        OnScreenLog.Add("     Variable : " + response.VarsForCrossSave[i].Variable);
                        OnScreenLog.Add("     OldVariable : " + response.VarsForCrossSave[i].OldVariable);
                        OnScreenLog.Add("     OwnerAccountId : " + response.VarsForCrossSave[i].OwnerAccountId);
                        OnScreenLog.Add("     LastChangedAuthorAccountId : " + response.VarsForCrossSave[i].LastChangedAuthorAccountId);
                    }
                }
            }
            else
            {
                if (response.Vars != null)
                {
                    for (int i = 0; i < response.Vars.Length; i++)
                    {
                        OnScreenLog.Add("Vars : ");
                        OnScreenLog.Add("     OwnerId : " + response.Vars[i].OwnerId);
                        OnScreenLog.Add("     HasData : " + response.Vars[i].HasData);
                        OnScreenLog.Add("     LastChangedDate : " + response.Vars[i].LastChangedDate);
                        OnScreenLog.Add("     LastChangedAuthorId : " + response.Vars[i].LastChangedAuthorId);
                        OnScreenLog.Add("     Variable : " + response.Vars[i].Variable);
                        OnScreenLog.Add("     OldVariable : " + response.Vars[i].OldVariable);
                        OnScreenLog.Add("     OwnerAccountId : " + response.Vars[i].OwnerAccountId);
                        OnScreenLog.Add("     LastChangedAuthorAccountId : " + response.Vars[i].LastChangedAuthorAccountId);
                    }
                }
            }
            OnScreenLog.AddNewLine();
        }
    }

    private void OutputTusGetUsersVariable(Sony.NP.Tus.VariablesResponse response)
    {
        OutputTusVariables(response);
    }

    private void OutputDataStatusForCrossSave(Sony.NP.Tus.TusDataStatusForCrossSave dataStatus)
    {
        OnScreenLog.Add("StatusesForCrossSave : ");
        OnScreenLog.Add("     OwnerAccountId : " + dataStatus.OwnerAccountId);
        OnScreenLog.Add("     LastChangedByAccountId : " + dataStatus.LastChangedByAccountId);
        OnScreenLog.Add("     OwnerNpId : " + dataStatus.OwnerNpId);
        OnScreenLog.Add("     LastChangedByNpId : " + dataStatus.LastChangedByNpId);

        OnScreenLog.Add("     HasData : " + dataStatus.HasData);
        OnScreenLog.Add("     LastChangedDate : " + dataStatus.LastChangedDate);
        OnScreenLog.Add("SupplementaryInfo : ");
        OutputData(dataStatus.SupplementaryInfo);
    }

    private void OutputDataStatus(Sony.NP.Tus.TusDataStatus dataStatus)
    {
        OnScreenLog.Add("DataStatus : ");

        OnScreenLog.Add("     Owner : " + dataStatus.Owner);
        OnScreenLog.Add("     LastChangedBy : " + dataStatus.LastChangedBy);

        OnScreenLog.Add("     HasData : " + dataStatus.HasData);
        OnScreenLog.Add("     LastChangedDate : " + dataStatus.LastChangedDate);

        OnScreenLog.Add("SupplementaryInfo : ");
        OutputData(dataStatus.SupplementaryInfo);
    }

    private void OutputTusGetUsersDataStatus(Sony.NP.Tus.DataStatusesResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Tus DataStatuses Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("ForCrossSave : " + response.ForCrossSave);

            if (response.ForCrossSave == true)
            {
                Sony.NP.Tus.TusDataStatusForCrossSave[] dataStatuses = response.StatusesForCrossSave;

                OnScreenLog.Add("Num Statuses : " + dataStatuses.Length);

                for (int i = 0; i < dataStatuses.Length; i++)
                {
                    OutputDataStatusForCrossSave(dataStatuses[i]);
                }
            }
            else
            {
                Sony.NP.Tus.TusDataStatus[] dataStatuses = response.Statuses;

                OnScreenLog.Add("Num Statuses : " + dataStatuses.Length);

                for (int i = 0; i < dataStatuses.Length; i++)
                {
                    OutputDataStatus(dataStatuses[i]);
                }
            }
            OnScreenLog.AddNewLine();
        }
    }

    private void OutputTusGetFriendsDataStatus(Sony.NP.Tus.FriendsDataStatusesResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Tus FriendsDataStatuses Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("TotalFriends : " + response.TotalFriends);

            OnScreenLog.Add("ForCrossSave : " + response.ForCrossSave);

            if (response.ForCrossSave == true)
            {
                Sony.NP.Tus.TusDataStatusForCrossSave[] dataStatuses = response.StatusesForCrossSave;

                OnScreenLog.Add("Num Statuses : " + dataStatuses.Length);

                for (int i = 0; i < dataStatuses.Length; i++)
                {
                    OutputDataStatusForCrossSave(dataStatuses[i]);
                }
            }
            else
            {
                Sony.NP.Tus.TusDataStatus[] dataStatuses = response.Statuses;

                OnScreenLog.Add("Num Statuses : " + dataStatuses.Length);

                for (int i = 0; i < dataStatuses.Length; i++)
                {
                    OutputDataStatus(dataStatuses[i]);
                }
            }
            OnScreenLog.AddNewLine();
        }
    }

}
#endif
