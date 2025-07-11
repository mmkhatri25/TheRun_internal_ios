using DG.Tweening;
using DigitalRuby.SimpleLUT;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsCanvasManager : MonoBehaviour
{
    [SerializeField] SimpleLUT simpleLUT;
    [SerializeField] private GameObject confirmationWindow;

    [Header("Slider")]
    public Slider brightnessSlider;
    public Slider volumeSlider;

    [Header("Text")]
    public SettingManager[] settingsText;

    public List<SettingManager> allSettings = new List<SettingManager>();

    AudioManager am;

    public static UnityAction SetButtonTextLanguage;


    // Start is called before the first frame update
    void Start()
    {
        volumeSlider.onValueChanged.AddListener(delegate { am.SetVolume(volumeSlider); });
    }


    public void SetSettingsData()//Sets saved Setting Data and slider position for volume and brightness when game is opened every time
    {

        if (am == null)
            am = FindObjectOfType<AudioManager>();

        float volume = GameManager.ins.savePrefs.VOLUME;
        am.SetVolume(volume);
        volumeSlider.value = volume;

        float brightness = GameManager.ins.savePrefs.BRIGHTNESS;
        brightnessSlider.value = brightness;


        foreach (var item in allSettings)
        {
            item.CheckSavedSettings();
        }

    }


    public void DeselectAllOptions()
    {
        foreach (var item in allSettings)
        {
            item.GetComponent<SettingButtonNavigation>().ExitPointerCustom();
        }

        ExitPointer(volumeSlider.transform);
        ExitPointer(brightnessSlider.transform);

    }



    public void RefreshTextOnLanguageChange()//Refresh Text for Localisation
    {
        foreach (SettingManager item in settingsText)
        {
            item.SetTextValue(item.currentIndex);
        }

        SetButtonTextLanguage?.Invoke();
    }

    public void AdjustBrightness()//Set brightness value from slider in settings UI
    {
        simpleLUT.Brightness = brightnessSlider.value;

        GameManager.ins.SetFloat(out GameManager.ins.savePrefs.BRIGHTNESS, brightnessSlider.value);
        GameManager.ins.SaveAllPref();
    }


    public void ResetSettings()
    {
        brightnessSlider.value = 0;
        AdjustBrightness();

        volumeSlider.value = 1;
        am.SetVolume(volumeSlider);


        foreach (var item in allSettings)
        {
            item.ResetSettings();
        }

        GameVideoManager gm = FindObjectOfType<GameVideoManager>();

        if (gm != null)
            gm.ShowSubtitles();
    }


    public void EnterPointer(Transform toScale)
    {
        toScale.DOScale(Vector3.one * 1.1f, 0.3f).SetUpdate(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound("HoverSFX");
    }

    public void ExitPointer(Transform toScale)
    {
        toScale.DOScale(Vector3.one, 0.3f).SetUpdate(true);
    }

    public void Highlight(GameObject toScale)
    {
        toScale.SetActive(true);
    }

    public void DeHighlight(GameObject toScale)
    {
        toScale.SetActive(false);
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        GameManager.ins.DeleteAll();
        SaveManager.DeleteAll();

        SceneManager.LoadScene(StaticStrings.MENU_SCENE);
    }

    public void ShowConfirmationWindow()
    {
        confirmationWindow.SetActive(true);
    }
}
