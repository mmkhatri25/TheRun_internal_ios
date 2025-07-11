using System;
using System.Collections.Generic;
using UnityEngine;



public enum Language
{
    English = 0,
    French,
    Italian,
    German,
    Spanish,
    Portuguese,
    Turkish,
    Russian,
    Japanese,
    Korean,
    Chinese,
    Arabic
}
public enum Character
{
    Male = 0,
    // Female
}

public enum GAME_MODE
{
    GAME_MODE = 0,
    CINEMA_MODE = 1
}

public enum VOICE
{
    MALE = 0,
    FEMALE = 1
}

public enum GAME_TYPE
{
    SOLO = 0,
    COMPETITIVE,
    ARCADE
}

public enum PLAYABLE_GAME_TYPE
{
    NONE = 0,
    MONKEY_SPLASH,
    RACER_CHASER,
    SUBMARINE_GAME,
    FIGHT_GAME,
    SNOTTY_SEWER,

}

public class StaticStrings : MonoBehaviour
{
    public static string GUID => Guid.NewGuid().ToString().Substring(0, 8);

    public static string GAME_SCENE = "GameScene";
    public static string LOGO_SCENE = "LogoScene";
    public static string MENU_SCENE = "MenuScene";


    public static string GAME_SAVE = "GAME_SAVE";
    public static string GAME_PREF = "GAME_PREF";


    //-------------AUDIO----------------------------------------
    public static string GameVideo = "GameVideo";
    public static string RestartVideoFile = "restart_menu.mp4";
    public static string Checkpoint = "Checkpoint";
    public static string CheckpointTime = "CheckpointTime";

}

public class ControlKeySprites
{
    //------------------MOUSE
    public static string M_RIGHT_CLICK = "<sprite name=\"OPNEW_58\"> ";
    public static string M_DRAG_ALL_DIREC = "<sprite name=\"OPNEW_67\"> ";


    //------------------KEYBOARD
    public static string ENTER = "<sprite name=\"OPNEW_28\"> ";
    public static string ESCAPE = "<sprite name=\"OPNEW_30\"> ";
    public static string TAB = "<sprite name=\"OPNEW_172\"> ";
    public static string LEFT_ARROW = "<sprite name=\"OPNEW_55\"> ";
    public static string RIGHT_ARROW = "<sprite name=\"OPNEW_120\"> ";
    public static string UP_ARROW = "<sprite name=\"OPNEW_175\"> ";
    public static string DOWN_ARROW = "<sprite name=\"OPNEW_24\"> ";
    public static string W = "<sprite name=\"OPNEW_177\"> ";
    public static string A = "<sprite name=\"OPNEW_10\"> ";
    public static string S = "<sprite name=\"OPNEW_136\"> ";
    public static string D = "<sprite name=\"OPNEW_22\"> ";
    public static string Q = "<sprite name=\"OPNEW_116\"> ";
    public static string E = "<sprite name=\"OPNEW_25\"> ";
    public static string Z = "<sprite name=\"OPNEW_218\"> ";
    public static string X = "<sprite name=\"OPNEW_187\"> ";
    public static string C = "<sprite name=\"OPNEW_18\"> ";
    public static string SPACEBAR = "<sprite name=\"OPNEW_169\"> ";
    public static string SHIFT = "<sprite name=\"OPNEW_166\"> ";

    //--------------------PLAYSTATION
    public static string PS_R1 = "<sprite name=\"OPNEW_104\"> ";
    public static string PS_R2 = "<sprite name=\"OPNEW_105\"> ";
    public static string PS_L1 = "<sprite name=\"OPNEW_91\"> ";
    public static string PS_L2 = "<sprite name=\"OPNEW_91\"> ";
    public static string PS_R_JOYSTICK = "<sprite name=\"OPNEW_97\"> ";
    public static string PS_L_JOYSTICK = "<sprite name=\"OPNEW_83\"> ";
    public static string PS_X = "<sprite name=\"OPNEW_74\"> ";
    public static string PS_O = "<sprite name=\"OPNEW_73\"> ";
    public static string PS_TRIANGLE = "<sprite name=\"OPNEW_110\"> ";
    public static string PS_SQUARE = "<sprite name=\"OPNEW_108\"> ";


