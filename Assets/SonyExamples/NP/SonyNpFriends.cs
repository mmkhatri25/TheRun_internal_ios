using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_PS4
public class SonyNpFriends : IScreen
{
	MenuLayout m_MenuFriends;

    // Keep the last friend response results so other menu items can use it.
    static public Sony.NP.Friends.FriendsResponse latestFriendsResponse;
    int friendResponseUserId = 0; // Which use fetched their friends list.

    public SonyNpFriends()
	{
        Initialize();
	}
    
	public MenuLayout GetMenu()
	{
		return m_MenuFriends;
	}

	public void OnEnter()
	{
	}

	public void OnExit()
	{
	}

	public void Process(MenuStack stack)
	{
		MenuFriends(stack);
	}

	public void Initialize()
	{
	    m_MenuFriends = new MenuLayout(this, 450, 20);
	}

    public void MenuFriends(MenuStack menuStack)
	{
        m_MenuFriends.Update();

        if (m_MenuFriends.AddItem("Request All Friends", "Get all friends for the current user."))
        {
            RequestAllFriends();
        }

        if (m_MenuFriends.AddItem("Request All Friends (Synchronous)", "Get all friends for the current user using a synchronous method."))
        {
            RequestAllFriendsSynchronous();
        }

        // Friends of Friends
        // This can only be called if a list of friends has already been recieved as that is used to
        // populate the friends of friends request.
        bool canFetchFriendsOfFriends = false;
        if (latestFriendsResponse != null && latestFriendsResponse.Friends != null && latestFriendsResponse.Friends.Length > 0)
        {
            // Must be this use that fetched their friends list.
            if (User.GetActiveUserId == friendResponseUserId)
            {
                canFetchFriendsOfFriends = true;
            }
        }

        if (m_MenuFriends.AddItem("Request Friends Of Friends", "Get friends of friends. Must have used 'Request All Friends' above before this can be used.", canFetchFriendsOfFriends))
        {
            RequestFriendsOfFriends();
        }

        if (m_MenuFriends.AddItem("Request Blocked Users", "Get the list of friends blocked by the current user"))
        {
            RequestBlockedUsers();
        }

        if (m_MenuFriends.AddItem("Display Friend Request Dialog", "Open a 'Friend Request' dialog. Uses a fixed account id."))
        {
            DisplayFriendRequestDialog();
        }

        if (m_MenuFriends.AddItem("Display Block User Dialog", "Open a 'Block User' dialog. Uses a fixed account id."))
        {
            DisplayBlockUserDialog();
        }

        if (m_MenuFriends.AddBackIndex("Back"))
		{
			menuStack.PopMenu();
		}
	}

