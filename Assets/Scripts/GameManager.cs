using Bayat.SaveSystem;
using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_SWITCH && !UNITY_EDITOR
using UnityEngine.Switch;
#endif

public class GameManager : GallerySaveSystem
{
    public bool usePlayerPref;

    public static GameManager ins;

    public static Action OnSettingsChanged;

    public string subtitleFolderName;
    public string currentLanguage;
    //public string mainCharacterName;
    public int subtitleSize;

    public bool isSubtitleON;
    public bool isPauseChoicesON;
    public bool isGameCompleted;
    public bool isEndingDisplayed;
    public bool isCheckPointCanvasActive;

    [Header("LifeTime Stats")]
    public int decisionsMade;
    public int scenesDiscovered;
    public int endingsFound;
    public int lifetime_deaths;
    public int deathsFoundInGameCount;

    [Header("Current Playthrough Stats")]
    public int SCORE_A;
    public int SCORE_B;
    public int CURRENT_DEATHS;
    public int CURRENT_DECISIONS;
    public float CHECKPOINT_VIDEO_TIME;

    [HideInInspector] public SubtitleDisplayer subtitleDisplayer;
    [HideInInspector] public SubtitleDisplayer subtitleDisplayerAlternate;
    GameUiManager gameUiManager;
    SteamAchievementsManager steamAchievements;

    [Header("------SAVE DATA------")]
    public GameSaveData gameSaveData;

    public SavePrefs savePrefs = new SavePrefs();


    public override void Awake()
    {
        base.Awake();

        if (ins == null)
        {
            ins = this;
            DontDestroyOnLoad(this);

            OnSettingsChanged += SetGameSettingsData;

#if UNITY_SWITCH && !UNITY_EDITOR
        Notification.notificationMessageReceived += NotificationMessageHandler;
#endif


        }
        else
        {
            Destroy(gameObject);
        }

        //OnSettingsChanged?.Invoke();

        LoadAllPref();
    }

    private void Start()
    {
        OnSettingsChanged?.Invoke();
    }

    public void SetGameSettingsData()//Sets all the settings data based on Prefs to make settings similar in Menu and Game Scene
    {
        currentLanguage = LocalizationManager.CurrentLanguage;
        subtitleSize = savePrefs.SUBTILE_SIZE;
        isSubtitleON = Convert.ToBoolean(savePrefs.SUBTILE_SETTING);
        isPauseChoicesON = Convert.ToBoolean(savePrefs.PAUSE_CHOICES);

        SetSubtitleDisplay(isSubtitleON);
        SetTimerDisplay(isPauseChoicesON);

    }

    public void SetTimerDisplay(bool value)//If Pause choices is ON display Timer else disable Timer UI
    {
        gameUiManager = FindObjectOfType<GameUiManager>();

        value = !value;

        if (gameUiManager != null)
        {
            gameUiManager.timer.SetActive(value);
        }
    }

    void SetSubtitleDisplay(bool value)//Enable or disable subtitle block based on setting value
    {
        if (GameVideoManager.ins == null)
        {
            return;
        }

        subtitleDisplayer = GameVideoManager.ins.subtitleDisplayer;
        subtitleDisplayerAlternate = GameVideoManager.ins.subtitleDisplayerAlternate;
        gameUiManager = FindObjectOfType<GameUiManager>();

        if (subtitleDisplayer != null)
        {
            subtitleDisplayer.subtitleDisplay.SetActive(value);
            subtitleDisplayerAlternate.subtitleDisplay.SetActive(!value);
            subtitleDisplayer.SetSubtitleSize();

            if (value == true)
                gameUiManager.SetChoicesPosition();
            else
                gameUiManager.ReSetChoicesPosition();
        }
    }

    public void ResetGameData()
    {

        SCORE_A = 0;
        SCORE_B = 0;
        CURRENT_DEATHS = 0;
        CURRENT_DECISIONS = 0;

        decisionsMade = gameSaveData.DECISIONS_MADE;
        lifetime_deaths = gameSaveData.LIFETIME_DEATHS;
        scenesDiscovered = gameSaveData.SCENES_DISCOVERED;
        endingsFound = gameSaveData.ENDINGS_FOUND;

        string statsTreeData = gameSaveData.STATS_VIDEO_LIST;//The tree data should be saved and never reset

        gameSaveData = new GameSaveData();


        gameSaveData.LIFETIME_DEATHS = lifetime_deaths;
        gameSaveData.STATS_VIDEO_LIST = statsTreeData;
        gameSaveData.DECISIONS_MADE = decisionsMade;
        gameSaveData.SCENES_DISCOVERED = scenesDiscovered;
        gameSaveData.ENDINGS_FOUND = endingsFound;


        //if (isGameCompleted)
        //{
        //    //Here the endings found and scenesDiscovered needs to be saved while other parameters needs to be reset
        //    endingsFound = gameSaveData.ENDINGS_FOUND;
        //    gameSaveData = new GameSaveData();
        //    gameSaveData.ENDINGS_FOUND = endingsFound;
        //}
        //else
        //{
        //    //As the Game was not completed even once All the parameters can be reset
        //    gameSaveData = new GameSaveData();
        //}


        //characterFound.charactersDiscovered.Clear();//Clear list of found characters
    }

