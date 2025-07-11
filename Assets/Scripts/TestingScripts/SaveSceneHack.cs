using Bayat.SaveSystem;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SaveSceneHack : MonoBehaviour
{
    [Space]
    [SerializeField] private string customJumpScene;

    private readonly List<string> gameProgress = new();

    [Button("Load Custom Video")]
    public void SaveProgressCustomVideo()
    {
        SceneDataLoader sceneDataLoader = new();
        sceneDataLoader.LoadData();
        DeleteSaveData();
        SaveProgressCustom(customJumpScene);
#if UNITY_EDITOR
        EditorApplication.EnterPlaymode();
#endif
    }

    private async void DeleteSaveData()
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


    public void SaveProgressCustom(string jumpIndex)
    {
        gameProgress.Add(jumpIndex);
        string jsonToSave = JsonHelper.ToJson(gameProgress.ToArray());

        Debug.Log(jsonToSave);

        GameSaveData gameSaveData = new();
        SavePrefs savePrefs = new();

        gameSaveData.GAME_PROGRESS = jsonToSave;

        savePrefs.GAME_SAVE_MALE = 1;

        string dataToSave = JsonUtility.ToJson(gameSaveData);
        string dataToSavePref = JsonUtility.ToJson(savePrefs);

        PlayerPrefs.SetString(StaticStrings.GAME_SAVE, dataToSave);
        PlayerPrefs.SetString(StaticStrings.GAME_PREF, dataToSavePref);

        SaveSystemAPI.WriteAllTextAsync(StaticStrings.GAME_SAVE, dataToSave);
        SaveSystemAPI.WriteAllTextAsync(StaticStrings.GAME_PREF, dataToSavePref);

        Debug.Log("SCENE JUMP SUCCESS");
    }
}

[System.Serializable]
public class SceneData
{
    public string ID;
    public int index;
}

[System.Serializable]
public class SceneDataList
{
    public List<SceneData> items;
}

public class SceneDataLoader
{
    private List<SceneData> sceneList;

    public void LoadData()
    {
        LoadJsonFromResources("data"); // assuming file name is "data.json"
    }

    void LoadJsonFromResources(string fileName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(fileName);
        if (jsonFile != null)
        {
            string wrappedJson = "{\"items\":" + jsonFile.text + "}";
            SceneDataList wrapper = JsonUtility.FromJson<SceneDataList>(wrappedJson);
            sceneList = wrapper.items;
        }
        else
        {
            Debug.LogError("JSON file not found in Resources folder.");
        }
    }

    public int GetIndexById(string id)
    {
        if (sceneList == null) return 0;

        foreach (var scene in sceneList)
        {
            if (scene.ID == id)
                return scene.index;
        }
        return 0;
    }
}
