#if NPT2_PRESENCE_TESTS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace NpToolkitTests
{
    [TestFixture, Description("Presence tests")]
    public class PresenceTests : BaseTestFramework
    {
        [UnityTest, Order(1), Description("Test sending the current game status")]
        public IEnumerator SetPresence()
        {
            yield return new WaitUntil(IsInitialized);
  
            Debug.Log("SetPresence test started");

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            try
            {
                Sony.NP.Presence.SetPresenceRequest request = new Sony.NP.Presence.SetPresenceRequest();

                request.UserId = GetPrimaryUserId();

                request.DefaultGameStatus = "Game status set by the SetPresence test";

                Sony.NP.Presence.LocalizedGameStatus[] localizedGameStatuses = new Sony.NP.Presence.LocalizedGameStatus[2];

                localizedGameStatuses[0].LanguageCode = "fr";
                localizedGameStatuses[0].GameStatus = "French version : Game status set by the SetPresence test";

                localizedGameStatuses[1].LanguageCode = "de";
                localizedGameStatuses[1].GameStatus = "German version : Game status set by the SetPresence test";

                request.LocalizedGameStatuses = localizedGameStatuses;

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Presence.SetPresence(request, response);

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

            Debug.Log("SetPresence - test passed");
        }

        [UnityTest, Order(2), Description("Get the current game presence")]
        public IEnumerator GetPresence()
        {
            yield return new WaitUntil(IsInitialized);
  
            Debug.Log("GetPresence test started");

            Sony.NP.Presence.PresenceResponse response = new Sony.NP.Presence.PresenceResponse();

            try
            {
                Sony.NP.Presence.GetPresenceRequest request = new Sony.NP.Presence.GetPresenceRequest();
                request.UserId = GetPrimaryUserId();

                request.FromUser = GetLocalAccountId(GetPrimaryUserId().Id);

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Presence.GetPresence(request, response);

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

            for(int i = 0; i < response.UserPresence.Platforms.Length; i++)
            {
                Debug.Log(response.UserPresence.Platforms[i].GameStatus);
            }

            Debug.Log("GetPresence - test passed");
        }

        [UnityTest, Order(3), Description("Delete the current presence data")]
        public IEnumerator DeletePresence()
        {
            yield return new WaitUntil(IsInitialized);
  
            Debug.Log("DeletePresence test started");

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            try
            {
                Sony.NP.Presence.DeletePresenceRequest request = new Sony.NP.Presence.DeletePresenceRequest();

                request.UserId = GetPrimaryUserId();         

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Presence.DeletePresence(request, response);

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

            Debug.Log("DeletePresence - test passed");
        }
    }
    
}
#endif

