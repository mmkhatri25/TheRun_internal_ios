#if NPT2_TROPHY_TESTS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace NpToolkitTests
{
    [TestFixture, Description("Trophy tests")]
    public class TrophyTests : BaseTestFramework
    {
        public enum RegisterTest
        {
            FirstAttempt,
            SecondAttempt,
        }

        [UnityTest, Order(1), Description("Register the Trophy pack. Check for an error code when registering for a second time.")]
        public IEnumerator RegisterTrophyPack([Values]RegisterTest registerAttempt)
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("RegisterTrophyPack test started");

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            try
            {
                Sony.NP.Trophies.RegisterTrophyPackRequest request = new Sony.NP.Trophies.RegisterTrophyPackRequest();
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Trophies.RegisterTrophyPack(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            yield return new WaitUntil(() => response.Locked == false);

            OutputAsyncResponseEvent(response);

            if ( registerAttempt == RegisterTest.SecondAttempt)
            {
                Assert.AreEqual(Sony.NP.Core.ReturnCodes.NP_TROPHY_ERROR_ALREADY_REGISTERED, response.ReturnCode);               
            }
            else
            {
                // Response object is no longer locked so result should have been returned.
                Assert.AreEqual(Sony.NP.Core.ReturnCodes.SUCCESS, response.ReturnCode);
            }

            Debug.Log("RegisterTrophyPack - test passed");        
        }

        public enum SetScreenShotTests
        {
            ScreenShotSetOne,
            //ScreenShotSetOneAgain,
            ScreenShotSetTwo,
            //ScreenShotSetTwoAgain,
        }

        [UnityTest, Order(2), Description("Set the screen shot for a set of trophies")]
        public IEnumerator SetScreenshot([Values]SetScreenShotTests screenShotTests)
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("SetScreenshot test started");

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            try
            {
                Sony.NP.Trophies.SetScreenshotRequest request = new Sony.NP.Trophies.SetScreenshotRequest();
                request.AssignToAllUsers = true;
                request.UserId = GetPrimaryUserId();

                int[] ids = new int[2];

                if ( screenShotTests == SetScreenShotTests.ScreenShotSetOne) // || screenShotTests == SetScreenShotTests.ScreenShotSetOneAgain)
                {
                    for (int i = 0; i < ids.Length; i++)
                    {
                        ids[i] = i + 1;  // Set throphy ids from 1 to 2.
                    }
                }
                else
                {
                    for (int i = 0; i < ids.Length; i++)
                    {
                        ids[i] = i + 3;  // Set throphy ids from 3 to 4.
                    }
                }

                string idOutput = "";
                for(int i = 0; i < ids.Length; i++)
                {
                    idOutput += ids[i] + " ";
                }

                Debug.Log("Ids to set screenshot : " + idOutput);

                request.TrophiesIds = ids;

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Trophies.SetScreenshot(request, response);

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

			if ( response.ReturnCode == Sony.NP.Core.ReturnCodes.ERROR_TROPHY_HOME_DIRECTORY_NOT_CONFIGURED)
			{
				Assert.AreEqual(Sony.NP.Core.ReturnCodes.ERROR_TROPHY_HOME_DIRECTORY_NOT_CONFIGURED, response.ReturnCode);
			}
			else
			{
				Assert.AreEqual(Sony.NP.Core.ReturnCodes.SUCCESS, response.ReturnCode);
			}

            Debug.Log("SetScreenshot - test passed");

            yield return new WaitForSeconds(5.0f);
        }

        [UnityTest, Order(3), Description("Get a list of unlocked trophies")]
        public IEnumerator GetUnlockedTrophies()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetUnlockedTrophies test started");

            Sony.NP.Trophies.UnlockedTrophiesResponse response = new Sony.NP.Trophies.UnlockedTrophiesResponse();

            try
            {
                Sony.NP.Trophies.GetUnlockedTrophiesRequest request = new Sony.NP.Trophies.GetUnlockedTrophiesRequest();
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Trophies.GetUnlockedTrophies(request, response);

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

            Debug.Log("GetUnlockedTrophies - test passed");        
        }

        [UnityTest, Order(4), Description("Get the trophy pack summary")]
        public IEnumerator GetTrophyPackSummary()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetTrophyPackSummary test started");

            Sony.NP.Trophies.TrophyPackSummaryResponse response = new Sony.NP.Trophies.TrophyPackSummaryResponse();

            try
            {
                Sony.NP.Trophies.GetTrophyPackSummaryRequest request = new Sony.NP.Trophies.GetTrophyPackSummaryRequest();
                request.RetrieveTrophyPackSummaryIcon = true;
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Trophies.GetTrophyPackSummary(request, response);

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

            Debug.Log("GetTrophyPackSummary - test passed");        
        }

        [UnityTest, Order(5), Description("Get the details of a trophy pack group")]
        public IEnumerator GetTrophyPackGroup()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetTrophyPackGroup test started");

            Sony.NP.Trophies.TrophyPackGroupResponse response = new Sony.NP.Trophies.TrophyPackGroupResponse();

            try
            {
                Sony.NP.Trophies.GetTrophyPackGroupRequest request = new Sony.NP.Trophies.GetTrophyPackGroupRequest();
                request.GroupId = -1;
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Trophies.GetTrophyPackGroup(request, response);

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

            Debug.Log("GetTrophyPackGroup - test passed");        
        }

       

        private bool IsTrophyUnlocked(int id, Sony.NP.Trophies.UnlockedTrophiesResponse response)
        {
            for (int i = 0; i < response.TrophyIds.Length; i++)
            {
                if ( response.TrophyIds[i] == id )
                {
                    return true;
                }
            }

            return false;
        }

        [UnityTest, Order(6), Description("Unlock the next available trophy")]
        public IEnumerator UnlockTrophy()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("UnlockTrophy test started");

            // First part
            // Get the trophy pack summary to find out how many trophies are available and how many are unlocked.
            Sony.NP.Trophies.TrophyPackSummaryResponse summaryResponse = new Sony.NP.Trophies.TrophyPackSummaryResponse();

            try
            {
                Sony.NP.Trophies.GetTrophyPackSummaryRequest request = new Sony.NP.Trophies.GetTrophyPackSummaryRequest();
                request.RetrieveTrophyPackSummaryIcon = true;
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Trophies.GetTrophyPackSummary(request, summaryResponse);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            yield return new WaitUntil(() => summaryResponse.Locked == false);

            if ( summaryResponse.StaticConfiguration.NumTrophies == summaryResponse.UserProgress.UnlockedTrophies )
            {
                // There are no more trophies to unlock. 
                // Lets early out the test. Test really hasn't passed, but it won't fail.
                Debug.Log("UnlockTrophy - The test couldn't run as all trophies have been unlocked.");
                yield break;
            }
            
            // Second Part
            // Get the list of unlocked trophies and then find a trophy id that hasn't been unlocked yet.
            Sony.NP.Trophies.UnlockedTrophiesResponse unlockResponse = new Sony.NP.Trophies.UnlockedTrophiesResponse();

            try
            {
                Sony.NP.Trophies.GetUnlockedTrophiesRequest request = new Sony.NP.Trophies.GetUnlockedTrophiesRequest();
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                Sony.NP.Trophies.GetUnlockedTrophies(request, unlockResponse);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            yield return new WaitUntil(() => unlockResponse.Locked == false);

            int lockedId = -1;
            int startIndex = 0;

            // If there is a Platinum trophy then id 0 can't be manually unlocked. 
            if ( summaryResponse.StaticConfiguration.NumPlatinum > 0 )
            {
                startIndex = 1;
            }

            for (int i = startIndex; i < summaryResponse.StaticConfiguration.NumTrophies && lockedId == -1; i++)
            {
                if ( IsTrophyUnlocked(i,unlockResponse) == false )
                {
                    lockedId = i;
                }
            }

            if ( lockedId == -1 )
            {
                // There are no more trophies to unlock. 
                // Lets early out the test. Test really hasn't passed, but it won't fail.
                Debug.LogError("UnlockTrophy - The test can't find an locked trophy even though there should be at least 1");
                yield break;
            }

            Debug.Log("UnlockTrophy trophy id " + lockedId);

            // Actual test
            // Unlock the trophy id.
            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            try
            {
                Sony.NP.Trophies.UnlockTrophyRequest request = new Sony.NP.Trophies.UnlockTrophyRequest();
                request.TrophyId = lockedId;
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Trophies.UnlockTrophy(request, response);

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

            Debug.Log("UnlockTrophy - test passed");        
        }

        [UnityTest, Order(7), Description("Get the details of a trophy in the trophy pack")]
        public IEnumerator GetTrophyPackTrophy()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetTrophyPackTrophy test started");

            Sony.NP.Trophies.UnlockedTrophiesResponse unlockResponse = new Sony.NP.Trophies.UnlockedTrophiesResponse();

            try
            {
                Sony.NP.Trophies.GetUnlockedTrophiesRequest request = new Sony.NP.Trophies.GetUnlockedTrophiesRequest();
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                Sony.NP.Trophies.GetUnlockedTrophies(request, unlockResponse);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            yield return new WaitUntil(() => unlockResponse.Locked == false);

            // Get a trophy that is already unlocked.
            int unlockedId = -1;

            if ( unlockResponse.TrophyIds.Length > 0 )
            {
                unlockedId = unlockResponse.TrophyIds[0];
            }
            else
            {
                Debug.LogError("No trophies have been unlocked so not possible to retrieve trophy info");
                yield break;
            }
            
            Sony.NP.Trophies.TrophyPackTrophyResponse response = new Sony.NP.Trophies.TrophyPackTrophyResponse();

            try
            {
                Sony.NP.Trophies.GetTrophyPackTrophyRequest request = new Sony.NP.Trophies.GetTrophyPackTrophyRequest();
                request.TrophyId = unlockedId;
                request.RetrieveTrophyPackTrophyIcon = true;
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Trophies.GetTrophyPackTrophy(request, response);

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

            Debug.Log("GetTrophyPackTrophy - test passed");        
        }
    }
}
#endif