    public void SetSubtitleFolder()
    {

        //Set the subtitle folder based on current language

        int index = savePrefs.LANGUAGE;

        Debugg.Log("Language index " + index);

        switch (index)
        {
            case (int)Language.English:
                subtitleFolderName = "en"; break;
            case (int)Language.French:
                subtitleFolderName = "fr"; break;
            case (int)Language.Italian:
                subtitleFolderName = "it"; break;
            case (int)Language.German:
                subtitleFolderName = "de"; break;
            case (int)Language.Spanish:
                subtitleFolderName = "es"; break;
            case (int)Language.Portuguese:
                subtitleFolderName = "ptbr"; break;
            case (int)Language.Turkish:
                subtitleFolderName = "tr"; break;
            case (int)Language.Russian:
                subtitleFolderName = "ru"; break;
            case (int)Language.Japanese:
                subtitleFolderName = "ja"; break;
            case (int)Language.Korean:
                subtitleFolderName = "ko"; break;
            case (int)Language.Chinese:
                subtitleFolderName = "zhs"; break;
            case (int)Language.Arabic:
                subtitleFolderName = "ar"; break;

        }
    }

    public void RestartGameFromCheckPoint()
    {
        string checkPointVideoName = PlayerPrefs.GetString(StaticStrings.Checkpoint, "none");
        float checkPointVideoTime = PlayerPrefs.GetFloat(StaticStrings.CheckpointTime, 0);

        if (checkPointVideoName != "none")
        {
            string savedJson = gameSaveData.GAME_PROGRESS;

            if (string.IsNullOrEmpty(savedJson))
                return;

            string[] savedIndex = JsonHelper.FromJson<string>(savedJson);

            List<string> indexes = new();

            for (int i = 0; i < savedIndex.Length; i++)
            {
                indexes.Add(savedIndex[i]);
                if (checkPointVideoName == savedIndex[i])
                {
                    break;
                }
            }

            string jsonToSave = JsonHelper.ToJson(indexes.ToArray());

            gameSaveData.GAME_PROGRESS = jsonToSave;

            SCORE_A = savePrefs.CHECKPOINT_SCORE_A;
            SCORE_B = savePrefs.CHECKPOINT_SCORE_B;
            CHECKPOINT_VIDEO_TIME = checkPointVideoTime;

            StartCoroutine(_RestartFromCheckPoint());
        }
        else
        {
            Debugg.Log("Checkpoint is NULL");
        }
    }

    IEnumerator _RestartFromCheckPoint()
    {
        SaveGame();

        yield return new WaitForSeconds(0.8f);

        SceneManager.LoadScene(StaticStrings.GAME_SCENE);
    }


    public void SaveGame()
    {
        //Convert the gameSaveData object to JSON and save into prefs for particular character

        string dataToSave = JsonUtility.ToJson(gameSaveData);
        SetInt(out savePrefs.GAME_SAVE_MALE, 1);
        SetString(out savePrefs.GAME_DATA_MALE, dataToSave);

        SetInt(out savePrefs.SCORE_A, SCORE_A);
        SetInt(out savePrefs.SCORE_B, SCORE_B);
        SetInt(out savePrefs.CURRENT_DECISIONS, CURRENT_DECISIONS);
        SetInt(out savePrefs.DEATHS, CURRENT_DEATHS);

        StartCoroutine(_SaveGame(dataToSave));
    }
    IEnumerator _SaveGame(string dataToSave)
    {
        SaveAllPref();

        yield return new WaitForSeconds(0.5f);

        Save(dataToSave);

        SaveStatsOnSteam();
    }


    void SaveStatsOnSteam()
    {
        if (steamAchievements == null)
            steamAchievements = FindObjectOfType<SteamAchievementsManager>();

        steamAchievements.SetdecisionsMadeStat(gameSaveData.DECISIONS_MADE);
        steamAchievements.SetScenesDiscoveredStat(gameSaveData.SCENES_DISCOVERED);
        steamAchievements.SetEndingsFoundStat(gameSaveData.ENDINGS_FOUND);
    }


