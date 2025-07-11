using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS4
public class SonyNpTrophies : IScreen
{
    MenuLayout m_MenuTrophies;
    int nextTrophyId = 1;

    public SonyNpTrophies()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuTrophies;
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
        m_MenuTrophies = new MenuLayout(this, 450, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuTrophies.Update();

        if (m_MenuTrophies.AddItem("Register Trophy Pack", "Register the trophy pack for the current user."))
        {
            RegisterTrophyPack();
        }

        User currentUser = User.GetActiveUser;

        bool enabled = currentUser.trophyPackRegistered;

        if (m_MenuTrophies.AddItem("Set Screenshot", "Sets a screen shot for the first 4 trophies for the current user.", enabled))
        {
            SetScreenshot();
        }

        if (m_MenuTrophies.AddItem("Unlock Trophy (" + nextTrophyId + ")", "Unlock a locked trophy for the current user. Can only use this is the trophy pack has been registered using 'Register Trophy Pack'.", enabled))
        {
            UnlockTrophy();
        }

        if (m_MenuTrophies.AddItem("Get Unlocked Trophies", "Get the list of unlocked trophies for the current user. Can only use this is the trophy pack has been registered using 'Register Trophy Pack'.", enabled))
        {
            GetUnlockedTrophies();
        }

        if (m_MenuTrophies.AddItem("Display Trophy Pack Dialog", "Display the trophy pack dialog for the current user.", enabled))
        {
            DisplayTrophyPackDialog();
        }

        if (m_MenuTrophies.AddItem("Get Trophy Pack Summary", "Get the summary for a trophy pack. Can only use this is the trophy pack has been registered using 'Register Trophy Pack'.", enabled))
        {
            GetTrophyPackSummary();
        }

        if (m_MenuTrophies.AddItem("Get Trophy Pack Group", "Get the trophy pack group. Can only use this is the trophy pack has been registered using 'Register Trophy Pack'.", enabled))
        {
            GetTrophyPackGroup();
        }

        if (m_MenuTrophies.AddItem("Get Trophy Pack Trophy", "Get info for a trophy from a trophy pack. Can only use this is the trophy pack has been registered using 'Register Trophy Pack'.", enabled))
        {
            GetTrophyPackTrophy();
        }

        if (m_MenuTrophies.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void RegisterTrophyPack()
    {
        try
        {
            Sony.NP.Trophies.RegisterTrophyPackRequest request = new Sony.NP.Trophies.RegisterTrophyPackRequest();
            request.UserId = User.GetActiveUserId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Trophies.RegisterTrophyPack(request, response);
            OnScreenLog.Add("RegisterTrophyPack Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SetScreenshot()
    {
        try
        {
            Sony.NP.Trophies.SetScreenshotRequest request = new Sony.NP.Trophies.SetScreenshotRequest();
            request.AssignToAllUsers = true;
            request.UserId = User.GetActiveUserId;

            int[] ids = new int[4];

            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = i + 1;  // Set throphy ids from 1 to 4.
            }

            request.TrophiesIds = ids;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Trophies.SetScreenshot(request, response);
            OnScreenLog.Add("SetScreenshot Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void UnlockTrophy()
    {
        try
        {
            Sony.NP.Trophies.UnlockTrophyRequest request = new Sony.NP.Trophies.UnlockTrophyRequest();
            request.TrophyId = nextTrophyId;
            request.UserId = User.GetActiveUserId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            nextTrophyId++;

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Trophies.UnlockTrophy(request, response);
            OnScreenLog.Add("GetUnlockedTrophies Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetUnlockedTrophies()
    {
        try
        {
            Sony.NP.Trophies.GetUnlockedTrophiesRequest request = new Sony.NP.Trophies.GetUnlockedTrophiesRequest();
            request.UserId = User.GetActiveUserId;

            Sony.NP.Trophies.UnlockedTrophiesResponse response = new Sony.NP.Trophies.UnlockedTrophiesResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Trophies.GetUnlockedTrophies(request, response);
            OnScreenLog.Add("GetUnlockedTrophies Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void DisplayTrophyPackDialog()
    {
        try
        {
            Sony.NP.Trophies.DisplayTrophyListDialogRequest request = new Sony.NP.Trophies.DisplayTrophyListDialogRequest();
            request.UserId = User.GetActiveUserId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Trophies.DisplayTrophyListDialog(request, response);
            OnScreenLog.Add("DisplayTrophyPackDialog Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetTrophyPackSummary()
    {
        try
        {
            Sony.NP.Trophies.GetTrophyPackSummaryRequest request = new Sony.NP.Trophies.GetTrophyPackSummaryRequest();
            request.RetrieveTrophyPackSummaryIcon = true;
            request.UserId = User.GetActiveUserId;

            Sony.NP.Trophies.TrophyPackSummaryResponse response = new Sony.NP.Trophies.TrophyPackSummaryResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Trophies.GetTrophyPackSummary(request, response);
            OnScreenLog.Add("GetTrophyPackSummary Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetTrophyPackGroup()
    {
        try
        {
            Sony.NP.Trophies.GetTrophyPackGroupRequest request = new Sony.NP.Trophies.GetTrophyPackGroupRequest();
            request.GroupId = -1;
            request.UserId = User.GetActiveUserId;

            Sony.NP.Trophies.TrophyPackGroupResponse response = new Sony.NP.Trophies.TrophyPackGroupResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Trophies.GetTrophyPackGroup(request, response);
            OnScreenLog.Add("GetTrophyPackGroup Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetTrophyPackTrophy()
    {
        try
        {
            Sony.NP.Trophies.GetTrophyPackTrophyRequest request = new Sony.NP.Trophies.GetTrophyPackTrophyRequest();
            request.TrophyId = 1;
            request.RetrieveTrophyPackTrophyIcon = true;
            request.UserId = User.GetActiveUserId;

            Sony.NP.Trophies.TrophyPackTrophyResponse response = new Sony.NP.Trophies.TrophyPackTrophyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Trophies.GetTrophyPackTrophy(request, response);
            OnScreenLog.Add("GetTrophyPackTrophy Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.Service == Sony.NP.ServiceTypes.Trophy)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.TrophyRegisterTrophyPack:
                    {
                        User user = User.FindUser(callbackEvent.UserId);

                        if ( user != null)
                        {
                            user.trophyPackRegistered = true;
                        }

                        OutputRegisterTrophyPack(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    }
                    break;
                case Sony.NP.FunctionTypes.TrophySetScreenshot:
                    OutputSetScreenshot(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.TrophyUnlock:
                    OutputTrophyUnlock(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.TrophyGetUnlockedTrophies:
                    OutputUnlockedTrophies(callbackEvent.Response as Sony.NP.Trophies.UnlockedTrophiesResponse);
                    break;
                case Sony.NP.FunctionTypes.TrophyDisplayTrophyListDialog:
                    OutputDisplayTrophyListDialog(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.TrophyGetTrophyPackSummary:
                    OutputGetTrophyPackSummary(callbackEvent.Response as Sony.NP.Trophies.TrophyPackSummaryResponse);
                    break;
                case Sony.NP.FunctionTypes.TrophyGetTrophyPackGroup:
                    OutputGetTrophyPackGroup(callbackEvent.Response as Sony.NP.Trophies.TrophyPackGroupResponse);
                    break;
                case Sony.NP.FunctionTypes.TrophyGetTrophyPackTrophy:
                    OutputGetTrophyPackTrophy(callbackEvent.Response as Sony.NP.Trophies.TrophyPackTrophyResponse);
                    break;
                default:
                    break;
            }
        }
    }

    private void OutputRegisterTrophyPack(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("RegisterTrophyPack Empty Response");

        if (response.Locked == false)
        {
        }
    }

    private void OutputSetScreenshot(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("SetScreenshot Empty Response");

        if (response.Locked == false)
        {

        }
    }

    private void OutputTrophyUnlock(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("TrophyUnlock Empty Response");

        if (response.Locked == false)
        {

        }
    }

    private void OutputUnlockedTrophies(Sony.NP.Trophies.UnlockedTrophiesResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("GetUnlockedTrophies Response");

        if (response.Locked == false)
        {
            if (response.TrophyIds != null)
            {
                OnScreenLog.Add("Number Unlocked Trophys = " + response.TrophyIds.Length);
                for(int i = 0; i < response.TrophyIds.Length; i++)
                {
                    OnScreenLog.Add("   : " + response.TrophyIds[i]);
                }
            }
        }
    }

    private void OutputDisplayTrophyListDialog(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("DisplayTrophyListDialog Empty Response");

        if (response.Locked == false)
        {

        }
    }

    private void OutputGetTrophyPackSummary(Sony.NP.Trophies.TrophyPackSummaryResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("TrophyPackSummaryResponse Response");

        if (response.Locked == false)
        {
            SonyNpMain.SetIconTexture(response.Icon);

            OnScreenLog.Add("Static Configuration");

            OnScreenLog.Add("   # Groups : " + response.StaticConfiguration.NumGroups);
            OnScreenLog.Add("   # Trophies : " + response.StaticConfiguration.NumTrophies);
            OnScreenLog.Add("   # Platinum : " + response.StaticConfiguration.NumPlatinum);
            OnScreenLog.Add("   # Gold : " + response.StaticConfiguration.NumGold);
            OnScreenLog.Add("   # Silver : " + response.StaticConfiguration.NumSilver);
            OnScreenLog.Add("   # Bronze : " + response.StaticConfiguration.NumBronze);
            OnScreenLog.Add("   Title : " + response.StaticConfiguration.Title);
            OnScreenLog.Add("   Description : " + response.StaticConfiguration.Description);

            OnScreenLog.Add("User Progress");

            OnScreenLog.Add("   Unlocked Trophies : " + response.UserProgress.UnlockedTrophies);
            OnScreenLog.Add("   Unlocked Platinum : " + response.UserProgress.UnlockedPlatinum);
            OnScreenLog.Add("   Unlocked Gold : " + response.UserProgress.UnlockedGold);
            OnScreenLog.Add("   Unlocked Silver : " + response.UserProgress.UnlockedSilver);
            OnScreenLog.Add("   Unlocked Bronze : " + response.UserProgress.UnlockedBronze);
            OnScreenLog.Add("   Progress % : " + response.UserProgress.ProgressPercentage);
        }
    }

    private void OutputGetTrophyPackGroup(Sony.NP.Trophies.TrophyPackGroupResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("TrophyPackGroupResponse Response");

        if (response.Locked == false)
        {
            SonyNpMain.SetIconTexture(response.Icon);

            OnScreenLog.Add("Static Configuration");

            OnScreenLog.Add("   Group Id : " + response.StaticConfiguration.GroupId);
            OnScreenLog.Add("   # Trophies : " + response.StaticConfiguration.NumTrophies);
            OnScreenLog.Add("   # Platinum : " + response.StaticConfiguration.NumPlatinum);
            OnScreenLog.Add("   # Gold : " + response.StaticConfiguration.NumGold);
            OnScreenLog.Add("   # Silver : " + response.StaticConfiguration.NumSilver);
            OnScreenLog.Add("   # Bronze : " + response.StaticConfiguration.NumBronze);
            OnScreenLog.Add("   Title : " + response.StaticConfiguration.Title);
            OnScreenLog.Add("   Description : " + response.StaticConfiguration.Description);

            OnScreenLog.Add("User Progress");

            OnScreenLog.Add("   Group Id : " + response.UserProgress.GroupId);
            OnScreenLog.Add("   Unlocked Trophies : " + response.UserProgress.UnlockedTrophies);
            OnScreenLog.Add("   Unlocked Platinum : " + response.UserProgress.UnlockedPlatinum);
            OnScreenLog.Add("   Unlocked Gold : " + response.UserProgress.UnlockedGold);
            OnScreenLog.Add("   Unlocked Silver : " + response.UserProgress.UnlockedSilver);
            OnScreenLog.Add("   Unlocked Bronze : " + response.UserProgress.UnlockedBronze);
            OnScreenLog.Add("   Progress % : " + response.UserProgress.ProgressPercentage);
        }
    }

    private void OutputGetTrophyPackTrophy(Sony.NP.Trophies.TrophyPackTrophyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("TrophyPackTrophyResponse Response");

        if (response.Locked == false)
        {
            SonyNpMain.SetIconTexture(response.Icon);

            OnScreenLog.Add("Static Configuration");

            OnScreenLog.Add("   Trophy Id : " + response.StaticConfiguration.TrophyId);
            OnScreenLog.Add("   Trophy Grade : " + response.StaticConfiguration.TrophyGrade);
            OnScreenLog.Add("   Group Id : " + response.StaticConfiguration.GroupId);
            OnScreenLog.Add("   Hidden : " + response.StaticConfiguration.Hidden);
            OnScreenLog.Add("   Name : " + response.StaticConfiguration.Name);
            OnScreenLog.Add("   Description : " + response.StaticConfiguration.Description);

            OnScreenLog.Add("User Progress");

            OnScreenLog.Add("   Trophy Id : " + response.UserProgress.TrophyId);
            OnScreenLog.Add("   Unlocked : " + response.UserProgress.Unlocked);
            OnScreenLog.Add("   Date Stamp : " + response.UserProgress.Timestamp.ToString());
        }
    }
}
#endif
