using ArabicSupport;
using DG.Tweening;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

public class SubtitleDisplayer : MonoBehaviour
{
    [SerializeField] private bool setSizeOnStart = true;
    public string alternatePath = "";
    public int subtitleAppeared;
    public int subtitleAppearedTotal;
    TextAsset Subtitle;

    public TextMeshProUGUI Text;
    public TextMeshProUGUI Text2;

    public CanvasGroup text_Image;
    public CanvasGroup text2_Image;

    [Range(0, 1)]
    public float FadeTime;

    private GameUiManager gameUi;
    public GameObject subtitleDisplay;

    private TextMeshProUGUI currentlyDisplayingText;
    private TextMeshProUGUI fadedOutText;

    private Coroutine routine;
    private Coroutine removalRoutine;

    private string subtitleFolderName = "";
    private SRTParser parser;

    private bool _isCheckPointSkip;
    private float _checkPointSkipTime;

    private void Awake()
    {
        gameUi = FindObjectOfType<GameUiManager>();
    }

    private void Start()
    {
        if (setSizeOnStart)
        {
            SetSubtitleSize();
        }
    }

    public IEnumerator Begin()
    {
        currentlyDisplayingText = Text;
        fadedOutText = Text2;

        currentlyDisplayingText.text = string.Empty;
        fadedOutText.text = string.Empty;

        currentlyDisplayingText.gameObject.SetActive(true);
        fadedOutText.gameObject.SetActive(true);

        yield return FadeTextOut(currentlyDisplayingText, text_Image);
        yield return FadeTextOut(fadedOutText, text2_Image);

        parser = new SRTParser(Subtitle);

        //if (removalRoutine != null)
        //{
        //    StopCoroutine(removalRoutine);
        //    removalRoutine = null;
        //}

        //removalRoutine = StartCoroutine(Testing(parser));

        if (_isCheckPointSkip)
        {
            _isCheckPointSkip = false;
            RemoveSubtitle(_checkPointSkipTime);
        }
        else
        {
            RemoveSubtitle((float)gameUi.gameVideoManager.currentPlayer.time); //This is done when a subtitle language is changed to remove already appeared subtitles
        }

        SubtitleBlock currentSubtitle = null;

        while (true)
        {

            while (gameUi.isGamePause)
                yield return null;


            var elapsed = (float)gameUi.gameVideoManager.currentPlayer.time;

            var subtitle = parser.GetForTime(elapsed);
            if (subtitle != null)
            {
                if (!subtitle.Equals(currentSubtitle))
                {
                    currentSubtitle = subtitle;

                    subtitleAppeared = parser.GetSubtitlesCount();

                    if (subtitleFolderName == "ar")
                    {
                        currentlyDisplayingText.text = ArabicFixer.Fix(currentSubtitle.Text, true, false);
                    }
                    else
                    {
                        currentlyDisplayingText.text = currentSubtitle.Text;
                    }

                    // And fade out the old one. Yield on this one to wait for the fade to finish before doing anything else.
                    StartCoroutine(FadeTextOut(fadedOutText, text2_Image));

                    // Yield a bit for the fade out to get part-way
                    yield return new WaitForSeconds(FadeTime / 3);

                    // Fade in the new current
                    yield return FadeTextIn(currentlyDisplayingText, text_Image);
                }
                yield return null;
            }
            else
            {
                Debugg.Log("Subtitles ended");
                StartCoroutine(FadeTextOut(currentlyDisplayingText, text_Image));
                yield return FadeTextOut(fadedOutText, text2_Image);
                currentlyDisplayingText.gameObject.SetActive(false);
                fadedOutText.gameObject.SetActive(false);
                yield break;
            }
        }
    }


    public void RemoveSubtitle(float time)
    {
        float elapsed = time;
        parser?.RemoveSubtitlesOnSkip(elapsed);
    }

    public void SetTimeFromCheckpoint(float time)
    {
        _checkPointSkipTime = time;
        _isCheckPointSkip = true;
    }

    void OnValidate()
    {
        FadeTime = ((int)(FadeTime * 10)) / 10f;
    }

