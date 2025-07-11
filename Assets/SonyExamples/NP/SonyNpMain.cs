#define USE_ASYNC_HANDLING
#define ENABLE_TUS_LOGGING

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading;


public class SonyNpMain : MonoBehaviour, IScreen
{
    public Material iconMaterial;

#if UNITY_PS4
    MenuStack m_MenuStack = null;
	MenuLayout m_MenuMain;
	bool m_NpReady = true;     // Is the NP plugin initialised and ready for use.

    SonyNpNotifications m_Notifications;
    SonyNpFriends m_Friends;
    SonyNpUserProfiles m_UserProfiles;
    SonyNpNetworkUtils m_NetworkUtils;
    SonyNpTrophies m_Trophies;
    SonyNpUtils m_NpUtils;
    SonyNpPresence m_Presence;
    SonyNpRanking m_Ranking;
    SonyNpMatching m_Matching;
    SonyNpTss m_Tss;
    SonyNpTus m_Tus;
    SonyNpMessaging m_Messaging;
    SonyNpCommerce m_Commerce;
    SonyNpAuth m_Auth;
    SonyNpWordFilter m_WordFilter;
    SonyNpActivityFeed m_ActivityFeed;

    static bool dialogOpened = false;


    //static SonyNpMain s_main;
    static Sony.NP.Icon currentIcon = null;
    static bool updateIcon = false;

    // This is called from any thread
    public static void SetIconTexture(Sony.NP.Icon icon)
    {
        currentIcon = icon;
        updateIcon = true;
    }

    // This must be called from the main thread otherwise the Texture2D can't be created.
    public void UpdateIcon()
    {
        if ( updateIcon == true )
        {
            updateIcon = false;

            if ( currentIcon != null )
            {
                // This will create the texture if it is not already cached in the currentIcon.           
                UnityEngine.Texture2D iconTexture = new UnityEngine.Texture2D(currentIcon.Width, currentIcon.Height);

                iconTexture.LoadImage(currentIcon.RawBytes);

                iconMaterial.mainTexture = iconTexture;

                OnScreenLog.Add("Updating icon material : W = " + iconTexture.width + " H = " + iconTexture.height);
            }
        }
    }

    void Start()
	{
        //s_main = this;

        m_MenuMain = new MenuLayout(this, 450, 20);

		m_MenuStack = new MenuStack();
		m_MenuStack.SetMenu(m_MenuMain);

        // Initialize the NP Toolkit.
        OnScreenLog.Add("Initializing NP");

#if UNITY_PS4
		OnScreenLog.Add(System.String.Format("Initial UserId:0x{0:X}  Primary UserId:0x{1:X}", UnityEngine.PS4.Utility.initialUserId, UnityEngine.PS4.Utility.primaryUserId));
#endif

        m_Notifications = new SonyNpNotifications();
        m_Friends = new SonyNpFriends();
        m_UserProfiles = new SonyNpUserProfiles();
        m_NetworkUtils = new SonyNpNetworkUtils();
        m_Trophies = new SonyNpTrophies();
        m_NpUtils = new SonyNpUtils();
        m_Presence = new SonyNpPresence();
        m_Ranking = new SonyNpRanking();
        m_Matching = new SonyNpMatching();
        m_Tss = new SonyNpTss();
        m_Tus = new SonyNpTus();
        m_Messaging = new SonyNpMessaging();
        m_Commerce = new SonyNpCommerce();
        m_Auth = new SonyNpAuth();
        m_WordFilter = new SonyNpWordFilter();
        m_ActivityFeed = new SonyNpActivityFeed();

        InitialiseNpToolkit();

      //  NetworkManager.Init();
    }

    public Sony.NP.InitResult initResult;

