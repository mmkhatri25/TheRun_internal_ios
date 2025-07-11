#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Linq;

#if !DISABLESTEAMWORKS
using Steamworks;

#endif

public class SteamAchievementsManager : MonoBehaviour
{
    [SerializeField] private string malePrefix;
    [SerializeField] private string femalePrefix;


    [SerializeField] private string decisionsMadeAPI = "_decisions_made";
    [SerializeField] private string scenesDiscoveredAPI = "_scenes_discovered";
    [SerializeField] private string endingsFoundAPI = "_endings_found";

    [SerializeField] private TrophyManager trophyManager;

    private bool isAchievementUnlocked;
    private string videoFileToCheck;

    [Space(20)]
    public List<AchievementsData> achievementsData = new List<AchievementsData>();


    public void SetAchievement(string achivementName)
    {

        Debug.Log("STEAM ACHIEVEMENT CALLED -------- " + achivementName);

        if (!SteamManager.Initialized)
            return;

#if !DISABLESTEAMWORKS

        SteamUserStats.GetAchievement(achivementName, out isAchievementUnlocked);

        if (!isAchievementUnlocked)
            SteamUserStats.SetAchievement(achivementName);

        SteamUserStats.StoreStats();
#endif

    }


    public void SetdecisionsMadeStat(float value)
    {
        string statName = StatsPrefixBasedOnCharacter() + decisionsMadeAPI;
        Debugg.Log(statName);

        SetStats(statName, value);
    }

    public void SetScenesDiscoveredStat(float value)
    {
        string statName = StatsPrefixBasedOnCharacter() + scenesDiscoveredAPI;
        SetStats(statName, value);
    }

    public void SetEndingsFoundStat(float value)
    {
        string statName = StatsPrefixBasedOnCharacter() + endingsFoundAPI;
        SetStats(statName, value);
    }




    void SetStats(string statName, float value)
    {
        if (!SteamManager.Initialized)
            return;


#if !DISABLESTEAMWORKS
        SteamUserStats.SetStat(statName, value);
#endif

    }


    string StatsPrefixBasedOnCharacter()
    {
        return malePrefix;
    }


    public void CheckForAchievement(DataRead data)
    {

        videoFileToCheck = data.ID;

        for (int i = 0; i < achievementsData.Count; i++)
        {
            if (achievementsData[i].Scene.Contains(videoFileToCheck))
            {
                string check = DependencyCheck(achievementsData[i].Scene);

                if (check == "1")
                {
                    SetAchievement(achievementsData[i].SteamAchievementName);

#if UNITY_PS4
                    trophyManager.UnlockTrophy(i + 1);//As in PS4 the 0 index trophy is for ALL trophy unlocking so index needs to start from 1
#endif

                }
                else
                {
                    Debug.Log("The Ending Scene equation is Incorrect. Please Check");
                }

                break;
            }
        }

    }




    public float GetAchievementTime(string videoName)
    {
        for (int i = 0; i < achievementsData.Count; i++)
        {
            if (achievementsData[i].Scene.Contains(videoName))
            {
                return achievementsData[i].timeToActivate;
            }
        }

        return 100000;//In case of error or no video found return time as high value to not activate achievement
    }

    public bool IsAchievementAvailable(string videoName)
    {
        for (int i = 0; i < achievementsData.Count; i++)
        {
            if (achievementsData[i].Scene.Contains(videoName))
            {
                return true;
            }
        }

        return false;
    }


    string DependencyCheck(string expression)
    {
        //Check the videos which are dependent based on Logical expression

        List<string> members = new List<string>();

        members.Add(videoFileToCheck);

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
