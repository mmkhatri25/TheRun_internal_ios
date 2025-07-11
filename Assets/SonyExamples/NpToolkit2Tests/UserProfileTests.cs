#if NPT2_USER_PROFILE_TESTS
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NpToolkitTests
{

    [TestFixture, Description("User profile tests")]
    public class UserProfileTests : BaseTestFramework
    {

        [UnityTest, Order(1), Description("Retrieve the current users profile from PSN.")]
        public IEnumerator GetNpProfile()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetNpProfile test started");

            Sony.NP.UserProfiles.NpProfilesResponse response = new Sony.NP.UserProfiles.NpProfilesResponse();

            try
            {
                Sony.NP.Core.NpAccountId accountId = GetLocalAccountId(GetPrimaryUserId().Id);
                
                Sony.NP.UserProfiles.GetNpProfilesRquest request = new Sony.NP.UserProfiles.GetNpProfilesRquest();
            
                request.UserId = GetPrimaryUserId();

                Sony.NP.Core.NpAccountId[] accountIds = new Sony.NP.Core.NpAccountId[1];
                accountIds[0] = accountId;

                request.AccountIds = accountIds;

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.UserProfiles.GetNpProfiles(request, response);

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

            Debug.Log("Retrieved user's profile - test passed.");
        }

        [UnityTest, Order(2), Description("Retrieve any verified accounts for the title.")]
        public IEnumerator GetVerifiedAccountsForTitle()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetVerifiedAccountsForTitle test started");

            Sony.NP.UserProfiles.NpProfilesResponse response = new Sony.NP.UserProfiles.NpProfilesResponse();

            try
            {
                // Use the account ids to fill in the request
                Sony.NP.UserProfiles.GetVerifiedAccountsForTitleRequest request = new Sony.NP.UserProfiles.GetVerifiedAccountsForTitleRequest();
                request.UserId = GetPrimaryUserId();

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.UserProfiles.GetVerifiedAccountsForTitle(request, response);

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

            Debug.Log("Retrieved verified account response - test passed");
        }
    }

}
#endif

