using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class VideoSequencePlayer : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public List<string> videoFileNames; // Example: ["Intro.mp4", "Scene1.mp4", "Outro.mp4"]

    private int currentVideoIndex = 0;

    private void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoFileNames == null || videoFileNames.Count == 0)
        {
            Debug.LogError("Video list is empty.");
            return;
        }

        videoPlayer.loopPointReached += OnVideoEnd;
        PlayVideoByIndex(currentVideoIndex);
    }

    private void PlayVideoByIndex(int index)
    {
        if (index < 0 || index >= videoFileNames.Count)
        {
            Debug.Log("All videos played.");
            return;
        }

        string videoPath = "file:///" + Path.Combine(StreamingAssetPath.GetVideoFolderPath(), videoFileNames[index]);

        videoPlayer.url = videoPath;

        Debug.Log("Playing video: " + videoFileNames[index]);
        videoPlayer.Play();
        //videoPlayer.prepareCompleted += OnPrepared;
    }

    private void OnPrepared(VideoPlayer vp)
    {
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.Play();
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        currentVideoIndex++;
        PlayVideoByIndex(currentVideoIndex);
    }

    public void SetVPSpeed(int value)
    {
        videoPlayer.playbackSpeed = value;
    }

    private void OnDestroy()
    {
        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.prepareCompleted -= OnPrepared;
    }
}
