using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

[Serializable]
public class SaveData
{
    public Dictionary<string, int> SavedInt = new Dictionary<string, int>();
    public Dictionary<string, float> SavedFloat = new Dictionary<string, float>();
    public Dictionary<string, string> SavedString = new Dictionary<string, string>();
}

public static class SaveManager
{
#if UNITY_SWITCH
    private static nn.account.Uid userId;
    private const string mountName = "MySave";
    private const string fileName = "MySaveData";

    public static string filePath;
    
    private static nn.fs.FileHandle fileHandle = new nn.fs.FileHandle();

    // Save journaling memory is used for each time files are created, deleted, or written.
    // The journaling memory is freed after nn::fs::CommitSaveData is called.
    // For any single time you save data, check the file size against your journaling size.
    // Check against the total save data size only when you want to be sure all files don't exceed the limit.
    // The variable journalSaveDataSize is only a value that is checked against in this code. The actual journal size is set in the
    // Unity editor in PlayerSettings > Publishing Settings > User account save data    
    private const int journalSaveDataSize = 819200;   // 16 KB. This value should be the actual journal size in bytes. This should by 32KB less than
    // the value entered in PlayerSettings > Publishing Settings > User account save data
#endif

    static string buildTitle = "Gallery_Save";
#if !UNITY_SWITCH
    static string dataPath = Application.persistentDataPath + "/Saves/";
#endif

    private static bool loadedData = false;

    private static bool initialized = false;

    public static SaveData data = new SaveData();

    public static void Initialize()
    {
#if UNITY_SWITCH && !UNITY_EDITOR
        if(!initialized){
            
            nn.account.Account.Initialize();
            nn.account.UserHandle userHandle = new nn.account.UserHandle();
            
            // Open the user that was selected before the application started.
            // This assumes that Startup user account is set to Required.
            if (nn.account.Account.TryOpenPreselectedUser(ref userHandle))
            {
                // Get the user ID of the preselected user account.
                nn.Result rresult = nn.account.Account.GetUserId(ref userId, userHandle);
                if (rresult.IsSuccess())
                {
                    Debug.LogFormat("Loaded User {0}", userId);
                }
                else
                {
                    Debug.LogErrorFormat("Failed loading user {0}",userId);
                }
            }
            else
            {
                // This method will only ever return false if StartupUserAccountOption is set to IsOptional.
                nn.Result rresult = nn.account.Account.ShowUserSelector(ref userId);
                while (nn.account.Account.ResultCancelledByUser.Includes(rresult))
                {
                    Debug.LogError("You must select a user account");
                }
                rresult = nn.account.Account.OpenUser(ref userHandle, userId);
				//rresult = nn.account.Account.GetLastOpenedUser(ref userId);
                if (rresult.IsSuccess())
                {
                    Debug.LogFormat("Loaded User {0}", userId);
                }
                else
                {
                    Debug.LogErrorFormat("Failed loading user {0}",userId);
                }
            }
            
            // Mount the save data archive as "save" for the selected user account.
            Debug.Log("Mounting save data archive");
            nn.Result result = nn.fs.SaveData.Mount(mountName, userId);
            
            if (result.IsSuccess() == false)
            {
                Debug.Log("Critical Error: File System could not be mounted.");
                result.abortUnlessSuccess();
                initialized = false;
            }
            else
            {
                Debug.LogFormat("Save Manager Initialized. Mounted {0} User {1}", mountName, userId);
                initialized = true;
            }
        }
#endif
    }

    public static void Unmount()
    {
#if UNITY_SWITCH
        nn.fs.FileSystem.Unmount(mountName);
#endif
    }

