using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;


public class CSVDataLoader : MonoBehaviour
{
    //This Script reads the data from the CSV file and loads the data in the DataRead object
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private int DownloadeVideoPathIndex;

    [ShowNonSerializedField]
    readonly string fileName = "The_Run";

    public List<DataRead> fileData = new();
    public List<DataReadSimple> fileDataSimple = new();

    private float result;
    private string videoFolderPath;

    //---------COLUMN NAMES-----------
    private const string ID = "ID";
    private const string CHOICES = "CHOICES";
    private const string NR_CHOICES = "NR_CHOICES";
    private const string CHOICE_IMAGES = "CHOICE_IMAGES";
    private const string CHECKPOINT = "CHECKPOINT";
    private const string SKIP_CHECKPOINT = "SKIP_CHECKPOINT";
    private const string SKIP_CP_DEPENDENCY = "SKIP_CP_DEPENDENCY";
    private const string CHOICE_DEPENDENCIES = "CHOICE_DEPENDENCIES";
    private const string SHUFFLE_CHOICES = "SHUFFLE_CHOICES";
    private const string TXT_CHOICES = "TXT_CHOICES";
    private const string SCORE_A = "SCORE_A";
    private const string SCORE_A_ANI = "SCORE_A_ANI";
    private const string SCORE_B = "SCORE_B";
    private const string SCORE_B_ANI = "SCORE_B_ANI";
    private const string LOC_GRAPHIC = "LOC_GRAPHIC";
    private const string NEXT_SC_DEPENDENCIES = "NEXT_SC_DEPENDENCIES";
    private const string TIME_CHOICES = "TIME_CHOICES";
    private const string DEF_VID = "DEF_VID";
    private const string NEXT_VIDS = "NEXT_VIDS";
    private const string SUB = "SUB";

    private void Awake()
    {
        videoFolderPath = StreamingAssetPath.GetVideoFolderPath();
    }