    public void SetSubtitleSize()
    {
        int min = 18, max = 30;

        int subtitleSize = GameManager.ins.subtitleSize;

        switch (subtitleSize)
        {
            case 0: min = 15; max = 20; break;//small
            case 1: min = 18; max = 30; break;//normal
            case 2: min = 21; max = 40; break;//large
        }

        Text.fontSizeMin = min;
        Text.fontSizeMax = max;

        //Text2.fontSizeMin = min;
        //Text2.fontSizeMax = max;
    }

    IEnumerator FadeTextOut(TextMeshProUGUI text, CanvasGroup textImage)
    {
        var toColor = text.color;
        toColor.a = 0;

        textImage.alpha = 0.85f;
        var toImageColor = 0.85f;

        //yield return Fade(text, toColor, Ease.OutSine);
        yield return Fade(textImage, toImageColor, Ease.OutSine);


    }

    IEnumerator FadeTextIn(TextMeshProUGUI text, CanvasGroup textImage)
    {
        var toColor = text.color;
        toColor.a = 1;

        textImage.alpha = 0.85f;
        var toImageColor = 0.85f;

        //yield return Fade(text, toColor, Ease.InSine);
        yield return Fade(textImage, toImageColor, Ease.InSine);
    }

    //IEnumerator Fade(TextMeshProUGUI text, Color toColor, Ease ease)
    //{
    //    yield return DOTween.To(() => text.color, color => text.color = color, toColor, FadeTime).SetEase(ease).WaitForCompletion();
    //}

    //IEnumerator Fade(Image text, Color toColor, Ease ease)
    //{
    //    yield return DOTween.To(() => text.color, color => text.color = color, toColor, FadeTime).SetEase(ease).WaitForCompletion();
    //}

    IEnumerator Fade(CanvasGroup cg, float toColor, Ease ease)
    {
        //yield return DOTween.To(() => cg.alpha, alpha => cg.alpha = alpha, toColor, FadeTime).SetEase(ease).WaitForCompletion();
        //yield return DOTween.To(() => cg.alpha, alpha => cg.alpha = alpha, toColor, FadeTime).SetEase(ease);
        yield return null;
    }

    //void SetTextEmpty()
    //{
    //    currentlyDisplayingText.text = string.Empty;
    //    fadedOutText.text = string.Empty;
    //}

    public void SetSubtitleAndShow(string folderName, string fileName)
    {
        subtitleFolderName = folderName;

        string path = StreamingAssetPath.GetSubtitleFolderPath() + folderName + "/" + alternatePath;

        Debugg.Log("Subtitle Path -- " + path);

        subtitleDisplay.SetActive(false);
        if (GameManager.ins.isSubtitleON)
        {
            if (string.IsNullOrEmpty(alternatePath))
            {
                subtitleDisplay.SetActive(true);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(alternatePath))
            {
                subtitleDisplay.SetActive(true);
            }
        }

        //subtitleDisplay.SetActive(GameManager.ins.isSubtitleON);

#if UNITY_ANDROID || UNITY_IOS
        if (!File.Exists(path + fileName))
        {
            Debugg.Log("File Not Exists");
            return;
        }
        StartCoroutine(ReadSubMobile(path + fileName));
#else
        if (!File.Exists(path + fileName))
        {
            Debugg.Log("File Not Exists");
            return;
        }

        StreamReader reader = new StreamReader(path + fileName);

        // Debugg.Log(reader.ReadToEnd());
        string subtitleData = reader.ReadToEnd();

        Subtitle = new TextAsset(subtitleData);

        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        routine = StartCoroutine(Begin());

        Debugg.Log("Subtitle Routine started------" + routine);
#endif

    }


    IEnumerator ReadSubMobile(string filePath)
    {
#if UNITY_ANDROID
        WWW data = new WWW(filePath);
        yield return data;

        if (string.IsNullOrEmpty(data.error))
        {
            Subtitle = new TextAsset(data.text);

            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }
            routine = StartCoroutine(Begin());


#elif UNITY_IOS

	yield return null;
    string subText = File.ReadAllText(filePath);

            if (!string.IsNullOrEmpty(subText))
            {
                Subtitle = new TextAsset(subText);

                if (routine != null)
                {
                    StopCoroutine(routine);
                    routine = null;
                }
                routine = StartCoroutine(Begin());
            }

#else
        yield return null;
#endif

    }




    TextAsset ConvertStringToTextAsset(string text)
    {
        TextAsset textAsset = new TextAsset(text);
        return textAsset;
    }
}


