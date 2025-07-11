#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif


using Bayat.Json.Utilities;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using I2.Loc;
using NaughtyAttributes;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameVideoManager : MonoBehaviour
{
    public static GameVideoManager ins;

    [Header("Debugging")]
    public bool skipTesting;
    public TextMeshProUGUI debugText;
    public TextMeshProUGUI scoreAText;
    public TextMeshProUGUI scoreBText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI checkPointText;
    public TextMeshProUGUI vpTime;

    [Header("Scripts")]
    public CSVDataLoader csvData;
    public SubtitleDisplayer subtitleDisplayer;
    public SubtitleDisplayer subtitleDisplayerAlternate;
    private GameManager gameManager;
    public GameUiManager gameUiManager;
    public EndingManager endingManager;
    public SteamAchievementsManager steamAchievement;

    [Header("Video Component")]
    //public VideoPlayer videoPlayer;
    public VideoPlayer videoPlayerPrefab;
    public Transform videoPlayerPrefabParent;
    public AudioSource audioSource;

    [Header("GameObjects")]
    public GameObject buttonPanel;
    public GameObject loadingScreen;

    [Header("Buttons")]
    public Button skipButton;
    public GameObject creditScreenSkipPanel;
    public List<Button> optionButtons = new List<Button>();

    [Header("Text")]
    public TextMeshProUGUI speedText;
    public List<TextMeshProUGUI> optionText = new List<TextMeshProUGUI>();

    [Header("Choices and Timer")]
    public Sprite choice_pinkSprite;
    public Sprite choice_blueSprite;
    public Sprite choice_redSprite;
    public Image fillImageSprite;
    public Transform fillImage;
    public TimerPanelView timerPanelView;

    [Header("CanvasGroup")]
    public CanvasGroup choiceCanvasGroup;
    public CanvasGroup skipButtonCanvasGroup;


    //Private Variables
    [ShowNonSerializedField] private DataRead nextVideoData;
    [ShowNonSerializedField] private DataRead currentVideodata;
    private bool startTimer = false;
    private float choiceTime;
    private string startVideo = "seq_1.mp4";
    private int currentIndex = 0;

    double videoDuration = 500;

    [HideInInspector] public bool isPlaybackCompleted;
    private bool hasChoices;
    private bool userMadeChoice;
    private bool isEndCredit;
    private bool canSkipVideo;
    private bool containsSkipPoint;
    private bool isVideoSkipped;
    private bool shouldCheckScore;
    private bool showRestartScreen;

    private bool hasLocalisationAnimation;

    private bool isAchievementAvailable;
    private float achievementActivateTime;

    private int userChoiceIndex;
    private float showChoiceAtTime;
    private float timerFillAmount = 1f;
    private readonly float endCreditSkipButtonTimer = 60f;

    //Score System
    private float Score_A_time;
    private float Score_A_panel_waitTime;

    private float Score_B_time;
    private float Score_B_panel_waitTime;

    private string dependentStringCheck;

    [Space(20)]
    public List<DataRead> gameProgress = new();
    private readonly List<string> progressIndex = new();
    private readonly List<string> statsTreeIndex = new();

    private Action OnVideoPlayed;

    private Player player { get { return ReInput.players.GetPlayer(0); } }

    public int defaultVideoIndex = -1;

    private CanvasGroup locItem;
    private bool shouldAutoSkip;
    public TextMeshProUGUI autoSkipStatusText;

    private VideoPlayer videoPlayerA;
    private VideoPlayer videoPlayerB;

    public VideoPlayer currentPlayer;
    private VideoPlayer nextPlayer;

    private readonly List<Transform> originalOrder = new();

    private void Awake()
    {
        ins = this;
        SaveOriginalOrder();
        skipButton.onClick.AddListener(SkipVideo);

        //These all events are executed simultaneously 
        OnVideoPlayed += ShowSubtitles;//Display subtitles
        OnVideoPlayed += NewSceneFound;//Increment new scene found count
        OnVideoPlayed += CheckSkipAndEndVideo;//Enable Skip button to skip video that are already played
        OnVideoPlayed += CheckAchievementAvailability;//Check if there is any achievement for current video
        OnVideoPlayed += LocalizationAnimation;//Initialize video which contains sprite animation
        OnVideoPlayed += ScoreSystem;//Initialize score addition details...score,timings and XY position of the animation
        OnVideoPlayed += CheckCheckPoint;//Initialize score addition details...score,timings and XY position of the animation
        OnVideoPlayed += ActivateLocGraphic;//

        gameManager = FindObjectOfType<GameManager>();
        gameManager.isEndingDisplayed = false;

        LoadLifeTimeGameProgress();
    }

    private async UniTask PrepareVideo(DataRead dataRead)
    {
        float startTime = Time.realtimeSinceStartup;
        nextPlayer.url = dataRead.videoFileUrl;
        nextPlayer.Prepare();

        await UniTask.WaitUntil(() => nextPlayer.isPrepared);

        float endTime = Time.realtimeSinceStartup;
        float prepareTime = endTime - startTime;

        Debugg.Log($"{dataRead.ID} prepared in {prepareTime:F3} seconds");

        double duration = nextPlayer.length;
        if (duration <= 0) // Fallback if length is incorrect
            duration = nextPlayer.frameCount / nextPlayer.frameRate;

        dataRead.videoDuration = duration;
    }

    private async UniTask PopulateVideoPlayers()
    {
        videoPlayerA = Instantiate(videoPlayerPrefab, videoPlayerPrefabParent);
        videoPlayerB = Instantiate(videoPlayerPrefab, videoPlayerPrefabParent);

        currentPlayer = videoPlayerA;
        nextPlayer = videoPlayerB;

        currentPlayer.targetTexture.Release();

        videoPlayerA.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayerA.SetTargetAudioSource(0, audioSource);
        videoPlayerA.loopPointReached += EndReached;
        videoPlayerA.started += NextVideoManager;
        videoPlayerA.started += SkipToCheckPointTime;

        videoPlayerB.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayerB.SetTargetAudioSource(0, audioSource);
        videoPlayerB.loopPointReached += EndReached;
        videoPlayerB.started += NextVideoManager;
        videoPlayerB.started += SkipToCheckPointTime;

        SetDirectAudioVolume();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.AddSound(StaticStrings.GameVideo, audioSource);
        }

        await PrepareVideo(csvData.GetNextVideoData(startVideo));
    }

    public void SetDirectAudioVolume()
    {
        //float volume = GameManager.ins.savePrefs.VOLUME;
        //videoPlayerA.SetDirectAudioVolume(0, volume);
        //videoPlayerB.SetDirectAudioVolume(0, volume);
    }

    private void SkipToCheckPointTime(VideoPlayer source)
    {
        if (currentVideodata != null && gameManager.CHECKPOINT_VIDEO_TIME != 0)
        {
            currentPlayer.targetTexture.Release();
            currentPlayer.time = gameManager.CHECKPOINT_VIDEO_TIME;
            subtitleDisplayer.SetTimeFromCheckpoint((float)gameManager.CHECKPOINT_VIDEO_TIME);
            subtitleDisplayerAlternate.SetTimeFromCheckpoint((float)gameManager.CHECKPOINT_VIDEO_TIME);
            gameManager.CHECKPOINT_VIDEO_TIME = 0;
            containsSkipPoint = false;
        }
    }

    private void Start()
    {
        StartNow().Forget();
    }

    private async UniTask StartNow()
    {
        //Add listerner to the buttons for clicking the options buttons
        //Checking for any saved game and loading the data
        //Start the video from beginiing if no saved data is found

        optionButtons[0].onClick.AddListener(() => OnOptionClicked(0));
        optionButtons[1].onClick.AddListener(() => OnOptionClicked(1));
        optionButtons[2].onClick.AddListener(() => OnOptionClicked(2));
        optionButtons[3].onClick.AddListener(() => OnOptionClicked(3));
        optionButtons[4].onClick.AddListener(() => OnOptionClicked(4));
        optionButtons[5].onClick.AddListener(() => OnOptionClicked(5));

        CheckSavedGame();

        await PopulateVideoPlayers();

        await UniTask.Delay(TimeSpan.FromSeconds(1f), ignoreTimeScale: false);
        loadingScreen.SetActive(false);

        StartVideoPlayer(csvData.GetNextVideoData(startVideo));

        StartCoroutine(_ShowChoiceAtTime());
    }

    private async UniTask OnVideoStartedPlaying()
    {
        if (!gameProgress.Contains(currentVideodata))
            gameProgress.Add(currentVideodata);

        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));

        shouldCheckScore = true;

        OnVideoPlayed?.Invoke();
        TotalTimeOfVideo();
        SaveGameData();

        if (shouldAutoSkip)
        {
            DOVirtual.DelayedCall(0.8f, SkipVideo);
        }
    }

    private void Update()
    {
        if (skipTesting)
            canSkipVideo = true;

        if (gameUiManager.isGamePause)//Pause the time when games is paused
            return;

        SkipButtonManager();
        ScoreManager();
        AchievementManager();
        LocGraphicManager();

        if (startTimer)
        {
            Timer();
        }

        if (currentVideodata != null)
        {
            vpTime.text = "VP Time : " + currentPlayer.time;
        }
    }


    private void CheckAchievementAvailability()//Check if Current video has any achievement and set activation time
    {
#if  !DISABLESTEAMWORKS || UNITY_PS4 || UNITY_GAMECORE

        if (currentVideodata != null)
        {
            isAchievementAvailable = steamAchievement.IsAchievementAvailable(currentVideodata.ID);

            if (isAchievementAvailable)
                achievementActivateTime = steamAchievement.GetAchievementTime(currentVideodata.ID);
        }
#endif

    }

    private void CheckCheckPoint()
    {
        if (currentVideodata != null)
        {
            if (currentVideodata.CHECKPOINT)
            {
                PlayerPrefs.SetString(StaticStrings.Checkpoint, currentVideodata.ID);
                PlayerPrefs.SetFloat(StaticStrings.CheckpointTime, currentVideodata.CHECKPOINT_TIME);
                checkPointText.text = "Checkpoint = " + currentVideodata.ID;

                gameManager.SetInt(out gameManager.savePrefs.CHECKPOINT_SCORE_A, gameManager.SCORE_A);
                gameManager.SetInt(out gameManager.savePrefs.CHECKPOINT_SCORE_B, gameManager.SCORE_B);
            }
        }
    }

    private void ActivateLocGraphic()
    {
        if (currentVideodata != null)
        {
            hasLocalisationAnimation = currentVideodata.LOCALISATION_KEY.Count > 0;

            if (locItem != null)
            {
                locItem.alpha = 0;
            }
            locItem = null;
        }
    }

    private void ScoreSystem()
    {
        if (currentVideodata.SCORE_A != -1)
        {
            Score_A_time = float.Parse(currentVideodata.SCORE_A_ANI.Split(',')[0].Trim());
            Score_A_panel_waitTime = float.Parse(currentVideodata.SCORE_A_ANI.Split(',')[1].Trim());
        }

        if (currentVideodata.SCORE_B != -1)
        {
            Score_B_time = float.Parse(currentVideodata.SCORE_B_ANI.Split(',')[0].Trim());
            Score_B_panel_waitTime = float.Parse(currentVideodata.SCORE_B_ANI.Split(',')[1].Trim());
        }

    }
    private void ScoreManager()
    {
        if (currentVideodata == null || !shouldCheckScore)
            return;

        if (currentVideodata.SCORE_A != -1)
        {
            if ((float)currentPlayer.time >= Score_A_time)
            {
                if (currentVideodata.SCORE_A == 0)//Reset Player scores
                {
                    gameManager.SCORE_A = 0;
                    scoreAText.text = "Score_A = " + 0;
                }
                else
                {
                    int totalScoreA = gameManager.SCORE_A + currentVideodata.SCORE_A;
                    gameManager.SCORE_A = totalScoreA;
                    scoreAText.text = "Score_A = " + totalScoreA;
                }

                if (isVideoSkipped)
                {
                    //gameUiManager.gameScorePanel.SetScoresTextOnSkip();
                }
                else
                {
                    //gameUiManager.gameScorePanel.ShowHideScorePanel(gameManager.SCORE_A, -1, Score_A_panel_waitTime, currentVideodata.SCORE_B_GRAPHIC);
                }
                NextVideoManager(null);
                currentVideodata.SCORE_A = -1;
                currentVideodata.SCORE_A_ANI = string.Empty;

            }
        }
        else
        {
            //This just displays Score Panel without any score changes for certain duration
            if (!string.IsNullOrEmpty(currentVideodata.SCORE_A_ANI))
            {
                if ((float)currentPlayer.time >= Score_A_time)
                {
                    //gameUiManager.gameScorePanel.ShowHideScorePanel(-1, -1, Score_A_panel_waitTime, currentVideodata.SCORE_B_GRAPHIC);
                    Score_A_time = 5000;
                }
            }
        }

        if (currentVideodata.SCORE_B != -1)
        {
            if ((float)currentPlayer.time >= Score_B_time)
            {
                if (currentVideodata.SCORE_B == 0)//Reset Player scores
                {
                    gameManager.SCORE_B = 0;
                    scoreBText.text = "Score_B = " + 0;
                }
                else
                {
                    int totalScoreB = gameManager.SCORE_B + currentVideodata.SCORE_B;
                    gameManager.SCORE_B = totalScoreB;
                    scoreBText.text = "Score_B = " + totalScoreB;
                }

                if (isVideoSkipped)
                {
                    // gameUiManager.gameScorePanel.SetScoresTextOnSkip();
                }
                else
                {
                    // gameUiManager.gameScorePanel.ShowHideScorePanel(-1, gameManager.SCORE_B, Score_B_panel_waitTime, currentVideodata.SCORE_B_GRAPHIC);
                }
                NextVideoManager(null);
                currentVideodata.SCORE_B = -1;
            }
        }
    }

    private void LocGraphicManager()
    {
        if (!hasLocalisationAnimation)
        {
            return;
        }

        //LocalisationKeyManager objectToRemove = null;
    }

    private void AchievementManager() //Called Every Frame to activate achievement after certain duration in the Video
    {
#if !DISABLESTEAMWORKS || UNITY_PS4 || UNITY_GAMECORE

        if (isAchievementAvailable)
        {
            if ((float)currentPlayer.time >= achievementActivateTime)
            {
                steamAchievement.CheckForAchievement(currentVideodata);
                isAchievementAvailable = false;
            }
        }

#endif

    }

    private void LocalizationAnimation()
    {
        if (!string.IsNullOrEmpty(currentVideodata.LOCALISATION))
        {
            hasLocalisationAnimation = true;
        }
        else
            hasLocalisationAnimation = false;
    }

    private void CheckSkipAndEndVideo()
    {
        //Check for videos that are already played and enable Skip button
        //Check if next video is the end_credit set the boolean to check once end_credit is played and increment the ending count

        if (gameManager.isGameCompleted)
        {
            //bool isPresentInSaveData = progressIndex.Contains(currentVideodata.ID);
            containsSkipPoint = currentVideodata.SKIP_CHECKPOINT != 0;

            //if (!isPresentInSaveData)
            //{
            //    if (currentVideodata != null)
            //    {
            //        containsSkipPoint = currentVideodata.SKIP_CHECKPOINT != 0;
            //        if (containsSkipPoint)
            //        {
            //            foreach (string videoData in currentVideodata.skipCheckpointDependecny)
            //            {
            //                containsSkipPoint = progressIndex.Contains(videoData);
            //                if (containsSkipPoint)
            //                {
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}

            StartCoroutine(EnableSkipButton());
        }

        if (!string.IsNullOrEmpty(currentVideodata.NEXT_VIDS) && currentVideodata.NEXT_VIDS.Contains("end"))
        {
            isEndCredit = true;
        }

        if (currentVideodata != null && currentVideodata.ID.Contains("end"))
        {
            // Here check is made that if end credit is seen atleast once by the player. If not user not cannot access pause menu
            int value = gameManager.savePrefs.END_CREDIT_SEEN_MALE;

            if (value == 0)
            {
                gameUiManager.isEndCreditSeenOnce = true;
                ShowEndCreditSkipButton().Forget();
            }
            else
            {
                gameUiManager.isEndCreditSeenOnce = false;
            }

        }

        if (currentVideodata != null && currentVideodata.NEXT_VIDS.Contains("RESTART"))
        {
            showRestartScreen = true;
        }
        else
        {
            showRestartScreen = false;
        }

    }

    private async UniTaskVoid ShowEndCreditSkipButton()
    {
        Debugg.Log("Waiting for Skip button");
        await UniTask.Delay(TimeSpan.FromSeconds(endCreditSkipButtonTimer), ignoreTimeScale: false);
        creditScreenSkipPanel.SetActive(true);
    }

    private void CheckSavedGame()//Load Game data and resume video from last saved video
    {
        LoadGameData();
    }

    private IEnumerator _ShowChoiceAtTime() //continuously check for if any video has choices and at what duration it needs to be enabled
    {
        while (true)
        {
            if (gameUiManager.isGamePause)
                yield return null;

            if (hasChoices)
            {
                if ((float)currentPlayer.time >= showChoiceAtTime)
                {
                    Debugg.Log("Showing choices Now");
                    SetchoiceData();
                }
            }

            if ((float)currentPlayer.time >= (videoDuration - 3))//If the user has not clicked the skip button disable the button before current video ends
                DisableSkipButton();

            yield return null;
        }
    }

    private void SkipButtonPressed()
    {
        if (canSkipVideo)
        {
            //if (currentPlayer.playbackSpeed < 4)
            //{
            //    SetVideoPlayerSpeed((int)currentPlayer.playbackSpeed + 1);
            //}
            //else
            //{
            //    SetVideoPlayerSpeed(1);
            //}

            skipButton.onClick.Invoke();
        }
    }

    private void SkipButtonManager()
    {
        //if (!skipButtonEnabled)
        //    return;
        if (player.GetButtonDown("Skip"))
        {
            SkipButtonPressed();
        }
        //skipButtontimer -= Time.deltaTime;
        //if (skipButtontimer <= 0)
        //    DisableSkipButton();
    }

    public void SetVideoPlayerSpeed(int value)
    {
        if (startTimer)
        {
            currentPlayer.playbackSpeed = 1;
            speedText.gameObject.SetActive(false);
            return;
        }

        currentPlayer.playbackSpeed = value;

        if (currentPlayer.playbackSpeed <= 1)
        {
            speedText.gameObject.SetActive(false);
        }
        else
        {
            speedText.gameObject.SetActive(true);
            speedText.text = value + "X";
        }
    }

    private void StartVideoPlayer(DataRead data)
    {
        SetVideoPlayerSpeed(1);
        //Set the currentIndex value as the Index for the current video being played

        if (data == null)
            return;

        currentIndex = data.index;
        currentVideodata = data;
        currentPlayer.url = currentVideodata.videoFileUrl;
        debugText.text = currentVideodata.ID;

        //Debugg.Log("Video Playing : " + currentPlayer.url);

        currentPlayer.Play();

        isPlaybackCompleted = false;
        userMadeChoice = false;

        //--------------Actions Performed after New Video Played-----------------
        OnVideoStartedPlaying().Forget();
    }


    public void ShowSubtitles()//Change Folder based on current language and Display subtitle when video starts playing
    {
        gameManager.SetSubtitleFolder();
        string subtitleFile = csvData.fileData[currentIndex].SUB;
        subtitleDisplayer.SetSubtitleAndShow(gameManager.subtitleFolderName, subtitleFile);
        subtitleDisplayerAlternate.SetSubtitleAndShow(gameManager.subtitleFolderName, subtitleFile);
    }

    private void NewSceneFound()
    {
        defaultVideoIndex = csvData.GetDefaultVideoIndex(currentVideodata);

        //When a new scene is discovered increment the count.
        //If the Game is already completed then check if the video was played previously and don't increment the count

        if (!statsTreeIndex.Contains(currentVideodata.ID))
            gameManager.scenesDiscovered++;

        gameManager.gameSaveData.SCENES_DISCOVERED = gameManager.scenesDiscovered;
    }

    private void EndReached(VideoPlayer vp)
    {
        EndReachedAysnc();
    }

    private void EndReachedAysnc()
    {
        //End of the current video Playback
        //If the next video is End Credit which is checked from previous video and isEndCredit is set True then increment endings Found parameter
        //If the ending was already found previously count remains the same

        //await UniTask.Yield();

        if (!GameManager.ins.isPauseChoicesON)
        {
            //Disable automatically when pause choices is disabled
            buttonPanel.SetActive(false);
        }

        isPlaybackCompleted = true;
        canSkipVideo = false;
        containsSkipPoint = false;
        isVideoSkipped = false;
        shouldCheckScore = false;

        if (isEndCredit)
        {
            Debugg.Log("End Index value --- " + currentVideodata.index);

            isEndCredit = false;
            endingManager.CheckForEndingCount(currentVideodata.ID);
        }

        SaveLifeTimeGameProgress();

        if (GameManager.ins.isPauseChoicesON)
        {
            if (currentVideodata.CHOICES)
            {
                if (!userMadeChoice)
                    return;
            }
        }

        (currentPlayer, nextPlayer) = (nextPlayer, currentPlayer);

        if (nextVideoData == null)//Game Has been completed. Return to main Menu
        {
            //The death stat to increase when restart screen appears also at the end of the game
            gameManager.CURRENT_DEATHS++;
            gameManager.lifetime_deaths++;
            gameManager.gameSaveData.LIFETIME_DEATHS = gameManager.lifetime_deaths;

            if (showRestartScreen)
            {
                gameUiManager.ShowHideCheckPointScreen(true);
            }
            else
            {
                gameManager.SetInt(out gameManager.savePrefs.END_CREDIT_SEEN_MALE, 1);
                GameCompletionHandler();
            }

        }
        else
        {
            StartVideoPlayer(nextVideoData);//Play next Video
        }
    }

    private void NextVideoManager(VideoPlayer vp)
    {
        NextVideoManagerAsync().Forget();
    }

    private async UniTask NextVideoManagerAsync()
    {
        if (currentVideodata.CHOICES)//If video has choices
        {
            if (userMadeChoice)
            {
                GotoNextVideo(currentVideodata.nextVideos[userChoiceIndex]);//If User has made choice play the respective video
            }
            else
            {
                if (string.IsNullOrEmpty(currentVideodata.DEF_VID))
                {
                    SelectDefaultOption(currentVideodata.DEFAULT_CHOICE);
                }
                else
                {
                    string myString = currentVideodata.DEF_VID;
                    int myInt = -1;

                    if (int.TryParse(myString, out myInt))
                    {
                        // String is a valid integer, and myInt contains the parsed value
                        Debugg.Log("String is an integer: " + myInt);
                    }
                    else
                    {
                        // String is not a valid integer
                        myInt = -1; // Set myInt to -1
                        Debugg.Log("String is not an integer. Default value used: " + myInt);
                    }

                    if (myInt == -1)
                    {
                        GotoNextVideo(currentVideodata.DEF_VID);//If no choice is made default video is played
                    }
                    else
                    {
                        //OnOptionClicked(myInt);
                        GotoNextVideo(UpdateDefaultVideo(myInt));
                    }


                }
            }
        }
        else if (currentVideodata.dependentVideos.Count > 0) //Check for any dependency based on other videos. Priority over default video
        {
            GotoNextVideo(CheckDepenciesNew());
        }
        else//Play Next video in order
        {
            GotoNextVideo(currentVideodata.NEXT_VIDS);
        }

        await UniTask.Yield();
    }


    private void TotalTimeOfVideo() //count the total duration of the current video and also the Time at which the choices needs to be displayed
    {
        StartCoroutine(_TotalTimeOfVideo());
    }

    private IEnumerator _TotalTimeOfVideo()
    {
        yield return new WaitForSeconds(0.5f);

        double duration = currentPlayer.length;
        if (duration <= 0)
            duration = currentPlayer.frameCount / currentPlayer.frameRate;

        videoDuration = duration;
        hasChoices = currentVideodata.CHOICES;
        if (hasChoices)
        {
            showChoiceAtTime = ((float)videoDuration - currentVideodata.TIME_CHOICES) - 1f;
        }

        if (currentVideodata != null)
        {
            currentVideodata.videoDuration = duration;
        }
    }

    private IEnumerator EnableSkipButton() //Enable skip button
    {
        yield return new WaitForSeconds(1f);
        //skipButtontimer = 10f;
        canSkipVideo = true;
        //skipButtonEnabled = true;
        skipButton.gameObject.SetActive(true);
        BeginFade(1, skipButtonCanvasGroup, null);

        if (currentVideodata != null && currentVideodata.ID.Contains("end") && gameUiManager.isEndCreditSeenOnce)
        {
            canSkipVideo = false;
        }

    }

    private void DisableSkipButton()
    {
        //skipButtonEnabled = false;
        BeginFade(-1, skipButtonCanvasGroup, () => skipButton.gameObject.SetActive(false));
    }

    public void SkipVideo() //Skip the video to the duration calculated based on videos which has choices and the one which doesn't
    {
        canSkipVideo = false;
        isVideoSkipped = true;
        double videoDuration = currentVideodata.videoDuration;

        double skippingTime;

        if (containsSkipPoint)
        {
            skippingTime = currentVideodata.SKIP_CHECKPOINT;
            canSkipVideo = true;
            containsSkipPoint = false;
        }
        else
        {
            if (currentVideodata.CHOICES)
            {
                skippingTime = (videoDuration - currentVideodata.TIME_CHOICES) - 2d; //Skip to 2 sec before choices appear
            }
            else
            {
                skippingTime = videoDuration - 2d; //Skip to 2 sec before video ends
            }
        }

        currentPlayer.time = skippingTime;

        //Debugg.Log("Video Player Time ------------------ " + videoDuration + "   " + skippingTime);

        subtitleDisplayer.RemoveSubtitle((float)skippingTime);//Remove subtitles from the point of skip to the skipped part
        subtitleDisplayerAlternate.RemoveSubtitle((float)skippingTime);//Remove subtitles from the point of skip to the skipped part

        DisableSkipButton();
    }

    private void SaveGameData() //Save the Game Progrss as JSON format in PlayerPrefs
    {
        if (gameProgress.Count == 0)
            return;

        List<string> indexes = new();

        foreach (DataRead item in gameProgress)
        {
            indexes.Add(item.ID);
        }

        string jsonToSave = JsonHelper.ToJson(indexes.ToArray());

        Debugg.Log(jsonToSave);

        gameManager.gameSaveData.GAME_PROGRESS = jsonToSave;
        gameManager.SaveGame();
    }


    private void LoadGameData()//Load the saved game JSON into object
    {
        string savedJson = gameManager.gameSaveData.GAME_PROGRESS;

        if (string.IsNullOrEmpty(savedJson))
            return;

        string[] savedIndex = JsonHelper.FromJson<string>(savedJson);
        Debugg.Log("Aray Length - " + savedIndex.Length);

        gameProgress.Clear();

        //When loading the game load the progress upto one video before the current video
        //This will prevent multiple saves of the same video index
        int loopCount = savedIndex.Length - 1;

        for (int i = 0; i < loopCount; i++)//Length - 1 
        {
            //Debugg.Log(savedIndex[i].ToString());
            gameProgress.Add(csvData.GetNextVideoData(savedIndex[i]));
        }

        startVideo = savedIndex[^1];

        //Debugging Texts
        scoreAText.text = "Score_A = " + gameManager.SCORE_A;
        scoreBText.text = "Score_B = " + gameManager.SCORE_B;
        livesText.text = "Deaths = " + gameManager.CURRENT_DEATHS;
        checkPointText.text = "Checkpoint = " + csvData.GetNextVideoData(PlayerPrefs.GetString(StaticStrings.Checkpoint, "none"))?.ID;

        if (!statsTreeIndex.Contains(startVideo))
            gameManager.scenesDiscovered--;

    }

    private void LoadLifeTimeGameProgress()
    {
        //This is the list which stores all the videos list when a certain gameplay is fully completed once

        string data = gameManager.savePrefs.SKIP_VIDEO_LIST_MALE;
        Debugg.Log("Lifetime Game Progress -- " + data);

        if (!string.IsNullOrEmpty(data))
        {
            string[] savedIndex = JsonHelper.FromJson<string>(data);

            for (int i = 0; i < savedIndex.Length; i++)
            {
                progressIndex.Add(savedIndex[i]);
            }
        }

        data = gameManager.gameSaveData.STATS_VIDEO_LIST;

        if (!string.IsNullOrEmpty(data))
        {
            string[] savedIndex = JsonHelper.FromJson<string>(data);

            for (int i = 0; i < savedIndex.Length; i++)
            {
                statsTreeIndex.Add(savedIndex[i]);
            }
        }
    }

    private void SaveLifeTimeGameProgress()
    {
        //Saved the Indexes of videos which are played thoughout the Game progress
        //This is kept saved once the player completes the game
        //It is reset if the player starts new game before completing the whole game

        foreach (var item in gameProgress)
        {
            if (!progressIndex.Contains(item.ID))
                progressIndex.Add(item.ID);
        }

        string jsonProgress = JsonHelper.ToJson(progressIndex.ToArray());

        gameManager.SetString(out gameManager.savePrefs.SKIP_VIDEO_LIST_MALE, jsonProgress);


        foreach (var item in progressIndex)
        {
            if (!statsTreeIndex.Contains(item))
                statsTreeIndex.Add(item);
        }

        string jsonStatsTreeProgress = JsonHelper.ToJson(statsTreeIndex.ToArray());

        gameManager.SetString(out gameManager.gameSaveData.STATS_VIDEO_LIST, jsonStatsTreeProgress);
    }

    private string CheckDepenciesNew()
    {
        //Checks if next videos are dependent on any previous videos being played

        dependentStringCheck = null;

        List<string> temp = new List<string>();
        temp.AddRange(currentVideodata.dependentVideos);

        for (int i = 0; i < temp.Count; i++)
        {
            string value = DependencyCheck(temp[i]);

            if (value.Equals("1"))
                return currentVideodata.nextVideos[i];
        }

        return currentVideodata.DEF_VID;
    }

    private void SetchoiceData()
    {
        //Sets the options text and count based on the data in the CSV with localisation
        //The timer and timer UI are reset 

        hasChoices = false;
        timerFillAmount = 1;
        fillImage.localScale = new Vector3(1, 1, 1);
        choiceTime = currentVideodata.TIME_CHOICES;

        SetOptionsTextWithLocalisation();
        UpdateChoiceButtonGraphic();

        gameManager.SetTimerDisplay(gameManager.isPauseChoicesON);

        buttonPanel.SetActive(true);
        DisableSkipButton();

        startTimer = true;
        SetVideoPlayerSpeed(1);
        BeginFade(1, choiceCanvasGroup, null);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void SetOptionsTextWithLocalisation()
    {
        //Get the options value from the csv file and check for localisation based on current language
        gameUiManager.DisableChoicesButtons();

        if (currentVideodata.CHOICES)
        {
            int numChoices = currentVideodata.NR_CHOICES;

            foreach (var item in optionButtons)
            {
                item.gameObject.SetActive(false);
            }

            foreach (var item in optionText)
            {
                item.color = Color.white;
            }

            if (numChoices > 4)
            {
                gameUiManager.optionRow1.SetActive(true);
                gameUiManager.optionRow2.SetActive(true);
            }
            else
            {
                gameUiManager.optionRow1.SetActive(true);
                gameUiManager.optionRow2.SetActive(false);
            }

            if (string.IsNullOrEmpty(currentVideodata.CHOICE_DEPENDENCIES))
            {
                for (int i = 0; i < numChoices; i++)
                {
                    //string optionValue = currentVideodata.ID.Split('.')[0] + "_" + (i + 1);
                    string optionValue = currentVideodata.choicesLocalisationKey[i];
                    optionText[i].text = LocalizationManager.GetTranslation(optionValue);
                    optionButtons[i].gameObject.SetActive(true);
                }
            }
            else
            {
                string[] choiceValueDependency = currentVideodata.CHOICE_DEPENDENCIES.Split(',');

                string combination = string.Empty;

                for (int j = 0; j < choiceValueDependency.Length; j++)
                {
                    DataRead dr = null;
                    if (choiceValueDependency[j].Contains("NOT"))
                    {
                        dr = gameProgress.Find(p => p.ID == choiceValueDependency[j].Trim().Split(' ')[1].Trim());

                        if (dr == null)//This scene? game type was not selected prviously
                        {
                            string optionValue = currentVideodata.ID.Split('.')[0] + "_" + (j + 1);
                            optionText[j].text = LocalizationManager.GetTranslation(optionValue);
                            optionButtons[j].gameObject.SetActive(true);
                            combination += j.ToString();
                        }
                    }
                    else
                    {
                        dr = gameProgress.Find(p => p.ID == choiceValueDependency[j].Trim());
                        if (j == 1)
                        {
                            string optionValue = currentVideodata.ID.Split('.')[0] + "_" + (j + 1);
                            optionText[1].text = LocalizationManager.GetTranslation(optionValue);
                            optionButtons[1].gameObject.SetActive(true);
                        }

                        if (dr != null)
                        {
                            string optionValue = currentVideodata.ID.Split('.')[0] + "_" + (j + 1);
                            optionText[j].text = LocalizationManager.GetTranslation(optionValue);
                            optionButtons[j].gameObject.SetActive(true);
                        }
                    }
                }
            }

        }
    }

    private void Timer()//Change Timer UI. Called in Update function continuosly
    {
        timerFillAmount -= Time.deltaTime / currentVideodata.TIME_CHOICES * currentPlayer.playbackSpeed;

        if (timerFillAmount < 0)
            timerFillAmount = 0;

        fillImage.localScale = new Vector3(timerFillAmount, 1, 1);
        choiceTime -= Time.deltaTime * currentPlayer.playbackSpeed;

        if (choiceTime <= 0) // Play default video if no option is selected
        {
            startTimer = false;
            if (!GameManager.ins.isPauseChoicesON && !isPlaybackCompleted)
            {
                BeginFade(-1, choiceCanvasGroup, () => buttonPanel.SetActive(false));

                NextVideoManager(currentPlayer);
            }
        }
    }

    private void SelectDefaultOption(int value)
    {
        //This method is called when user dows not select any option and
        //the video has dependencies based on option selection
        //So a default option is invoked along with its dependency 

        startTimer = false;
        userChoiceIndex = value;

        string[] optionValues = currentVideodata.TXT_CHOICES.Split(',');

        dependentStringCheck = null;
        if (!string.IsNullOrEmpty(currentVideodata.DEPENDENCIES))
        {
            //string checker = optionText[value].text;
            string checker = optionValues[value].Trim();

            for (int i = 0; i < currentVideodata.dependentVideos.Count; i++)
            {
                if (currentVideodata.dependentVideos[i].Contains(checker))
                {
                    dependentStringCheck = checker;

                    string value1 = DependencyCheck(currentVideodata.dependentVideos[i]);

                    if (value1.Equals("1"))
                    {
                        userChoiceIndex = i;
                        break;
                    }
                }
            }
        }

        GotoNextVideo(currentVideodata.nextVideos[userChoiceIndex]);
    }



    public void OnOptionClicked(int value)
    {
        //When user clicks on any choices button
        //The next videos corresponds to each index value, 0,1 & 2
        //There is one to one relationship for videos to be played based on option clicked
        //If there is any dependency it is given priority over option clicked

        startTimer = false;
        userMadeChoice = true;
        userChoiceIndex = value;

        string[] optionValues = currentVideodata.TXT_CHOICES.Split(',');


        BeginFade(-1, choiceCanvasGroup, () => buttonPanel.SetActive(false));

        gameManager.decisionsMade++;
        gameManager.gameSaveData.DECISIONS_MADE = gameManager.decisionsMade;

        gameManager.CURRENT_DECISIONS++;

        dependentStringCheck = null;

        if (!string.IsNullOrEmpty(currentVideodata.DEPENDENCIES))
        {
            //string checker = optionText[value].text;
            string checker = optionValues[value].Trim();

            for (int i = 0; i < currentVideodata.dependentVideos.Count; i++)
            {
                if (currentVideodata.dependentVideos[i].Contains(checker))
                {
                    dependentStringCheck = checker;

                    string value1 = DependencyCheck(currentVideodata.dependentVideos[i]);

                    if (value1.Equals("1"))
                    {
                        userChoiceIndex = i;
                        break;
                    }
                }
            }
        }

        NextVideoManager(currentPlayer);

        if (isPlaybackCompleted)
        {
            StartVideoPlayer(nextVideoData);
        }
    }

    private void GotoNextVideo(string value) //Search and save data for next video to be played in the sequence
    {
        Debugg.Log("goto next video called");

        if (string.IsNullOrEmpty(value) || value.Equals("RESTART"))
        {
            nextVideoData = null;
        }
        else
        {
            nextVideoData = csvData.GetNextVideoData(value);
        }

        if (nextVideoData != null)
        {
            PrepareVideo(nextVideoData).Forget();
        }
    }

    private void GameCompletionHandler() //Save Prefs when the user reaches the End_Credits file
    {
        gameManager.SetInt(out gameManager.savePrefs.GAME_COMPLETED_MALE, 1);

        gameManager.isGameCompleted = true;
        creditScreenSkipPanel.SetActive(false);

        endingManager.DisplayEnding();
        gameManager.isEndingDisplayed = true;
        gameUiManager.isGamePause = true;

        gameManager.SaveGame();
    }


    public void AutoSkipToggle()
    {
        shouldAutoSkip = !shouldAutoSkip;

        autoSkipStatusText.text = "Auto Skip : OFF";

        if (shouldAutoSkip)
        {
            autoSkipStatusText.text = "Auto Skip : ON";
        }
    }



    // Save the original order of children
    private void SaveOriginalOrder()
    {
        originalOrder.Clear();
        foreach (Button child in optionButtons)
        {
            originalOrder.Add(child.transform);
        }
    }

    private void UpdateChoiceButtonGraphic()
    {
        if (currentVideodata != null)
        {
            timerPanelView.SetTimerColor(currentVideodata.CHOICE_IMAGE);
        }

        foreach (Button item in optionButtons)
        {
            if (currentVideodata != null)
            {
                SpriteState spriteState = item.spriteState;

                spriteState.pressedSprite = GetSprite(currentVideodata.CHOICE_IMAGE);
                spriteState.selectedSprite = GetSprite(currentVideodata.CHOICE_IMAGE);

                item.spriteState = spriteState;
            }
        }
    }

    private Sprite GetSprite(string choiceImage)
    {
        return choiceImage switch
        {
            "pink" => choice_pinkSprite,
            "blue" => choice_blueSprite,
            "red" => choice_redSprite,
            _ => choice_pinkSprite,
        };
    }

    private string ExpressionReplacer(string value, string valueToBeReplaced)
    {
        int start = value.IndexOf("*");
        int end = value.IndexOf("*", start + 1) + 1;
        string result = value.Substring(start, end - start);
        value = value.Replace(result, valueToBeReplaced);
        return value;
    }

    private string DependencyCheck(string expression)
    {
        //Check the videos which are dependent based on Logical expression
        if (expression.Contains("<") || expression.Contains(">") || expression.Contains("="))
        {
            if (expression.Contains("AND") || expression.Contains("OR") || expression.Contains("NOT"))
            {
                string result = expression.Split(new string[] { "*" }, 3, StringSplitOptions.None)[1];

                string isExpressionTrue = ComparisonCheck(result);

                string updatedExpression = ExpressionReplacer(expression, isExpressionTrue);

                Debugg.Log("Updated expression - " + updatedExpression);

                return DependencyCheck(updatedExpression);
            }
            else
                return ComparisonCheck(expression);
        }
        else
        {
            //Check the videos which are dependent based on boolean expression

            List<string> members = new List<string>();

            foreach (var item in gameProgress)
                members.Add(item.ID);

            if (dependentStringCheck != null)
                members.Add(dependentStringCheck.Replace(" ", ""));

            expression = expression.Replace(" ", "");

            //Debugg.Log("Before Operator expression -- " + expression);

            expression = expression.Replace("OR", " || ");
            expression = expression.Replace("AND", " && ");
            expression = expression.Replace("NOT", " ! ");

            //Debugg.Log("Operator expression -- " + expression);

            Regex RE = new Regex(@"([\(\)\! ])");
            string[] tokens = RE.Split(expression);
            string eqOutput = String.Empty;
            string[] operators = new string[] { "&&", "||", "!", ")", "(" };

            foreach (string tok in tokens)
            {
                //Debugg.Log("Tokens ------ " + tok);

                if (tok == "1")
                {
                    eqOutput += "1";
                    continue;
                }

                if (tok == "0")
                {
                    eqOutput += "0";
                    continue;
                }


                if (tok.Trim() == String.Empty)
                    continue;
                if (operators.Contains(tok))
                {
                    eqOutput += tok;
                }
                else if (members.Contains(tok))
                {
                    eqOutput += "1";
                }
                else
                {
                    eqOutput += "0";
                }
            }

            Debugg.Log("Binary Operation String --- " + eqOutput);

            while (eqOutput.Length > 1)
            {
                if (eqOutput.Contains("!1"))
                    eqOutput = eqOutput.Replace("!1", "0");
                else if (eqOutput.Contains("!0"))
                    eqOutput = eqOutput.Replace("!0", "1");
                else if (eqOutput.Contains("1&&1"))
                    eqOutput = eqOutput.Replace("1&&1", "1");
                else if (eqOutput.Contains("1&&0"))
                    eqOutput = eqOutput.Replace("1&&0", "0");
                else if (eqOutput.Contains("0&&1"))
                    eqOutput = eqOutput.Replace("0&&1", "0");
                else if (eqOutput.Contains("0&&0"))
                    eqOutput = eqOutput.Replace("0&&0", "0");
                else if (eqOutput.Contains("1||1"))
                    eqOutput = eqOutput.Replace("1||1", "1");
                else if (eqOutput.Contains("1||0"))
                    eqOutput = eqOutput.Replace("1||0", "1");
                else if (eqOutput.Contains("0||1"))
                    eqOutput = eqOutput.Replace("0||1", "1");
                else if (eqOutput.Contains("0||0"))
                    eqOutput = eqOutput.Replace("0||0", "0");
                else if (eqOutput.Contains("(1)"))
                    eqOutput = eqOutput.Replace("(1)", "1");
                else if (eqOutput.Contains("(0)"))
                    eqOutput = eqOutput.Replace("(0)", "0");
            }

            Debugg.Log("Result : " + eqOutput);

            return eqOutput;
        }
    }

    private string ComparisonCheck(string expression)
    {
        string[] temp = expression.Split(' ');

        int value;
        //int valueToCompare = gameManager.relationshipPoints;
        int valueToCompare = 0;

        if (temp[0].ToLower().Equals("score_a"))
        {
            valueToCompare = gameManager.SCORE_A;
            value = int.Parse(temp[2]);

            Debugg.Log("score A compare valueToCompare = " + valueToCompare + " Value = " + value);
        }
        else if (temp[0].ToLower().Equals("score_b"))
        {
            valueToCompare = gameManager.SCORE_B;
            value = int.Parse(temp[2]);

            Debugg.Log("score B compare valueToCompare = " + valueToCompare + " Value = " + value);
        }

        Debugg.Log("Compare Symbol - " + temp[1]);

        if (temp[1].Equals("<"))
        {
            int.TryParse(temp[2].Trim(), out value);

            return (valueToCompare < value) ? "1" : "0";
        }
        else if (temp[1].Equals(">"))
        {
            int.TryParse(temp[2].Trim(), out value);

            return (valueToCompare > value) ? "1" : "0";
        }
        else if (temp[1].Equals("<="))
        {
            int.TryParse(temp[2].Trim(), out value);

            Debugg.LogWarning("Score compare valueToCompare = " + valueToCompare + " Value = " + value);

            return (valueToCompare <= value) ? "1" : "0";
        }
        else if (temp[1].Equals(">="))
        {
            int.TryParse(temp[2].Trim(), out value);

            return (valueToCompare >= value) ? "1" : "0";
        }
        else if (temp[1].Equals("="))
        {
            int.TryParse(temp[2].Trim(), out value);

            return (valueToCompare == value) ? "1" : "0";
        }

        return "0";
    }

    public void BeginFade(int direction, CanvasGroup cg, Action obj)
    {
        if (direction == -1)
        {
            cg.alpha = 1f;
        }
        else
        {
            cg.alpha = 0f;
        }

        StartCoroutine(Fade(direction, cg, obj));
    }

    IEnumerator Fade(int direction, CanvasGroup cg, Action obj)
    {
        int fadeDir;
        float fadeSpeed = 3f;

        fadeDir = direction;

        if (fadeDir == -1)
        {

            while (cg.alpha > 0)
            {
                cg.alpha += fadeDir * fadeSpeed * Time.deltaTime;
                cg.alpha = Mathf.Clamp01(cg.alpha);
                yield return null;
            }

        }
        else
        {
            while (cg.alpha < 1)
            {
                cg.alpha += fadeDir * fadeSpeed * Time.deltaTime;
                cg.alpha = Mathf.Clamp01(cg.alpha);
                yield return null;
            }
        }

        obj?.Invoke();

    }



    //Custom Hack Functions which are specific for certain scenes only
    string UpdateDefaultVideo(int value)//This is just for getting default videos based on index of choice from CSV
    {
        Debugg.Log("Custom default video + " + value);

        string[] optionValues = currentVideodata.TXT_CHOICES.Split(',');

        dependentStringCheck = null;

        if (!string.IsNullOrEmpty(currentVideodata.DEPENDENCIES))
        {
            string checker = optionValues[value].Trim();

            for (int i = 0; i < currentVideodata.dependentVideos.Count; i++)
            {
                if (currentVideodata.dependentVideos[i].Contains(checker))
                {
                    dependentStringCheck = checker;

                    string value1 = DependencyCheck(currentVideodata.dependentVideos[i]);

                    if (value1.Equals("1"))
                    {
                        Debugg.Log("Custom default video result " + currentVideodata.nextVideos[i]);
                        return currentVideodata.nextVideos[i];
                    }
                }
            }
        }

        return string.Empty;
    }

    public DataRead CurrentData()
    {
        return currentVideodata;
    }
}