    // Start is called before the first frame update
    void Start()
    {

        if (string.IsNullOrEmpty(fileName))
            return;

        List<Dictionary<string, object>> data = CSVReader.Read(fileName);

        int indexValue = 0;//index counter is separate cause some rows of CSV can be empty leading to wrong index

        for (var i = 0; i < data.Count; i++)
        {
            DataRead dr = new();
            DataReadSimple drs = new();

            dr.ID = data[i][ID].ToString();
            drs.ID = data[i][ID].ToString();


            if (string.IsNullOrEmpty(dr.ID))
            {
                continue;
            }

            dr.index = indexValue;
            drs.index = indexValue;
            indexValue++;

#if UNITY_ANDROID
            dr.videoFileUrl = Path.Combine(videoFolderPath, dr.ID);
#else
            //Debug.Log(dr.ID);
            if (i >= DownloadeVideoPathIndex)
            {
                dr.videoFileUrl = "file:///" + Path.Combine(Application.persistentDataPath, dr.ID);

            }
            else
            {
                dr.videoFileUrl = "file:///" + Path.Combine(videoFolderPath, dr.ID);

            }
#endif

            if (data[i][CHOICES].ToString().ToLower() == "true")
                dr.CHOICES = true;
            else
                dr.CHOICES = false;

            if (!string.IsNullOrEmpty(data[i][CHECKPOINT].ToString()))
            {
                string[] checkPointData = data[i][CHECKPOINT].ToString().Split(',');

                if (checkPointData[0].Trim().ToLower() == "true")
                    dr.CHECKPOINT = true;

                float.TryParse(checkPointData[1].Trim().ToString(), out dr.CHECKPOINT_TIME);
            }

            if (!string.IsNullOrEmpty(data[i][SKIP_CHECKPOINT].ToString()))
            {
                string checkPointData = data[i][SKIP_CHECKPOINT].ToString();

                float.TryParse(checkPointData.Trim().ToString(), out dr.SKIP_CHECKPOINT);
            }

            if (!string.IsNullOrEmpty(data[i][SKIP_CP_DEPENDENCY].ToString()))
            {
                string[] checkPointData = data[i][SKIP_CP_DEPENDENCY].ToString().Split(',');

                foreach (string item in checkPointData)
                {
                    string itemWithFormat = item + ".mp4";
                    if (itemWithFormat.Equals(dr.ID))
                    {
                        continue;
                    }
                    dr.skipCheckpointDependecny.Add(itemWithFormat);
                }
            }


            if (float.TryParse(data[i][NR_CHOICES].ToString(), out result))
                dr.NR_CHOICES = (int)result;

            dr.TXT_CHOICES = data[i][TXT_CHOICES].ToString();
            dr.CHOICE_IMAGE = data[i][CHOICE_IMAGES].ToString().Trim();

            for (var choiceCountIndex = 0; choiceCountIndex < dr.NR_CHOICES; choiceCountIndex++)
            {
                dr.choicesLocalisationKey.Add(dr.ID.Split('.')[0] + "_" + (choiceCountIndex + 1));
            }

            dr.CHOICE_DEPENDENCIES = data[i][CHOICE_DEPENDENCIES].ToString();

            if (!string.IsNullOrEmpty(data[i][SHUFFLE_CHOICES].ToString()))
            {
                if (data[i][SHUFFLE_CHOICES].ToString().ToLower() == "true")
                    dr.SHUFFLE_CHOICES = true;
            }

            if (float.TryParse(data[i][DEF_VID].ToString(), out result))
                dr.DEFAULT_CHOICE = (int)result;

            if (float.TryParse(data[i][SCORE_A].ToString(), out result))
                dr.SCORE_A = (int)result;

            if (float.TryParse(data[i][SCORE_B].ToString(), out result))
                dr.SCORE_B = (int)result;

            if (float.TryParse(data[i][TIME_CHOICES].ToString(), out result))
                dr.TIME_CHOICES = result;



            dr.SCORE_A_ANI = data[i][SCORE_A_ANI].ToString();
            dr.SCORE_B_ANI = data[i][SCORE_B_ANI].ToString();
            dr.LOCALISATION = data[i][LOC_GRAPHIC].ToString();

            dr.DEPENDENCIES = data[i][NEXT_SC_DEPENDENCIES].ToString();
            dr.DEF_VID = data[i][DEF_VID].ToString();
            dr.NEXT_VIDS = data[i][NEXT_VIDS].ToString();
            dr.SUB = data[i][SUB].ToString();

            if (!string.IsNullOrEmpty(dr.LOCALISATION))
            {
                string[] localTemp = dr.LOCALISATION.Trim().Split('+');

                for (int k = 0; k < localTemp.Length; k++)
                {
                    LocalisationKeyManager lowKey = new LocalisationKeyManager();

                    string[] timeData = localTemp[k].Split(',');

                    float startTime = float.Parse(timeData[0].Trim());
                    float waitTime = float.Parse(timeData[1].Trim());

                    lowKey.timeToActivate = startTime;
                    lowKey.waitDuration = waitTime;

                    dr.LOCALISATION_KEY.Add(lowKey);
                }
            }

            string[] temp = dr.NEXT_VIDS.Split(',');
            for (int j = 0; j < temp.Length; j++)
            {
                temp[j] = temp[j].Trim();
            }
            dr.nextVideos.AddRange(temp.ToList());

            if (!string.IsNullOrEmpty(dr.DEPENDENCIES))
            {
                temp = dr.DEPENDENCIES.Split(',');

                for (int j = 0; j < temp.Length; j++)
                {
                    temp[j] = temp[j].Trim();
                }

                dr.dependentVideos.AddRange(temp.ToList());
            }

            if (dr.SHUFFLE_CHOICES)
            {
                ShuffleChoices(dr);
            }


            fileData.Add(dr);
            fileDataSimple.Add(drs);
        }

        //string jsonToSave = JsonHelper.ToJson(fileDataSimple.ToArray());

        //Debug.Log(jsonToSave);

        //text.text = jsonToSave;

    }