    //--------------------XBOX
    public static string XB_RB = "<sprite name=\"OPNEW_208\"> ";
    public static string XB_RT = "<sprite name=\"OPNEW_211\"> ";
    public static string XB_LB = "<sprite name=\"OPNEW_211\"> ";
    public static string XB_LT = "<sprite name=\"OPNEW_199\"> ";
    public static string XB_R_JOYSTICK = "<sprite name=\"OPNEW_201\"> ";
    public static string XB_L_JOYSTICK = "<sprite name=\"OPNEW_189\"> ";
    public static string XB_A = "<sprite name=\"OPNEW_178\"> ";
    public static string XB_B = "<sprite name=\"OPNEW_179\"> ";
    public static string XB_X = "<sprite name=\"OPNEW_215\"> ";
    public static string XB_Y = "<sprite name=\"OPNEW_216\"> ";

    //--------------------SWITCH
    public static string SW_RT = "<sprite name=\"OPNEW_160\"> ";
    public static string SW_LT = "<sprite name=\"OPNEW_147\"> ";
    public static string SW_R_JOYSTICK = "<sprite name=\"OPNEW_151\"> ";
    public static string SW_L_JOYSTICK = "<sprite name=\"OPNEW_138\"> ";
    public static string SW_A = "<sprite name=\"OPNEW_121\"> ";
    public static string SW_B = "<sprite name=\"OPNEW_122\"> ";
}


[System.Serializable]
public class GameSaveData
{
    public string GAME_PROGRESS;
    public string STATS_VIDEO_LIST;//The list of videos that are watched atleast once. Data remains saved even if new game is started
    public string CHARACTER_FOUND;
    public int DECISIONS_MADE;
    public int SCENES_DISCOVERED;
    public int ENDINGS_FOUND;
    public int LIFETIME_DEATHS;
}


[System.Serializable]
public class CharacterDataCollection
{
    public List<CharacterData> charactersDiscovered = new List<CharacterData>();
}

[System.Serializable]
public class CharacterData
{
    public string name;
    public string type;
    public float activationTime;
    public bool locked = true;

    public bool fail = false;
    public bool success = false;
    public bool revival = false;
    public bool dead = false;
}


[System.Serializable]
public class SavePrefs
{
    public int GAME_SAVE_MALE = 0;
    public string GAME_DATA_MALE;
    public string SKIP_VIDEO_LIST_MALE;

    public int GAME_COMPLETED_MALE = 0;
    public int END_CREDIT_SEEN_MALE;

    public int SCORE_A;
    public int SCORE_B;
    public int DEATHS;
    public int CURRENT_DECISIONS;

    public int CHECKPOINT_SCORE_A;
    public int CHECKPOINT_SCORE_B;

    public string ENDING_SCREENS;

    public int SUBTILE_SETTING;
    public int SUBTILE_SIZE = 0;
    public string DISPLAY_MODE;
    public string RESOLUTION;
    public int PAUSE_CHOICES;
    public int VOICE_SELECTED;//0 -> Male, 1 -> Female
    public int LANGUAGE = 0;
    public float BRIGHTNESS = 0;
    public float VOLUME = 1f;



    public void ClearData()
    {
        GAME_SAVE_MALE = 0;
        GAME_DATA_MALE = string.Empty;
        SKIP_VIDEO_LIST_MALE = string.Empty;
        GAME_COMPLETED_MALE = 0;
        END_CREDIT_SEEN_MALE = 0;

        ENDING_SCREENS = string.Empty;

        SUBTILE_SETTING = 0;
        SUBTILE_SIZE = 0;
        DISPLAY_MODE = string.Empty;
        RESOLUTION = string.Empty;
        PAUSE_CHOICES = 0;
        LANGUAGE = 0;
        BRIGHTNESS = 0;
        VOLUME = 1f;
    }



}