    public void RequestAllFriends()
    {
        try
        {
            Sony.NP.Friends.GetFriendsRequest request = new Sony.NP.Friends.GetFriendsRequest();
            request.Mode = Sony.NP.Friends.FriendsRetrievalModes.all;
            request.Limit = 0;
            request.Offset = 0;
            request.Async = true;
            request.UserId = User.GetActiveUserId;

            Sony.NP.Friends.FriendsResponse response = new Sony.NP.Friends.FriendsResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Friends.GetFriends(request, response);
            OnScreenLog.Add("RequestAllFriends Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void RequestAllFriendsSynchronous()
    {
        try
        {
            Sony.NP.Friends.GetFriendsRequest request = new Sony.NP.Friends.GetFriendsRequest();
            request.Mode = Sony.NP.Friends.FriendsRetrievalModes.all;
            request.Limit = 0;
            request.Offset = 0;
            request.Async = false;
            request.UserId = User.GetActiveUserId;

            Sony.NP.Friends.FriendsResponse response = new Sony.NP.Friends.FriendsResponse();

            OnScreenLog.Add("RequestAllFriends Synchronous");
            Sony.NP.Friends.GetFriends(request, response);
            OutputFriends(response);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void RequestFriendsOfFriends()
    {
        try
        {
            Sony.NP.Friends.GetFriendsOfFriendsRequest request = new Sony.NP.Friends.GetFriendsOfFriendsRequest();
            request.UserId = User.GetActiveUserId;

            int numberAccounts = latestFriendsResponse.Friends.Length;

            numberAccounts = Math.Min(numberAccounts, Sony.NP.Friends.GetFriendsOfFriendsRequest.MAX_ACCOUNT_IDS);

            Sony.NP.Core.NpAccountId[] acountIds = new Sony.NP.Core.NpAccountId[numberAccounts];

            for (int i = 0; i < numberAccounts; i++)
            {
                acountIds[i] = latestFriendsResponse.Friends[i].Profile.OnlineUser.AccountId;
            }

            request.AccountIds = acountIds;

            request.Async = true;

            Sony.NP.Friends.FriendsOfFriendsResponse response = new Sony.NP.Friends.FriendsOfFriendsResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Friends.GetFriendsOfFriends(request, response);
            OnScreenLog.Add("RequestFriendsOfFriends Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void RequestBlockedUsers()
    {
        try
        {
            Sony.NP.Friends.GetBlockedUsersRquest request = new Sony.NP.Friends.GetBlockedUsersRquest();
            request.Mode = Sony.NP.Friends.BlockedUsersRetrievalMode.all;
            request.Limit = 0;
            request.Offset = 0;
            request.Async = true;
            request.UserId = User.GetActiveUserId;

            Sony.NP.Friends.BlockedUsersResponse response = new Sony.NP.Friends.BlockedUsersResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Friends.GetBlockedUsers(request, response);
            OnScreenLog.Add("RequestBlockedUsers Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    static UInt64 SPINT_ACCOUNT_ID = 1037116572376272627; // Taken from Sony NpToolkit sample - Stevehd

    public void DisplayFriendRequestDialog()
    {
        try
        {
            Sony.NP.Friends.DisplayFriendRequestDialogRequest request = new Sony.NP.Friends.DisplayFriendRequestDialogRequest();
            request.TargetUser = SPINT_ACCOUNT_ID;
            request.UserId = User.GetActiveUserId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Friends.DisplayFriendRequestDialog(request, response);
            OnScreenLog.Add("DisplayFriendRequestDialogRequest Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void DisplayBlockUserDialog()
    {
        try
        {
            Sony.NP.Friends.DisplayBlockUserDialogRequest request = new Sony.NP.Friends.DisplayBlockUserDialogRequest();
            request.TargetUser = SPINT_ACCOUNT_ID;
            request.UserId = User.GetActiveUserId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Friends.DisplayBlockUserDialog(request, response);
            OnScreenLog.Add("DisplayBlockUserDialog Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.Service == Sony.NP.ServiceTypes.Friends)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.FriendsGetFriends:
                    latestFriendsResponse = callbackEvent.Response as Sony.NP.Friends.FriendsResponse;
                    friendResponseUserId = callbackEvent.UserId.Id;
                    OutputFriends(latestFriendsResponse);
                    break;
                case Sony.NP.FunctionTypes.FriendsGetFriendsOfFriends:
                    OutputFriendsOfFriends(callbackEvent.Response as Sony.NP.Friends.FriendsOfFriendsResponse);
                    break;
                case Sony.NP.FunctionTypes.FriendsGetBlockedUsers:
                    OutputBlockedUsers(callbackEvent.Response as Sony.NP.Friends.BlockedUsersResponse);
                    break;
                case Sony.NP.FunctionTypes.FriendsDisplayFriendRequestDialog:
                    //Sony.NP.Core.ReturnCodes.DialogResultOK
                    //Sony.NP.Core.ReturnCodes.DialogResultUserCanceled
                    //Sony.NP.Core.ReturnCodes.DialogResultAborted
                    break;
                case Sony.NP.FunctionTypes.FriendsDisplayBlockUserDialog:
                    //Sony.NP.Core.ReturnCodes.DialogResultOK
                    //Sony.NP.Core.ReturnCodes.DialogResultUserCanceled
                    //Sony.NP.Core.ReturnCodes.DialogResultAborted
                    break;           
                default:
                    break;
            }
        }
    }

    private void OutputFriends(Sony.NP.Friends.FriendsResponse friendsResponse)
    {
        if (friendsResponse == null) return;

        OnScreenLog.Add("Friends Response");

        if (friendsResponse.Locked == false)
        {
            for (int i = 0; i < friendsResponse.Friends.Length; i++)
            {
                Sony.NP.Friends.Friend friend = friendsResponse.Friends[i];

                if (friend != null)
                {
                    string output = friend.ToString();
                    OnScreenLog.Add(output);
                }
            }
        }
    }

    private void OutputFriendsOfFriends(Sony.NP.Friends.FriendsOfFriendsResponse friendsOfFriendsResponse)
    {
        if (friendsOfFriendsResponse == null) return;

        OnScreenLog.Add("Friends Of Friends Response");

        if (friendsOfFriendsResponse.Locked == false)
        {
            for (int i = 0; i < friendsOfFriendsResponse.FriendsOfFriends.Length; i++)
            {
                Sony.NP.Friends.FriendsOfFriend friendsOfFriend = friendsOfFriendsResponse.FriendsOfFriends[i];

                string output = string.Format("0x{0:X} : {1} : Number Friends ({2})\n", friendsOfFriend.OriginalFriend.AccountId, friendsOfFriend.OriginalFriend.OnlineID.Name, friendsOfFriend.Users.Length );

                OnScreenLog.Add(output);

                for (int u = 0; u < friendsOfFriend.Users.Length; u++)
                {
                    output = string.Format("       0x{0:X} : {1}\n", friendsOfFriend.Users[u].AccountId, friendsOfFriend.Users[u].OnlineID.Name);
                    OnScreenLog.Add(output);
                }
            }
        }
    }

    private void OutputBlockedUsers(Sony.NP.Friends.BlockedUsersResponse blockedUsersResponse)
    {
        if (blockedUsersResponse == null) return;

        OnScreenLog.Add("Blocked Users Response");

        if (blockedUsersResponse.Locked == false)
        {
            if (blockedUsersResponse.Users != null && blockedUsersResponse.Users.Length > 0)
            {
                for (int i = 0; i < blockedUsersResponse.Users.Length; i++)
                {
                    OnScreenLog.Add("Blocked User = : " + blockedUsersResponse.Users[i].ToString());
                }
            }
            else
            {
                OnScreenLog.Add("No blocked users returned");
            }
        }
    }

    private void OutputUpdateFriendsList(Sony.NP.Friends.FriendListUpdateResponse friendListUpdateResponse)
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
    }

}
#endif
