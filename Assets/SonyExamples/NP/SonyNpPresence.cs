using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

#if UNITY_PS4
public class SonyNpPresence : IScreen
{
    MenuLayout m_MenuPresence;

    public SonyNpPresence()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuPresence;
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
        m_MenuPresence = new MenuLayout(this, 450, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuPresence.Update();

        if (m_MenuPresence.AddItem("Delete Presence", "Delete the current presence for this user. This clears presence information like the Game Status text. Using 'Get Presence' after this will show an empty game status."))
        {
            DeletePresence();
        }

        if (m_MenuPresence.AddItem("Set Presence", "Set the current presence for this user. This sets presence data like the game status. Using 'Get Presence' after this will show so game status text."))
        {
            SetPresence();
        }

        if (m_MenuPresence.AddItem("Get Presence", "Get the current presence for this user. Returns additional presence data set in 'Set Presence'."))
        {
            GetPresence();
        }

        if (m_MenuPresence.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void DeletePresence()
    {
        try
        {
            Sony.NP.Presence.DeletePresenceRequest request = new Sony.NP.Presence.DeletePresenceRequest();

            request.UserId = User.GetActiveUserId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Presence.DeletePresence(request, response);
            OnScreenLog.Add("DeletePresence Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    const int GameDataSize = 128;

    public byte[] GenerateRandomGameData()
    {
        System.Random rand = new System.Random();

        byte[] gameData = new byte[GameDataSize];

        rand.NextBytes(gameData);

        return gameData;
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


    public void SetPresence()
    {
        try
        {
            Sony.NP.Presence.SetPresenceRequest request = new Sony.NP.Presence.SetPresenceRequest();

            request.UserId = User.GetActiveUserId;

            request.DefaultGameStatus = "This is some random presence text set by NpToolkit2 on frame " + OnScreenLog.FrameCount;

            Sony.NP.Presence.LocalizedGameStatus[] localizedGameStatuses = new Sony.NP.Presence.LocalizedGameStatus[2];

            localizedGameStatuses[0].LanguageCode = "fr";
            localizedGameStatuses[0].GameStatus = "French version : Set by NpToolkit2 on frame " + OnScreenLog.FrameCount;

            localizedGameStatuses[1].LanguageCode = "de";
            localizedGameStatuses[1].GameStatus = "German version : Set by NpToolkit2 on frame " + OnScreenLog.FrameCount;

            request.LocalizedGameStatuses = localizedGameStatuses;

            byte[] data = GenerateRandomGameData();

            OnScreenLog.Add("Random Data to Set:");
            OutputData(data);

            request.BinaryGameData = data;
            
            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Presence.SetPresence(request, response);
            OnScreenLog.Add("SetPresence Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetPresence()
    {
        // Fetch the local user to account id mapping.
        Sony.NP.UserProfiles.LocalUsers users = new Sony.NP.UserProfiles.LocalUsers();
        try
        {
            Sony.NP.UserProfiles.GetLocalUsers(users);
        }
        catch (Sony.NP.NpToolkitException)
        {
            // This means that one or more of the user has an error code associated with them. This might mean they are not signed in or are not signed up to an online account.
        }

        try
        {
            Sony.NP.Presence.GetPresenceRequest request = new Sony.NP.Presence.GetPresenceRequest();
            request.UserId = User.GetActiveUserId;

            Sony.NP.Presence.PresenceResponse response = new Sony.NP.Presence.PresenceResponse();

            for (int i = 0; i < users.LocalUsersIds.Length; i++)
            {
                if (users.LocalUsersIds[i].UserId.Id == request.UserId.Id)
                {
                    request.FromUser = users.LocalUsersIds[i].AccountId;
                }
            }

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Presence.GetPresence(request, response);
            OnScreenLog.Add("GetPresence Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.Service == Sony.NP.ServiceTypes.Presence)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.PresenceDeletePresence:
                    OutputDeletePresence(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.PresenceSetPresence:
                    OutputSetPresence(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.PresenceGetPresence:
                    OutputGetPresence(callbackEvent.Response as Sony.NP.Presence.PresenceResponse);
                    break;
                default:
                    break;
            }
        }
    }

    private void OutputDeletePresence(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("DeletePresence Empty Response");

        if (response.Locked == false)
        {

        }
    }

    private void OutputSetPresence(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("SetPresence Empty Response");

        if (response.Locked == false)
        {

        }
    }

    private void OutputGetPresence(Sony.NP.Presence.PresenceResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("GetPresence Response");

        if (response.Locked == false)
        {
            Sony.NP.Presence.UserPresence userPresence = response.UserPresence;

            OnScreenLog.Add(userPresence.ToString());

            if (userPresence.Platforms != null)
            {
                for (int i = 0; i < userPresence.Platforms.Length; i++)
                {
                    OnScreenLog.Add("   Platform : " + userPresence.Platforms[i].Platform);
                    OnScreenLog.Add("   Title Id : " + userPresence.Platforms[i].TitleId);
                    OnScreenLog.Add("   Title Name : " + userPresence.Platforms[i].TitleName);
                    OnScreenLog.Add("   Game Status : " + userPresence.Platforms[i].GameStatus);

                    byte[] data = userPresence.Platforms[i].BinaryGameData;
                    if (data != null && data.Length > 0)
                    {
                        OnScreenLog.Add("Data : ");
                        OutputData(data);
                    }

                }
            }
           
        }
    }
}
#endif
