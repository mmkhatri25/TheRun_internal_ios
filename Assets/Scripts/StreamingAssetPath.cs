using UnityEngine;

public static class StreamingAssetPath
{
    public static string GetVideoFolderPath()
    {

//#if UNITY_EDITOR
//        return "E:\\The_Run_Videos\\";
//#endif

#if UNITY_ANDROID
        return Application.streamingAssetsPath + "/Videos/";
#else
        return Application.streamingAssetsPath + "/Videos/";
#endif
    }

    public static string GetSubtitleFolderPath()
    {
#if UNITY_ANDROID
        return Application.streamingAssetsPath + "/Subtitles/";
#else
        return Application.streamingAssetsPath + "/Subtitles/";
#endif
    }

}