    public static void Save()
    {
        #region SwitchSave
#if UNITY_SWITCH && !UNITY_EDITOR
        byte[] dataByteArray;
        using (MemoryStream stream = new MemoryStream(journalSaveDataSize))
        {
            BinaryWriter binaryWriter = new BinaryWriter(stream);
            binaryWriter.Write(data.SavedInt.Count);
            binaryWriter.Write(data.SavedFloat.Count);
            binaryWriter.Write(data.SavedString.Count);
            foreach (var _keyValuePair in data.SavedInt)
            {
                binaryWriter.Write(_keyValuePair.Key);
                binaryWriter.Write(_keyValuePair.Value);
            }
            foreach (var _keyValuePair in data.SavedFloat)
            {
                binaryWriter.Write(_keyValuePair.Key);
                binaryWriter.Write(_keyValuePair.Value);
            }
            foreach (var _keyValuePair in data.SavedString)
            {
                binaryWriter.Write(_keyValuePair.Key);
                binaryWriter.Write(_keyValuePair.Value);
            }
            stream.Close();
            dataByteArray = stream.GetBuffer();
            
            Debug.Assert(dataByteArray.LongLength == journalSaveDataSize);
        }
                
        // This method prevents the user from quitting the game while saving.
        // This is required for Nintendo Switch Guideline 0080
        // This method must be called on the main thread.
        UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
        
        // The NintendoSDK plug-in uses a FileHandle object for file operations.
        nn.fs.FileHandle handle = new nn.fs.FileHandle();

        filePath = string.Format("{0}:/{1}", mountName, fileName);
        while (true)
        {
            // Attempt to open the file in write mode.
            nn.Result rresult = nn.fs.File.Open(ref handle, filePath, nn.fs.OpenFileMode.Write);
            // Check if file was opened successfully.
            if (rresult.IsSuccess())
            {
                Debug.LogFormat("File {0} is open and ready to write", filePath);
                // Exit the loop because the file was successfully opened.
                break;
            }
            else
            {
                if (nn.fs.FileSystem.ResultPathNotFound.Includes(rresult)) {
                    // Create a file with the size of the encoded data if no entry exists.
                    rresult = nn.fs.File.Create(filePath, dataByteArray.LongLength);
                    // Check if the file was successfully created.
                    if (!rresult.IsSuccess())
                    {
                        Debug.LogErrorFormat("Failed to create {0}: {1} - Length {2}", filePath, rresult.ToString(),dataByteArray.LongLength);
                        return;
                    }
                    else
                    {
                        Debug.LogFormat("Created Save File {0}: {1}", filePath, rresult.ToString());
                    }
                }
                else
                {
                    // Generic fallback error handling for debugging purposes.
                    Debug.LogErrorFormat("Failed to open {0}: {1}", filePath, rresult.ToString());
                    return;
                }
            }
        }
        
        // Set the file to the size of the binary data.
        nn.Result result = nn.fs.File.SetSize(handle, dataByteArray.LongLength);

        // You do not need to handle this error if you are sure there will be enough space.
        if (nn.fs.FileSystem.ResultUsableSpaceNotEnough.Includes(result))
        {
            Debug.LogErrorFormat("Insufficient space to write {0} bytes to {1}", dataByteArray.LongLength, filePath);
            return;
        }
        
        // NOTE: Calling File.Write() with WriteOption.Flush incurs two write operations.
        result = nn.fs.File.Write(handle, 0, dataByteArray, dataByteArray.LongLength, nn.fs.WriteOption.Flush);

        // You do not need to handle this error if you are sure there will be enough space.
        // if (nn.fs.ResultUsableSpaceNotEnough.Includes(result))
        // {
        //     Debug.LogErrorFormat("Insufficient space to write {0} bytes to {1}", dataByteArray.LongLength, filePath);
        // }

        // The file must be closed before committing.
        nn.fs.File.Close(handle);

        // Verify that the write operation was successful before committing.
        if (!result.IsSuccess())
        {
            Debug.LogErrorFormat("Failed to write {0}: {1}", filePath, result.ToString());
            return;
        }
        
        // This method moves the data from the journaling area to the main storage area.
        // If you do not call this method, all changes will be lost when the application closes.
        // Only call this when you are sure that all previous operations succeeded.
        nn.fs.FileSystem.Commit(mountName);

        // End preventing the user from quitting the game while saving. 
        // This is required for Nintendo Switch Guideline 0080
        UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
#endif
        #endregion

        #region NotSwitchSave
#if !UNITY_SWITCH
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }

        string saveDataPath = dataPath + buildTitle + ".sf";

