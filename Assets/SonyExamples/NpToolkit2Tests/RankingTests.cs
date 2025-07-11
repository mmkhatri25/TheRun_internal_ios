#if NPT2_RANKING_TESTS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace NpToolkitTests
{
    [TestFixture, Description("Ranking tests")]
    public class RankingTests : BaseTestFramework
    {
        static long previousScore = 0;

	    [UnityTest, Order(1), Description("Get the Local users ranks from a board.")]
        public IEnumerator GetUsersRanks()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetUsersRanks test started");

            Sony.NP.Ranking.UsersRanksResponse response = new Sony.NP.Ranking.UsersRanksResponse();

            try
            {
                Sony.NP.Ranking.GetUsersRanksRequest request = new Sony.NP.Ranking.GetUsersRanksRequest();

                request.UserId = GetPrimaryUserId();
                request.BoardId = 0;

                List<Sony.NP.Ranking.ScoreAccountIdPcId> scoreAccounts = new List<Sony.NP.Ranking.ScoreAccountIdPcId>();

                Sony.NP.Ranking.ScoreAccountIdPcId newId;

                newId.accountId = GetLocalAccountId(GetPrimaryUserId().Id);
                newId.pcId = Sony.NP.Ranking.MIN_PCID;

                scoreAccounts.Add(newId);

                // Assign the array of ids, to lookup on the Ranking server.
                request.Users = scoreAccounts.ToArray();

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Ranking.GetUsersRanks(request, response);

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

            // User may not have a score
            if ( response.Users.Length > 0 )
            {
                Debug.Log("BoardId = " + response.BoardId + " : Score " + response.Users[0].ScoreValue);

                previousScore = response.Users[0].ScoreValue;
            }

            Debug.Log("GetUsersRanks - test passed");        
        }

        [UnityTest, Order(2), Description("Set a higher score for the current user.")]
        public IEnumerator SetScore()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("SetScore test started");

            Sony.NP.Ranking.TempRankResponse response = new Sony.NP.Ranking.TempRankResponse();

            try
            {
                Sony.NP.Ranking.SetScoreRequest request = new Sony.NP.Ranking.SetScoreRequest();

                request.UserId = GetPrimaryUserId();
                request.Score = previousScore+100;
                request.Comment = "This is some test text.";

                Debug.Log("Setting a new score of " + request.Score);

                byte[] gameInfo = new byte[4];
                for (int i = 0; i < gameInfo.Length; i++)
                {
                    gameInfo[i] = (byte)i;
                }

                request.GameInfoData = gameInfo;            

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Ranking.SetScore(request, response);

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

			if ( response.ReturnCode == Sony.NP.Core.ReturnCodes.NP_COMMUNITY_SERVER_ERROR_NOT_BEST_SCORE)
			{			 
				Assert.AreEqual(Sony.NP.Core.ReturnCodes.NP_COMMUNITY_SERVER_ERROR_NOT_BEST_SCORE, response.ReturnCode);
			}
			else
			{			 
				Assert.AreEqual(Sony.NP.Core.ReturnCodes.SUCCESS, response.ReturnCode);
			}

            Debug.Log("TempRank = " + response.TempRank);
            
            Debug.Log("SetScore - test passed");        
        }

        [UnityTest, Order(3), Description("Get the range of ranks from a board")]
        public IEnumerator GetRangeOfRanks()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetRangeOfRanks test started");

            Sony.NP.Ranking.RangeOfRanksResponse response = new Sony.NP.Ranking.RangeOfRanksResponse();

            try
            {
                Sony.NP.Ranking.GetRangeOfRanksRequest request = new Sony.NP.Ranking.GetRangeOfRanksRequest();

                request.UserId = GetPrimaryUserId();
                request.BoardId = 0;
                request.Range = 40;
                request.StartRank = 1;

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Ranking.GetRangeOfRanks(request, response);

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

            Debug.Log("GetRangeOfRanks - test passed");        
        }

        [UnityTest, Order(4), Description("Get the range of ranks from a board for the current users friends")]
        public IEnumerator GetFriendsRanks()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetFriendsRanks test started");
         
            Sony.NP.Ranking.FriendsRanksResponse response = new Sony.NP.Ranking.FriendsRanksResponse();

            try
            {
                Sony.NP.Ranking.GetFriendsRanksRequest request = new Sony.NP.Ranking.GetFriendsRanksRequest();

                request.UserId = GetPrimaryUserId();
                request.BoardId = 0;
                request.StartRank = 1;

                // Make the async call which will return the Request Id 
                int requestId = Sony.NP.Ranking.GetFriendsRanks(request, response);

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

            Debug.Log("GetFriendsRanks - test passed");        
        }

    }
}
#endif
