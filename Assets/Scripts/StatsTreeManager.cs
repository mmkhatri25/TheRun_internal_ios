using Cysharp.Threading.Tasks;
using I2.Loc;
using Rewired;
//using Steamworks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI_Spline_Renderer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class StatsTreeManager : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField]
    private ZoomAndPan zoomPan;
    [SerializeField] private CSVDataLoader csvDataLoader;
    [SerializeField] private GameUiManager gameUiManager;
    [SerializeField] private Button deathScreenButton;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI endingFound;
    [SerializeField] private TextMeshProUGUI scenesDiscovered;
    [SerializeField] private TextMeshProUGUI decisionsMade;
    [SerializeField] private TextMeshProUGUI deaths;

    [SerializeField] private int totalEndings;
    [SerializeField] private int totalScenes;


    public Transform root;
    public GameObject closeButton;
    public Image treeNode;
    public Material noAlphaBlendMat;
    public List<StatsTreeData> treeData = new List<StatsTreeData>();

    private readonly string SCENES_DISCOVERED_KEY = "Scenes Discovered";
    private readonly string ENDINGS_FOUND_KEY = "Endings found";
    private readonly string TOTAL_DECISIONS_MADE_KEY = "total_decisions_made";
    private readonly string DECISIONS_MADE_KEY = "Decisions Made";
    private readonly string TOTAL_DEATHS_KEY = "total_deaths";
    private readonly string DEATHS_KEY = "DeathsText";

    private StatsTreeData currentNodeBounce;
    public static UnityAction OnStatsClosedEvent;
    private Player player { get { return ReInput.players.GetPlayer(0); } }

    private List<string> savedIndexList;

    private bool isStatstreeOpen;

    private const string _achievementKey = "master_explorer";
    void SetStatsData() //Stats data to be displayed on the UI
    {

        if (SceneManager.GetActiveScene().name == StaticStrings.MENU_SCENE)//Menu Scene
        {
            decisionsMade.text = LocalizationManager.GetTranslation(TOTAL_DECISIONS_MADE_KEY) + "\n" + GameManager.ins.decisionsMade.ToString();
            scenesDiscovered.text = LocalizationManager.GetTranslation(SCENES_DISCOVERED_KEY) + "\n" + GameManager.ins.scenesDiscovered + " / " + totalScenes;
            endingFound.text = LocalizationManager.GetTranslation(ENDINGS_FOUND_KEY) + "\n" + GameManager.ins.endingsFound + " / " + totalEndings;
            deaths.text = LocalizationManager.GetTranslation(TOTAL_DEATHS_KEY) + "\n" + GameManager.ins.lifetime_deaths;
        }
        else if (SceneManager.GetActiveScene().name == StaticStrings.GAME_SCENE)//Game Scene
        {
            scenesDiscovered.gameObject.SetActive(false);
            endingFound.gameObject.SetActive(false);

            decisionsMade.text = LocalizationManager.GetTranslation(DECISIONS_MADE_KEY) + "\n" + GameManager.ins.CURRENT_DECISIONS;
            deaths.text = LocalizationManager.GetTranslation(DEATHS_KEY) + "\n" + GameManager.ins.CURRENT_DEATHS;
        }


    }

    public void OnStatsClosed()
    {
        isStatstreeOpen = false;
        root.gameObject.SetActive(false);
        closeButton.SetActive(false);
        OnStatsClosedEvent?.Invoke();
    }


    private void Update()
    {
        if (gameUiManager != null && !gameUiManager.isGamePause)
        {
            return;
        }

        if (player.GetButtonDown("Skip") && isStatstreeOpen)
        {
            SelectDeathCanvasButton();
        }
    }

    private void SelectDeathCanvasButton()
    {
        EventSystem.current.SetSelectedGameObject(deathScreenButton.gameObject);
    }

    public void OnStatsAccessed()
    {
        closeButton.SetActive(true);
        root.gameObject.SetActive(true);
        isStatstreeOpen = true;

        foreach (var item in treeData)
        {
            item.DisableNode(noAlphaBlendMat, GetLockedColor());
        }

        SetStatsData();

        string savedJson = "";
        string currentGamesavedJson = "";
        List<string> currentGameIndexList = new();

        if (SceneManager.GetActiveScene().name == StaticStrings.MENU_SCENE)//Menu Scene
        {
            savedJson = GameManager.ins.gameSaveData.STATS_VIDEO_LIST;//Show Lifetime progress tree in Menu

        }
        else if (SceneManager.GetActiveScene().name == StaticStrings.GAME_SCENE)//Game Scene
        {
            savedJson = GameManager.ins.gameSaveData.STATS_VIDEO_LIST;//Show Lifetime progress tree in Menu
            currentGamesavedJson = GameManager.ins.gameSaveData.GAME_PROGRESS;//Show current progress tree in Gameplay
            currentGameIndexList = JsonHelper.FromJson<string>(currentGamesavedJson).ToList();
        }


        if (string.IsNullOrEmpty(savedJson))
        {
            savedJson = JsonHelper.ToJson(new List<string>().ToArray());
        }

        savedIndexList = JsonHelper.FromJson<string>(savedJson).ToList();

        scenesDiscovered.text = LocalizationManager.GetTranslation(SCENES_DISCOVERED_KEY) + "\n" + savedIndexList.Count() + " / " + totalScenes;

        if (SceneManager.GetActiveScene().name == StaticStrings.GAME_SCENE)//Game Scene
        {
            if (savedIndexList.Count() < 0)
            {
                zoomPan.zoomLocation = ZoomAndPan.ZOOM_LOCATION.LEFT;
            }
            else
            {
                if (csvDataLoader.GetNextVideoData(savedIndexList[^1]).index <= 43)
                {
                    zoomPan.zoomLocation = ZoomAndPan.ZOOM_LOCATION.LEFT;
                }
                if (csvDataLoader.GetNextVideoData(savedIndexList[^1]).index >= 44 &&
                    csvDataLoader.GetNextVideoData(savedIndexList[^1]).index < 60)
                {
                    zoomPan.zoomLocation = ZoomAndPan.ZOOM_LOCATION.CENTER;
                }

                if (csvDataLoader.GetNextVideoData(savedIndexList[^1]).index > 60)
                {
                    zoomPan.zoomLocation = ZoomAndPan.ZOOM_LOCATION.RIGHT;
                }
            }
        }

        if (SceneManager.GetActiveScene().name != StaticStrings.MENU_SCENE)//Scene other than Menu
        {
            ActivateNodeMenuScene(savedIndexList).Forget();
            ActivateNodeGameScene(currentGameIndexList).Forget();
        }
        else
        {
            ActivateNodeMenuScene(savedIndexList).Forget();
        }
    }

    private async UniTask ActivateNodeMenuScene(List<string> savedIndexList)
    {
        List<StatsTreeData> nodeCountData = new();
        int count = 0;

        foreach (StatsTreeData item in treeData)
        {
            foreach (SceneConditions sc_condition in item.conditions)
            {
                count++;
                string check = DependencyParserUtility.DependencyCheck(sc_condition.sceneConditions, savedIndexList);

                if (check.Equals("1"))
                {
                    if (!nodeCountData.Contains(item))
                    {
                        nodeCountData.Add(item);
                    }
                    item.EnableNode();
                    item.uiObject.transform.SetAsLastSibling();
                    break;
                }
            }

            await UniTask.Yield();
        }

        if (nodeCountData.Count >= treeData.Count)
        {
            //if (SteamManager.Initialized)
            //{
            //    SteamUserStats.GetAchievement(_achievementKey, out bool isAchievementUnlocked);
            //    if (!isAchievementUnlocked) SteamUserStats.SetAchievement(_achievementKey);
            //}
        }
    }

    private async UniTask ActivateNodeGameScene(List<string> currentGameIndexList)
    {
        if (SceneManager.GetActiveScene().name != StaticStrings.MENU_SCENE)//Scene other than Menu
        {
            VideoPlayer vp = null;
            string currentVideoId = string.Empty;
            if (GameVideoManager.ins)
            {
                vp = GameVideoManager.ins.currentPlayer;
                DataRead currentVideoData = GameVideoManager.ins.CurrentData();
                currentVideoId = currentVideoData?.ID;
            }

            foreach (StatsTreeData item in treeData)
            {
                foreach (SceneConditions sc_condition in item.conditions)
                {
                    if (sc_condition.sceneConditions.Contains(currentVideoId))
                    {
                        if (vp != null && vp.time < sc_condition.nodeTiming)
                        {
                            continue;
                        }
                    }

                    string check = DependencyParserUtility.DependencyCheck(sc_condition.sceneConditions, currentGameIndexList);

                    if (check.Equals("1"))
                    {
                        item.EnableNode();
                        item.uiObject.transform.SetAsLastSibling();
                        currentNodeBounce?.nodeAnim.StopBounce();
                        item.nodeAnim.BounceCurrentNode();
                        currentNodeBounce = item;
                        break;
                    }
                }

                await UniTask.Yield();
            }

            if (GameManager.ins)
            {
                if (GameManager.ins.isEndingDisplayed)
                {
                    currentNodeBounce?.nodeAnim.StopBounce();
                }
            }
        }
    }

    private Color GetLockedColor()
    {
        ColorUtility.TryParseHtmlString("#333333", out Color op);
        return op;
    }

    [ContextMenu("Fillnames")]
    public void FillNames()
    {
        foreach (var item in treeData)
        {
            string[] names = item.uiObject.name.Split("sc");
            string finalName = "sc" + names[1] + ".mp4";
            item.sceneName = finalName;
            item.localisationKey = item.uiObject.name;
            Debug.Log(item.localisationKey);
        }
    }
}