    public void LoadSavedGame() //Load gameSavedata object from the saved Prefs
    {
        if (savePrefs.GAME_SAVE_MALE == 1)
        {
            //gameSaveData = GetGameSaveData(StaticStrings.GAME_DATA_MALE);
            Load();

            isGameCompleted = Convert.ToBoolean(savePrefs.GAME_COMPLETED_MALE);

            SetSavedStats(gameSaveData);

        }

    }

    public GameSaveData GetGameSaveData(string character)
    {
        string dataToLoad = PlayerPrefs.GetString(character);
        return JsonUtility.FromJson<GameSaveData>(dataToLoad);
    }

    void SetSavedStats(GameSaveData gameSaveData)//Sets the parameters for current stats
    {
        decisionsMade = gameSaveData.DECISIONS_MADE;
        scenesDiscovered = gameSaveData.SCENES_DISCOVERED;
        endingsFound = gameSaveData.ENDINGS_FOUND;
        lifetime_deaths = gameSaveData.LIFETIME_DEATHS;

        SCORE_A = savePrefs.SCORE_A;
        SCORE_B = savePrefs.SCORE_B;
        CURRENT_DEATHS = savePrefs.DEATHS;
        CURRENT_DECISIONS = savePrefs.CURRENT_DECISIONS;
    }

    private void OnDestroy()
    {
        //PlayerPrefs.Unmount();
    }


    ////////////////////////////SAVING LOADING SECTION////////////////////////////////////////////////////////
    public override void Save(string data)
    {
        if (usePlayerPref)
        {

#if UNITY_SWITCH
            InternalSetString(StaticStrings.GAME_SAVE, data);

            SaveManager.Save();
#else
            PlayerPrefs.SetString(StaticStrings.GAME_SAVE, data);

#endif
            PlayerPrefs.Save();

        }
        else
            SaveSystemAPI.WriteAllTextAsync(StaticStrings.GAME_SAVE, data);
    }

    public override async void Load()
    {
        if (usePlayerPref)
        {
            string data = string.Empty;

#if UNITY_SWITCH

            data = InternalGetString(StaticStrings.GAME_SAVE, string.Empty);
           
#else
            data = PlayerPrefs.GetString(StaticStrings.GAME_SAVE, string.Empty);

#endif

            if (string.IsNullOrEmpty(data))
                gameSaveData = new GameSaveData();
            else
                gameSaveData = JsonUtility.FromJson<GameSaveData>(data);

        }
        else
        {
            string data = await SaveSystemAPI.ReadAllTextAsync(StaticStrings.GAME_SAVE);
            gameSaveData = JsonUtility.FromJson<GameSaveData>(data);
        }

    }

    public async void LoadAllPref()
    {
        if (usePlayerPref)
        {
            string data = string.Empty;

#if UNITY_SWITCH
            data = InternalGetString(StaticStrings.GAME_PREF, string.Empty);
            //data = SaveManager.GetString(StaticStrings.GAME_PREF);
#else
            data = PlayerPrefs.GetString(StaticStrings.GAME_PREF, string.Empty);
#endif

            if (string.IsNullOrEmpty(data))
                savePrefs = new SavePrefs();
            else
                savePrefs = JsonUtility.FromJson<SavePrefs>(data);
        }
        else
        {
            if (await SaveSystemAPI.ExistsAsync(StaticStrings.GAME_PREF))
            {
                string data = await SaveSystemAPI.ReadAllTextAsync(StaticStrings.GAME_PREF);
                savePrefs = JsonUtility.FromJson<SavePrefs>(data);
            }
            else
            {
                SaveAllPref();
            }
        }
    }

    public void SaveAllPref()
    {
        string dataToSave = JsonUtility.ToJson(savePrefs);

        if (usePlayerPref)
        {


#if UNITY_SWITCH
            InternalSetString(StaticStrings.GAME_PREF, dataToSave);
            // SaveManager.SetString(StaticStrings.GAME_PREF, dataToSave);
            // SaveManager.Save();
#else
            PlayerPrefs.SetString(StaticStrings.GAME_PREF, dataToSave);
#endif

            PlayerPrefs.Save();

        }
        else
            SaveSystemAPI.WriteAllTextAsync(StaticStrings.GAME_PREF, dataToSave);
    }


    public void SetString(out string container, string data)
    {
        container = data;
    }

    public void SetInt(out int container, int data)
    {
        container = data;
    }

    public void SetFloat(out float container, float data)
    {
        container = data;
    }

    public override async Task<string> GetString(string ID)
    {
        string data = await SaveSystemAPI.ReadAllTextAsync(ID);

        return data;
    }


