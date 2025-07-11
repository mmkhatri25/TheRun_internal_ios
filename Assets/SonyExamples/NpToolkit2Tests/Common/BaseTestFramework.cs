#if NPT2_INIT_TESTS
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NpToolkitTests
{

    public class BaseTestFramework
    {
        //static public MonoBehaviour _mb; // The surrogate MonoBehaviour that we'll use to manage coroutines.
 
        [OneTimeSetUp]
        public void Setup()
        {
            //Debug.Log("Executing Tests");

            //SceneManager.LoadScene("NpToolkitSample", LoadSceneMode.Single);

            //Debug.Log("NpToolkit2 Tests Initialized.");
        }

        static bool initializationForTestComplete = false;
        //static bool initializationSucceeded = false;

        //static int attemptCount = 0;

        public bool IsInitialized()
        {
            if (initializationForTestComplete == true)
            {
                return true;
            }

            if ( Time.realtimeSinceStartup < 5.0f )
            {
                return false;
            }

            InitialisePlugin();

            FindUsers();

            //// Wait for NpToolkit scene to initialize NpToolkit2 correctly.
            //if ( SonyNpMain.initResult.Initialized == false )
            //{
            //    attemptCount++;
            //    if ( attemptCount > 1000)
            //    {
            //        // Too many frames have passed. Throw this as an error but allow any other tests waiting on ActiveUserAvailable()
            //        initializationForTestComplete = true;
            //        Debug.LogAssertion("NpToolkit2 hasn't been initialized in time.");
            //    }

            //    return false;
            //}

            // Make sure there is at least one active pad.
            //while(GamePad.activeGamePad == null)
            //{
            //    attemptCount++;

            //    if ( attemptCount > 1000)
            //    {
            //        // Too many frames have passed. Throw this as an error but allow any other tests waiting on ActiveUserAvailable()
            //        initializationForTestComplete = true;
            //        Debug.LogAssertion("No user has been signed into PSN, so many tests will fail.");
            //    }
            //}

            initializationForTestComplete = true;
           // initializationSucceeded = true;

            return true;
        }

        static public Sony.NP.InitResult initResult;
        static Sony.NP.UserProfiles.LocalUsers users = new Sony.NP.UserProfiles.LocalUsers();
        static private int primaryUserId = 0;
    
        static public Sony.NP.Core.UserServiceUserId GetPrimaryUserId()
        {
            if (primaryUserId < 0)
            {
                return Sony.NP.Core.UserServiceUserId.UserIdInvalid;
            }

            return users.LocalUsersIds[primaryUserId].UserId;
        }

        static public Sony.NP.Core.NpAccountId GetLocalAccountId(int localUserId)
        {
            for (int i = 0; i < users.LocalUsersIds.Length; i++)
            {
                if (users.LocalUsersIds[i].UserId.Id == localUserId)
                {
                    return users.LocalUsersIds[i].AccountId;
                }
            }
           
            return 0;
        }

        private void InitialisePlugin()
        {
            Sony.NP.Main.OnAsyncEvent += OnAsyncEvent;

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

			init.SetPushNotificationsFlags(Sony.NP.PushNotificationsFlags.NewGameDataMessage | Sony.NP.PushNotificationsFlags.NewInGameMessage |
										Sony.NP.PushNotificationsFlags.NewInvitation | Sony.NP.PushNotificationsFlags.UpdateBlockedUsersList |
										Sony.NP.PushNotificationsFlags.UpdateFriendPresence | Sony.NP.PushNotificationsFlags.UpdateFriendsList);
            try
            {
                initResult = Sony.NP.Main.Initialize(init);

                if (initResult.Initialized == true)
                {
                    Debug.Log("NpToolkit Initialized ");
                    //Debug.Log("Plugin SDK Version : " + initResult.SceSDKVersion.ToString() );
                }
                else
                {
                    Debug.Log("NpToolkit not initialized ");
                }
            }
            catch (Sony.NP.NpToolkitException e)
            {
                 Debug.LogError("Exception During Initialization : " + e.ExtendedMessage);
            }
#if UNITY_EDITOR
            catch (System.DllNotFoundException e)
            {
                Debug.LogError("Missing DLL Expection : " + e.Message);
                Debug.LogError("The sample APP will not run in the editor.");
            }
#endif

        }

        static Dictionary<Sony.NP.ResponseBase, Sony.NP.NpCallbackEvent> callbackEvents = new Dictionary<Sony.NP.ResponseBase, Sony.NP.NpCallbackEvent>();
        static System.Object syncObj = new  System.Object();
        static private void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
        {
            //OnScreenLog.Add("Event: Service = (" + callbackEvent.Service + ") : API Called = (" + callbackEvent.ApiCalled + ") : Request Id = (" + callbackEvent.NpRequestId + ") : Calling User Id = (" + callbackEvent.UserId + ")");
            lock(syncObj)
            {
                callbackEvents.Add(callbackEvent.Response, callbackEvent);
            }
        }

        private void FindUsers()
        {
            try
            {
                Sony.NP.UserProfiles.GetLocalUsers(users);
            }
            catch (Sony.NP.NpToolkitException)
            {
                // This means that one or more of the user has an error code associated with them. This might mean they are not signed in or are not signed up to an online account.
            }

            primaryUserId = -1;
            // Use the account ids to fill in the request
            for (int i = 0; i < users.LocalUsersIds.Length && primaryUserId < 0; i++)
            {
                if (users.LocalUsersIds[i].UserId.Id != Sony.NP.Core.UserServiceUserId.UserIdInvalid &&
                    users.LocalUsersIds[i].AccountId.Id != 0)
                {
                    primaryUserId = i;
                }
            }

            Debug.Log("Find Users complete");
        }

        public void OutputAsyncResponseEvent(Sony.NP.ResponseBase response)
        {
            Sony.NP.NpCallbackEvent callbackEvent = null;

            lock(syncObj)
            {
                if ( callbackEvents.TryGetValue(response, out callbackEvent) == true )
                {
                    callbackEvents.Remove(response);
                }
            }

            if ( callbackEvent != null)
            {
                Debug.Log("Event: Service = (" + callbackEvent.Service + ") : API Called = (" + callbackEvent.ApiCalled + ") : Request Id = (" + callbackEvent.NpRequestId + ") : Calling User Id = (" + callbackEvent.UserId + ")");

                Debug.Log("Response : " + callbackEvent.Response.ConvertReturnCodeToString(callbackEvent.ApiCalled));

                if (callbackEvent.Response.HasServerError)
                {
                    string errorMsg = System.String.Format("Server Error : returnCode = (0x{0:X}) : webApiNextAvailableTime = ({1}) : httpStatusCode = ({2})", response.ReturnCode, response.ServerError.WebApiNextAvailableTime, response.ServerError.HttpStatusCode);
                    Debug.Log(errorMsg);

                    Debug.Log("Server Error : jsonData = " + response.ServerError.JsonData);
                }
            }
            else
            {
                 Debug.Log("No Response object found");
            }
        }

        public void LogException(System.Exception e)
        {
            if ( e is Sony.NP.NpToolkitException)
            {
                Sony.NP.NpToolkitException npe = e as Sony.NP.NpToolkitException;

                Debug.LogError("NpException : " + npe.ExtendedMessage);
            }
            else
            {
                Debug.LogError("Exception : " + e.Message);
            }
        }
    }
}
#endif