
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if UNITY_PS4
public class SonyNpActivityFeed : IScreen
{
    MenuLayout m_MenuActivityFeed;

    public SonyNpActivityFeed()
    {
        Initialize();
    }

    public MenuLayout GetMenu()
    {
        return m_MenuActivityFeed;
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
        m_MenuActivityFeed = new MenuLayout(this, 450, 20);
    }

    public void MenuUserProfiles(MenuStack menuStack)
    {

        m_MenuActivityFeed.Update();

        if (m_MenuActivityFeed.AddItem("Post In-Game Story", "Posts a custom feed containing an In-Game story and two buttons to website URLs"))
        {
            PostInGameStory();
        }

        if (m_MenuActivityFeed.AddItem("Post In-Game Start", "Posts a custom feed to launch the title and a button which will lauch the title with custom aguments."))
        {
            PostInGameStartStory();
        }

        if (m_MenuActivityFeed.AddItem("Get User Feed", "Retrieves feeds from current user."))
        {
            GetFeed(Sony.NP.ActivityFeed.FeedType.UserFeed);
        }

        if (m_MenuActivityFeed.AddItem("Get User News", "Retrieves news from current user."))
        {
            GetFeed(Sony.NP.ActivityFeed.FeedType.UserNews);
        }

        if (m_MenuActivityFeed.AddItem("Get Title Feed", "Retrieves feeds posted by all users."))
        {
            GetFeed(Sony.NP.ActivityFeed.FeedType.TitleFeed);
        }

        if (m_MenuActivityFeed.AddItem("Get Title News", "Retrieves news posted by all users."))
        {
            GetFeed(Sony.NP.ActivityFeed.FeedType.TitleNews);
        }

        if (m_MenuActivityFeed.AddItem("Set Liked", "Marks the first retrieved Story as liked", firstStory != null))
        {
            SetLiked();
        }

        if (m_MenuActivityFeed.AddItem("Get Who Liked", "Get a list of users who liked the the first retrieved Story", firstStory != null))
        {
            GetWhoLiked();
        }

        if (m_MenuActivityFeed.AddItem("Post Played With", "Sets a list of users as 'Players Met' in the system. must have at least 2 user logged in", User.users != null && User.users.Length > 1))
        {
            PostPlayedWith();
        }

        if (m_MenuActivityFeed.AddItem("Get Played With", "Gets a list of users from the 'Players Met' feeds."))
        {
            GetPlayedWith();
        }

        if (m_MenuActivityFeed.AddItem("Get Shared Videos", "Gets system feeds of videos shared by the user."))
        {
            GetSharedVideos();
        }

        if (m_MenuActivityFeed.AddBackIndex("Back"))
        {
            menuStack.PopMenu();
        }
    }

