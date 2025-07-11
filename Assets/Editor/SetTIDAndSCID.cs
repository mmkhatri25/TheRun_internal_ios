using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class SetTIDAndSCID
{
	[MenuItem("XboxOne/Reset Title Settings")]
	// Use this for initialization
	static void ResetTIDSCID()
	{
        // Param file settings.
        PlayerSettings.XboxOne.TitleId = "4C51822B";
        PlayerSettings.XboxOne.SCID = "1cd30100-93a1-4c62-a8a8-1e294c51822b";

        AssetDatabase.Refresh();
    }

    // Replace whatever Input Manager you currently have with one to work with the Nptoolkit Sample
    [MenuItem("XboxOne/Set Title Settings For Achievements 2017")]
    static void SetAchievements2017TIDSCID()
    {
        // Param file settings.
        PlayerSettings.XboxOne.TitleId = "1A81FB73";
        PlayerSettings.XboxOne.SCID = "dd5d0100-6626-4bb3-a6a9-81991a81fb73";

        AssetDatabase.Refresh();
    }
}
