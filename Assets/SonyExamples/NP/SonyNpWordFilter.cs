using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS4
public class SonyNpWordFilter : IScreen
{
    MenuLayout m_MenuWordFilter;

    public SonyNpWordFilter()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuWordFilter;
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
        m_MenuWordFilter = new MenuLayout(this, 450, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuWordFilter.Update();

        if (m_MenuWordFilter.AddItem("Filter Comment", "Filter a comment and remove any profanity."))
        {
            FilterComment();
        }

        if (m_MenuWordFilter.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void FilterComment()
    {
        try
        {
            Sony.NP.WordFilter.FilterCommentRequest request = new Sony.NP.WordFilter.FilterCommentRequest();

            request.Comment = "Hello, this is a fucking crap bullshit message that tests the vulgarity filter.";

            Sony.NP.WordFilter.SanitizedCommentResponse response = new Sony.NP.WordFilter.SanitizedCommentResponse();

            int requestId = Sony.NP.WordFilter.FilterComment(request, response);
            OnScreenLog.Add("Filter Comment Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.Service == Sony.NP.ServiceTypes.WordFilter)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.WordfilterFilterComment:
                    OutputSanitizedComment(callbackEvent.Response as Sony.NP.WordFilter.SanitizedCommentResponse);
                    break;
                default:
                    break;
            }
        }
    }

    private void OutputSanitizedComment(Sony.NP.WordFilter.SanitizedCommentResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Sanitized Comment Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("ResultComment : " + response.ResultComment);
            OnScreenLog.Add("IsCommentChanged : " + response.IsCommentChanged);
        }
    }
}
#endif
