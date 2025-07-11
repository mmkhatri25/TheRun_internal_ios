using DG.Tweening;
using Rewired;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private float loadDelay = 3.5f;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Image glow;
    [SerializeField] private Slider progressBar;

    public static UnityAction OnLoadingComplete;
    private string videoPath;
    private bool _isLoading;

    private Player player { get { return ReInput.players.GetPlayer(0); } }

    private void Awake()
    {
        videoPath = StreamingAssetPath.GetVideoFolderPath() + "loading.mp4";
        videoPlayer.url = videoPath;
        videoPlayer.Play();

        progressBar.maxValue = loadDelay;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_isLoading)
        {
            progressBar.value += Time.deltaTime;
            if (progressBar.value >= progressBar.maxValue)
            {
                _isLoading = false;
                ShowGlow();
            }
        }
    }

    public void StartLoading()
    {
        ResetValues();
        _isLoading = true;
        ManagePlayerInput(false);
        gameObject.SetActive(true);
    }

    private void ShowGlow()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(glow.DOFade(1, 0.25f).SetEase(Ease.OutBounce));
        seq.AppendInterval(0.1f);
        seq.Append(glow.DOFade(0, 0.25f));
        seq.AppendInterval(0.1f);
        seq.AppendCallback(() =>
        {
            ResetValues();
            videoPlayer.targetTexture.Release();
            OnLoadingComplete?.Invoke();
            OnLoadingComplete = null;
            //gameObject.SetActive(false);
        });
    }

    private void ResetValues()
    {
        var color = glow.color;
        color.a = 0;
        glow.color = color;
        progressBar.value = 0;
        _isLoading = false;
        ManagePlayerInput(true);
    }

    private void ManagePlayerInput(bool isEnabled)
    {
        player.controllers.maps.SetAllMapsEnabled(isEnabled);
    }
}