    public static async void Delete()
    {
        PlayerPrefs.DeleteAll();

        bool result = await SaveSystemAPI.ExistsAsync(Character.Male.ToString());
        if (result)
        {
            await SaveSystemAPI.DeleteAsync(Character.Male.ToString());
        }

        result = await SaveSystemAPI.ExistsAsync(StaticStrings.GAME_PREF);
        if (result)
        {
            await SaveSystemAPI.DeleteAsync(StaticStrings.GAME_PREF);
        }
    }



    void RequestToClose()
    {
        Debugg.Log("Attempting to close");



#if UNITY_SWITCH && !UNITY_EDITOR
        //Save the Game Data
        PlayerPrefsSwitch.PlayerPrefsSwitch.Term();

        SaveManager.Unmount();

#elif UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }

#if UNITY_SWITCH && !UNITY_EDITOR

    void NotificationMessageHandler(Notification.Message message)
    {
        Debug.Log("Received Notifcation Handler: " + message.ToString());
        switch (message)
        {
            case Notification.Message.ExitRequest:

                SaveAllPref();
                string dataToSave = JsonUtility.ToJson(gameSaveData);
                Save(characterSelected, dataToSave);

                RequestToClose();
                Debug.Log("Requested to Close");
                break;

            case Notification.Message.FocusStateChanged:
                SaveAllPref();
                dataToSave = JsonUtility.ToJson(gameSaveData);
                Save(characterSelected, dataToSave);
                break;
        }
    }
#endif




#if UNITY_EDITOR
    [MenuItem("Edit/SaveManagerClear", false)]
#endif
    public void DeleteAll()
    {
        savePrefs = new SavePrefs();
        gameSaveData = new GameSaveData();
        Delete();
        ResetGameData();
    }


    /////////////////////FOR SWITCH SAVING//////////////////////////////////////

    void InternalSetString(string key, string value)
    {

        key += "_SwitchPlayerPrefs"; // Adds a suffix to the key to avoid overriding any other value saved (could be anything)
        int oldCount = 0;

        if (!PlayerPrefs.HasKey(key + "_Count")) // Gets the length of the previous string saved if any
            oldCount = PlayerPrefs.GetInt(key + "_Count");

        int count = value.Length;
        PlayerPrefs.SetInt(key + "_Count", count); // Updates the "count" value to the length of the current string

        int roundedCount = count - (count % 2); // "round" the count to an even value

        int i = 0;
        for (; i < roundedCount; i += 2) // iterates 2 by 2 into the string 
        {
            int charPair = value[i] << 16; // stores the first character of the pair into the int then shift from 16 bit to make space for the second char
            charPair += value[i + 1]; // adds the second character

            PlayerPrefs.SetInt(key + $"_[{i / 2}]", charPair); // saves the pair adding the pair ID as suffix to the key
        }

        if (roundedCount < count) // if the count was odd, saves the last char
        {
            int last = value[count - 1];
            PlayerPrefs.SetInt(key + $"_[{(roundedCount / 2) + 1}]", last);
            i += 2;
        }

        for (; i < oldCount; i += 2) // delete any remaining keys from the the previously saved string
        {
            PlayerPrefs.DeleteKey(key + $"_[{i / 2}]");
        }

        Debugg.Log($"PlayerPrefs : Setting String [{key}] _to_ {value}");
    }

    string InternalGetString(string key, string defaultValue)
    {
        key += "_SwitchPlayerPrefs"; // Adds the same suffix than in InternalSetString
        if (!PlayerPrefs.HasKey(key + "_Count")) return defaultValue; // if there is no count saved then it means that this string was never saved > return default;

        int count = PlayerPrefs.GetInt(key + "_Count"); // Gets the length of the previous string saved with this key

        if (count <= 0) return string.Empty;

        int roundedCount = count - (count % 2); // "round" the count to an even value
        StringBuilder str = new StringBuilder(count);

        for (int i = 0; i < roundedCount / 2; i++)// iterates 2 by 2 into the string
        {
            int charPair = PlayerPrefs.GetInt(key + $"_[{i}]"); // Gets the pair of characters

            str.Append((char)(charPair >> 16)); // adds at the end of the string the first character (the left one) by shifting 16 bits to the right
                                                // adds at the end of the string the second character by shifting 16 bits to the left to "erase" the left char then 16 bits to the right again
            str.Append((char)((charPair << 16) >> 16));
        }

        if (roundedCount < count) // if the count was odd, gets the last char
        {
            char last = (char)PlayerPrefs.GetInt(key + $"_[{(roundedCount / 2) + 1}]");
            str.Append(last);
        }

        Debug.Log($"PlayerPrefs : Getting String [{key}] ___ {str}"); // if str is good here if meens that everything went well 
        return str.ToString();
    }



}