[System.Serializable]
public class SceneConditions
{
    public string sceneConditions;
    public float nodeTiming;
}

[System.Serializable]
public class StatsTreeData
{
    public string sceneName;
    public string localisationKey;
    public List<SceneConditions> conditions;
    public GameObject uiObject;
    public TextMeshProUGUI nodeText;
    [HideInInspector] public CurrentNodeItem nodeAnim;

    private Image uiImage;
    private List<UISplineRenderer> uiSplineRenderer;

    private Gradient origGradient;
    private Color origColor;

    public void DisableNode(Material noalphaMat, Color lockedColor)
    {
        if (uiImage == null) { uiImage = uiObject.GetComponent<Image>(); }
        if (uiSplineRenderer == null)
        {
            uiSplineRenderer = uiObject.GetComponentsInChildren<UISplineRenderer>().ToList();
            for (int i = 0; i < uiSplineRenderer.Count; i++)
            {
                origGradient = uiSplineRenderer[i].GetColorGradient();
                origColor = uiSplineRenderer[i].color;

                uiSplineRenderer[i].color = lockedColor;
                uiSplineRenderer[i].SetColorGradient(GetGradientColor(lockedColor));
            }
        }

        if (nodeAnim == null)
        {
            nodeAnim = uiObject.GetComponent<CurrentNodeItem>();
        }

        uiImage.color = Color.black;
        uiImage.material = noalphaMat;
        nodeText.text = "";
    }


    public void EnableNode()
    {
        //uiImage = uiObject.GetComponent<Image>();
        uiImage.color = Color.white;
        uiImage.material = null;

        for (int i = 0; i < uiSplineRenderer.Count; i++)
        {
            uiSplineRenderer[i].color = Color.white;
            uiSplineRenderer[i].SetColorGradient(GetGradientColor(origColor));
        }

        if (!string.IsNullOrEmpty(localisationKey))
        {
            nodeText.text = LocalizationManager.GetTranslation(localisationKey);
        }
    }

    private Gradient GetGradientColor(Color colorToApply)
    {
        // Create a new gradient
        Gradient gradient = new Gradient();

        // Define the color keys (color and time)
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0].color = colorToApply;
        colorKeys[0].time = 0f; // Start of gradient
        colorKeys[1].color = colorToApply;
        colorKeys[1].time = 1f; // End of gradient

        // Define the alpha keys (alpha and time)
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1f; // Fully opaque
        alphaKeys[0].time = 0f;
        alphaKeys[1].alpha = 1f; // Fully opaque
        alphaKeys[1].time = 1f;

        // Assign keys to the gradient
        gradient.SetKeys(colorKeys, alphaKeys);

        return gradient;
    }

}
