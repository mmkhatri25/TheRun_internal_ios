using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS4
public class SonyNpAuth : IScreen
{
    MenuLayout m_MenuAuth;

    public SonyNpAuth()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuAuth;
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
        m_MenuAuth = new MenuLayout(this, 450, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuAuth.Update();

        if (m_MenuAuth.AddItem("Get Auth Code", "Gets an authorization code from the PSN servers for the calling user."))
        {
            GetAuthCode();
        }

        if (m_MenuAuth.AddItem("Get Id Token", "Gets an Id Token from the PSN servers for the calling user."))
        {
            GetIdToken();
        }

        if (m_MenuAuth.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void GetAuthCode()
    {
        try
        {
            Sony.NP.Auth.GetAuthCodeRequest request = new Sony.NP.Auth.GetAuthCodeRequest();

            // test values from SDK nptoolkit sample ... replace with your own project values
            Sony.NP.Auth.NpClientId clientId = new Sony.NP.Auth.NpClientId();
            clientId.Id = "c8c483e7-f0b4-420b-877b-307fcb4c3cdc";

            request.ClientId = clientId;
            request.Scope = "psn:s2s";

            Sony.NP.Auth.AuthCodeResponse response = new Sony.NP.Auth.AuthCodeResponse();

            int requestId = Sony.NP.Auth.GetAuthCode(request, response);
            OnScreenLog.Add("Get Auth Code Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetIdToken()
    {
        try
        {
            Sony.NP.Auth.GetIdTokenRequest request = new Sony.NP.Auth.GetIdTokenRequest();

            // test values from SDK nptoolkit sample ... replace with your own project values
            Sony.NP.Auth.NpClientId clientId = new Sony.NP.Auth.NpClientId();
            clientId.Id = "c8c483e7-f0b4-420b-877b-307fcb4c3cdc";

            Sony.NP.Auth.NpClientSecret clientSecret = new Sony.NP.Auth.NpClientSecret();
            clientSecret.Secret = "Your own client secret";

            request.ClientId = clientId;
            request.ClientSecret = clientSecret;
            request.Scope = "psn:s2s";

            Sony.NP.Auth.IdTokenResponse response = new Sony.NP.Auth.IdTokenResponse();

            int requestId = Sony.NP.Auth.GetIdToken(request, response);
            OnScreenLog.Add("Get Id Token Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.Service == Sony.NP.ServiceTypes.Auth)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.AuthGetAuthCode:
                    OutputAuthCode(callbackEvent.Response as Sony.NP.Auth.AuthCodeResponse);
                    break;
                case Sony.NP.FunctionTypes.AuthGetIdToken:
                    OutputIdToken(callbackEvent.Response as Sony.NP.Auth.IdTokenResponse);
                    break;
                default:
                    break;
            }
        }
    }

    private void OutputAuthCode(Sony.NP.Auth.AuthCodeResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Auth Code Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("AuthCode : " + response.AuthCode);
            OnScreenLog.Add("IssuerId : " + response.IssuerId);
        }
    }

    private void OutputIdToken(Sony.NP.Auth.IdTokenResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Id Token Response");

        if (response.Locked == false)
        {
            if ( response.IsErrorCode == true )
            {
                OnScreenLog.AddWarning("Don't forget to replace the ClientSecret with your own client secret, otherwise an error will occur.");
            }
            OnScreenLog.Add("IdToken : " + response.IdToken);
        }
    }
}
#endif