        FileStream fileStream = File.Open(saveDataPath, FileMode.Create);
        using (BinaryWriter writer = new BinaryWriter(fileStream))
        {
            writer.Write(data.SavedInt.Count);
            writer.Write(data.SavedFloat.Count);
            writer.Write(data.SavedString.Count);
            foreach (var _keyValuePair in data.SavedInt)
            {
                writer.Write(_keyValuePair.Key);
                writer.Write(_keyValuePair.Value);
                //Debug.Log("Writing "+_keyValuePair.Key+" / "+_keyValuePair.Value);
            }
            foreach (var _keyValuePair in data.SavedFloat)
            {
                writer.Write(_keyValuePair.Key);
                writer.Write(_keyValuePair.Value);
            }
            foreach (var _keyValuePair in data.SavedString)
            {
                writer.Write(_keyValuePair.Key);
                writer.Write(_keyValuePair.Value);
            }
        }
        fileStream.Close();
        Debug.Log("Saved to " + saveDataPath);
#endif
        #endregion
    }
    
    public static bool LoadData()
    {
        if (!initialized) Initialize();
        
        SaveManager.loadedData = true;

        data.SavedInt.Clear();
        data.SavedFloat.Clear();
        data.SavedString.Clear();
        
        #region SwitchLoad
#if UNITY_SWITCH && !UNITY_EDITOR
        Debug.Log("Loading data");

        // The NintendoSDK plug-in uses a FileHandle object for file operations.
        nn.fs.FileHandle handle = new nn.fs.FileHandle();

        filePath = string.Format("{0}:/{1}", mountName, fileName);
// Attempt to open the file in read-only mode.
        nn.Result result = nn.fs.File.Open(ref handle, filePath, nn.fs.OpenFileMode.Read);
        if (!result.IsSuccess())
        {
            if (nn.fs.FileSystem.ResultPathNotFound.Includes(result))
            {
                Debug.LogFormat("File not found: {0}", filePath);
                return false;
            }
            else
            {
                Debug.LogErrorFormat("Unable to open {0}: {1}", filePath, result.ToString());
                return false;
            }
        }

        // Get the file size.
        long fileSize = 0;
        nn.fs.File.GetSize(ref fileSize, handle);
        // Allocate a buffer that matches the file size.
        byte[] bytedata = new byte[fileSize];
        // Read the save data into the buffer.
        nn.fs.File.Read(handle, 0, bytedata, fileSize);
        // Close the file.
        nn.fs.File.Close(handle);

        using (MemoryStream stream = new MemoryStream(bytedata))
        {
            BinaryReader reader = new BinaryReader(stream);
            int IntCount = reader.ReadInt32();
            int FloatCount = reader.ReadInt32();
            int StringCount = reader.ReadInt32();
            for (int i = 0; i < IntCount; i++)
            {
                data.SavedInt[reader.ReadString()] = reader.ReadInt32();
            }
            for (int i = 0; i < FloatCount; i++)
            {
                data.SavedFloat[reader.ReadString()] = reader.ReadSingle();
            }
            for (int i = 0; i < StringCount; i++)
            {
                data.SavedString[reader.ReadString()] = reader.ReadString();
            }
        }
#endif
        #endregion
        Debug.Log("Data Loaded");
        #region NonSwitchLoad
#if !UNITY_SWITCH
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }
        string saveDataPath = dataPath + buildTitle + ".sf";

        if (File.Exists(saveDataPath))
        {
            FileStream fileStream = File.Open(saveDataPath, FileMode.Open);
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                int IntCount = reader.ReadInt32();
                int FloatCount = reader.ReadInt32();
                int StringCount = reader.ReadInt32();
                for (int i = 0; i < IntCount; i++)
                {
                    data.SavedInt[reader.ReadString()] = reader.ReadInt32();
                }
                for (int i = 0; i < FloatCount; i++)
                {
                    data.SavedFloat[reader.ReadString()] = reader.ReadSingle();
                }
                for (int i = 0; i < StringCount; i++)
                {
                    data.SavedString[reader.ReadString()] = reader.ReadString();
                }
            }
            fileStream.Close();
        }
#endif
        #endregion
        return true;
    }
    
    public static bool HasKey(string key)
    {
        bool haskey = false;
        if (data.SavedFloat.ContainsKey(key)) haskey = true;
        if (data.SavedInt.ContainsKey(key)) haskey = true;
        if (data.SavedString.ContainsKey(key)) haskey = true;
        return haskey;
    }

    
    public static int GetInt(string key, int defaultValue = 0)
    {
        if (!initialized) Initialize();

        if (!loadedData) LoadData();

        int returnValue = defaultValue;
        if (!SaveManager.data.SavedInt.TryGetValue(key, out returnValue))
        {
            returnValue = PlayerPrefs.GetInt(key, defaultValue);
        }
        return returnValue;
    }

    public static float GetFloat(string key, float defaultValue = 0)
    {
        if (!initialized) Initialize();

        if (!loadedData) LoadData();

        float returnValue = defaultValue;
        if (!SaveManager.data.SavedFloat.TryGetValue(key, out returnValue))
        {
            returnValue = PlayerPrefs.GetFloat(key, defaultValue);
        }
        return returnValue;
    }

    public static string GetString(string key, string defaultValue = "")
    {
        if (!initialized) Initialize();

        if (!loadedData) LoadData();

        string returnValue = defaultValue;
        if (!SaveManager.data.SavedString.TryGetValue(key, out returnValue))
        {
            returnValue = PlayerPrefs.GetString(key, defaultValue);
        }
        return returnValue;
    }

    public static void SetInt(string key, int setValue)
    {
        data.SavedInt[key] = setValue;
    }

    public static void SetFloat(string key, float setValue)
    {
        data.SavedFloat[key] = setValue;
    }

    public static void SetString(string key, string setValue)
    {
        data.SavedString[key] = setValue;
    }

#if UNITY_EDITOR
    [MenuItem("Edit/SaveManagerClear", false)]
#endif
    public static void DeleteAll()
    {
        data.SavedInt.Clear();
        data.SavedFloat.Clear();
        data.SavedString.Clear();
        Save();
    }


    public static void DeleteKey(string keyToDel)
    {
        if (data.SavedInt.ContainsKey(keyToDel)) data.SavedInt.Remove(keyToDel);
        if (data.SavedFloat.ContainsKey(keyToDel)) data.SavedFloat.Remove(keyToDel);
        if (data.SavedString.ContainsKey(keyToDel)) data.SavedString.Remove(keyToDel);
    }

}