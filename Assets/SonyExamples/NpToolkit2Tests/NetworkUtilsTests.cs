#if NPT2_NETWORK_UTILS_TESTS
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NpToolkitTests
{
    [TestFixture, Description("Network Utils tests")]
    public class NetworkUtilsTests : BaseTestFramework
    {
        [UnityTest, Order(1), Description("Get the network bandwidth info.")]
        public IEnumerator GetBandwidthInfo()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetBandwidthInfo test started");

            Sony.NP.NetworkUtils.BandwidthInfoResponse response = new Sony.NP.NetworkUtils.BandwidthInfoResponse();

            try
            {
                Sony.NP.NetworkUtils.GetBandwidthInfoRequest request = new Sony.NP.NetworkUtils.GetBandwidthInfoRequest();
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.NetworkUtils.GetBandwidthInfo(request, response);

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

            Debug.Log("GetBandwidthInfo - test passed.");
        }

        [UnityTest, Order(2), Description("Get the basic network info.")]
        public IEnumerator GetBasicNetworkInfo()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetBasicNetworkInfo test started");

             Sony.NP.NetworkUtils.BasicNetworkInfoResponse response = new Sony.NP.NetworkUtils.BasicNetworkInfoResponse();

            try
            {
                Sony.NP.NetworkUtils.GetBasicNetworkInfoRequest request = new Sony.NP.NetworkUtils.GetBasicNetworkInfoRequest();
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.NetworkUtils.GetBasicNetworkInfoInfo(request, response);

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

            Debug.Log("GetBasicNetworkInfo - test passed.");
        }

        [UnityTest, Order(3), Description("Get the detailed network info.")]
        public IEnumerator GetDetailedNetworkInfo()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetDetailedNetworkInfo test started");

            Sony.NP.NetworkUtils.DetailedNetworkInfoResponse response = new Sony.NP.NetworkUtils.DetailedNetworkInfoResponse();

            try
            {
                Sony.NP.NetworkUtils.GetDetailedNetworkInfoRequest request = new Sony.NP.NetworkUtils.GetDetailedNetworkInfoRequest();
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.NetworkUtils.GetDetailedNetworkInfo(request, response);

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

            Debug.Log("GetDetailedNetworkInfo - test passed.");
        }
    }
}
#endif

