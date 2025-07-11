#if NPT2_WORD_FILTER_TESTS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace NpToolkitTests
{
    [TestFixture, Description("Word Filter tests")]
    public class WordFilterTests : BaseTestFramework
    {
        [UnityTest, Order(1), Description("Test filtering a text string")]
        public IEnumerator FilterComment()
        {
            yield return new WaitUntil(IsInitialized);
  
            Debug.Log("FilterComment test started");

            Sony.NP.WordFilter.SanitizedCommentResponse response = new Sony.NP.WordFilter.SanitizedCommentResponse();

            try
            {
                Sony.NP.WordFilter.FilterCommentRequest request = new Sony.NP.WordFilter.FilterCommentRequest();

                request.Comment = "Hello, this is a fucking crap bullshit message that tests the vulgarity filter.";

                int requestId = Sony.NP.WordFilter.FilterComment(request, response);

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

            Assert.AreEqual(response.ResultComment, "Hello, this is a ******* crap ******** message that tests the vulgarity filter.");

            Debug.Log("FilterComment - test passed");
        }
	}
}
#endif

