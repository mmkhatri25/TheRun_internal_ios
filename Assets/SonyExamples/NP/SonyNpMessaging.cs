using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

#if UNITY_PS4
public class SonyNpMessaging : IScreen
{
    MenuLayout m_MenuMessaging;

    Sony.NP.Messaging.GameDataMessageImage thumbnailImage = new Sony.NP.Messaging.GameDataMessageImage();

    bool hasCurrentMsg = false;
    UInt64 currentGameMsgId = 0;
    bool currentMsgHasAttachment = false;

    public SonyNpMessaging()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuMessaging;
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

    public void SetCurrentMessage(UInt64 gameMsgId, bool msgHasAttachment)
    {
        //OnScreenLog.AddWarning("Set Current Msg : " + gameMsgId + " : Has Attachment = " + msgHasAttachment);
        hasCurrentMsg = true;
        currentGameMsgId = gameMsgId;
        currentMsgHasAttachment = msgHasAttachment;
    }

    public void ClearCurrentMessage()
    {
        hasCurrentMsg = false;
    }

    public void Initialize()
    {
        m_MenuMessaging = new MenuLayout(this, 450, 20);

        // Must define an image for the message otherwise it can't be created.
        // Also streamingAssetsPath needs to be called on the main thread. 
        thumbnailImage.ImgPath = Application.streamingAssetsPath + "/PS4SessionImage.jpg";
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuMessaging.Update();

        if (m_MenuMessaging.AddItem("Send In-Game Message", "Send an in-game message."))
        {
            SendInGameMessage();
        }

        if (m_MenuMessaging.AddItem("Display Received Messages Dialog", "Display the received game data message dialog."))
        {
            DisplayReceivedGameDataMessagesDialog();
        }

        if (m_MenuMessaging.AddItem("Send Game Data Message", "Send a game data message."))
        {
            SendGameDataMessage();
        }

        if (m_MenuMessaging.AddItem("Get Received Game Data Messages", "Retrieve a list of game data messages."))
        {
            GetReceivedGameDataMessages();
        }

        if (m_MenuMessaging.AddItem("Comsume a Game Data Message", "Consume the first retrieved  message or a message 'Accepted' by the user in the 'Game Alerts' dialog.", hasCurrentMsg))
        {
            ConsumeGameDataMessage();
        }

        if (m_MenuMessaging.AddItem("Get Game Data Message Thumbnail", "Get the thumbnail image of the first retrieved message or a message 'Accepted' by the user in the 'Game Alerts' dialog.", hasCurrentMsg))
        {
            GetGameDataMessageThumbnail();
        }

        if (m_MenuMessaging.AddItem("Get Game Data Message Attachment", "Get the attachment of the first retrieved message.", hasCurrentMsg && currentMsgHasAttachment))
        {
            GetGameDataMessageAttachment();
        }

        if (m_MenuMessaging.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public byte[] GenerateRandomGameData(int size)
    {
        System.Random rand = new System.Random();

        byte[] gameData = new byte[size];

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


    public void SendInGameMessage()
    {
        try
        {
            Sony.NP.Messaging.SendInGameMessageRequest request = new Sony.NP.Messaging.SendInGameMessageRequest();

            request.UserId = User.GetActiveUserId;
            request.Message = GenerateRandomGameData(32);

            OnScreenLog.Add("Random Data to send:");
            OutputData(request.Message);

            request.RecipientPlatformType = Sony.NP.Core.PlatformType.ps4;

            // Send message to the active user.
            request.RecipientId = SonyNpUserProfiles.GetLocalAccountId(User.GetActiveUserId);

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Messaging.SendInGameMessage(request, response);
            OnScreenLog.Add("SendInGameMessage Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void DisplayReceivedGameDataMessagesDialog()
    {
        try
        {
            Sony.NP.Messaging.DisplayReceivedGameDataMessagesDialogRequest request = new Sony.NP.Messaging.DisplayReceivedGameDataMessagesDialogRequest();

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Messaging.DisplayReceivedGameDataMessagesDialog(request, response);
            OnScreenLog.Add("DisplayReceivedGameDataMessagesDialog Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SendGameDataMessage()
    {
        try
        {
            Sony.NP.Messaging.SendGameDataMessageRequest request = new Sony.NP.Messaging.SendGameDataMessageRequest();

            request.UserId = User.GetActiveUserId;

            request.TextMessage = "Example text message";
            request.DataName = "An attachment of binary data.";
            request.DataDescription = "Attached random binary data from the sample app.";

            request.Attachment = GenerateRandomGameData(32);

            OnScreenLog.Add("Random Data to send:");
            OutputData(request.Attachment);

            request.EnableDialog = true;
            request.ExpireMinutes = 20;
            request.IsPS4Available = true;

            // Send message to the active user.
            Sony.NP.Core.NpAccountId[] recipients = new Sony.NP.Core.NpAccountId[1];
            recipients[0] = SonyNpUserProfiles.GetLocalAccountId(User.GetActiveUserId);

            request.Recipients = recipients;

            request.Thumbnail = thumbnailImage;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Messaging.SendGameDataMessage(request, response);
            OnScreenLog.Add("SendGameDataMessage Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void ConsumeGameDataMessage()
    {
        try
        {
            Sony.NP.Messaging.ConsumeGameDataMessageRequest request = new Sony.NP.Messaging.ConsumeGameDataMessageRequest();

            request.GameDataMsgId = currentGameMsgId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.Messaging.ConsumeGameDataMessage(request, response);
            OnScreenLog.Add("ConsumeGameDataMessage Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetReceivedGameDataMessages()
    {
        try
        {
            Sony.NP.Messaging.GetReceivedGameDataMessagesRequest request = new Sony.NP.Messaging.GetReceivedGameDataMessagesRequest();

            request.Offset = 0;
            request.PageSize = 50;

            Sony.NP.Messaging.GameDataMessagesResponse response = new Sony.NP.Messaging.GameDataMessagesResponse();

            int requestId = Sony.NP.Messaging.GetReceivedGameDataMessages(request, response);
            OnScreenLog.Add("GetReceivedGameDataMessages Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetGameDataMessageThumbnail()
    {
        try
        {
            Sony.NP.Messaging.GetGameDataMessageThumbnailRequest request = new Sony.NP.Messaging.GetGameDataMessageThumbnailRequest();

            request.GameDataMsgId = currentGameMsgId;

            Sony.NP.Messaging.GameDataMessageThumbnailResponse response = new Sony.NP.Messaging.GameDataMessageThumbnailResponse();

            int requestId = Sony.NP.Messaging.GetGameDataMessageThumbnail(request, response);
            OnScreenLog.Add("GetGameDataMessageThumbnail Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetGameDataMessageAttachment()
    {
        try
        {
            Sony.NP.Messaging.GetGameDataMessageAttachmentRequest request = new Sony.NP.Messaging.GetGameDataMessageAttachmentRequest();

            request.GameDataMsgId = currentGameMsgId;

            Sony.NP.Messaging.GameDataMessageAttachmentResponse response = new Sony.NP.Messaging.GameDataMessageAttachmentResponse();

            int requestId = Sony.NP.Messaging.GetGameDataMessageAttachment(request, response);
            OnScreenLog.Add("DisplayReceivedGameDataMessagesDialog Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.Service == Sony.NP.ServiceTypes.Messaging)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.MessagingSendInGameMessage:
                    OutputSendInGameMessage(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.MessagingDisplayReceivedGameDataMessagesDialog:
                    OutputDisplayReceivedGameDataMessagesDialog(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.MessagingSendGameDataMessage:
                    OutputSendGameDataMessage(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.MessagingConsumeGameDataMessage:
                    OutputConsumeGameDataMessage(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.MessagingGetReceivedGameDataMessages:
                    OutputGetReceivedGameDataMessages(callbackEvent.Response as Sony.NP.Messaging.GameDataMessagesResponse);
                    break;
                case Sony.NP.FunctionTypes.MessagingGetGameDataMessageThumbnail:
                    OutputGetGameDataMessageThumbnail(callbackEvent.Response as Sony.NP.Messaging.GameDataMessageThumbnailResponse);
                    break;
                case Sony.NP.FunctionTypes.MessagingGetGameDataMessageAttachment:
                    OutputGetGameDataMessageAttachment(callbackEvent.Response as Sony.NP.Messaging.GameDataMessageAttachmentResponse);
                    break;
                default:
                    break;
            }
        }

        // Notifications
        if (callbackEvent.Service == Sony.NP.ServiceTypes.Notification)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.NotificationGameCustomDataEvent:
                    {
                        Sony.NP.Messaging.GameCustomDataEventResponse gameCustomDataEventResponse = (Sony.NP.Messaging.GameCustomDataEventResponse)callbackEvent.Response;
                        
                        if ( gameCustomDataEventResponse.IsErrorCode == false)
                        {
                            // It's not possible to tell from the system event if the message has an attachment.
                            SetCurrentMessage(gameCustomDataEventResponse.ItemId, false);
                        }
                    }
                    break;
            }
        }
    }

    private void OutputSendInGameMessage(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("SendInGameMessage Empty Response");

        if (response.Locked == false)
        {
        }
    }

    private void OutputDisplayReceivedGameDataMessagesDialog(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("DisplayReceivedGameDataMessagesDialog Empty Response");

        if (response.Locked == false)
        {
        }
    }

    private void OutputSendGameDataMessage(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("SendGameDataMessage Empty Response");

        if (response.Locked == false)
        {
        }
    }

    private void OutputConsumeGameDataMessage(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("ConsumeGameDataMessage Empty Response");

        if (response.Locked == false)
        {
        }

        ClearCurrentMessage();
    }

    private void OutputGetReceivedGameDataMessages(Sony.NP.Messaging.GameDataMessagesResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("GameDataMessagesResponse Response");

        if (response.Locked == false)
        {
            if (response.GameDataMessages == null || response.GameDataMessages.Length == 0)
            {
                OnScreenLog.Add("Response contains no data messages.");
            }
            else
            {
                OnScreenLog.Add("Number Data Messages : " + response.GameDataMessages.Length);
                for(int i = 0; i < response.GameDataMessages.Length; i++)
                {
                    Sony.NP.Messaging.GameDataMessage gameDataMessage = response.GameDataMessages[i];

                    if ( i == 0 )
                    {
                        SetCurrentMessage(gameDataMessage.GameDataMsgId, gameDataMessage.DataType == Sony.NP.Messaging.GameCustomDataTypes.Attachment);
                    }

                    OnScreenLog.Add("GameDataMsg : " + i);

                    OnScreenLog.Add("   GameDataMsgId : " + gameDataMessage.GameDataMsgId);
                    OnScreenLog.Add("   FromUser : " + gameDataMessage.FromUser);

                    OnScreenLog.Add("   ReceivedDate : " + gameDataMessage.ReceivedDate);
                    OnScreenLog.Add("   ExpiredDate : " + gameDataMessage.ExpiredDate);
                    OnScreenLog.Add("   IsPS4Available : " + gameDataMessage.IsPS4Available);
                    OnScreenLog.Add("   IsPSVitaAvailable : " + gameDataMessage.IsPSVitaAvailable);
                    OnScreenLog.Add("   DataType : " + gameDataMessage.DataType);

                    if (gameDataMessage.DataType == Sony.NP.Messaging.GameCustomDataTypes.Url)
                    {
                        OnScreenLog.Add("   Url : " + gameDataMessage.Url);
                    }

                    OnScreenLog.Add("   HasDetails : " + gameDataMessage.HasDetails);

                    if (gameDataMessage.HasDetails == true)
                    {
                        OnScreenLog.Add("        DataName : " + gameDataMessage.Details.DataName);
                        OnScreenLog.Add("        DataDescription : " + gameDataMessage.Details.DataDescription);
                        OnScreenLog.Add("        TextMessage : " + gameDataMessage.Details.TextMessage);
                    }

                    OnScreenLog.Add("   IsUsed : " + gameDataMessage.IsUsed);
                }
            }
        }
    }

    private void OutputGetGameDataMessageThumbnail(Sony.NP.Messaging.GameDataMessageThumbnailResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("GameDataMessageThumbnailResponse Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("GameDataMsgId : " + response.GameDataMsgId);

            if (response.Thumbnail != null)
            {
                OnScreenLog.Add(" Thumbnail Length : " + response.Thumbnail.Length);
            }
            else
            {
                OnScreenLog.Add("Thumbnail : None");
            }      
        }

        ClearCurrentMessage();
    }

    private void OutputGetGameDataMessageAttachment(Sony.NP.Messaging.GameDataMessageAttachmentResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("GameDataMessageAttachment Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("GameDataMsgId : " + response.GameDataMsgId);

            OnScreenLog.Add("Attachment :");
            if (response.Attachment != null)
            {
                OutputData(response.Attachment);
            }
            else
            {
                OnScreenLog.Add("None");
            }
        }

        ClearCurrentMessage();
    }
}
#endif
