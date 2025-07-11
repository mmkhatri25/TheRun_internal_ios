using NaughtyAttributes;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class DependencyCheck : MonoBehaviour
{


    //public string input;

    public int SCORE_A;
    public int SCORE_B;
    private Player player { get { return ReInput.players.GetPlayer(0); } }

    public TextMeshProUGUI textelement;

    [TextArea]
    public string dependentStringCheck;

    public List<string> members = new List<string>();


    public VideoPlayer video;

    public List<int> gameProgress = new List<int>();

    //string path = Application.streamingAssetsPath + "/Subtitles/ar/" + "1981_scene_01_02.srt";




    private void Awake()
    {
        //ReadSubtitle();

        //StartCoroutine(ReadSub());


    }





    void ReadSubtitle()
    {
        string path = StreamingAssetPath.GetSubtitleFolderPath() + "en/1981_scene_01_02";


        Debug.Log(path);

        StringReader reader = new StringReader(path);

        if (!File.Exists(path))
        {
            Debug.Log("File Not Exists");
            return;
        }


        Debug.Log(reader);

        string subtitleData = reader.ReadToEnd();

        TextAsset Subtitle = new TextAsset(subtitleData);


        textelement.text = Subtitle.text;

    }







    //[ContextMenu("SaveProgressCustom")]
    [Button("Save Custom Progress")]
    public void SaveProgressCustom()
    {
        string jsonToSave = JsonHelper.ToJson(gameProgress.ToArray());

        Debug.Log(jsonToSave);

        GameSaveData gameSaveData = new GameSaveData();

        gameSaveData.GAME_PROGRESS = jsonToSave;

        string dataToSave = JsonUtility.ToJson(gameSaveData);

        PlayerPrefs.SetString(StaticStrings.GAME_SAVE, dataToSave);
    }

    //[ContextMenu("Readsubtitles")]
    //public void ReadSubtitle()
    //{
    //    StreamReader reader = new StreamReader(path);

    //    // Debug.Log(reader.ReadToEnd());

    //    string subtitleData = reader.ReadToEnd();

    //    var Subtitle = new TextAsset(subtitleData);

    //    textelement.text = ArabicFixer.Fix(Subtitle.text, true, false);
    //}



    // Start is called before the first frame update







    IEnumerator Start()
    {

        //dependentStringCheck = "";
        string eq = "1981_scene_231.mp4";

        DependencyCheckLogic(dependentStringCheck);

        // print(DependencyCheckLogic(eq));
        print(eq);

        //print(float.Parse("-305"));
        int result = -1;
        int.TryParse("sc_320.mp4", out result);
        print(result);

        string myString = "sc_320.mp4";
        int myInt = -1; // Default value

        if (int.TryParse(myString, out myInt))
        {
            // String is a valid integer, and myInt contains the parsed value
            Debug.Log("String is an integer: " + myInt);
        }
        else
        {
            // String is not a valid integer
            Debug.Log("String is not an integer. Default value used: " + myInt);
        }


        yield return new WaitForSeconds(3);



        yield return null;

    }


    private void Update()
    {


    }


    string ExpressionReplacer(string value, string valueToBeReplaced)
    {
        print(value);

        int start = value.IndexOf("*");
        int end = value.IndexOf("*", start + 1) + 1;

        print(start + "  " + end);

        string result = value.Substring(start, end - start);

        print(result);

        value = value.Replace(result, valueToBeReplaced);

        return value;
    }


    string DependencyCheckLogic(string expression)
    {
        //Check the videos which are dependent based on Logical expression
        if (expression.Contains("<") || expression.Contains(">") || expression.Contains("="))
        {

            if (expression.Contains("AND") || expression.Contains("OR") || expression.Contains("NOT"))
            {
                string result = expression.Split(new string[] { "*" }, 3, StringSplitOptions.None)[1];

                print(result);

                string isExpressionTrue = ComparisonCheck(result);

                print(isExpressionTrue);

                string updatedExpression = ExpressionReplacer(expression, isExpressionTrue);

                print(updatedExpression);
                return DependencyCheckLogic(updatedExpression);
            }
            else
                return ComparisonCheck(expression);
        }
        else
        {

            if (dependentStringCheck != null)
                members.Add(dependentStringCheck.Replace(" ", ""));

            expression = expression.Replace(" ", "");

            Debug.Log("Before Operator expression -- " + expression);

            expression = expression.Replace("OR", " || ");
            expression = expression.Replace("AND", " && ");
            expression = expression.Replace("NOT", " ! ");

            Debug.Log("Operator expression -- " + expression);

            Regex RE = new Regex(@"([\(\)\! ])");
            string[] tokens = RE.Split(expression);
            string eqOutput = String.Empty;
            string[] operators = new string[] { "&&", "||", "!", ")", "(" };

            foreach (string tok in tokens)
            {
                Debug.Log("Tokens ------ " + tok);


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

            textelement.text = "Eq : " + eqOutput;

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

            Debug.Log(eqOutput);

            textelement.text += " Result : " + eqOutput;
            return eqOutput;
        }
    }

    string ComparisonCheck(string expression)
    {
        string[] temp = expression.Split(' ');

        int value = 0;
        int valueToCompare = 0;

        if (temp[0].ToLower() == "score_a")
        {
            valueToCompare = SCORE_A;
            value = int.Parse(temp[2]);
        }

        if (temp[0].ToLower() == "score_b")
        {
            valueToCompare = SCORE_B;
            value = int.Parse(temp[2]);
        }

        Debug.Log("Value=" + value + " Value to compare : " + valueToCompare);


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


}
