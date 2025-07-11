using I2.Loc;
//using Steamworks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class DeathsDisplayManager : MonoBehaviour
{
    [SerializeField] private int totalDeaths;
    [SerializeField] private TextMeshProUGUI deathsFound;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameManager _gameManager;

    [Header("On Screen Close")]
    [SerializeField] private List<GameObject> closeObjectsList;
    [SerializeField] private List<GameObject> enableObjectsList;

    [SerializeField] private List<DeathsData> _deaths = new List<DeathsData>();


    private CSVDataLoader _csvDataLoader;

    private readonly string DEATHS_FOUND_KEY = "DeathsFoundText";

    private const string _achievementKey = "death_became_her";

    private void Awake()
    {
        _gameManager = GameManager.ins;
        _csvDataLoader = FindAnyObjectByType<CSVDataLoader>();

        videoPlayer.url = StreamingAssetPath.GetVideoFolderPath() + "loading.mp4";
        videoPlayer.Play();
    }

    private void OnEnable()
    {
        StatsTreeManager.OnStatsClosedEvent += OnStatsClose;
        _gameManager ??= GameManager.ins;
        _gameManager.deathsFoundInGameCount = 0;

        string videoList = _gameManager.gameSaveData.STATS_VIDEO_LIST;
        List<string> savedScenesList = new();

        if (!string.IsNullOrEmpty(videoList))
        {
            savedScenesList = JsonHelper.FromJson<string>(videoList).ToList();
        }

        foreach (DeathsData item in _deaths)
        {
            string check = DependencyParserUtility.DependencyCheck(item.sceneName, savedScenesList);

            if (check.Equals("1"))
            {
                item.deathIcon.UnLock();
                _gameManager.deathsFoundInGameCount++;
                continue;
            }

            item.deathIcon.Lock();
        }

        deathsFound.text = LocalizationManager.GetTranslation(DEATHS_FOUND_KEY) + "\n" + GameManager.ins.deathsFoundInGameCount + " / " + totalDeaths;

        if (GameManager.ins.deathsFoundInGameCount == totalDeaths)
        {

            #if UNITY_IOS || UNITY_ANDROID
            #else
            if (SteamManager.Initialized)
            {
                SteamUserStats.GetAchievement(_achievementKey, out bool isAchievementUnlocked);
                if (!isAchievementUnlocked) SteamUserStats.SetAchievement(_achievementKey);
            }
            #endif
        }

    }

    private void OnDisable()
    {
        StatsTreeManager.OnStatsClosedEvent -= OnStatsClose;
    }

    public void OnStatsClose()
    {
        foreach (GameObject item in closeObjectsList)
        {
            item.SetActive(false);
        }

        foreach (GameObject item in enableObjectsList)
        {
            item.SetActive(true);
        }
    }
}

[System.Serializable]
public class DeathsData
{
    public string id;
    public string sceneName;
    public DeathViewIcon deathIcon;
}