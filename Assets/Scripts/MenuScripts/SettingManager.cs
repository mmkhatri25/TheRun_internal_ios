using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public int currentIndex = 0;


    public enum Setting_Type
    {
        LANGUAGE = 0,
        SUBTILE_TOGGLE,
        SUBTILE_SIZE,
        DISPLAY_MODE,
        RESOLUTION,
        PAUSE_CHOICES
    }


    public TextMeshProUGUI textValue;
    public Button nextButton;
    public Button previousButton;
    public Transform highlighter;

    public Setting_Type setting;


    public List<string> values = new List<string>();
    string conversion;

    SettingsCanvasManager settingsCanvasManager;
    GameVideoManager gameVideoManager;
    private EventSystem _eventSystem;

    private void Awake()
    {
        _eventSystem = EventSystem.current;


        nextButton.onClick.AddListener(NextButton);
        previousButton.onClick.AddListener(PreviousButton);

        settingsCanvasManager = FindObjectOfType<SettingsCanvasManager>();
        gameVideoManager = FindObjectOfType<GameVideoManager>();

        if (currentIndex == 0)
            SetInteractable(previousButton, false);

        if (values.Count == 1)
        {
            SetInteractable(nextButton, false);
            SetInteractable(previousButton, false);
        }

        SetTextValue(currentIndex);


#if !UNITY_ANDROID || !UNITY_IOS
        if (setting == Setting_Type.RESOLUTION)
        {
            //Resolution[] resolutions = Screen.resolutions.Where(resolution => resolution.refreshRate == 60).Distinct().ToArray();//Gets the list of resolutions the monitor supports
            Resolution[] resolutions = GetResolutions().ToArray();
            values.Clear();
            foreach (var res in resolutions)
            {
                values.Add(res.width + " x " + res.height);
            }
        }
#endif
    }

    private void Start()
    {
        CheckSavedSettings();
    }


    public void SetTextValue(int index)
    {
        //Change the transalation based on I2 Language Plugin
        //If no translation is found keep it in original language

        //DOES NOT WORK IN UNITY EDITOR

        try
        {
            LocalizationManager.TryGetTranslation(values[index].ToLower(), out conversion);
            if (conversion == "" || conversion == null)
            {
                textValue.text = values[index];
            }
            else
                textValue.text = conversion;
        }
        catch
        {
            print("Resolution not available");
        }
    }

    //public void HighlightButton(GameObject thisButton)
    //{
    //    Debug.Log(_eventSystem.currentSelectedGameObject + " " + thisButton.name);


    //    if (_eventSystem.currentSelectedGameObject.name == thisButton.name)
    //        return;

    //    _eventSystem.SetSelectedGameObject(null);
    //    _eventSystem.SetSelectedGameObject(thisButton);

    //}


    public void NextButton()//Handle Next Button to cycle through different options
    {
        Debug.Log(EventSystem.current.gameObject + "  " + gameObject);

        currentIndex++;
        SetTextValue(currentIndex);

        ToggleNextAndPreviousButtons();
        ChangeSetting();

        if (EventSystem.current != null && EventSystem.current.gameObject != gameObject)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        // HighlightButton(nextButton.transform.parent.gameObject);

        if (GameManager.ins != null)
            GameManager.OnSettingsChanged?.Invoke();

    }

    public void PreviousButton()//Handle Previous Button to cycle through different options
    {


        //if (highlighter != null && !highlighter.gameObject.activeSelf)
        //{
        //    EventSystem.current.SetSelectedGameObject(gameObject);
        //    settingsCanvasManager.EnterPointer(highlighter);
        //    settingsCanvasManager.Highlight(highlighter.gameObject);
        //}
        currentIndex--;
        SetTextValue(currentIndex);

        ToggleNextAndPreviousButtons();
        ChangeSetting();

        Debug.Log(EventSystem.current.gameObject + "  " + gameObject);
        if (EventSystem.current != null && EventSystem.current.gameObject != gameObject)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        // HighlightButton(previousButton.transform.parent.gameObject);

        if (GameManager.ins != null)
            GameManager.OnSettingsChanged?.Invoke();
    }

    void ToggleNextAndPreviousButtons()//If is is last option then turn off next button and vice versa for previous button
    {
        SetInteractable(nextButton, true);
        SetInteractable(previousButton, true);

        if (currentIndex == 0)
        {
            SetInteractable(previousButton, false);
        }

        if (currentIndex == values.Count - 1)
            SetInteractable(nextButton, false);
    }


    void ChangeSetting()//Change the settings based on the type
    {
        switch (setting)
        {
            case Setting_Type.LANGUAGE: Changelanguage(); break;
            case Setting_Type.SUBTILE_TOGGLE: ToggleSubtitle(); break;
            case Setting_Type.SUBTILE_SIZE: ToggleSubtitleSize(); break;
            case Setting_Type.DISPLAY_MODE: ToggleDisplayMode(); break;
            case Setting_Type.RESOLUTION: ChangeResolution(); break;
            case Setting_Type.PAUSE_CHOICES: ChangePauseOption(); break;
        }
    }





    public void CheckSavedSettings()
    {
        //When Game is opened every time the settings needs to be loaded same as previous state
        //This is used to set values and toggles in the settings screen

        switch (setting)
        {
            case Setting_Type.LANGUAGE: CheckLanguage(GameManager.ins.savePrefs.LANGUAGE); break;
            case Setting_Type.SUBTILE_TOGGLE: CheckToggles(GameManager.ins.savePrefs.SUBTILE_SETTING); break;
            case Setting_Type.SUBTILE_SIZE: CheckToggles(GameManager.ins.savePrefs.SUBTILE_SIZE); break;
            case Setting_Type.DISPLAY_MODE: CheckDisplayMode(); break;
            case Setting_Type.RESOLUTION: CheckResolution(); break;
            case Setting_Type.PAUSE_CHOICES: CheckToggles(GameManager.ins.savePrefs.PAUSE_CHOICES); break;
        }
    }

    private void CheckLanguage(int data)
    {
        CheckToggles(data);
    }

    private void CheckResolution()
    {

        Resolution resolution = new Resolution();
        resolution.width = Screen.width;
        resolution.height = Screen.height;

        for (int i = 0; i < values.Count; i++)
        {
            string r = resolution.width + " x " + resolution.height;

            if (values[i].Equals(r))
            {
                currentIndex = i;
                break;
            }
        }

        SetTextValue(currentIndex);
        ToggleNextAndPreviousButtons();
    }

    private void CheckDisplayMode()
    {
        FullScreenMode mode = Screen.fullScreenMode;

        if (mode == FullScreenMode.Windowed)
        {
            currentIndex = 1;
        }
        else if (mode == FullScreenMode.ExclusiveFullScreen)
        {
            currentIndex = 0;
        }

        SetTextValue(currentIndex);
        ToggleNextAndPreviousButtons();
    }

    private void CheckToggles(int data)
    {
        SetTextValue(data);
        currentIndex = data;
        ToggleNextAndPreviousButtons();
    }

    private void ChangePauseOption()
    {
        GameManager.ins.SetInt(out GameManager.ins.savePrefs.PAUSE_CHOICES, currentIndex);
        GameManager.ins.SaveAllPref();
    }

    private void ChangeResolution()
    {
        string[] value = textValue.text.Split('x');
        Screen.SetResolution(int.Parse(value[0]), int.Parse(value[1]), Screen.fullScreen);
    }

    private void ToggleDisplayMode()
    {
        if (currentIndex == 0)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }

    private void ToggleSubtitleSize()
    {
        GameManager.ins.SetInt(out GameManager.ins.savePrefs.SUBTILE_SIZE, currentIndex);
        GameManager.ins.SaveAllPref();
    }

    private void ToggleSubtitle()
    {
        GameManager.ins.SetInt(out GameManager.ins.savePrefs.SUBTILE_SETTING, currentIndex);
        GameManager.ins.SaveAllPref();
    }

    private void Changelanguage()
    {
        if (LocalizationManager.HasLanguage(values[currentIndex]))
        {
            LocalizationManager.CurrentLanguage = values[currentIndex];
            settingsCanvasManager.RefreshTextOnLanguageChange();
        }

        GameManager.ins.SetInt(out GameManager.ins.savePrefs.LANGUAGE, currentIndex);
        GameManager.ins.SaveAllPref();

        if (gameVideoManager != null)
        {
            gameVideoManager.ShowSubtitles();
            gameVideoManager.SetOptionsTextWithLocalisation();
        }
    }

    void SetInteractable(Button button, bool value)
    {
        button.interactable = value;
    }


    public void ResetSettings()
    {
        switch (setting)
        {
            case Setting_Type.LANGUAGE: currentIndex = 0; Changelanguage(); break;
            case Setting_Type.SUBTILE_TOGGLE: currentIndex = 0; ToggleSubtitle(); break;
            case Setting_Type.SUBTILE_SIZE: currentIndex = 1; ToggleSubtitleSize(); break;
            case Setting_Type.DISPLAY_MODE: currentIndex = 0; ToggleDisplayMode(); break;
            case Setting_Type.RESOLUTION: StartCoroutine(ResetResolution()); break;
            case Setting_Type.PAUSE_CHOICES: currentIndex = 0; ChangePauseOption(); break;
        }

        CheckSavedSettings();

    }

    IEnumerator ResetResolution()
    {
        Screen.SetResolution(1920, 1080, Screen.fullScreen);

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < values.Count; i++)
        {
            string r = 1920 + " x " + 1080;

            if (values[i].Equals(r))
            {
                currentIndex = i;
                break;
            }
        }

        SetTextValue(currentIndex);
        ToggleNextAndPreviousButtons();


    }


    public static List<Resolution> GetResolutions()
    {
        //Filters out all resolutions with low refresh rate:
        Resolution[] resolutions = Screen.resolutions;
        HashSet<Tuple<int, int>> uniqResolutions = new HashSet<Tuple<int, int>>();
        Dictionary<Tuple<int, int>, int> maxRefreshRates = new Dictionary<Tuple<int, int>, int>();
        for (int i = 0; i < resolutions.GetLength(0); i++)
        {
            //Add resolutions (if they are not already contained)
            Tuple<int, int> resolution = new Tuple<int, int>(resolutions[i].width, resolutions[i].height);
            uniqResolutions.Add(resolution);
            //Get highest framerate:
            if (!maxRefreshRates.ContainsKey(resolution))
            {
                maxRefreshRates.Add(resolution, resolutions[i].refreshRate);
            }
            else
            {
                maxRefreshRates[resolution] = resolutions[i].refreshRate;
            }
        }
        //Build resolution list:
        List<Resolution> uniqResolutionsList = new List<Resolution>(uniqResolutions.Count);
        foreach (Tuple<int, int> resolution in uniqResolutions)
        {
            Resolution newResolution = new Resolution();
            newResolution.width = resolution.Item1;
            newResolution.height = resolution.Item2;
            if (maxRefreshRates.TryGetValue(resolution, out int refreshRate))
            {
                newResolution.refreshRate = refreshRate;
            }
            uniqResolutionsList.Add(newResolution);
        }
        return uniqResolutionsList;
    }

}