    private void ShuffleChoices(DataRead dr)
    {
        if (!string.IsNullOrEmpty(dr.DEPENDENCIES))
        {
            return;
        }

        if (dr.SHUFFLE_CHOICES)
        {
            string options = dr.TXT_CHOICES;
            List<string> strings = new List<string>(options.Split(',').Select(s => s.Trim()));

            ShuffleLinkedLists(strings, dr.nextVideos, dr.choicesLocalisationKey);
            string shuffledOptions = string.Join(", ", strings);
            dr.TXT_CHOICES = shuffledOptions;
        }
    }


    public DataRead GetNextVideoData(string ID)
    {
        return fileData.Find(p => p.ID == ID);
    }

    public DataRead GetNextVideoData(int index)
    {
        return fileData.Find(p => p.index == index);
    }

    public int GetDefaultVideoIndex(DataRead videoData)
    {
        if (!string.IsNullOrEmpty(videoData.DEF_VID))
        {
            for (int i = 0; i < videoData.nextVideos.Count; i++)
            {
                if (videoData.DEF_VID == videoData.nextVideos[i])
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private void ShuffleLinkedLists<T1, T2, T3>(List<T1> list, List<T2> array, List<T3> list2)
    {
        if (list.Count != array.Count)
        {
            throw new ArgumentException("The list and array must have the same count.");
        }

        System.Random random = new System.Random();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);

            // Swap elements in the list
            T1 temp1 = list[i];
            list[i] = list[j];
            list[j] = temp1;

            // Swap elements in the list
            T3 temp2 = list2[i];
            list2[i] = list2[j];
            list2[j] = temp2;

            // Swap elements in the array
            T2 temp3 = array[i];
            array[i] = array[j];
            array[j] = temp3;
        }
    }

}

[System.Serializable]
public class DataReadSimple
{
    public string ID;
    public int index;
}


[System.Serializable]
public class DataRead
{
    public string ID;
    public int index;
    public string videoFileUrl;
    public string videoFileUrl_male;
    public double videoDuration;
    public bool CHOICES;
    public bool CHECKPOINT;
    public float CHECKPOINT_TIME;
    public float SKIP_CHECKPOINT;
    public int NR_CHOICES;
    public string CHOICE_IMAGE;
    public bool SHUFFLE_CHOICES;
    public string CHOICE_DEPENDENCIES;
    public string TXT_CHOICES;
    [HideInInspector]
    public List<string> choicesLocalisationKey = new();
    public int DEFAULT_CHOICE;

    public string LOCALISATION;
    public string LOCALISATION_OBJ;
    public List<LocalisationKeyManager> LOCALISATION_KEY = new();

    public int SCORE_A = -1;
    public string SCORE_A_ANI;

    public int SCORE_B = -1;
    public string SCORE_B_ANI;

    public string DEPENDENCIES;
    public float TIME_CHOICES;
    public string DEF_VID;
    public string NEXT_VIDS;
    public string SUB;
    public List<string> nextVideos = new();
    public List<string> dependentVideos = new();
    public List<string> skipCheckpointDependecny = new();
}

[System.Serializable]
public class EndingData
{
    public string TitleEnding;
    public string Scene;
    public Sprite ImageEnding;
    public bool wasEndingCounted;
}

[System.Serializable]
public class AchievementsData
{
    public string SteamAchievementName;
    public string Scene;
    public float timeToActivate;
}

[System.Serializable]
public class LocalisationKeyManager
{
    public float timeToActivate;
    public float waitDuration;
    public string localizationKey;
    public bool isActivated;

}

[System.Serializable]
public class AnimationClipsManager
{
    public string AnimationName;
    public RectTransform AnimationObject;
}