    void InitialiseNpToolkit()
    {
        Sony.NP.Main.OnAsyncEvent += Main_OnAsyncEvent;

        Sony.NP.InitToolkit init = new Sony.NP.InitToolkit();

        init.contentRestrictions.DefaultAgeRestriction = 0;

        Sony.NP.AgeRestriction[] ageRestrictions = new Sony.NP.AgeRestriction[2];

        ageRestrictions[0] = new Sony.NP.AgeRestriction(10, new Sony.NP.Core.CountryCode("fr"));
        ageRestrictions[1] = new Sony.NP.AgeRestriction(15, new Sony.NP.Core.CountryCode("au"));

        init.contentRestrictions.AgeRestrictions = ageRestrictions;

        // Only do this if age restriction isn't required for the product. See documentation for details.
        // init.contentRestrictions.ApplyContentRestriction = false;

        init.threadSettings.affinity = Sony.NP.Affinity.AllCores; // Sony.NP.Affinity.Core2 | Sony.NP.Affinity.Core4;

        // Mempools
        init.memoryPools.JsonPoolSize = 6 * 1024 * 1024;
		init.memoryPools.SslPoolSize *= 4;

		init.memoryPools.MatchingSslPoolSize *= 4;
		init.memoryPools.MatchingPoolSize *= 4;

		init.SetPushNotificationsFlags(Sony.NP.PushNotificationsFlags.NewInGameMessage |
										Sony.NP.PushNotificationsFlags.NewInvitation | Sony.NP.PushNotificationsFlags.UpdateBlockedUsersList |
										Sony.NP.PushNotificationsFlags.UpdateFriendPresence | Sony.NP.PushNotificationsFlags.UpdateFriendsList);

        try
        {
            initResult = Sony.NP.Main.Initialize(init);

            if (initResult.Initialized == true)
            {
                OnScreenLog.Add("NpToolkit Initialized ");
                OnScreenLog.Add("Plugin SDK Version : " + initResult.SceSDKVersion.ToString() );
				OnScreenLog.Add("Plugin DLL Version : " + initResult.DllVersion.ToString());
            }
            else
            {
                OnScreenLog.Add("NpToolkit not initialized ");
            }
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception During Initialization : " + e.ExtendedMessage);
        }
#if UNITY_EDITOR
        catch (DllNotFoundException e)
        {
            OnScreenLog.AddError("Missing DLL Expection : " + e.Message);
            OnScreenLog.AddError("The sample APP will not run in the editor.");
        }
#endif

        string[] args = System.Environment.GetCommandLineArgs();

        if (args.Length > 0)
        {
            OnScreenLog.Add("Args:");

            for (int i = 0; i < args.Length; i++)
            {
                OnScreenLog.Add("  " + args[i]);
            }
        }
        else
        {
            OnScreenLog.Add("No Args");
        }


        OnScreenLog.AddNewLine();

        GamePad[] gamePads = GetComponents<GamePad>();

        User.Initialise(gamePads);

        LogStartUp();
    }

#if USE_ASYNC_HANDLING
    // NOTE : This is called on the "Sony NP" thread and is independent of the Behaviour update.
    // This thread is created by the SonyNP.dll when NpToolkit2 is initialised.
    private void Main_OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        OnScreenLog.Add("Event: Service = (" + callbackEvent.Service + ") : API Called = (" + callbackEvent.ApiCalled + ") : Request Id = (" + callbackEvent.NpRequestId + ") : Calling User Id = (" + callbackEvent.UserId + ")");

        HandleAsynEvent(callbackEvent);
    }

    void Update()
    {
        //Sony.NP.Main.Update();
        UpdateIcon();

       // NetworkManager.Update();
    }
