#if NPT2_TSS_TESTS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace NpToolkitTests
{
    [TestFixture, Description("Tss tests")]
    public class TssTests : BaseTestFramework
    {
        [UnityTest, Order(1), Description("Test Tss Meta Data")]
        public IEnumerator GetTssData([Values]bool retrieveStatusOnly)
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("TssMetaData test started");

            Sony.NP.Tss.TssDataResponse response = new Sony.NP.Tss.TssDataResponse();

            try
            {
                Sony.NP.Tss.GetDataRequest request = new Sony.NP.Tss.GetDataRequest();

                request.TssSlotId = 0;
                request.RetrieveStatusOnly = retrieveStatusOnly;
                request.UserId = GetPrimaryUserId();

                int requestId = Sony.NP.Tss.GetData(request, response);

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

            Debug.Log("TssMetaData - test passed");
        }
    }
}
#endif


