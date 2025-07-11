using I2.Loc;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class DeathTeaseTexts : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI deathTexts;

    [SerializeField] private VideoPlayer videoPlayer;

    private void OnEnable()
    {
        int deathTextIndex = GameManager.ins.CURRENT_DEATHS;
        if (GameManager.ins.CURRENT_DEATHS == 0)
        {
            deathTextIndex++;
        }

        if (deathTextIndex > 8)
        {
            deathTextIndex = 8;
        }

        string key = "DeathText_" + deathTextIndex;
        string translation = LocalizationManager.GetTranslation(key);

        if (translation == null)
        {
            deathTexts.text = key;
        }

        else
        {
            deathTexts.text = translation;
        }

#if UNITY_ANDROID
            videoPlayer.url = Path.Combine(videoFolderPath, dr.ID);
#else
        //Debug.Log(dr.ID);
        videoPlayer.url = "file:///" + Path.Combine(StreamingAssetPath.GetVideoFolderPath(), StaticStrings.RestartVideoFile);
#endif

        videoPlayer.Play();
    }
}
