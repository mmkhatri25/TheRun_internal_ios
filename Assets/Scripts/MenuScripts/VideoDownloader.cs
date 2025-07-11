using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class VideoDownloader : MonoBehaviour
{
    public readonly string BASE_URL = "https://the-run-bucket.s3.eu-west-2.amazonaws.com/The_Run_Videos/";

    [SerializeField] ProgressBar progressBar;
    [SerializeField] GameObject canvas;
    [SerializeField] TextMeshProUGUI downloadCountText;
    [SerializeField] TextMeshProUGUI downloadSizeText;

    public string dbFilename;
    public int startIndex;
    public long totalDownloadSize;

    public List<DownloadDataList> fileData = new();
    public List<string> videoUrls = new();

    public List<DownloadDataList> t1_data = new();
    public List<DownloadDataList> t2_data = new();

    private int downloadCount = 0;
    private float totalVideoFiles = 0;
    private float totalPercentComplete = 0;

    private bool threadOneComplete;
    private bool threadTwoComplete;
    private bool downloadSizeFetched;

    private float t1Progress;
    private float t2Progress;

    private readonly WaitForSeconds waitTime = new WaitForSeconds(0.25f);

    public float ProgressComplete
    {
        set
        {
            progressBar.PercentComplete = value;
        }
    }

    private void Awake()
    {
        Application.targetFrameRate = -1;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

#if UNITY_IOS
        UnityEngine.iOS.Device.hideHomeButton = true;
#endif

    }

    IEnumerator Start()
    {
#if UNITY_ANDROID

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
#endif

        if (string.IsNullOrEmpty(dbFilename))
            yield break;

        List<Dictionary<string, object>> data = CSVReader.Read(dbFilename);

        for (var i = startIndex; i < data.Count; i++)
        {
            DownloadDataList dr = new()
            {
                index = i,
                ID = data[i]["ID"].ToString()
            };
            //dr.URL = data[i]["URL"].ToString().Trim();
            videoUrls.Add(BASE_URL + dr.ID);
            fileData.Add(dr);
        }

        totalVideoFiles = data.Count - startIndex;

        Debug.Log("Total Video -- " + totalVideoFiles);

        List<DownloadDataList> tempVid = new();

        tempVid.AddRange(fileData);

        foreach (var item in tempVid)
        {
            // Debug.Log(item.ID);

            if (File.Exists(Application.persistentDataPath + "/" + item.ID))
            {
                Debug.Log("------File Exists---");
                fileData.Remove(item);
                videoUrls.Remove(item.ID);
                downloadCount++;
            }
        }

        if (downloadCount == totalVideoFiles)
        {
            SceneManager.LoadScene(StaticStrings.LOGO_SCENE);

            yield break;
        }
        else
        {
            FetchTotalDownloadSize();
            progressBar.PercentComplete = (1 / totalVideoFiles) * downloadCount;
            totalPercentComplete = progressBar.PercentComplete;
        }


        if (fileData.Count % 2 == 0)//Even Files
        {
            for (int i = 0; i < fileData.Count / 2; i++)
            {
                t1_data.Add(fileData[i]);
            }

            for (int i = fileData.Count / 2; i < fileData.Count; i++)
            {
                t2_data.Add(fileData[i]);
            }
        }
        else
        {
            for (int i = 0; i < (fileData.Count - 1) / 2; i++)
            {
                t1_data.Add(fileData[i]);
            }

            for (int i = (fileData.Count - 1) / 2; i < fileData.Count; i++)
            {
                t2_data.Add(fileData[i]);
            }
        }

        Debug.Log("T1 === " + t1_data.Count);
        Debug.Log("T2 === " + t2_data.Count);


        if (fileData.Count != 0)
        {
            string dots = "";
            while (!downloadSizeFetched)
            {
                downloadCountText.text = "Calulating Size" + dots;
                yield return waitTime;
                dots += ".";

                if (dots == "....")
                {
                    dots = "";
                }
            }

            Invoke(nameof(DownLoadVideo_Thread1), 0.1f);
            Invoke(nameof(DownLoadVideo_Thread2), 0.6f);

            StartCoroutine(TotalDownloadProgress());
        }
        else
        {
            SceneManager.LoadScene(StaticStrings.LOGO_SCENE);
        }
    }


    public void DownLoadVideo_Thread1()
    {
        StartCoroutine(_DownLoadVideo_Thread1());
    }


    IEnumerator _DownLoadVideo_Thread1()
    {
        canvas.SetActive(true);
        totalPercentComplete += t1Progress;

        //downloadCountText.text = "Downloading Part " + (downloadCount + 1) + "/" + totalVideoFiles;
        downloadCountText.text = "DOWNLOADING CONTENT";

        string downPath = BASE_URL + t1_data[0].ID;
        string savePath = Path.Combine(Application.persistentDataPath, t1_data[0].ID);

        Debug.Log(downPath);

        var www = new UnityWebRequest(downPath);
        www.method = UnityWebRequest.kHttpVerbGET;

        DownloadHandlerFile df = new DownloadHandlerFile(savePath);
        df.removeFileOnAbort = true;
        www.downloadHandler = df;

        UnityWebRequestAsyncOperation operation = www.SendWebRequest();

        yield return DownloadProgress_T1(operation);

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("success");

            //File.WriteAllBytes(savePath, www.downloadHandler.data);

            yield return new WaitForEndOfFrame();

            www.Dispose();

            downloadCount++;

            t1_data.RemoveAt(0);

            yield return new WaitForEndOfFrame();

            if (t1_data.Count != 0)
                DownLoadVideo_Thread1();
            else
            {
                threadOneComplete = true;

                if (threadOneComplete && threadTwoComplete)
                    SceneManager.LoadScene(StaticStrings.LOGO_SCENE);
            }

        }
    }

    public void DownLoadVideo_Thread2()
    {
        StartCoroutine(_DownLoadVideo_Thread2());
    }

    IEnumerator _DownLoadVideo_Thread2()
    {
        canvas.SetActive(true);
        totalPercentComplete += t2Progress;

        //downloadCountText.text = "Downloading Part " + (downloadCount + 1) + "/" + totalVideoFiles;
        downloadCountText.text = "DOWNLOADING CONTENT";

        string downPath = BASE_URL + t2_data[0].ID;
        string savePath = Path.Combine(Application.persistentDataPath, t2_data[0].ID);

        Debug.Log(downPath);

        var www = new UnityWebRequest(downPath);
        www.method = UnityWebRequest.kHttpVerbGET;

        DownloadHandlerFile df = new DownloadHandlerFile(savePath);
        df.removeFileOnAbort = true;
        www.downloadHandler = df;

        UnityWebRequestAsyncOperation operation = www.SendWebRequest();

        yield return DownloadProgress_T2(operation);

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("success");

            //File.WriteAllBytes(savePath, www.downloadHandler.data);

            yield return new WaitForEndOfFrame();

            www.Dispose();

            downloadCount++;

            t2_data.RemoveAt(0);

            yield return new WaitForEndOfFrame();

            if (t2_data.Count != 0)
                DownLoadVideo_Thread2();
            else
            {
                threadTwoComplete = true;

                if (threadOneComplete && threadTwoComplete)
                    SceneManager.LoadScene(StaticStrings.LOGO_SCENE);
            }

        }
    }

    IEnumerator DownloadProgress_T1(UnityWebRequestAsyncOperation operation)
    {
        while (!operation.isDone)
        {
            t1Progress = (operation.progress * (1 / totalVideoFiles));
            yield return null;
        }

    }
    IEnumerator DownloadProgress_T2(UnityWebRequestAsyncOperation operation)
    {
        while (!operation.isDone)
        {
            t2Progress = (operation.progress * (1 / totalVideoFiles));
            yield return null;
        }
    }


    IEnumerator TotalDownloadProgress()
    {
        while (!threadOneComplete && !threadTwoComplete)
        {
            ProgressComplete = totalPercentComplete;
            yield return null;
        }
    }

    private async void FetchTotalDownloadSize()
    {
        downloadCountText.text = "Calulating Size";


        long totalSize = await GetTotalDownloadSizeAsync(videoUrls);

        totalDownloadSize = totalSize;
        downloadSizeText.text = "Size : " + FormatBytes(totalSize);

        downloadSizeFetched = true;
        //Debug.Log("Final Total Size in Bytes: " + totalSize);
        //Debug.Log("Formatted Size: " + FormatBytes(totalSize));
    }



    public async Task<long> GetTotalDownloadSizeAsync(List<string> urls)
    {
        long totalSize = 0;

        foreach (string url in urls)
        {
            long size = await GetFileSizeAsync(url);
            if (size >= 0)
            {
                totalSize += size;
            }
            else
            {
                Debug.LogWarning($"Could not get size for: {url}");
            }
        }

        Debug.Log($"Total Download Size: {FormatBytes(totalSize)}");
        return totalSize;
    }

    private async Task<long> GetFileSizeAsync(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Head(url))
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield(); // Use await UniTask.Yield() if using UniTask

#if UNITY_2020_1_OR_NEWER
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogWarning($"Error getting size from {url}: {request.error}");
                return -1;
            }

            string contentLengthStr = request.GetResponseHeader("Content-Length");
            if (!string.IsNullOrEmpty(contentLengthStr) && long.TryParse(contentLengthStr, out long size))
            {
                return size;
            }

            return -1;
        }
    }

    public string FormatBytes(long bytes)
    {
        const double KB = 1 << 10;
        const double MB = 1 << 20;
        const double GB = 1 << 30;

        if (bytes >= GB) return $"{bytes / GB:0.00} GB";
        if (bytes >= MB) return $"{bytes / MB:0.00} MB";
        if (bytes >= KB) return $"{bytes / KB:0.00} KB";
        return $"{bytes} B";
    }
}


class VideoData
{
    public bool success;
    public string message;
    public List<string> data = new List<string>();
}

[System.Serializable]
public class DownloadDataList
{
    public int index;
    public string ID;
    public string URL;
}