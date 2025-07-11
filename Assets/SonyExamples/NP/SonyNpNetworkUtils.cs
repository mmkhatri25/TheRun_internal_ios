using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS4
public class SonyNpNetworkUtils : IScreen
{
    MenuLayout m_MenuNetworkUtils;

    public SonyNpNetworkUtils()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuNetworkUtils;
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
        m_MenuNetworkUtils = new MenuLayout(this, 450, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuNetworkUtils.Update();

        if (m_MenuNetworkUtils.AddItem("Request Bandwidth Info", "Get the current network bandwidth info."))
        {
            RequestBandwidthInfo();
        }

        if (m_MenuNetworkUtils.AddItem("Request Basic Network Info", "Get the basic network info."))
        {
            RequestBasicNetworkInfo();
        }

        if (m_MenuNetworkUtils.AddItem("Request Detailed Network Info", "Get the full network info."))
        {
            RequestDetailedNetworkInfo();
        }

        if (m_MenuNetworkUtils.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void RequestBandwidthInfo()
    {
        try
        {
            Sony.NP.NetworkUtils.GetBandwidthInfoRequest request = new Sony.NP.NetworkUtils.GetBandwidthInfoRequest();
            request.UserId = User.GetActiveUserId;

            Sony.NP.NetworkUtils.BandwidthInfoResponse response = new Sony.NP.NetworkUtils.BandwidthInfoResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.NetworkUtils.GetBandwidthInfo(request, response);
            OnScreenLog.Add("GetBandwidthInfo Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void RequestBasicNetworkInfo()
    {
        try
        {
            Sony.NP.NetworkUtils.GetBasicNetworkInfoRequest request = new Sony.NP.NetworkUtils.GetBasicNetworkInfoRequest();
            request.UserId = User.GetActiveUserId;

            Sony.NP.NetworkUtils.BasicNetworkInfoResponse response = new Sony.NP.NetworkUtils.BasicNetworkInfoResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.NetworkUtils.GetBasicNetworkInfoInfo(request, response);
            OnScreenLog.Add("GetBasicNetworkInfoInfo Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void RequestDetailedNetworkInfo()
    {
        try
        {
            Sony.NP.NetworkUtils.GetDetailedNetworkInfoRequest request = new Sony.NP.NetworkUtils.GetDetailedNetworkInfoRequest();
            request.UserId = User.GetActiveUserId;

            Sony.NP.NetworkUtils.DetailedNetworkInfoResponse response = new Sony.NP.NetworkUtils.DetailedNetworkInfoResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.NetworkUtils.GetDetailedNetworkInfo(request, response);
            OnScreenLog.Add("GetDetailedNetworkInfoRequest Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.Service == Sony.NP.ServiceTypes.NetworkUtils)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.NetworkUtilsGetBandwidthInfo:
                    OutputBandwidthInfo(callbackEvent.Response as Sony.NP.NetworkUtils.BandwidthInfoResponse);
                    break;
                case Sony.NP.FunctionTypes.NetworkUtilsGetBasicNetworkInfo:
                    OutputBasicNetworkInfo(callbackEvent.Response as Sony.NP.NetworkUtils.BasicNetworkInfoResponse);
                    break;
                case Sony.NP.FunctionTypes.NetworkUtilsGetDetailedNetworkInfo:
                    OutputDetailedNetworkInfo(callbackEvent.Response as Sony.NP.NetworkUtils.DetailedNetworkInfoResponse);
                    break;
                default:
                    break;
            }
        }
    }

    private void OutputBandwidthInfo(Sony.NP.NetworkUtils.BandwidthInfoResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("BandwidthInfo Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add(response.Bandwidth.ToString());
        }
    }

    private void OutputBasicNetworkInfo(Sony.NP.NetworkUtils.BasicNetworkInfoResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("BasicNetworkInfo Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("IP Address : " + response.IpAddress);
            OnScreenLog.Add("Nat Info : " + response.NatInfo.ToString());
            OnScreenLog.Add("Connection Status : " + response.ConnectionStatus);
        }
    }

    private void OutputDetailedNetworkInfo(Sony.NP.NetworkUtils.DetailedNetworkInfoResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("DetailedNetworkInfoResponse Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("Nat Info : " + response.NatInfo.ToString());
            OnScreenLog.Add("Connection Status : " + response.ConnectionStatus);
            OnScreenLog.Add("Device : " + response.Device);
            OnScreenLog.Add("Ethernet Addr : " + response.EthernetAddress.ToString());

            OnScreenLog.Add("RSSI % : " + response.RssiPercentage);
            OnScreenLog.Add("Channel : " + response.Channel);

            OnScreenLog.Add("MTU : " + response.MTU);
            OnScreenLog.Add("Link : " + response.Link);

            OnScreenLog.Add("Wifi Security : " + response.WifiSecurity);
            OnScreenLog.Add("IP Config : " + response.IpConfig);

            OnScreenLog.Add("HTTP Proxy Config : " + response.HttpProxyConfig);
            OnScreenLog.Add("BSSID : " + response.BSSID.ToString());

            OnScreenLog.Add("SSID : " + response.SSID);
            OnScreenLog.Add("DHCP Host name : " + response.DhcpHostname);
            OnScreenLog.Add("PPPoE Auth Name : " + response.PPPoeAuthName);
            OnScreenLog.Add("IP Address : " + response.IpAddress);
            OnScreenLog.Add("Netmask : " + response.Netmask);

            OnScreenLog.Add("Default Route : " + response.DefaultRoute);
            OnScreenLog.Add("Primary DNS : " + response.PrimaryDNS);
            OnScreenLog.Add("Secondary DNS : " + response.SecondaryDNS);
            OnScreenLog.Add("HTTP Proxy Server : " + response.HttpProxyServer);
        }
    }
}
#endif
