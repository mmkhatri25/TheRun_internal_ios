using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

#if UNITY_PS4
public class SonyNpRanking : IScreen
{
    MenuLayout m_MenuRanking;

    Int64 nextScore = 100;

    public SonyNpRanking()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuRanking;
    }

    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void Process(MenuStack stack)
    {
        MenuUserProfiles(stack);
    }

    public void Initialize()
    {
        m_MenuRanking = new MenuLayout(this, 450, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {
        m_MenuRanking.Update();

        if (m_MenuRanking.AddItem("Set Score (" + nextScore + ")", "Set the score for the current user. When uploading a score it may take more than 10 minutes for the board to change. This includes any game data also related to the score. Use 'Get Users Ranks' to set the next score if one is already set."))
        {
            SetScore();
        }

        if (m_MenuRanking.AddItem("Get Range Of Ranks", "Get the range of ranks from a board."))
        {
            GetRangeOfRanks();
        }

        if (m_MenuRanking.AddItem("Get Friends Ranks", "Get the range of ranks from a board for the current users friends."))
        {
            GetFriendsRanks();
        }

        if (m_MenuRanking.AddItem("Get Users Ranks", "Get the Local users ranks from a board. Also sets the 'Score' value so it is greater than the current users score if they have one. "))
        {
            GetUsersRanks();
        }

        if (m_MenuRanking.AddItem("Set Game Data", "Set some game data to a board for the current user. This requires the user has already uploaded a score and the score set in this request matches that score."))
        {
            SetGameData();
        }

        if (m_MenuRanking.AddItem("Get Game Data", "Get the game data from a board for the current user."))
        {
            GetGameData();
        }

        if (m_MenuRanking.AddItem("Set Game Data In Chunks", "Starts a background thread and does multiple syncrhous calls to SetGameData, send the game data in chunks."))
        {
            SetGameDataInChuncks();
        }

        if (m_MenuRanking.AddItem("Get Game Data In Chunks", "Starts a background thread and does multiple syncrhous calls to GetGameData, to return the game data in chunks."))
        {
            GetGameDataInChuncks();
        }

        if (m_MenuRanking.AddItem("Increment Score", "Increment the score used by 'Set Score', 'Set Game Data' and 'Set Game Data In Chunks'."))
        {
            nextScore++;
            OnScreenLog.Add("Incremented Score : " + nextScore);
            OnScreenLog.AddNewLine();
        }

        if (m_MenuRanking.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    const int GameDataSize = 180;

    public byte[] GenerateRandomGameData()
    {
        Random rand = new Random();

        byte[] gameData = new byte[GameDataSize];

        rand.NextBytes(gameData);

        return gameData;
    }

    public void OutputData(byte[] data)
    {
        string output = "";
        string outputLine = "";
        for (int i = 0; i < data.Length; i++)
        {
            outputLine += data[i] + ", ";

            if (outputLine.Length > 160)
            {
                output += outputLine + "\n      ";
                outputLine = "";
            }
        }

        output += outputLine;

        OnScreenLog.Add(output, true);
        OnScreenLog.AddNewLine();
    }

    public void SetScore()
    {
        try
        {
            Sony.NP.Ranking.SetScoreRequest request = new Sony.NP.Ranking.SetScoreRequest();

            request.UserId = User.GetActiveUserId;
            request.Score = nextScore;
            request.Comment = "This is some test text.";

            byte[] gameInfo = new byte[4];
            for (int i = 0; i < gameInfo.Length; i++)
            {
                gameInfo[i] = (byte)i;
            }

            request.GameInfoData = gameInfo;

            Sony.NP.Ranking.TempRankResponse response = new Sony.NP.Ranking.TempRankResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Ranking.SetScore(request, response);
            OnScreenLog.Add("SetScore Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetRangeOfRanks()
    {
        try
        {
            Sony.NP.Ranking.GetRangeOfRanksRequest request = new Sony.NP.Ranking.GetRangeOfRanksRequest();

            request.UserId = User.GetActiveUserId;
            request.BoardId = 0;
            request.Range = 40;
            request.StartRank = 1;

            Sony.NP.Ranking.RangeOfRanksResponse response = new Sony.NP.Ranking.RangeOfRanksResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Ranking.GetRangeOfRanks(request, response);
            OnScreenLog.Add("GetRangeOfRanks Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetFriendsRanks()
    {
        try
        {
            Sony.NP.Ranking.GetFriendsRanksRequest request = new Sony.NP.Ranking.GetFriendsRanksRequest();

            request.UserId = User.GetActiveUserId;
            request.BoardId = 0;
            request.StartRank = 1;

            Sony.NP.Ranking.FriendsRanksResponse response = new Sony.NP.Ranking.FriendsRanksResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Ranking.GetFriendsRanks(request, response);
            OnScreenLog.Add("GetFriendsRanks Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetUsersRanks()
    {
        // Build the array of users by just using the local user account ids.
        Sony.NP.UserProfiles.LocalUsers users = new Sony.NP.UserProfiles.LocalUsers();
        try
        {
            Sony.NP.UserProfiles.GetLocalUsers(users);
        }
        catch (Sony.NP.NpToolkitException)
        {
            // This means that one or more of the user has an error code associated with them. This might mean they are not signed in or are not signed up to an online account.
        }

        try
        {
            Sony.NP.Ranking.GetUsersRanksRequest request = new Sony.NP.Ranking.GetUsersRanksRequest();

            request.UserId = User.GetActiveUserId;
            request.BoardId = 0;

            List<Sony.NP.Ranking.ScoreAccountIdPcId> scoreAccounts = new List<Sony.NP.Ranking.ScoreAccountIdPcId>();

            for (int i = 0; i < users.LocalUsersIds.Length; i++)
            {
                if (users.LocalUsersIds[i].UserId.Id != Sony.NP.Core.UserServiceUserId.UserIdInvalid &&
                    users.LocalUsersIds[i].AccountId.Id != 0)
                {
                    Sony.NP.Ranking.ScoreAccountIdPcId newId;

                    newId.accountId = users.LocalUsersIds[i].AccountId;
                    newId.pcId = Sony.NP.Ranking.MIN_PCID;

                    scoreAccounts.Add(newId);
                }
            }

            // Assign the array of ids, to lookup on the Ranking server.
            request.Users = scoreAccounts.ToArray();

            Sony.NP.Ranking.UsersRanksResponse response = new Sony.NP.Ranking.UsersRanksResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Ranking.GetUsersRanks(request, response);
            OnScreenLog.Add("GetUsersRanks Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SetGameData()
    {
        try
        {
            Sony.NP.Ranking.SetGameDataRequest request = new Sony.NP.Ranking.SetGameDataRequest();

            request.UserId = User.GetActiveUserId;

            request.BoardId = 0;
            request.IdOfPrevChunk = 0;
            request.Score = nextScore;
            request.PcId = 0;

            byte[] gameData = GenerateRandomGameData();

            OnScreenLog.Add("Random Data to Set:");
            OutputData(gameData);

            request.SetDataChunk(gameData, 0, (UInt64)gameData.Length, (UInt64)gameData.Length);

            Sony.NP.Ranking.SetGameDataResultResponse response = new Sony.NP.Ranking.SetGameDataResultResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Ranking.SetGameData(request, response);
            OnScreenLog.Add("SetGameData Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetGameData()
    {
        try
        {
            Sony.NP.Core.NpAccountId accountId = SonyNpUserProfiles.GetLocalAccountId(User.GetActiveUserId);

            if (accountId.Id == 0)
            {
                OnScreenLog.AddError("Error: Can't find AccountId for current local user.");
                return;
            }

            Sony.NP.Ranking.GetGameDataRequest request = new Sony.NP.Ranking.GetGameDataRequest();

            request.UserId = User.GetActiveUserId;

            byte[] data = new byte[GameDataSize];

            request.BoardId = 0;
            request.IdOfPrevChunk = 0;
            request.AccountId = accountId;
            request.PcId = 0;

            request.SetRcvDataChunk(data, 0, (UInt64)data.Length);

            Sony.NP.Ranking.GetGameDataResultResponse response = new Sony.NP.Ranking.GetGameDataResultResponse();

            // Make the async call which will return the Request Id 
            int requestId = Sony.NP.Ranking.GetGameData(request, response);
            OnScreenLog.Add("GetGameData Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void SetGameDataInChuncks()
    {
        Thread thread = new Thread(new ThreadStart(DoSetGameDataInChuncks));
        thread.Start();
    }

    public void DoSetGameDataInChuncks()
    {
        try
        {
            Sony.NP.Ranking.SetGameDataRequest request = new Sony.NP.Ranking.SetGameDataRequest();
            request.Async = false;

            request.UserId = User.GetActiveUserId;

            request.BoardId = 0;
            request.IdOfPrevChunk = 0;
            request.Score = nextScore;
            request.PcId = 0;

            Sony.NP.Ranking.SetGameDataResultResponse response = new Sony.NP.Ranking.SetGameDataResultResponse();

            byte[] gameData = GenerateRandomGameData();
            OnScreenLog.Add("Random Data to Set:");
            OutputData(gameData);

            OnScreenLog.Add("SetGameDataInChuncks : Sending data");

            request.SetDataChunk(gameData, 0, 60, (UInt64)gameData.Length);
            Sony.NP.Ranking.SetGameData(request, response);

            OnScreenLog.Add("SetGameDataInChuncks : Sending chunk 2 - Prev Chunk id = " + response.ChunkId);
            request.IdOfPrevChunk = response.ChunkId;
            request.SetDataChunk(gameData, 60, 60, (UInt64)gameData.Length);
            Sony.NP.Ranking.SetGameData(request, response);

            OnScreenLog.Add("SetGameDataInChuncks : Sending chunk 3 - Prev Chunk id = " + response.ChunkId);
            request.IdOfPrevChunk = response.ChunkId;
            request.SetDataChunk(gameData, 120, 60, (UInt64)gameData.Length);
            Sony.NP.Ranking.SetGameData(request, response);

            OnScreenLog.Add("SetGameDataInChuncks : Sent all data");
            OnScreenLog.AddNewLine();
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void GetGameDataInChuncks()
    {
        Thread thread = new Thread(new ThreadStart(DoGetGameDataInChuncks));
        thread.Start();
    }

    public void DoGetGameDataInChuncks()
    {
        try
        {
            Sony.NP.Core.NpAccountId accountId = SonyNpUserProfiles.GetLocalAccountId(User.GetActiveUserId);

            if (accountId.Id == 0)
            {
                OnScreenLog.AddError("Error: Can't find AccountId for current local user.");
                return;
            }

            Sony.NP.Ranking.GetGameDataRequest request = new Sony.NP.Ranking.GetGameDataRequest();

            request.UserId = User.GetActiveUserId;

            byte[] data = new byte[GameDataSize];

            request.BoardId = 0;
            request.IdOfPrevChunk = 0;
            request.AccountId = accountId;
            request.PcId = 0;

            request.SetRcvDataChunk(data, 0, 60);

            request.Async = false;

            Sony.NP.Ranking.GetGameDataResultResponse response = new Sony.NP.Ranking.GetGameDataResultResponse();

            OnScreenLog.Add("GetGameDataInChuncks : Getting data");

            Sony.NP.Ranking.GetGameData(request, response);

            OutputData(data);

            OnScreenLog.Add("GetGameDataInChuncks : Getting chunk 2 - Prev Chunk id = " + response.ChunkId);
            request.IdOfPrevChunk = response.ChunkId;
            request.SetRcvDataChunk(data, 60, 60);
            Sony.NP.Ranking.GetGameData(request, response);

            OutputData(data);

            OnScreenLog.Add("GetGameDataInChuncks : Getting chunk 3 - Prev Chunk id = " + response.ChunkId);
            request.IdOfPrevChunk = response.ChunkId;
            request.SetRcvDataChunk(data, 120, 60);
            Sony.NP.Ranking.GetGameData(request, response);

            OutputData(data);

            OnScreenLog.Add("GetGameDataInChuncks : Received all data");
            OnScreenLog.AddNewLine();
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.Service == Sony.NP.ServiceTypes.Ranking)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.RankingSetScore:
                    OutputSetScore(callbackEvent.Response as Sony.NP.Ranking.TempRankResponse);
                    break;
                case Sony.NP.FunctionTypes.RankingGetRangeOfRanks:
                    OutputRangeOfRanks(callbackEvent.Response as Sony.NP.Ranking.RangeOfRanksResponse);
                    break;
                case Sony.NP.FunctionTypes.RankingGetFriendsRanks:
                    OutputFriendsRank(callbackEvent.Response as Sony.NP.Ranking.FriendsRanksResponse);
                    break;
                case Sony.NP.FunctionTypes.RankingGetUsersRanks:
                    OutputUsersRank(callbackEvent.Response as Sony.NP.Ranking.UsersRanksResponse, callbackEvent.UserId);
                    break;
                case Sony.NP.FunctionTypes.RankingSetGameData:
                    OutputSetGameDataResult(callbackEvent.Response as Sony.NP.Ranking.SetGameDataResultResponse);
                    break;
                case Sony.NP.FunctionTypes.RankingGetGameData:
                    OutputGetGameDataResult(callbackEvent.Response as Sony.NP.Ranking.GetGameDataResultResponse);
                    break;
                default:
                    break;
            }
        }
    }

    private void OutputSetScore(Sony.NP.Ranking.TempRankResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputSetScore Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("Temporary Rank : " + response.TempRank);
        }
    }

    private void OutputRangeOfRanks(Sony.NP.Ranking.RangeOfRanksResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputRangeOfRanks Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("IsCrossSaveInformation : " + response.IsCrossSaveInformation);
            OnScreenLog.Add("UpdateTime : " + response.UpdateTime);
            OnScreenLog.Add("TotalEntriesOnBoard : " + response.TotalEntriesOnBoard);
            OnScreenLog.Add("BoardId : " + response.BoardId);
            OnScreenLog.Add("NumValidEntries : " + response.NumValidEntries);
            OnScreenLog.Add("StartRank : " + response.StartRank);

            OnScreenLog.AddNewLine();
            OnScreenLog.Add("Ranking Info:");

            if (response.IsCrossSaveInformation == true)
            {
                if (response.RankDataForCrossSave != null)
                {
                    for (UInt32 i = 0; i < response.RankDataForCrossSave.Length; i++)
                    {
                        OutputScoreRankDataForCrossSave(response.RankDataForCrossSave[i]);
                    }
                }
            }
            else
            {
                if (response.RankData != null)
                {
                    for (UInt32 i = 0; i < response.RankData.Length; i++)
                    {
                        OutputScoreRankData(response.RankData[i]);
                    }
                }
            }
        }
    }

    private void OutputFriendsRank(Sony.NP.Ranking.FriendsRanksResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputFriendsRank Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("IsCrossSaveInformation : " + response.IsCrossSaveInformation);
            OnScreenLog.Add("NumFriends : " + response.NumFriends);
            OnScreenLog.Add("UpdateTime : " + response.UpdateTime);
            OnScreenLog.Add("TotalEntriesOnBoard : " + response.TotalEntriesOnBoard);
            OnScreenLog.Add("TotalFriendsOnBoard : " + response.TotalFriendsOnBoard);
            OnScreenLog.Add("BoardId : " + response.BoardId);
            OnScreenLog.Add("FriendsWithPcId : " + response.FriendsWithPcId);

            OnScreenLog.AddNewLine();
            OnScreenLog.Add("Ranking Info:");

            if (response.IsCrossSaveInformation == true)
            {
                for (UInt64 i = 0; i < response.NumFriends; i++)
                {
                    OutputScoreRankDataForCrossSave(response.RankDataForCrossSave[i]);
                }
            }
            else
            {
                for (UInt64 i = 0; i < response.NumFriends; i++)
                {
                    OutputScoreRankData(response.RankData[i]);
                }
            }
        }
    }

    private void OutputUsersRank(Sony.NP.Ranking.UsersRanksResponse response, Sony.NP.Core.UserServiceUserId userId)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputUsersRank Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("IsCrossSaveInformation : " + response.IsCrossSaveInformation);
            OnScreenLog.Add("NumUsers : " + response.NumUsers);
            OnScreenLog.Add("NumValidUsers : " + response.NumValidUsers);
            OnScreenLog.Add("UpdateTime : " + response.UpdateTime);
            OnScreenLog.Add("TotalEntriesOnBoard : " + response.TotalEntriesOnBoard);
            OnScreenLog.Add("BoardId : " + response.BoardId);

            OnScreenLog.AddNewLine();
            OnScreenLog.Add("Ranking Info:");

            if (response.IsCrossSaveInformation == true)
            {
                for (UInt64 i = 0; i < response.NumUsers; i++)
                {
                    OutputScorePlayerRankDataForCrossSave(response.UsersForCrossSave[i]);
                }
            }
            else
            {
                for (UInt64 i = 0; i < response.NumUsers; i++)
                {
                    OutputScorePlayerRankData(response.Users[i]);
                }
            }

            // Get the local users score and set that as the next score.
            Sony.NP.Core.NpAccountId accountId = SonyNpUserProfiles.GetLocalAccountId(userId.Id);

            if (response.IsCrossSaveInformation == false)
            {
                for (UInt64 i = 0; i < response.NumUsers; i++)
                {
                    if ( response.Users[i].HasData == true )
                    {
                        if (response.Users[i].AccountId == accountId)
                        {
                            nextScore = response.Users[i].ScoreValue + 1;
                        }
                    }
                }
            }
        }
    }

    private string OutputScoreRankDataBase(Sony.NP.Ranking.ScoreRankDataBase rankData)
    {
        string output = "";

        output += "        Comment : " + rankData.Comment;
        output += "\n        PcId : " + rankData.PcId;
        output += " SerialRank : " + rankData.SerialRank;
        output += " Rank : " + rankData.Rank;
        output += " HighestRank : " + rankData.HighestRank;
        output += " HasGameData : " + rankData.HasGameData;
        output += " ScoreValue : " + rankData.ScoreValue;
        output += "\n        RecordDate : " + rankData.RecordDate;
        output += " AccountId : " + rankData.AccountId;

        if ( rankData.GameInfo != null && rankData.GameInfo.Length > 0 )
        {
            output += "\n       ";

            for(int i = 0; i < rankData.GameInfo.Length; i++)
            {
                output += rankData.GameInfo[i] + ", ";
            }
        }

        return output;
    }

    private void OutputScoreRankDataForCrossSave(Sony.NP.Ranking.ScoreRankDataForCrossSave rankData)
    {
        string output = OutputScoreRankDataBase(rankData);

        OnScreenLog.Add("NpId : " + rankData.NpId.Handle);
        OnScreenLog.Add(output, true);
    }

    private void OutputScoreRankData(Sony.NP.Ranking.ScoreRankData rankData)
    {
        string output = OutputScoreRankDataBase(rankData);

        OnScreenLog.Add("OnlineId : " + rankData.OnlineId);
        OnScreenLog.Add(output, true);
    }

    private void OutputScorePlayerRankDataForCrossSave(Sony.NP.Ranking.ScorePlayerRankDataForCrossSave rankData)
    {
        if (rankData.HasData == false)
        {
            OnScreenLog.Add("Do Data for this Rank entry.");
        }
        else
        {
            OutputScoreRankDataForCrossSave(rankData);
        }
    }

    private void OutputScorePlayerRankData(Sony.NP.Ranking.ScorePlayerRankData rankData)
    {
        if (rankData.HasData == false)
        {
            OnScreenLog.Add("Do Data for this Rank entry.");
        }
        else
        {
            OutputScoreRankData(rankData);
        }
    }

    private void OutputSetGameDataResult(Sony.NP.Ranking.SetGameDataResultResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputSetGameDataResult Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("ChunkId : " + response.ChunkId);
        }
    }

    private void OutputGetGameDataResult(Sony.NP.Ranking.GetGameDataResultResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("OutputGetGameDataResult Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("TotalSize : " + response.TotalSize);
            OnScreenLog.Add("RcvDataSize : " + response.RcvDataSize);
            OnScreenLog.Add("RcvDataValidSize : " + response.RcvDataValidSize);
            OnScreenLog.Add("ChunkId : " + response.ChunkId);
            OnScreenLog.Add("StartIndex : " + response.StartIndex);

            UInt64 lastIndex = response.StartIndex + response.RcvDataValidSize;

            if (response.RcvData != null && response.RcvData.Length > 0)
            {
                string output = "";
                string outputLine = "";
                for (int i = 0; i < (int)lastIndex; i++)
                {
                    outputLine += response.RcvData[i] + ", ";

                    if (outputLine.Length > 160)
                    {
                        output += outputLine + "\n      ";
                        outputLine = "";
                    }
                }

                output += outputLine;

                OnScreenLog.Add("RcvData : " + output, true);
            }
            else
            {
                OnScreenLog.Add("RcvData : Null"); // This shouldn't happen
            }
        }
    }

}
#endif