#else

    // This is an example of how to process the events on the main thread.
    // The Main_OnAsyncEvent method is still called on a seperate thread, but the event is added to a queue.
    // In the MonoBehaviour Update() method a single event is dequeued per frame, if there is one, and then processed.
    // There is a synchronisation object (pendingSyncObject) which is locked when anything is added or removed from the queue.

    Queue<Sony.NP.NpCallbackEvent> pendingEvents = new Queue<Sony.NP.NpCallbackEvent>();
    System.Object pendingSyncObject = new System.Object();

    // NOTE : This is called on the "Sony NP" thread and is independent of the Behaviour update.
    private void Main_OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        try
        {
            lock (pendingSyncObject)
            {
                pendingEvents.Enqueue(callbackEvent);
            }
        }
        catch (Exception e)
        {
            OnScreenLog.AddError("Main_OnAsyncEvent General Exception = " + e.Message);
            OnScreenLog.AddError(e.StackTrace);
            Console.Error.WriteLine(e.StackTrace); // Output to the PS4 Stderr TTY
        }
    }

    void Update()
    {
        //Sony.NP.Main.Update();
        lock (pendingSyncObject)
        {
            // Dequeue 1 item per frame and process the event
            if(pendingEvents.Count > 0 )
            {
                Sony.NP.NpCallbackEvent callbackEvent = pendingEvents.Dequeue();

                OnScreenLog.Add("Event: Service = (" + callbackEvent.Service + ") : API Called = (" + callbackEvent.ApiCalled + ") : Request Id = (" + callbackEvent.NpRequestId + ") : Calling User Id = (" + callbackEvent.UserId + ")");

                HandleAsynEvent(callbackEvent);
            }
        }

        UpdateIcon();

        NetworkManager.Update();
    }
