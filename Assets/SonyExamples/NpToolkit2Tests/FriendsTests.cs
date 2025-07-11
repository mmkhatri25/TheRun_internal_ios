#if NPT2_FRIENDS_TESTS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using System;

namespace NpToolkitTests
{

    [TestFixture, Description("Friends tests")]
    public class FriendsTests : BaseTestFramework
    {
        private Sony.NP.Friends.FriendsResponse InternalRequestAllFriends(Sony.NP.Friends.FriendsRetrievalModes retrievalMode)
        {
            Sony.NP.Friends.FriendsResponse response = new Sony.NP.Friends.FriendsResponse();

            try
            {
                Sony.NP.Friends.GetFriendsRequest request = new Sony.NP.Friends.GetFriendsRequest();
                request.Mode = retrievalMode;
                request.Limit = 0;
                request.Offset = 0;
                request.Async = true;
                request.UserId = GetPrimaryUserId();
           
                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Friends.GetFriends(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            return response;
        }

        [UnityTest, Order(1), Description("Retrieve the current users friends list.")]
        public IEnumerator GetFriends([Values(Sony.NP.Friends.FriendsRetrievalModes.all, 
                                                 Sony.NP.Friends.FriendsRetrievalModes.online,
                                                 Sony.NP.Friends.FriendsRetrievalModes.tryCached)]Sony.NP.Friends.FriendsRetrievalModes retrievalMode)
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetFriends test started (" + retrievalMode + ")");

            Sony.NP.Friends.FriendsResponse response = InternalRequestAllFriends(retrievalMode);

            yield return new WaitUntil(() => response.Locked == false);

            OutputAsyncResponseEvent(response);

            // Response object is no longer locked so result should have been returned.
            Assert.AreEqual(Sony.NP.Core.ReturnCodes.SUCCESS, response.ReturnCode);

            Debug.Log("GetFriends - test passed");
        }

        [UnityTest, Order(2), Description("Retrieve friends of the current user first friend.")]
        public IEnumerator GetFriendsOfFriends()
        {
            yield return new WaitUntil(IsInitialized);
  
            Debug.Log("GetFriendsOfFriends test started");

            Sony.NP.Friends.FriendsResponse friendsResponse = InternalRequestAllFriends(Sony.NP.Friends.FriendsRetrievalModes.all);

            yield return new WaitUntil(() => friendsResponse.Locked == false);

            // Response object is no longer locked so result should have been returned.
            Assert.AreEqual(friendsResponse.ReturnCode, Sony.NP.Core.ReturnCodes.SUCCESS);

            Assert.Greater(friendsResponse.Friends.Length, 0);
      
            Sony.NP.Friends.FriendsOfFriendsResponse response = new Sony.NP.Friends.FriendsOfFriendsResponse();

            try
            {
                // Use the account ids to fill in the request
                Sony.NP.Friends.GetFriendsOfFriendsRequest request = new Sony.NP.Friends.GetFriendsOfFriendsRequest();
                request.UserId = GetPrimaryUserId();

                Sony.NP.Core.NpAccountId[] acountIds = new Sony.NP.Core.NpAccountId[1];
                acountIds[0] = friendsResponse.Friends[0].Profile.OnlineUser.AccountId;
                request.AccountIds = acountIds;

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Friends.GetFriendsOfFriends(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            yield return new WaitUntil(() => response.Locked == false);

            OutputAsyncResponseEvent(response);

            // Response object is no longer locked so result should have been returned.
            Assert.AreEqual(Sony.NP.Core.ReturnCodes.SUCCESS, response.ReturnCode);

            Debug.Log("GetFriendsOfFriends - test passed");
        }

        [UnityTest, Order(3), Description("Retrieve a list of blocked users.")]
        public IEnumerator RequestBlockedUsers()
        {
            yield return new WaitUntil(IsInitialized);
  
            Debug.Log("RequestBlockedUsers test started");

            Sony.NP.Friends.BlockedUsersResponse response = new Sony.NP.Friends.BlockedUsersResponse();

            try
            {
                Sony.NP.Friends.GetBlockedUsersRquest request = new Sony.NP.Friends.GetBlockedUsersRquest();
                request.Mode = Sony.NP.Friends.BlockedUsersRetrievalMode.all;
                request.Limit = 0;
                request.Offset = 0;
                request.Async = true;
                request.UserId = GetPrimaryUserId();
             
                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Friends.GetBlockedUsers(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            yield return new WaitUntil(() => response.Locked == false);

            OutputAsyncResponseEvent(response);

            // Response object is no longer locked so result should have been returned.
            Assert.AreEqual(Sony.NP.Core.ReturnCodes.SUCCESS, response.ReturnCode);

            Debug.Log("RequestBlockedUsers - test passed");
        }

        static UInt64 SPINT_ACCOUNT_ID = 1037116572376272627; // Taken from Sony NpToolkit sample - Stevehd

        [UnityTest, Order(4), Description("Display the friends request dialog")]
        public IEnumerator DisplayFriendRequestDialog()
        {
            yield return new WaitUntil(IsInitialized);
  
            Debug.Log("DisplayFriendRequestDialog test started");

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = -1;

            try
            {
                Sony.NP.Friends.DisplayFriendRequestDialogRequest request = new Sony.NP.Friends.DisplayFriendRequestDialogRequest();
                request.TargetUser = SPINT_ACCOUNT_ID;
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                requestId = Sony.NP.Friends.DisplayFriendRequestDialog(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            Debug.Log("Waiting 5 seconds : " + response.Locked);

            yield return new WaitForSeconds(5);

            Debug.Log("Closing dialog : " + response.Locked);

            // Now try closing the dialog
            bool result = Sony.NP.Main.AbortRequest((uint)requestId);

            Debug.Log("Closed dialog : " + result);
            //yield return new WaitUntil(() => response.Locked == false);

            //OutputAsyncResponseEvent(response);

            // Response object is no longer locked so result should have been returned.
            Debug.Log("Return code = " + response.ReturnCode + " : " + response.ReturnCodeValue.ToString("X8"));

            //DIALOG_RESULT_USER_CANCELED
            //Assert.AreEqual(Sony.NP.Core.ReturnCodes.SUCCESS, response.ReturnCode);

            Debug.Log("DisplayFriendRequestDialog - test passed");
        }
    } 

}
#endif


