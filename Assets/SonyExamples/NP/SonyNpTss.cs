using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS4
public class SonyNpTss : IScreen
{
    MenuLayout m_MenuTss;

    public SonyNpTss()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuTss;
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
        m_MenuTss = new MenuLayout(this, 450, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuTss.Update();

        if (m_MenuTss.AddItem("Get Meta Data", "Get the TSS meta data from slot 0, which provides the size of the data but not the actual data buffer."))
        {
            GetTssMetaData();
        }

        if (m_MenuTss.AddItem("Get Data", "Get the TSS data from slot 0."))
        {
            GetTssData();
        }

        if (m_MenuTss.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void GetTssMetaData()
    {
        try
        {
            Sony.NP.Tss.GetDataRequest request = new Sony.NP.Tss.GetDataRequest();

            request.TssSlotId = 2;
            request.RetrieveStatusOnly = true;
            request.UserId = User.GetActiveUserId;

            Sony.NP.Tss.TssDataResponse response = new Sony.NP.Tss.TssDataResponse();

            int requestId = Sony.NP.Tss.GetData(request, response);
            OnScreenLog.Add("Tss GetData Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetTssData()
    {
        try
        {
            Sony.NP.Tss.GetDataRequest request = new Sony.NP.Tss.GetDataRequest();

            request.TssSlotId = 2;
            request.RetrieveStatusOnly = false;
            request.UserId = User.GetActiveUserId;

            Sony.NP.Tss.TssDataResponse response = new Sony.NP.Tss.TssDataResponse();

            int requestId = Sony.NP.Tss.GetData(request, response);
            OnScreenLog.Add("Tss GetData Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.Service == Sony.NP.ServiceTypes.Tss)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.TssGetData:
                    OutputTssData(callbackEvent.Response as Sony.NP.Tss.TssDataResponse);
                    break;
                default:
                    break;
            }
        }
    }

    private void OutputTssData(Sony.NP.Tss.TssDataResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Tss GetData Response");

        if (response.Locked == false)
        {
            if (response.Data != null && response.Data.Length > 0)
            {
                string dataOutput = "";

                int maxSize = Math.Min(response.Data.Length, 255);

                for (int i = 0; i < maxSize; i++)
                {
                    dataOutput += response.Data[i] + ", ";
                }

                OnScreenLog.Add("Data:");
                OnScreenLog.Add(dataOutput);
            }
            else
            {
                OnScreenLog.Add("Data: (Empty)");
            }

            OnScreenLog.Add("Last Modified : " + response.LastModified);
            OnScreenLog.Add("Status : " + response.StatusCode);
            OnScreenLog.Add("ContentLength : " + response.ContentLength);
        }
        else
        {
            OnScreenLog.AddError("Response locked");
        }
    }
}
#endif
