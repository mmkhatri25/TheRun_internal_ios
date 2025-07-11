using I2.Loc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI endingTitle;
    [SerializeField] private Image endingImage;
    [SerializeField] SteamAchievementsManager steamAchievements;


    [Space(20)]
    public List<EndingData> endingDatas = new List<EndingData>();

    private List<int> endingSeenIndex = new List<int>();
    private string endingSceneFound = "";

    private GameUiManager gameUiManager;
    private GameManager gameManager;

    public bool isEndScreenDisplayed;


    private void Awake()
    {
        gameUiManager = FindObjectOfType<GameUiManager>();
        gameManager = FindObjectOfType<GameManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadEndingScreenAppeared();
    }


    public void CheckForEndingCount(string videoFileName)
    {
        //Count the ending scene and set the wasEndingCounted status as true
        //If video is already counted then counting is skipped

        for (int i = 0; i < endingDatas.Count; i++)
        {
            if (endingDatas[i].Scene.Contains(videoFileName))
            {
                endingSceneFound = videoFileName;

                if (endingDatas[i].wasEndingCounted)
                    break;
                else
                {
                    gameManager.endingsFound++;
                    gameManager.gameSaveData.ENDINGS_FOUND = gameManager.endingsFound;
                    gameManager.SaveGame();

                    endingDatas[i].wasEndingCounted = true;
                    SaveEndingScreenAppeared();

                    break;
                }
            }
        }
    }


    public void DisplayEnding()
    {
        // Refresh all the stats 
        // Display ending screen with Text Heading

        if (string.IsNullOrEmpty(endingSceneFound))
        {
            int count = gameUiManager.gameVideoManager.gameProgress.Count();
            string videoName = gameUiManager.gameVideoManager.gameProgress[count - 2].ID;
            CheckForEndingCount(videoName);
        }


        for (int i = 0; i < endingDatas.Count; i++)
        {
            if (endingDatas[i].Scene.Contains(endingSceneFound))
            {

                string check = DependencyCheck(endingDatas[i].Scene);

                if (check == "1")
                {
                    endingTitle.text = LocalizationManager.GetTranslation(endingDatas[i].TitleEnding);
                    endingImage.sprite = endingDatas[i].ImageEnding;
                }
                else
                {
                    Debugg.Log("The Ending Scene equation is Incorrect. Please Check");
                }

                break;
            }
        }

        gameUiManager.pauseMenuCanvas.SetActive(true);
        gameUiManager.pauseMenuContainer.SetActive(false);
        gameUiManager.endingMenuContainer.SetActive(true);

        isEndScreenDisplayed = true;

        //gameUiManager.controls.Disable();
    }


    void LoadEndingScreenAppeared()
    {
        //Load saved data for the endings screen which were already seen when game scene is loaded

        string data = "";

        data = gameManager.savePrefs.ENDING_SCREENS;

        if (!string.IsNullOrEmpty(data))
        {
            int[] savedIndex = JsonHelper.FromJson<int>(data);

            for (int i = 0; i < savedIndex.Length; i++)
            {
                endingDatas[savedIndex[i]].wasEndingCounted = true;

                endingSeenIndex.Add(savedIndex[i]);
            }
        }
    }

    void SaveEndingScreenAppeared()
    {
        foreach (var item in endingDatas)
        {
            if (item.wasEndingCounted)
                endingSeenIndex.Add(endingDatas.IndexOf(item));
        }

        string jsonProgress = JsonHelper.ToJson(endingSeenIndex.ToArray());

        gameManager.SetString(out gameManager.savePrefs.ENDING_SCREENS, jsonProgress);

        gameManager.SaveGame();

    }


    string DependencyCheck(string expression)
    {
        //Check the videos which are dependent based on Logical expression

        List<string> members = new List<string>();

        members.Add(endingSceneFound);

        expression = expression.Replace(" ", "");

        expression = expression.Replace("OR", " || ");
        expression = expression.Replace("AND", " && ");
        expression = expression.Replace("NOT", " ! ");

        Regex RE = new Regex(@"([\(\)\! ])");
        string[] tokens = RE.Split(expression);
        string eqOutput = String.Empty;
        string[] operators = new string[] { "&&", "||", "!", ")", "(" };

        foreach (string tok in tokens)
        {
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

        if (tokens.Length == 0)
        {
            eqOutput += "1";
        }

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

        return eqOutput;
    }
}

