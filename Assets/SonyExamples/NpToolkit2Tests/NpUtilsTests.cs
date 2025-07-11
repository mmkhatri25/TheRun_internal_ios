#if NPT2_NPUTILS_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NpToolkitTests
{
    [TestFixture, Description("NP Utils tests")]
    public class NpUtilsTests : BaseTestFramework
    {
        [UnityTest, Order(1), Description("Tests if the current user is allow PSN access (Synchronous and Asynchronous tests)")]
        public IEnumerator CheckAvailablity([Values]bool async)
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("CheckAvailablity test started");

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            try
            {
                Sony.NP.NpUtils.CheckAvailablityRequest request = new Sony.NP.NpUtils.CheckAvailablityRequest();
                request.UserId = GetPrimaryUserId();
                request.Async = async;

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.NpUtils.CheckAvailablity(request, response);

                if ( async == true )
                {
                    Assert.GreaterOrEqual(requestId, 0);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Call failed");
                LogException(e);
            }

            if ( async == true )
            {
                yield return new WaitUntil(() => response.Locked == false);

                OutputAsyncResponseEvent(response);
            }

            // Response object is no longer locked so result should have been returned.
            Assert.AreEqual(Sony.NP.Core.ReturnCodes.SUCCESS, response.ReturnCode);

            Debug.Log("CheckAvailablity - test passed.");
        }

        [UnityTest, Order(3), Description("Test if the calling user has a PSN account")]
        public IEnumerator CheckPlus()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("CheckPlus test started");

            Sony.NP.NpUtils.CheckPlusResponse response = new Sony.NP.NpUtils.CheckPlusResponse();

            try
            {
                Sony.NP.NpUtils.CheckPlusRequest request = new Sony.NP.NpUtils.CheckPlusRequest();
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.NpUtils.CheckPlus(request, response);

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

            Debug.Log("CheckPlus - test passed.");
        }

        [UnityTest, Order(4), Description("Tests if the current user is allow PSN access")]
        public IEnumerator GetParentalControlInfo()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetParentalControlInfo test started");

            Sony.NP.NpUtils.GetParentalControlInfoResponse response = new Sony.NP.NpUtils.GetParentalControlInfoResponse();

            try
            {
                Sony.NP.NpUtils.GetParentalControlInfoRequest request = new Sony.NP.NpUtils.GetParentalControlInfoRequest();
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.NpUtils.GetParentalControlInfo(request, response);

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

            Debug.Log("GetParentalControlInfo - test passed.");
        }

        [UnityTest, Order(5), Description("Tests notifing use of PSN features")]
        public IEnumerator NotifyPlusFeature()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("NotifyPlusFeature test started");

            try
            {
                Sony.NP.NpUtils.NotifyPlusFeature(GetPrimaryUserId());
            }
            catch (System.Exception e)
            {
                Debug.LogError("NotifyPlusFeature call failed");
                LogException(e);
            }

            Debug.Log("NotifyPlusFeature - test passed.");
        }
	}
	
}
#endif