    public void PostInGameStory()
    {
        try
        {
            Sony.NP.ActivityFeed.PostInGameStoryRequest request = new Sony.NP.ActivityFeed.PostInGameStoryRequest();

            request.UserId = User.GetActiveUserId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            Sony.NP.ActivityFeed.Media media = new Sony.NP.ActivityFeed.Media();

            media.LargeImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/7f/Wikipedia-logo-en.png";

            request.Media = media;

            request.UserComment = "This is an in game post test comment from Unity on frame " + OnScreenLog.FrameCount;
            request.SubType = 0;

            Sony.NP.ActivityFeed.Caption[] captions = new Sony.NP.ActivityFeed.Caption[1];

            captions[0].LanguageCode = "en";
            captions[0].CaptionText = "$ONLINE_ID did an in game post from $TITLE_NAME on frame " + OnScreenLog.FrameCount;

            request.Captions = captions;

            Sony.NP.ActivityFeed.Caption[] condCaptions = new Sony.NP.ActivityFeed.Caption[1];

            condCaptions[0].LanguageCode = "en";
            condCaptions[0].CaptionText = "Condensed caption - frame " + OnScreenLog.FrameCount;

            request.CondensedCaptions = condCaptions;

            Sony.NP.ActivityFeed.Action action1 = new Sony.NP.ActivityFeed.Action();
            Sony.NP.ActivityFeed.ButtonCaption[] captions1 = new Sony.NP.ActivityFeed.ButtonCaption[1];

            action1.ActionType = Sony.NP.ActivityFeed.ActionType.Url;
            action1.Uri = "https://en.wikipedia.org/wiki/Main_Page";
            captions1[0].LanguageCode = "en";
            captions1[0].Text = "Wiki Btn";
            action1.ButtonCaptions = captions1;

            Sony.NP.ActivityFeed.Action action2 = new Sony.NP.ActivityFeed.Action();
            Sony.NP.ActivityFeed.ButtonCaption[] captions2 = new Sony.NP.ActivityFeed.ButtonCaption[1];

            action2.ActionType = Sony.NP.ActivityFeed.ActionType.Url;
            action2.Uri = "https://www.youtube.com/";
            captions2[0].LanguageCode = "en";
            captions2[0].Text = "YouTube Btn";
            action2.ButtonCaptions = captions2;

            Sony.NP.ActivityFeed.Action[] actions = new Sony.NP.ActivityFeed.Action[2];

            actions[0] = action1;
            actions[1] = action2;

            request.Actions = actions;

            int requestId = Sony.NP.ActivityFeed.PostInGameStory(request, response);
            OnScreenLog.Add("Post InGame Story Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void PostInGameStartStory()
    {
        try
        {
            Sony.NP.ActivityFeed.PostInGameStoryRequest request = new Sony.NP.ActivityFeed.PostInGameStoryRequest();

            request.UserId = User.GetActiveUserId;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            Sony.NP.ActivityFeed.Media media = new Sony.NP.ActivityFeed.Media();

            media.LargeImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/7f/Wikipedia-logo-en.png";

            request.Media = media;

            request.UserComment = "This is a start game post test comment from Unity on frame " + OnScreenLog.FrameCount;
            request.SubType = 0;

            Sony.NP.ActivityFeed.Caption[] captions = new Sony.NP.ActivityFeed.Caption[1];

            captions[0].LanguageCode = "en";
            captions[0].CaptionText = "$ONLINE_ID did an in game post from $TITLE_NAME on frame " + OnScreenLog.FrameCount;

            request.Captions = captions;

            Sony.NP.ActivityFeed.Caption[] condCaptions = new Sony.NP.ActivityFeed.Caption[1];

            condCaptions[0].LanguageCode = "en";
            condCaptions[0].CaptionText = "Condensed caption - frame " + OnScreenLog.FrameCount;

            request.CondensedCaptions = condCaptions;

            Sony.NP.ActivityFeed.Action action1 = new Sony.NP.ActivityFeed.Action();
            Sony.NP.ActivityFeed.ButtonCaption[] captions1 = new Sony.NP.ActivityFeed.ButtonCaption[1];

            action1.ActionType = Sony.NP.ActivityFeed.ActionType.StartGame;
            action1.StartGameArguments = "Launch_Args_Frame_Num=" + OnScreenLog.FrameCount;
            captions1[0].LanguageCode = "en";
            captions1[0].Text = "Start With Args";
            action1.ButtonCaptions = captions1;

            Sony.NP.ActivityFeed.Action[] actions = new Sony.NP.ActivityFeed.Action[1];

            actions[0] = action1;

            request.Actions = actions;

            int requestId = Sony.NP.ActivityFeed.PostInGameStory(request, response);
            OnScreenLog.Add("Post InGame Story Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    static Sony.NP.ActivityFeed.Story firstStory;

    public void GetFeed(Sony.NP.ActivityFeed.FeedType feedType)
    {
      
        try
        {
            Sony.NP.ActivityFeed.GetFeedRequest request = new Sony.NP.ActivityFeed.GetFeedRequest();

            Sony.NP.Core.NpAccountId accountId = SonyNpUserProfiles.GetLocalAccountId(User.GetActiveUserId);

            request.UserId = User.GetActiveUserId;

            request.User = accountId;
            request.FeedType = feedType;

            OnScreenLog.Add("Post Feed Type : " + feedType);
            OnScreenLog.Add("Post Account Id : " + accountId.ToString());

            Sony.NP.ActivityFeed.FeedResponse response = new Sony.NP.ActivityFeed.FeedResponse();

            int requestId = Sony.NP.ActivityFeed.GetFeed(request, response);
            OnScreenLog.Add("Get Feed Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    void SetLiked()
    {
        try
        {
            Sony.NP.ActivityFeed.SetLikedRequest request = new Sony.NP.ActivityFeed.SetLikedRequest();

            request.UserId = User.GetActiveUserId;

            request.StoryId = firstStory.StoryId;
            request.IsLiked = true;

            OnScreenLog.Add("Story Id = " + firstStory.StoryId.Id);
            OnScreenLog.Add("Story Comment = " + firstStory.UserComment);

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.ActivityFeed.SetLiked(request, response);
            OnScreenLog.Add("Set Liked Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    void GetWhoLiked()
    {
        try
        {
            Sony.NP.ActivityFeed.GetWhoLikedRequest request = new Sony.NP.ActivityFeed.GetWhoLikedRequest();

            request.UserId = User.GetActiveUserId;

            request.StoryId = firstStory.StoryId;

            OnScreenLog.Add("Story Id = " + firstStory.StoryId.Id);
            OnScreenLog.Add("Story Comment = " + firstStory.UserComment);

            Sony.NP.ActivityFeed.UsersWhoLikedResponse response = new Sony.NP.ActivityFeed.UsersWhoLikedResponse();

            int requestId = Sony.NP.ActivityFeed.GetWhoLiked(request, response);
            OnScreenLog.Add("Get Who Liked Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    void PostPlayedWith()
    {
        // Fetch the local user to account id mapping.
        Sony.NP.UserProfiles.LocalUsers users = new Sony.NP.UserProfiles.LocalUsers();
        try
        {
            Sony.NP.UserProfiles.GetLocalUsers(users);
        }
        catch (Sony.NP.NpToolkitException)
        {
            // This means that one or more of the user has an error code associated with them. This might mean they are not signed in or are not signed up to an online account.
        }

        if (users.LocalUsersIds.Length < 2) return;

        int activeUserId = User.GetActiveUserId;

        List<Sony.NP.Core.NpAccountId> playedWith = new List<Sony.NP.Core.NpAccountId>();

        for (int i = 0; i < users.LocalUsersIds.Length; i++)
        {
            if(users.LocalUsersIds[i].UserId.Id != activeUserId && users.LocalUsersIds[i].UserId.Id != Sony.NP.Core.UserServiceUserId.UserIdInvalid)
            {
                playedWith.Add(users.LocalUsersIds[i].AccountId);
            }
        }

        try
        {
            Sony.NP.ActivityFeed.PostPlayedWithRequest request = new Sony.NP.ActivityFeed.PostPlayedWithRequest();

            request.UserId = activeUserId;
            request.UserIds = playedWith.ToArray();
            request.PlayedWithDescription = "Played NpToolkit2 together on frame " + OnScreenLog.FrameCount;

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            int requestId = Sony.NP.ActivityFeed.PostPlayedWith(request, response);
            OnScreenLog.Add("Post Played With Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    void GetPlayedWith()
    {
        try
        {
            Sony.NP.ActivityFeed.GetPlayedWithRequest request = new Sony.NP.ActivityFeed.GetPlayedWithRequest();

            request.UserId = User.GetActiveUserId;

            Sony.NP.ActivityFeed.PlayedWithFeedResponse response = new Sony.NP.ActivityFeed.PlayedWithFeedResponse();

            int requestId = Sony.NP.ActivityFeed.GetPlayedWith(request, response);
            OnScreenLog.Add("Get Played With Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    void GetSharedVideos()
    {
        try
        {
            Sony.NP.ActivityFeed.GetSharedVideosRequest request = new Sony.NP.ActivityFeed.GetSharedVideosRequest();

            request.UserId = User.GetActiveUserId;
            request.User = SonyNpUserProfiles.GetLocalAccountId(User.GetActiveUserId);

            Sony.NP.ActivityFeed.SharedVideosResponse response = new Sony.NP.ActivityFeed.SharedVideosResponse();

            int requestId = Sony.NP.ActivityFeed.GetSharedVideos(request, response);
            OnScreenLog.Add("Get Shared Videos Async : Request Id = " + requestId);
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }
    }

    public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.Service == Sony.NP.ServiceTypes.ActivityFeed)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.ActivityFeedPostInGameStory:
                    OutputPostInGameStory(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.ActivityFeedGetFeed:
                    OutputFeed(callbackEvent.Response as Sony.NP.ActivityFeed.FeedResponse);
                    break;
                case Sony.NP.FunctionTypes.ActivityFeedSetLiked:
                    OutputSetLiked(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.ActivityFeedGetWhoLiked:
                    OutputGetWhoLiked(callbackEvent.Response as Sony.NP.ActivityFeed.UsersWhoLikedResponse);
                    break;
                case Sony.NP.FunctionTypes.ActivityFeedPostPlayedWith:
                    OutputPostPlayedWith(callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                    break;
                case Sony.NP.FunctionTypes.ActivityFeedGetPlayedWith:
                    OutputPlayedWithFeed(callbackEvent.Response as Sony.NP.ActivityFeed.PlayedWithFeedResponse);
                    break;
                case Sony.NP.FunctionTypes.ActivityFeedGetSharedVideos:
                    OutputSharedVideos(callbackEvent.Response as Sony.NP.ActivityFeed.SharedVideosResponse);
                    break;
                default:
                    break;
            }
        }

        // Notifications
        if (callbackEvent.Service == Sony.NP.ServiceTypes.Notification)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.NotificationLaunchAppEvent:
                    {
                        Sony.NP.Main.LaunchAppEventResponse launchAppEventResponse = (Sony.NP.Main.LaunchAppEventResponse)callbackEvent.Response;

                        if (launchAppEventResponse.IsErrorCode == false)
                        {
                            OutputLaunchApp(launchAppEventResponse);
                        }
                    }
                    break;
            }
        }
    }

    private void OutputLaunchApp(Sony.NP.Main.LaunchAppEventResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Launch App Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("Args: " + response.Args);
        }
    }

    private void OutputPostInGameStory(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Post InGame Story Response");
    }

    private void OutputFeed(Sony.NP.ActivityFeed.FeedResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Feed Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("Num Stories: " + response.Stories.Length);

            if (response.Stories.Length > 0)
            {
                firstStory = response.Stories[0];

                for (int i = 0; i < response.Stories.Length; i++)
                {
                    OnScreenLog.Add("Story : " + i);

                    OutputStory(response.Stories[i]);

                    OnScreenLog.AddNewLine();
                }
            }
        }
    }

    private void OutputStory(Sony.NP.ActivityFeed.Story story)
    {
        OnScreenLog.Add("   Creation Data : " + story.CreationDate);
        OnScreenLog.Add("   User Comment : " + story.UserComment);

        OnScreenLog.Add("   Media : ");
        OnScreenLog.Add("      Large Image Url : " + story.Media.LargeImageUrl);
        OnScreenLog.Add("      Video URL : " + story.Media.VideoUrl);

        OnScreenLog.Add("   Story Id : " + story.StoryId.Id);

        OnScreenLog.Add("   Caption : " + story.Caption.CaptionText + " (" + story.Caption.LanguageCode + ")");

        OnScreenLog.Add("   Story Type : " + story.StoryType);

        OnScreenLog.Add("   SubType : " + story.SubType);

        OnScreenLog.Add("   Post Creator : ");
        OnScreenLog.Add("      Online User : " + story.PostCreator.User.ToString());
        OnScreenLog.Add("      Avatar Url : " + story.PostCreator.AvatarUrl);
        OnScreenLog.Add("      Real Name : " + GetRealName(story.PostCreator.RealName));

        OnScreenLog.Add("   Num Likes : " + story.NumLikes);
        OnScreenLog.Add("   Num Comments : " + story.NumComments);
        OnScreenLog.Add("   Is Reshareable : " + story.IsReshareable);
        OnScreenLog.Add("   Is Liked : " + story.IsLiked);
    }

    private string GetRealName(Sony.NP.Profiles.RealName rn)
    {
        return string.Format(" {0} {1} {2}\n", rn.FirstName, rn.MiddleName, rn.LastName);
    }

    private void OutputSetLiked(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Set Liked Response");
    }

    private void OutputGetWhoLiked(Sony.NP.ActivityFeed.UsersWhoLikedResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Get Who Like Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("Num Users: " + response.Users.Length);

            for (int i = 0; i < response.Users.Length; i++)
            {
                OnScreenLog.Add("Story User : " + i);
                OnScreenLog.Add("      Online User : " + response.Users[i].User.ToString());
                OnScreenLog.Add("      Avatar Url : " + response.Users[i].AvatarUrl);
                OnScreenLog.Add("      Real Name : " + GetRealName(response.Users[i].RealName));

                OnScreenLog.AddNewLine();
            }
        }
    }

    private void OutputPostPlayedWith(Sony.NP.Core.EmptyResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Post Played With Response");

        if (response.Locked == false)
        {

        }
    }

    private void OutputPlayedWithFeed(Sony.NP.ActivityFeed.PlayedWithFeedResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Played With Feed Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("Num Stories: " + response.Stories.Length);

            for (int i = 0; i < response.Stories.Length; i++)
            {
                OnScreenLog.Add("Story : " + i);
                OnScreenLog.Add("      Story Id : " + response.Stories[i].StoryId.Id);
                OnScreenLog.Add("      TitleName : " + response.Stories[i].TitleName);
                OnScreenLog.Add("      Date : " + response.Stories[i].Date);
                OnScreenLog.Add("      TitleId : " + response.Stories[i].TitleId.Id);
                OnScreenLog.Add("      PlayedWithDescription : " + response.Stories[i].PlayedWithDescription);

                OnScreenLog.Add("      Num Users: " + response.Stories[i].Users.Length);

                for (int u = 0; u < response.Stories[i].Users.Length; u++)
                {
                    OnScreenLog.Add("          Users: " + response.Stories[i].Users[u].User.ToString());
                    OnScreenLog.Add("          Avatar Url : " + response.Stories[i].Users[u].AvatarUrl);
                    OnScreenLog.Add("          Real Name : " + GetRealName(response.Stories[i].Users[u].RealName));
                }

                OnScreenLog.AddNewLine();
            }
        }
    }

    private void OutputSharedVideos(Sony.NP.ActivityFeed.SharedVideosResponse response)
    {
        if (response == null) return;

        OnScreenLog.Add("Shared Videos Response");

        if (response.Locked == false)
        {
            OnScreenLog.Add("Num Videos: " + response.Videos.Length);

            for (int i = 0; i < response.Videos.Length; i++)
            {
                Sony.NP.ActivityFeed.SharedVideo video = response.Videos[i];

                OnScreenLog.Add("Video : " + i);
                OnScreenLog.Add("      StoryId : " + video.StoryId.Id);
                OnScreenLog.Add("      Caption : " + video.Caption.CaptionText + " (" + video.Caption.LanguageCode + ")");

                Sony.NP.ActivityFeed.StoryUser storyUser = video.SourceCreator;

                OnScreenLog.Add("      StoryUser : ");
                OnScreenLog.Add("          User : " + storyUser.User.ToString());
                OnScreenLog.Add("          Avatar Url : " + storyUser.AvatarUrl);
                OnScreenLog.Add("          Real Name : " + GetRealName(storyUser.RealName));

                OnScreenLog.Add("      SNType : " + video.SNType);
                OnScreenLog.Add("      VideoId : " + video.VideoId);
                OnScreenLog.Add("      CreationDate : " + video.CreationDate);
                OnScreenLog.Add("      VideoDuration : " + video.VideoDuration);
                OnScreenLog.Add("      Comment : " + video.Comment);
            }
        }
    }

}
#endif
