using Rewired;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class UiManager : MonoBehaviour
{

    public static UiManager instance;
    public Fading fade;
    public SettingsCanvasManager settingsCanvasManager;
    public StatsCanvasManager statsCanvasManager;
    public StatsTreeManager statsTreeManager;

    [Header("GameObjects")]
    public GameObject menuCanvas;
    public GameObject settingsCanvas;
    public GameObject characterCanvas;
    public GameObject statsCanvas;
    public GameObject popupCanvas;
    public GameObject pauseChoicesMenu;
    public LoadingScreen loadingScreen;
    public GameObject logoCanvas;
    public GameObject blurCanvas;
    public GameObject menuIntoQuote;


    [Header("Buttons")]
    public Button resume;
    public Button startGame;
    public Button stats;
    public Button settings;
    public Button exitGame;
    public Button yesButton;
    public Button settingsBackButton;


    [Header("Video Player")]
    public VideoPlayer videoPlayer;
    public string mainMenuVideo;
    public string introVideo;
    public string characterSelectVideo;

    DefaultButtonHighlight defaultButtonHighlight;
    private Player player { get { return ReInput.players.GetPlayer(0); } }

    private void Awake()
    {
        instance = this;

        fade = FindObjectOfType<Fading>();
        videoPlayer.isLooping = true;
        videoPlayer.SetTargetAudioSource(0, AudioManager.Instance.GetAudioSource("BGVideoSound"));

        videoPlayer.url = StreamingAssetPath.GetVideoFolderPath() + mainMenuVideo;
        Debug.Log("Menu video path - " + videoPlayer.url);

        videoPlayer.Play();
        fade.BeginFade(-1);

        AddListerners();
    }

    private void Start()
    {
        SetButtonSelection(startGame.gameObject);
        settingsCanvasManager.SetSettingsData();

        GameManager.ins.LoadSavedGame();

        if (GameManager.ins.savePrefs.GAME_SAVE_MALE == 1)//Check for any saved games. If available show Resume Option
        {
            resume.gameObject.SetActive(true);

            Navigation navigation = startGame.navigation;
            navigation.selectOnUp = resume;
            startGame.navigation = navigation;

            Navigation navigation2 = exitGame.navigation;
            navigation2.selectOnDown = resume;
            exitGame.navigation = navigation2;
        }
    }


    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void AddListerners()
    {
        startGame.onClick.AddListener(NewGameButtonClicked);
        exitGame.onClick.AddListener(ExitGameButtonClicked);
        settings.onClick.AddListener(SettingsButtonClicked);
        resume.onClick.AddListener(ResumeGameButtonClicked);
        stats.onClick.AddListener(OnStatsButtonClicked);
        settingsBackButton.onClick.AddListener(SettingsClosedButtonClicked);
    }

    void SetButtonSelection(GameObject current)
    {
        if (current.TryGetComponent<HoverAnimation>(out HoverAnimation ha))
        {
            ha.EnterPointer();
        }
    }

    public void SettingsButtonClicked()
    {
        blurCanvas.SetActive(true);
        settingsCanvas.SetActive(true);
        menuCanvas.SetActive(false);
    }

    public void SettingsClosedButtonClicked()
    {
        blurCanvas.SetActive(false);
        settingsCanvas.SetActive(false);
        menuCanvas.SetActive(true);

    }

    public void StartGameButtonClicked()
    {
        Debug.Log("Start Game button clicked");

        //fade.FadeInOut(SetNewGameIntroVideo);
    }

    public void ShowResetGameScreen()
    {
        fade.FadeInOut(delegate
        {
            popupCanvas.SetActive(true);
        });
    }

    void OnStatsButtonClicked()
    {
        blurCanvas.SetActive(true);
        menuCanvas.SetActive(false);

        statsTreeManager.OnStatsAccessed();
    }

    public void BackButton()
    {
        fade.FadeInOut(BackButton_OnFadeComplete);
    }

    void BackButton_OnFadeComplete()
    {
        videoPlayer.targetTexture.Release();
        //ChangeVideoClip(mainMenuVideo, true);
        characterCanvas.SetActive(false);
        menuCanvas.SetActive(true);
    }

    public void StartGameNow()
    {
        StartCoroutine(Loading());
    }

    IEnumerator Loading()
    {
        ResetGameData();
        LoadingScreen.OnLoadingComplete += () => SceneManager.LoadScene(StaticStrings.GAME_SCENE);
        loadingScreen.StartLoading();
        yield return null;
    }

    void ResetGameData()
    {
        GameManager.ins.ResetGameData();
    }

    public void ExitGameButtonClicked()
    {
        Application.Quit();
    }

    public void ClearAllPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    public void BackButtonUsingKey(Button button)
    {
        //controls.Gameplay.Pause.performed += ctx => button.onClick.Invoke();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == StaticStrings.MENU_SCENE)
        {

        }
    }

    public void ResumeGameButtonClicked()
    {
        GameManager.ins.LoadSavedGame();//Load Saved game data and populate saved variables

        SceneManager.LoadScene("GameScene");
    }

    public void NewGameButtonClicked()
    {
        menuCanvas.SetActive(false);
        GameManager.ins.isGameCompleted = Convert.ToBoolean(GameManager.ins.savePrefs.GAME_COMPLETED_MALE);

        if (GameManager.ins.savePrefs.GAME_SAVE_MALE == 1)//Check for any saved games. If available show pop up choice to reset game progress
        {
            ShowResetGameScreen();
        }
        else
        {
            fade.FadeInOut(() => GameFeaturesSelectionScreen.instance.pauseChoiceMenu.SetActive(true));
        }

        if (GameManager.ins.isGameCompleted)//Here data is loaded to not rest 'Endings' count and 'Scenes Discovered count'
            GameManager.ins.LoadSavedGame();

    }



    //public void ClearAllPrefs()
    //{
    //    PlayerPrefs.DeleteAll();
    //}

}
