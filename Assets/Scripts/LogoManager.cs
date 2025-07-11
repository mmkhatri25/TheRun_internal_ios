using Bayat.SaveSystem;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;


public class LogoManager : MonoBehaviour
{

    [SerializeField] private VideoPlayer logoVideoPlayer;
    [SerializeField] private string logoVideoFileName;
    [SerializeField] private LoadingScreen loadingScreen;

    float volume = 1;

    public bool usePlayerPref;


    private void Start()
    {
        Application.targetFrameRate = -1;

        logoVideoPlayer.loopPointReached += OnLogoVideoEnded;

        CultureInfo dotCulture = new("en-US");
        CultureInfo.DefaultThreadCurrentCulture = dotCulture;
        CultureInfo.DefaultThreadCurrentUICulture = dotCulture;

#if UNITY_EDITOR
        Debugg.EnableLogs = true;
#else
        Debugg.EnableLogs = false;
#endif


#if UNITY_SWITCH && !UNITY_EDITOR
        SaveManager.Initialize();
        PlayerPrefsSwitch.PlayerPrefsSwitch.Init();
#endif

        if (string.IsNullOrEmpty(logoVideoFileName))
        {
            LoadingScreen.OnLoadingComplete += () => SceneManager.LoadScene(StaticStrings.MENU_SCENE);
            loadingScreen.StartLoading();
        }
        else
        {
#if UNITY_ANDROID
                logoVideoPlayer.url = Application.streamingAssetsPath + "/Videos/" + logoVideoFileName;
#else
            logoVideoPlayer.url = Application.streamingAssetsPath + "/Videos/" + logoVideoFileName;
#endif
        }

#if UNITY_GAMECORE
        StartCoroutine(UnityEngine.GameCore.PlayerPrefs.InitializeAsync(OnPlayerPrefsInit));
#endif

        LoadVolume();
    }

    private void OnPlayerPrefsInit()
    {
        Debug.Log("PlayerPrefs initialized, you can now work normally");
    }


    public async void LoadVolume()
    {
        if (usePlayerPref)
        {
            if (PlayerPrefs.HasKey(StaticStrings.GAME_PREF))
            {
                string data = PlayerPrefs.GetString(StaticStrings.GAME_PREF);
                volume = JsonUtility.FromJson<SavePrefs>(data).VOLUME;
            }
        }
        else if (await SaveSystemAPI.ExistsAsync("SavePref"))
        {
            string data = await SaveSystemAPI.ReadAllTextAsync("SavePref");
            volume = JsonUtility.FromJson<SavePrefs>(data).VOLUME;
        }

        logoVideoPlayer.GetTargetAudioSource(0).volume = volume;
    }


    private void OnLogoVideoEnded(VideoPlayer source)
    {
        LoadingScreen.OnLoadingComplete += () => SceneManager.LoadScene(StaticStrings.MENU_SCENE);
        loadingScreen.StartLoading();
    }


}