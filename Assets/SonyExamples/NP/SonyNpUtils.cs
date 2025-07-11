using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS4
public class SonyNpUtils : IScreen
{
    MenuLayout m_MenuUtils;

    public SonyNpUtils()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuUtils;
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
        m_MenuUtils = new MenuLayout(this, 450, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuUtils.Update();

        // This need proper testing and data.
        //if (m_MenuUtils.AddItem("Set TitleId For Development", "Set the title Id during developement when a titleid.dat file is not available."))
        //{
        //    SetTitleIdForDevelopment();
        //}

        if (m_MenuUtils.AddItem("Display Signin Dialog", "Display the signin dialog for the current user."))
        {
            DisplaySigninDialog();
        }

        if (m_MenuUtils.AddItem("Check Availablity", "Check the PSN availablity for the current user."))
        {
            CheckAvailablity();
        }

        if (m_MenuUtils.AddItem("Check Plus", "Check if the current user has a Plus account."))
        {
            CheckPlus(true);
        }

        if (m_MenuUtils.AddItem("Check Plus (Synchronous)", "Check if the current user has a Plus account using a synchronous method."))
        {
            CheckPlus(false);
        }

        if (m_MenuUtils.AddItem("Get Parental Control Info", "Return the current users age and parental control settings."))
        {
            GetParentalControlInfo();
        }

        if (m_MenuUtils.AddItem("Notify Plus Feature", "Check if the current user has a Plus account using a synchronous method."))
        {
            NotifyPlusFeature();
        }

        if (m_MenuUtils.AddItem("Show Memory Stats", "Display memory pool usage from the GetMemoryPoolStats method."))
        {
            ShowMemoryStats();
        }

        if (m_MenuUtils.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void SetTitleIdForDevelopment()
    {
        try
        {
            Sony.NP.NpUtils.SetTitleIdForDevelopmentRequest request = new Sony.NP.NpUtils.SetTitleIdForDevelopmentRequest();

            request.titleId = "Test";
            request.titleSecretString = "abcdefghijklmnop";
            request.titleSecretStringSize = (UInt32)request.titleSecretString.Length;

            request.UserId = User.GetActiveUserId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.NpUtils.SetTitleIdForDevelopment(request, response);
            OnScreenLog.Add("SetTitleIdForDevelopment Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void DisplaySigninDialog()
    {
        try
        {
            Sony.NP.NpUtils.DisplaySigninDialogRequest request = new Sony.NP.NpUtils.DisplaySigninDialogRequest();
            request.UserId = User.GetActiveUserId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.NpUtils.DisplaySigninDialog(request, response);
            OnScreenLog.Add("DisplaySigninDialog Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void CheckAvailablity()
    {
        try
        {
            Sony.NP.NpUtils.CheckAvailablityRequest request = new Sony.NP.NpUtils.CheckAvailablityRequest();
            request.UserId = User.GetActiveUserId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.NpUtils.CheckAvailablity(request, response);
            OnScreenLog.Add("CheckAvailablity Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void CheckPlus(bool async)
    {
        try
        {
            Sony.NP.NpUtils.CheckPlusRequest request = new Sony.NP.NpUtils.CheckPlusRequest();
            request.UserId = User.GetActiveUserId;
            request.Async = async;

            Sony.NP.NpUtils.CheckPlusResponse response = new Sony.NP.NpUtils.CheckPlusResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.NpUtils.CheckPlus(request, response);

            if (async == true)
            {
                OnScreenLog.Add("CheckPlus Async : Request Id = " + (UInt32)requestId);
            }
            else
            {
                // For synchronous the requestId is actually a return code.
                OnScreenLog.Add("CheckPlus Synchronous : Return code = (0x" + requestId.ToString("X8") + ")");

                if (response.ReturnCodeValue < 0)
                {
                    OnScreenLog.AddError("Response : " + response.ConvertReturnCodeToString(request.FunctionType));
                }
                else
                {
                    OnScreenLog.Add("Response : " + response.ConvertReturnCodeToString(request.FunctionType));
                }

                OutputCheckPlus(response);
            }
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetParentalControlInfo()
    {
        try
        {
            Sony.NP.NpUtils.GetParentalControlInfoRequest request = new Sony.NP.NpUtils.GetParentalControlInfoRequest();
            request.UserId = User.GetActiveUserId;
            request.Async = true;

            Sony.NP.NpUtils.GetParentalControlInfoResponse response = new Sony.NP.NpUtils.GetParentalControlInfoResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.NpUtils.GetParentalControlInfo(request, response);

            OnScreenLog.Add("GetParentalControlInfo Async : Request Id = " + (UInt32)requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void NotifyPlusFeature()
    {
        try
        {
            OnScreenLog.Add("Calling NotifyPlusFeature...");

            Sony.NP.NpUtils.NotifyPlusFeature(User.GetActiveUserId);

            OnScreenLog.Add("NotifyPlusFeature called successfully.");
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    private void ShowMemoryStats()
    {
        var stats = new Sony.NP.MemoryPoolStats();
        int err = Sony.NP.Main.GetMemoryPoolStats(out stats);
        UnityEngine.Debug.Assert(err == 0);

        OnScreenLog.Add("Memory Pool Statistics:");
        var http = stats.httpPoolStats;
        OnScreenLog.Add(string.Format("stats.httpPoolStats: current {0} max {1} total {2}.", http.currentInUseSize, http.maxInUseSize, http.poolSize));
        var ssl = stats.sslPoolStats;
        OnScreenLog.Add(string.Format("stats.sslPoolStats: current {0} max {1} total {2}.", ssl.currentInUseSize, ssl.maxInUseSize, ssl.poolSize));
        var webStats = stats.webApiPoolStats;
        OnScreenLog.Add(string.Format("stats.webApiPoolStats: current {0} max {1} total {2}.", webStats.currentInUseSize, webStats.maxInUseSize, webStats.poolSize));
        var net = stats.netPoolStats;
        OnScreenLog.Add(string.Format("stats.netPoolStats: current {0} max {1} total {2}.", net.currentInUseSize, net.maxInUseSize, net.poolSize));
        var npt = stats.npToolkitPoolStats;
        OnScreenLog.Add(string.Format("stats.npToolkitPoolStats: current {0} max {1} total {2}.", npt.currentInUseSize, npt.maxInUseSize, npt.poolSize));
        var json = stats.jsonPoolStats;
        OnScreenLog.Add(string.Format("stats.jsonPoolStats: current {0} max {1} total {2}.", json.currentInUseSize, json.maxInUseSize, json.poolSize));
        var matching = stats.matchingPoolStats;
        OnScreenLog.Add(string.Format("stats.matchingPoolStats: current {0} max {1} total {2}.", matching.curMemUsage, matching.maxMemUsage, matching.totalMemSize));
        var matchingSsl = stats.matchingSslPoolStats;
        OnScreenLog.Add(string.Format("stats.matchingSslPoolStats: current {0} max {1} total {2}.", matchingSsl.curMemUsage, matchingSsl.maxMemUsage, matchingSsl.totalMemSize));
        var mesg = stats.inGameMsgPoolStats;
        OnScreenLog.Add(string.Format("stats.inGameMsgPoolStats: current {0} max {1} total {2}.", mesg.currentInUseSize, mesg.maxInUseSize, mesg.poolSize));
    }

    public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.Service == Sony.NP.ServiceTypes.NpUtils)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.NpUtilsSetTitleIdForDevelopment:
                    OutputSetTitleIdForDevelopment(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.NpUtilsDisplaySigninDialog:
                    OutputDisplaySigninDialog(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.NpUtilsCheckAvailability:
                    OutputCheckAvailability(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.NpUtilsCheckPlus:
                    OutputCheckPlus(callbackEvent.Response as Sony.NP.NpUtils.CheckPlusResponse);
                    break;
                case Sony.NP.FunctionTypes.NpUtilsGetParentalControlInfo:
                    OutputGetParentalControlInfo(callbackEvent.Response as Sony.NP.NpUtils.GetParentalControlInfoResponse);
                    break;
                default:
                    break;
            }
        }
    }

    private void OutputSetTitleIdForDevelopment(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("SetTitleIdForDevelopment Empty Response");

        if (response.Locked == false)
        {

        }
    }

    private void OutputDisplaySigninDialog(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("DisplaySigninDialog Empty Response");

        if (response.Locked == false)
        {

        }
    }

    private void OutputCheckAvailability(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("CheckAvailability Empty Response");

        if (response.Locked == false)
        {
            
        }
    }

    private void OutputCheckPlus(Sony.NP.NpUtils.CheckPlusResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("CheckPlus Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("Authorized : " + response.Authorized);        
        }
    }

    private void OutputGetParentalControlInfo(Sony.NP.NpUtils.GetParentalControlInfoResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("GetParentalControlInfo Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("Age : " + response.Age);
            OnScreenLog.Add("ChatRestriction : " + response.ChatRestriction);      
            OnScreenLog.Add("UGCRestriction : " + response.UGCRestriction);          
        }
    }

}
#endif