#endif

    private void HandleAsynEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        try
        {
            if (callbackEvent.Response != null)
            {
                if (callbackEvent.Response.ReturnCodeValue < 0)
                {
                    OnScreenLog.AddError("Response : " + callbackEvent.Response.ConvertReturnCodeToString(callbackEvent.ApiCalled));
                }
                else
                {
                    OnScreenLog.Add("Response : " + callbackEvent.Response.ConvertReturnCodeToString(callbackEvent.ApiCalled));
                }

                if (callbackEvent.Response.HasServerError)
                {
                    OutputSeverError(callbackEvent.Response);
                }

                LogService(callbackEvent.Service);
            }

            switch (callbackEvent.Service)
            {
                case Sony.NP.ServiceTypes.Notification:
                    {
                        switch (callbackEvent.ApiCalled)
                        {
                            case Sony.NP.FunctionTypes.NotificationDialogOpened:
                                dialogOpened = true;
                                GamePad.EnableInput(!dialogOpened);
                                break;
                            case Sony.NP.FunctionTypes.NotificationDialogClosed:
                                dialogOpened = false;
                                GamePad.EnableInput(!dialogOpened);
                                break;
                        }

                        m_Notifications.OnAsyncEvent(callbackEvent);

                        // Also call matching as this needs to process some notifications.
                        m_Matching.OnAsyncEvent(callbackEvent);

                        // Also call messaging as this needs to process some notifications.
                        m_Messaging.OnAsyncEvent(callbackEvent);

                        // Also call activity feed as this needs to process some notifications.
                        m_ActivityFeed.OnAsyncEvent(callbackEvent);
                    }
                    break;
                case Sony.NP.ServiceTypes.Friends:
                    m_Friends.OnAsyncEvent(callbackEvent);
                    break;
                case Sony.NP.ServiceTypes.UserProfile:
                    m_UserProfiles.OnAsyncEvent(callbackEvent);
                    break;
                case Sony.NP.ServiceTypes.NetworkUtils:
                    m_NetworkUtils.OnAsyncEvent(callbackEvent);
                    break;
                case Sony.NP.ServiceTypes.Trophy:
                    m_Trophies.OnAsyncEvent(callbackEvent);
                    break;
                case Sony.NP.ServiceTypes.NpUtils:
                    m_NpUtils.OnAsyncEvent(callbackEvent);
                    break;
                case Sony.NP.ServiceTypes.Presence:
                    m_Presence.OnAsyncEvent(callbackEvent);
                    break;
                case Sony.NP.ServiceTypes.Ranking:
                    m_Ranking.OnAsyncEvent(callbackEvent);
                    break;
                case Sony.NP.ServiceTypes.Matching:
                    m_Matching.OnAsyncEvent(callbackEvent);
                    break;
                case Sony.NP.ServiceTypes.Tss:
                    m_Tss.OnAsyncEvent(callbackEvent);
                    break;
                case Sony.NP.ServiceTypes.Tus:
                    m_Tus.OnAsyncEvent(callbackEvent);
                    break;
                case Sony.NP.ServiceTypes.Messaging:
                    m_Messaging.OnAsyncEvent(callbackEvent);
                    break;
                case Sony.NP.ServiceTypes.Commerce:
                    m_Commerce.OnAsyncEvent(callbackEvent);
                    break;
                case Sony.NP.ServiceTypes.Auth:
                    m_Auth.OnAsyncEvent(callbackEvent);
                    break;
                case Sony.NP.ServiceTypes.WordFilter:
                    m_WordFilter.OnAsyncEvent(callbackEvent);
                    break;
                case Sony.NP.ServiceTypes.ActivityFeed:
                    m_ActivityFeed.OnAsyncEvent(callbackEvent);
                    break;                 
                default:
                    break;
            }

            OnScreenLog.AddNewLine();

         //   NetworkManager.OnAsyncEvent(callbackEvent);
        }
        catch ( Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Main_OnAsyncEvent NpToolkit Exception = " + e.ExtendedMessage);
            Console.Error.WriteLine(e.ExtendedMessage); // Output to the PS4 Stderr TTY
            Console.Error.WriteLine(e.StackTrace); // Output to the PS4 Stderr TTY
        }
        catch (Exception e)
        {
            OnScreenLog.AddError("Main_OnAsyncEvent General Exception = " + e.Message);
            OnScreenLog.AddError(e.StackTrace);
            Console.Error.WriteLine(e.StackTrace); // Output to the PS4 Stderr TTY
        }
    }

    private void OutputSeverError(Sony.NP.ResponseBase response)
    {
        if (response == null) return;

        if (response.HasServerError)
        {
            string errorMsg = String.Format("Server Error : returnCode = (0x{0:X}) : webApiNextAvailableTime = ({1}) : httpStatusCode = ({2})", response.ReturnCode, response.ServerError.WebApiNextAvailableTime, response.ServerError.HttpStatusCode);
            OnScreenLog.AddError(errorMsg);

            OnScreenLog.AddError("Server Error : jsonData = " + response.ServerError.JsonData);
        }
    }

    void MenuMain()
    {
       
        m_MenuMain.Update();

        if (m_NpReady)
        {
            bool signedIn = true;

            if (m_MenuMain.AddItem("User Profiles", signedIn))
            {
                m_MenuStack.PushMenu(m_UserProfiles.GetMenu());
            }

            if (m_MenuMain.AddItem("Trophies", signedIn))
            {
                m_MenuStack.PushMenu(m_Trophies.GetMenu());
            }

            if (m_MenuMain.AddItem("Friends", signedIn))
            {
                m_MenuStack.PushMenu(m_Friends.GetMenu());
            }

            if (m_MenuMain.AddItem("Presence", signedIn))
            {
                m_MenuStack.PushMenu(m_Presence.GetMenu());
            }

            if (m_MenuMain.AddItem("Ranking", signedIn))
            {
                m_MenuStack.PushMenu(m_Ranking.GetMenu());
            }

            if (m_MenuMain.AddItem("Matching", signedIn))
            {
                m_MenuStack.PushMenu(m_Matching.GetMenu());
            }           

            if (m_MenuMain.AddItem("Network Utils", signedIn))
            {
                m_MenuStack.PushMenu(m_NetworkUtils.GetMenu());
            }

            if (m_MenuMain.AddItem("Np Utils", signedIn))
            {
                m_MenuStack.PushMenu(m_NpUtils.GetMenu());
            }

            if (m_MenuMain.AddItem("TSS", signedIn))
            {
                m_MenuStack.PushMenu(m_Tss.GetMenu());
            }

            if (m_MenuMain.AddItem("TUS", signedIn))
            {
                m_MenuStack.PushMenu(m_Tus.GetMenu());
            }

            if (m_MenuMain.AddItem("Messaging", signedIn))
            {
                m_MenuStack.PushMenu(m_Messaging.GetMenu());
            }

            if (m_MenuMain.AddItem("Commerce", signedIn))
            {
                m_MenuStack.PushMenu(m_Commerce.GetMenu());
            }

            if (m_MenuMain.AddItem("Auth", signedIn))
            {
                m_MenuStack.PushMenu(m_Auth.GetMenu());
            }

            if (m_MenuMain.AddItem("Activity Feed", signedIn))
            {
                m_MenuStack.PushMenu(m_ActivityFeed.GetMenu());
            }

            if (m_MenuMain.AddItem("Word Filter", signedIn))
            {
                m_MenuStack.PushMenu(m_WordFilter.GetMenu());
            }

			if (m_MenuMain.AddItem("Abort All Requests", signedIn && Sony.NP.Main.GetPendingRequests().Count > 0 ))
            {
				var pendingList = Sony.NP.Main.GetPendingRequests();

				foreach(var request in pendingList)
				{
					Sony.NP.Main.AbortRequest(request.NpRequestId);
				}
            }
        }
	}

	public void OnEnter()
	{
		//System.Security.Cryptography.RijndaelManaged
	}

	public void OnExit()
	{
	}

	public void Process(MenuStack stack)
	{
		MenuMain();

	}

    void OnGUI()
    {
        MenuLayout activeMenu = m_MenuStack.GetMenu();
        activeMenu.GetOwner().Process(m_MenuStack);

        DisplayPendingRequestsList();

        string userOutput = User.Output();
        GUI.TextArea(new Rect(Screen.width * 0.01f, Screen.height * 0.01f, Screen.width * 0.23f, Screen.height * 0.07f), userOutput);

        if (GamePad.activeGamePad != null && GamePad.activeGamePad.IsSquarePressed)
        {
            // Clear the OnScreenLog.
            OnScreenLog.Clear();
        }

        GUI.TextArea(new Rect(Screen.width * 0.25f, Screen.height * 0.95f, Screen.width * 0.70f, Screen.height * 0.02f), "Press 'Square' to clear the screen log. Use Right stick to scroll the screen log. Press 'R3' to reset the scroll.");

        if (GamePad.activeGamePad != null )
        {
            Vector2 rightStick = GamePad.activeGamePad.GetThumbstickRight;

            if (rightStick.y > 0.1f )
            {
                OnScreenLog.ScrollDown(rightStick.y * rightStick.y);
            }
            else if (rightStick.y < -0.1f)
            {
                OnScreenLog.ScrollUp(rightStick.y * rightStick.y);
            }

            if ( GamePad.activeGamePad.IsR3Pressed == true )
            {
                OnScreenLog.ScrollReset();
            }
        }
    }

    public void DisplayPendingRequestsList()
    {
        var pendingRequests = Sony.NP.Main.GetPendingRequests();

        if (pendingRequests == null)
        {
            return;
        }

        if (GamePad.activeGamePad == null)
        {
            return;
        }

        if (GamePad.activeGamePad.IsTrianglePressed )
        {
            // Abort last pending request
            if (pendingRequests.Count > 0)
            {
                var abortRequest = pendingRequests[pendingRequests.Count - 1];
                Sony.NP.Main.AbortRequest(abortRequest.NpRequestId);
            }
        }

        string pendingOutput = "Press 'Triangle' to Abort last pending request.\n\n";
        pendingOutput += String.Format("{0,-12} {1,-30} {2,-6}\n", "Request Id", "Response Type", "Aborting");

        foreach (var pendingRequest in pendingRequests)
        {
            string responseTypeText;

            if (pendingRequest.Request != null)
            {
                responseTypeText = pendingRequest.Request.GetType().ToString();
            }
            else
            {
                responseTypeText = "None";
            }

            pendingOutput += String.Format("{0,-10} {1,-30} {2,-5}\n", pendingRequest.NpRequestId, responseTypeText, pendingRequest.AbortPending);
        }

        GUI.TextArea(new Rect(Screen.width * 0.01f, Screen.height * 0.8f, Screen.width * 0.23f, Screen.height * 0.17f), pendingOutput);
    }

    void LogStartUp()
    {
#if ENABLE_TUS_LOGGING
        Thread thread = new Thread(new ThreadStart(ExecuteLogStartUp));
        thread.Start();
#endif
    }

    bool enableTUSLogging = true;

    public void ExecuteLogStartUp()
    {
        try
        {
            while (GamePad.activeGamePad == null)
            {

            }

            bool foundOnlineUser = false;

            Sony.NP.UserProfiles.LocalLoginUserId foundUserId = new Sony.NP.UserProfiles.LocalLoginUserId();

            Sony.NP.UserProfiles.LocalUsers users = new Sony.NP.UserProfiles.LocalUsers();
            try
            {
                Sony.NP.UserProfiles.GetLocalUsers(users);
            }
            catch (Sony.NP.NpToolkitException)
            {
                // This means that one or more of the user has an error code associated with them. This might mean they are not signed in or are not signed up to an online account.
            }

            for (int i = 0; i < users.LocalUsersIds.Length; i++)
            {
                Sony.NP.UserProfiles.LocalLoginUserId localUserId = users.LocalUsersIds[i];

                if (localUserId.UserId.Id != Sony.NP.Core.UserServiceUserId.UserIdInvalid &&
                    localUserId.SceErrorCode == (int)Sony.NP.Core.ReturnCodes.SUCCESS)
                {
                    try
                    {
                        // Local user is signed in
                        // Firstly check to make sure the user is signed in and allow to use 
                        Sony.NP.NpUtils.CheckAvailablityRequest caRequest = new Sony.NP.NpUtils.CheckAvailablityRequest();
                        caRequest.UserId = localUserId.UserId.Id;
                        caRequest.Async = false;
                        Sony.NP.Core.EmptyResponse caResponse = new Sony.NP.Core.EmptyResponse();

                        Sony.NP.NpUtils.CheckAvailablity(caRequest, caResponse);

                        if (caResponse.ReturnCode == Sony.NP.Core.ReturnCodes.SUCCESS)
                        {
                            foundOnlineUser = true;
                            foundUserId = localUserId;
                            break;
                        }
                    }
                    catch (Sony.NP.NpToolkitException)
                    {
                        OnScreenLog.AddError("An error occured while checking the user availability");
                    }
                }
            }

            if (foundOnlineUser == false)
            {
                enableTUSLogging = false;
                return;
            }

            Sony.NP.Tus.AddToAndGetVariableRequest request = new Sony.NP.Tus.AddToAndGetVariableRequest();

            Sony.NP.Tus.VirtualUserID id = new Sony.NP.Tus.VirtualUserID();
            id.Name = "_ERGVirtualUser1";
            request.TusUser = new Sony.NP.Tus.UserInput(id);

            Sony.NP.Tus.VariableInput var = new Sony.NP.Tus.VariableInput();
            var.Value = 1;
            var.SlotId = 6;

            request.Var = var;

            //Sony.NP.Tus.AtomicAddToAndGetVariableResponse response = new Sony.NP.Tus.AtomicAddToAndGetVariableResponse();
            Sony.NP.Tus.VariablesResponse response = new Sony.NP.Tus.VariablesResponse();

            request.UserId = foundUserId.UserId;

            request.Async = false;

            Sony.NP.Tus.AddToAndGetVariable(request, response);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    class LoggingCounts
    {
        public int[] serviceUsage = new int[(int)Sony.NP.ServiceTypes.Size];
        public int totalCount = 0;

        public void Reset()
        {
            totalCount = 0;
            for (int i = 0; i < serviceUsage.Length; i++)
            {
                serviceUsage[i] = 0;
            }
        }
    }

    LoggingCounts currentCounts = new LoggingCounts();
    LoggingCounts uploadCounts = new LoggingCounts();

    static int currentThreshold = 1;

    void LogService(Sony.NP.ServiceTypes service)
    {
#if ENABLE_TUS_LOGGING

        if (enableTUSLogging == false) return;

        if (service <= Sony.NP.ServiceTypes.Invalid) return;
        if (service > Sony.NP.ServiceTypes.Notification) return;

        // Don't log notifications - there are too many to count.
        if (service == Sony.NP.ServiceTypes.Notification) return;

        currentCounts.serviceUsage[(int)service]++;
        currentCounts.totalCount++;

        // If the total number of pending counts to upload has exceeded its threashold then send that data to the TUS server.
        // If any one total has exceeding its threadshold also send all the pending counts.
        if (currentCounts.totalCount > 20 || currentCounts.serviceUsage[(int)service] >= currentThreshold)
        {
            // Swap over the counts and upload them
            uploadCounts.Reset();
            LoggingCounts temp = currentCounts;
            currentCounts = uploadCounts;
            uploadCounts = temp;

            // Sony.NP.ServiceTypes starts at index 1 (Auth) as 0 = Invalid
            Thread thread = new Thread(new ThreadStart(ExecuteLogService));
            thread.Start();

            if (currentThreshold < 8)
            {
                currentThreshold *= 2;
            }
        }
#endif
    }

    public void ExecuteLogService()
    {
        // Taken from "Using the Title User Storage" documentation
        // Reference Count and Voting
        // By preparing a TUS variable that others can write to, and using sceNpTusAddAndGetVariableA(), you can see how many times you have been referenced.
        // The TUS variable of a virtual user can be counted up using sceNpTusAddAndGetVariableA() to realize a vote-casting mechanism.
        // If you use this mechanism, however, adopt a scheme on the application side where each user's number of references or votes can be limited. 
        // For example, make a recording when the save data or title user storage is used to make a reference or to cast a vote, 
        // and control the count or frequency of this operation. A specification that lacks this control will lead to excessive loads on the server 
        // and/or result in an unrealistic score. Make sure the necessary precautions are taken to avoid such situations.
        try
        {
            for (int i = 1; i < uploadCounts.serviceUsage.Length; i++)
            {
                if (uploadCounts.serviceUsage[i] > 0)
                {
                    Sony.NP.Tus.AddToAndGetVariableRequest request = new Sony.NP.Tus.AddToAndGetVariableRequest();

                    Sony.NP.Tus.VirtualUserID id = new Sony.NP.Tus.VirtualUserID();
                    id.Name = "_ERGVirtualUser1";
                    request.TusUser = new Sony.NP.Tus.UserInput(id);

                    Sony.NP.Tus.VariableInput var = new Sony.NP.Tus.VariableInput();
                    var.Value = uploadCounts.serviceUsage[i];
                    var.SlotId = i + 6;

                    request.Var = var;

                    //Sony.NP.Tus.AtomicAddToAndGetVariableResponse response = new Sony.NP.Tus.AtomicAddToAndGetVariableResponse();
                    Sony.NP.Tus.VariablesResponse response = new Sony.NP.Tus.VariablesResponse();

                    request.UserId = User.GetActiveUserId;

                    request.Async = false;

                    Sony.NP.Tus.AddToAndGetVariable(request, response);
                }
            }
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }
#endif

}
