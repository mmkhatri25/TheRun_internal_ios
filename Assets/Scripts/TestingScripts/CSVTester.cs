using I2.Loc;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CSVTester : MonoBehaviour
{
    [Header("Scripts")]
    public CSVDataLoader csvData;

    [Header("GameObjects")]
    public GameObject buttonPanel;

    [Header("Buttons")]
    public List<Button> optionButtons = new List<Button>();

    [Header("Text")]
    public TextMeshProUGUI finalGameSequence;
    public List<TextMeshProUGUI> optionText = new List<TextMeshProUGUI>();

    //Private Variables
    DataRead nextVideoData;
    DataRead currentVideodata;
    int startIndex = 0;
    int currentIndex = 0;


    [HideInInspector] public bool isPlaybackCompleted;
    bool hasChoices;
    bool userMadeChoice;
    bool isEndCredit;
    bool showRestartScreen;

    int userChoiceIndex;

    string dependentStringCheck;

    [Space(20)]
    public List<DataRead> gameProgress = new List<DataRead>();
    List<int> progressIndex = new List<int>();
    List<int> statsTreeIndex = new List<int>();

    private Action OnVideoPlayed;

    private Player player { get { return ReInput.players.GetPlayer(0); } }

    public int defaultVideoIndex = -1;

    private string delimeter = " -> ";


    private void Awake()
    {

        //These all events are executed simultaneously 
        OnVideoPlayed += NewSceneFound;//Increment new scene found count
        OnVideoPlayed += CheckSkipAndEndVideo;//Enable Skip button to skip video that are already played
    }

    IEnumerator Start()
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

        yield return new WaitForSeconds(1f);

        StartVideoPlayer(csvData.fileData[startIndex]);
        NextVideoManager();
        //StartCoroutine(_ShowChoiceAtTime());
    }

    private void CheckSkipAndEndVideo()
    {
        //Check for videos that are already played and enable Skip button
        //Check if next video is the end_credit set the boolean to check once end_credit is played and increment the ending count

        if (!string.IsNullOrEmpty(currentVideodata.NEXT_VIDS) && currentVideodata.NEXT_VIDS.Contains("end"))
        {
            isEndCredit = true;
        }

        if (currentVideodata != null && currentVideodata.ID.Contains("end"))
        {
            finalGameSequence.text += " END CREDITS ";
        }

        if (currentVideodata != null && currentVideodata.NEXT_VIDS.Contains("RESTART"))
        {
            finalGameSequence.text += " YOU DIED ";
            showRestartScreen = true;
        }
        else
        {
            showRestartScreen = false;
        }

    }

    void StartVideoPlayer(DataRead data)
    {

        //Set the currentIndex value as the Index for the current video being played

        if (data == null)
            return;

        currentIndex = data.index;
        currentVideodata = data;

        finalGameSequence.text += currentVideodata.ID + delimeter;

        //TotalTimeOfVideo();
        SetchoiceData();

        isPlaybackCompleted = false;
        userMadeChoice = false;

        if (!gameProgress.Contains(currentVideodata))
            gameProgress.Add(currentVideodata);

        //--------------Actions Performed after New Video Played-----------------
        OnVideoPlayed?.Invoke();
        EndReached();
        SaveGameData();
    }


    void NewSceneFound()
    {
        defaultVideoIndex = csvData.GetDefaultVideoIndex(currentVideodata);
    }

    void EndReached()
    {
        //End of the current video Playback
        //If the next video is End Credit which is checked from previous video and isEndCredit is set True then increment endings Found parameter
        //If the ending was already found previously count remains the same

        isPlaybackCompleted = true;

        if (isEndCredit)
        {
            Debug.Log("End Index value --- " + currentVideodata.index);
            isEndCredit = false;
        }

        SaveLifeTimeGameProgress();

        if (currentVideodata.CHOICES)
        {
            if (!userMadeChoice)
                return;
        }

        if (nextVideoData == null)//Game Has been completed. Return to main Menu
        {
            Debug.Log("------------GAME COMPLETED----------------");
        }
        else
        {
            NextVideoManager();
            StartVideoPlayer(nextVideoData);//Play next Video
        }
    }

    void NextVideoManager()
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
                        Debug.Log("String is an integer: " + myInt);
                    }
                    else
                    {
                        // String is not a valid integer
                        myInt = -1; // Set myInt to -1
                        Debug.Log("String is not an integer. Default value used: " + myInt);
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
    }



    //void TotalTimeOfVideo() //count the total duration of the current video and also the Time at which the choices needs to be displayed
    //{
    //    if (currentVideodata.CHOICES)//If video has choices
    //    {
    //        hasChoices = true;
    //    }
    //    else
    //        hasChoices = false;
    //}

    void SaveGameData() //Save the Game Progrss as JSON format in PlayerPrefs
    {
        if (gameProgress.Count == 0)
            return;

        List<int> indexes = new List<int>();

        foreach (DataRead item in gameProgress)
        {
            indexes.Add(item.index);
        }

        string jsonToSave = JsonHelper.ToJson(indexes.ToArray());

    }


    void SaveLifeTimeGameProgress()
    {
        //Saved the Indexes of videos which are played thoughout the Game progress
        //This is kept saved once the player completes the game
        //It is reset if the player starts new game before completing the whole game

        foreach (var item in gameProgress)
        {
            if (!progressIndex.Contains(item.index))
                progressIndex.Add(item.index);
        }

        string jsonProgress = JsonHelper.ToJson(progressIndex.ToArray());


        foreach (var item in progressIndex)
        {
            if (!statsTreeIndex.Contains(item))
                statsTreeIndex.Add(item);
        }
    }

    string CheckDepenciesNew()
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

    void SetchoiceData()
    {
        //Sets the options text and count based on the data in the CSV with localisation
        //The timer and timer UI are reset 

        hasChoices = false;

        SetOptionsTextWithLocalisation();

        buttonPanel.SetActive(true);

    }

    public void SetOptionsTextWithLocalisation()
    {
        //Get the options value from the csv file and check for localisation based on current language
        foreach (var item in optionButtons)
        {
            item.gameObject.SetActive(false);
        }

        foreach (var item in optionText)
        {
            item.color = Color.black;
        }

        //optionButtons[0].GetComponent<OptionSelectManager>().enabled = true;
        //optionButtons[1].GetComponent<OptionSelectManager>().enabled = false;
        //optionButtons[2].GetComponent<OptionSelectManager>().enabled = false;

        if (currentVideodata.CHOICES)
        {
            int numChoices = currentVideodata.NR_CHOICES;

            foreach (var item in optionButtons)
            {
                item.gameObject.SetActive(false);
            }

            if (string.IsNullOrEmpty(currentVideodata.CHOICE_DEPENDENCIES))
            {
                for (int i = 0; i < numChoices; i++)
                {
                    string optionValue = currentVideodata.ID.Split('.')[0] + "_" + (i + 1);
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

            if (PlayerPrefs.HasKey(currentVideodata.ID))
            {
                int buttonIndex = PlayerPrefs.GetInt(currentVideodata.ID);
                optionText[buttonIndex].color = Color.red;
            }
            //optionButtons[i].gameObject.SetActive(true);
        }
    }

    void Timer()//Change Timer UI. Called in Update function continuosly
    {

    }

    void SelectDefaultOption(int value)
    {
        //This method is called when user dows not select any option and
        //the video has dependencies based on option selection
        //So a default option is invoked along with its dependency 

        //print("------------------DEFAULT choice was invoked------------------");

        //userMadeChoice = true;
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

        //NextVideoManager(videoPlayer);
    }



    public void OnOptionClicked(int value)
    {
        //When user clicks on any choices button
        //The next videos corresponds to each index value, 0,1 & 2
        //There is one to one relationship for videos to be played based on option clicked
        //If there is any dependency it is given priority over option clicked

        userMadeChoice = true;
        userChoiceIndex = value;

        string[] optionValues = currentVideodata.TXT_CHOICES.Split(',');

        buttonPanel.SetActive(false);

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

        finalGameSequence.text += "(" + currentVideodata.TXT_CHOICES.Split(",")[value].Trim() + ") ";
        PlayerPrefs.SetInt(currentVideodata.ID, value);

        NextVideoManager();
        StartVideoPlayer(nextVideoData);
    }

    void GotoNextVideo(string value) //Search and save data for next video to be played in the sequence
    {
        Debug.Log("Goto next video called | Video ID => " + value);

        if (string.IsNullOrEmpty(value))
        {
            nextVideoData = null;
            //GameCompletionHandler();
        }
        else
        {

            nextVideoData = csvData.GetNextVideoData(value);
        }
    }


    string ExpressionReplacer(string value, string valueToBeReplaced)
    {
        int start = value.IndexOf("*");
        int end = value.IndexOf("*", start + 1) + 1;
        string result = value.Substring(start, end - start);
        value = value.Replace(result, valueToBeReplaced);
        return value;
    }

    string DependencyCheck(string expression)
    {
        //Check the videos which are dependent based on Logical expression
        if (expression.Contains("<") || expression.Contains(">") || expression.Contains("="))
        {
            if (expression.Contains("AND") || expression.Contains("OR") || expression.Contains("NOT"))
            {
                string result = expression.Split(new string[] { "*" }, 3, StringSplitOptions.None)[1];

                string isExpressionTrue = ComparisonCheck(result);

                string updatedExpression = ExpressionReplacer(expression, isExpressionTrue);

                Debug.Log("Updated expression - " + updatedExpression);

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

            //Debug.Log("Before Operator expression -- " + expression);

            expression = expression.Replace("OR", " || ");
            expression = expression.Replace("AND", " && ");
            expression = expression.Replace("NOT", " ! ");

            //Debug.Log("Operator expression -- " + expression);

            Regex RE = new Regex(@"([\(\)\! ])");
            string[] tokens = RE.Split(expression);
            string eqOutput = String.Empty;
            string[] operators = new string[] { "&&", "||", "!", ")", "(" };

            foreach (string tok in tokens)
            {
                //Debug.Log("Tokens ------ " + tok);

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

            Debug.Log("Binary Operation String --- " + eqOutput);

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

            Debug.Log("Result : " + eqOutput);

            return eqOutput;
        }
    }

    string ComparisonCheck(string expression)
    {
        string[] temp = expression.Split(' ');

        int value;
        //int valueToCompare = gameManager.relationshipPoints;
        int valueToCompare = 0;

        if (temp[0].ToLower().Equals("score_a"))
        {
            value = int.Parse(temp[2]);

            Debug.Log("score A compare valueToCompare = " + valueToCompare + " Value = " + value);
        }
        else if (temp[0].ToLower().Equals("score_b"))
        {
            value = int.Parse(temp[2]);

            Debug.Log("score B compare valueToCompare = " + valueToCompare + " Value = " + value);
        }

        Debug.Log("Compare Symbol - " + temp[1]);

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

            Debug.LogWarning("Score compare valueToCompare = " + valueToCompare + " Value = " + value);

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

    //Custom Hack Functions which are specific for certain scenes only
    string UpdateDefaultVideo(int value)//This is just for getting default videos for sc_310 and sc_316
    {
        Debug.Log("Custom default video + " + value);

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
                        Debug.Log("Custom default video result " + currentVideodata.nextVideos[i]);
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
