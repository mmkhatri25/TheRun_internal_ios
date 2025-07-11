using DG.Tweening;
using I2.Loc;
using Rewired;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUiManager : MonoBehaviour
{
    //public CustomControls controls;
    public SettingsCanvasManager settingsCanvasManager;
    public StatsTreeManager statsTreeManager;
    public GameVideoManager gameVideoManager;
    //public ScorePanel gameScorePanel;
    [SerializeField] private OptionSelectManager optionSelectManager;//This script is used for selection of a option using arrow keys
    private Fading fade;
    public bool isGamePause = false;
    public bool isGameCompleted = false;
    public bool isEndCreditSeenOnce = false;

    [Header("GameObjects")]
    public GameObject settingCanvas;
    public GameObject pauseMenuCanvas;
    public GameObject blurCanvas;
    public GameObject timer;
    public GameObject uiChoicesParent;
    public GameObject pauseMenuContainer;
    public GameObject endingMenuContainer;
    public GameObject mobileUIButtons;

    [Header("Buttons")]
    public Button resume;
    public Button settings;
    public Button stats;
    public Button statsBtnEnding;
    public Button exit;
    public Button pauseGameButton;
    public Button restartGameButton;

    [Header("Cup Game Buttons")]
    public GameObject optionRow1;
    public GameObject optionRow2;

    [Header("Checkpoint")]
    public GameObject checkPointGO;
    public CanvasGroup checkPointCanvas;
    public TextMeshProUGUI totalDeathsText;


    Vector3 originalTimerPos;
    Vector3 originaluiChoicesPos;

    private Player player { get { return ReInput.players.GetPlayer(0); } }

    private bool _isStatsWindowOpen;

    public float hideDelay = 3f;        // Time after which button hides

    private Coroutine hideCoroutine;

    private void Awake()
    {
        //controls = new CustomControls();
        originalTimerPos = timer.GetComponent<RectTransform>().localPosition;
        originaluiChoicesPos = uiChoicesParent.GetComponent<RectTransform>().localPosition;

        fade = FindObjectOfType<Fading>();


#if UNITY_ANDROID || UNITY_IOS
        mobileUIButtons.SetActive(true);
#else
        if (Application.platform == RuntimePlatform.Switch)
            mobileUIButtons.SetActive(true);
        else
            mobileUIButtons.SetActive(false);
#endif
        //When the game is opened every time it checks if the games was completed or not for that character
        //This check helps to enable skipping feature in the game

        int value = 0;
        value = GameManager.ins.savePrefs.GAME_COMPLETED_MALE;

        switch (value)
        {
            case 0: isGameCompleted = false; break;
            case 1: isGameCompleted = true; break;
        }

        AddListerners();
    }

    // Start is called before the first frame update
    void Start()
    {
        GameManager.ins.isCheckPointCanvasActive = false;
        StatsTreeManager.OnStatsClosedEvent += OnStatsClose;
        GameManager.OnSettingsChanged?.Invoke();//Set the Settings data in Game Scene as set in the Menu Scene
        settingsCanvasManager.SetSettingsData();// Set the brightness and volume Settings data in Game Scene as set in the Menu Scene
        pauseGameButton.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (player.GetButtonDown("UICancel") && !gameVideoManager.endingManager.isEndScreenDisplayed && !_isStatsWindowOpen)
        {
            GamePause();
        }

        // Check for any touch or mouse click
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
        {
            ShowPauseButton();
        }
    }


    void GamePause()
    {
        if (isEndCreditSeenOnce)
            return;

        if (GameManager.ins.isCheckPointCanvasActive)
        {
            return;
        }

        if (isGamePause)
        {
            OnResumeGameButtonClicked();
        }
        else
        {
            Debug.Log("Game Pause Called");
            OnPauseGameButtonClicked();
        }
    }

    void AddListerners()
    {
        resume.onClick.AddListener(OnResumeGameButtonClicked);
        settings.onClick.AddListener(OnSettingsButtonClicked);
        stats.onClick.AddListener(OnStatsButtonClicked);
        statsBtnEnding.onClick.AddListener(OnStatsButtonClicked);
        exit.onClick.AddListener(OnExitButtonClicked);
        pauseGameButton.onClick.AddListener(GamePause);
        restartGameButton.onClick.AddListener(OnRestartGameFromCheckPoint);
    }

    void OnSettingsButtonClicked() //Access settings Screen UI
    {
        settingCanvas.SetActive(true);
        pauseMenuCanvas.SetActive(false);
    }

    public void OnResumeGameButtonClicked() //Resume video playback and disable all pause menu UI
    {
        blurCanvas.SetActive(false);
        pauseMenuCanvas.SetActive(false);
        settingCanvas.SetActive(false);
        statsTreeManager.OnStatsClosed();

        gameVideoManager.SetDirectAudioVolume();

        if (!gameVideoManager.isPlaybackCompleted)
        {
            gameVideoManager.audioSource.Play();
            gameVideoManager.currentPlayer.Play();
        }

        isGamePause = false;

        SetTimeScale();
    }

    void OnPauseGameButtonClicked()//Pause video and enable pause menu UI
    {
        blurCanvas.SetActive(true);
        pauseMenuCanvas.SetActive(true);
        gameVideoManager.currentPlayer.Pause();
        isGamePause = true;
        optionSelectManager.isSelected = false;
        settingsCanvasManager.DeselectAllOptions();

        SetTimeScale(0);
    }


    void OnStatsButtonClicked() //Refresh Stats data UI when stats button is clicked and display UI screen
    {
        pauseMenuCanvas.SetActive(false);
        endingMenuContainer.SetActive(false);
        statsTreeManager.OnStatsAccessed();
        _isStatsWindowOpen = true;
    }

    public void OnExitButtonClicked()//Back to Main Menu
    {
        AudioManager.Instance.StopSound("CheckpointSound");
        StartCoroutine(ExitGame());
    }

    public void OnRestartGameFromCheckPoint()
    {
        AudioManager.Instance.StopSound("CheckpointSound");
        GameManager.ins.RestartGameFromCheckPoint();
    }

    IEnumerator ExitGame()
    {
        SetTimeScale();
        pauseMenuCanvas.SetActive(false);
        fade.BeginFade(1);

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(StaticStrings.MENU_SCENE);
    }

    public void ChangeVideoSpeed(int value)//For Testing Purpose 
    {
        gameVideoManager.currentPlayer.playbackSpeed = value;
    }



    public void SetChoicesPosition()
    {
        timer.GetComponent<RectTransform>().localPosition = new Vector3(originalTimerPos.x, originalTimerPos.y + 100f, originalTimerPos.z);
        uiChoicesParent.GetComponent<RectTransform>().localPosition = new Vector3(originaluiChoicesPos.x, originaluiChoicesPos.y + 100f, originaluiChoicesPos.z);
    }

    public void ReSetChoicesPosition()
    {
        timer.GetComponent<RectTransform>().localPosition = originalTimerPos;
        uiChoicesParent.GetComponent<RectTransform>().localPosition = originaluiChoicesPos;
    }

    public void ShowHideCheckPointScreen(bool value)
    {
        if (value)
        {
            totalDeathsText.text = LocalizationManager.GetTranslation("total_deaths") + " : " + GameManager.ins.CURRENT_DEATHS;
            checkPointGO.SetActive(true);
            GameManager.ins.isCheckPointCanvasActive = true;
            checkPointCanvas.gameObject.SetActive(true);
            checkPointCanvas.alpha = 0f;
            checkPointCanvas.DOFade(1f, 1f);

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(restartGameButton.gameObject);

            AudioManager.Instance.PlaySound("CheckpointSound");
        }
        else
        {
            checkPointGO.SetActive(false);
            GameManager.ins.isCheckPointCanvasActive = false;
            checkPointCanvas.gameObject.SetActive(false);
            checkPointCanvas.alpha = 0f;

            AudioManager.Instance.StopSound("CheckpointSound");
        }

    }

    public void DisableChoicesButtons()
    {
        optionRow1.SetActive(false);
        optionRow2.SetActive(false);
    }

    public void OnStatsClose()
    {
        _isStatsWindowOpen = false;

        if (gameVideoManager.endingManager.isEndScreenDisplayed)
        {
            pauseMenuCanvas.SetActive(true);
            endingMenuContainer.SetActive(true);
            pauseMenuContainer.SetActive(false);
        }
    }

    private void OnDisable()
    {
        StatsTreeManager.OnStatsClosedEvent -= OnStatsClose;
    }

    public void SetTimeScale(int value = 1)
    {
        Time.timeScale = value;
        gameVideoManager.SetVideoPlayerSpeed(value);
    }

    void ShowPauseButton()
    {
        pauseGameButton.GetComponent<Image>().DOFade(1f, 0.3f).SetUpdate(true);

        pauseGameButton.gameObject.SetActive(true);

        // Reset timer if user taps again
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        pauseGameButton.GetComponent<Image>().DOFade(0f, 0.3f).SetUpdate(true);
        pauseGameButton.gameObject.SetActive(false);
    }

}
