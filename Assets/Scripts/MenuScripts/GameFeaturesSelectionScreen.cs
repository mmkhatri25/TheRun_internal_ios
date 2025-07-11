using UnityEngine;

public class GameFeaturesSelectionScreen : MonoBehaviour
{
    public static GameFeaturesSelectionScreen instance;
    [SerializeField] bool skipTesting;

    public GameObject pauseChoiceMenu;
    public GameObject skipButtonTutorialScreen;

    private void Awake()
    {
        instance = this;
    }

    public void OnPauseChoicesSelected()//Set Pause choices option ON and start the Game
    {
        GameManager.ins.SetInt(out GameManager.ins.savePrefs.PAUSE_CHOICES, 1);
        GameManager.ins.SaveAllPref();

        //pauseChoiceMenu.SetActive(false);
        SkipTesting();

        if (GameManager.ins.isGameCompleted)
        {
            //UiManager.instance.skipTutorialPanel.SetActive(true);
            UiManager.instance.fade.FadeInOut(() =>
            {
                pauseChoiceMenu.SetActive(false);
                skipButtonTutorialScreen.SetActive(true);
            });
        }
        else
        {
            StartGame();
        }

    }

    public void OnTimedChoicesSelected()//Set Pause choices option OFF and start the Game
    {
        GameManager.ins.SetInt(out GameManager.ins.savePrefs.PAUSE_CHOICES, 0);
        GameManager.ins.SaveAllPref();
        //pauseChoiceMenu.SetActive(false);
        SkipTesting();

        if (GameManager.ins.isGameCompleted)
        {
            //UiManager.instance.skipTutorialPanel.SetActive(true);
            UiManager.instance.fade.FadeInOut(() =>
            {
                pauseChoiceMenu.SetActive(false);
                skipButtonTutorialScreen.SetActive(true);
            });
        }
        else
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        UiManager.instance.fade.FadeInOut(() =>
        {
            pauseChoiceMenu.SetActive(false);
            UiManager.instance.StartGameNow();
        });
    }

    void SkipTesting()
    {
        if (skipTesting)
            GameManager.ins.isGameCompleted = true;
    }

}
