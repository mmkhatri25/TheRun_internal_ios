using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using I2.Loc;
using UnityEditor;

public class AnimationComponent : MonoBehaviour
{

    public bool isTextFade;
    public bool isImageFade;
    public TextMeshProUGUI spriteText;


    CanvasGroup cg;
    TextMeshProUGUI text;
    [HideInInspector] public RectTransform rect;
    GameUiManager gameUi;
    Animator anim;

    private float fadeSpeed = 1f;//Higher the value faster the fade speed


    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        text = GetComponent<TextMeshProUGUI>();
        rect = GetComponent<RectTransform>();
        gameUi = FindObjectOfType<GameUiManager>();
        anim = GetComponent<Animator>();
    }


    IEnumerator Fade(int direction, CanvasGroup cg, Action obj, float waitDuration = 0.1f)
    {
        int fadeDir;
        float fadeSpeed = this.fadeSpeed;

        fadeDir = direction;

        yield return null;


        while (gameUi.isGamePause)
            yield return null;


        if (fadeDir == -1)// Hide 
        {
            while (cg.alpha > 0)
            {
                cg.alpha += fadeDir * fadeSpeed * Time.deltaTime;
                cg.alpha = Mathf.Clamp01(cg.alpha);
                yield return new WaitForSeconds(0);
            }

        }
        else
        {
            while (cg.alpha < 1)// show 
            {
                cg.alpha += fadeDir * fadeSpeed * Time.deltaTime;
                cg.alpha = Mathf.Clamp01(cg.alpha);
                yield return new WaitForSeconds(0);
            }
        }

        yield return new WaitForSeconds(waitDuration);

        obj?.Invoke();

    }


    public void Show(string localisationKey, float waitDuration)
    {
        text.text = LocalizationManager.GetTranslation(localisationKey);
        StartCoroutine(Fade(1, cg, () => StartCoroutine(Fade(-1, cg, () => gameObject.SetActive(false))), waitDuration));
    }


    public void ShowImage(float waitDuration)
    {
        StartCoroutine(Fade(1, cg, () => StartCoroutine(Fade(-1, cg, () => gameObject.SetActive(false))), waitDuration));
    }



    public void ShowAnimation(string localisationKey, Vector3 pos)
    {
        spriteText.text = LocalizationManager.GetTranslation(localisationKey);
        rect.anchoredPosition = pos;
    }


    public void DisableObject()
    {
        gameObject.SetActive(false);
    }

    public void StopAnimation()
    {
        if(anim!=null)
            anim.StopPlayback();

        DisableObject();
    }
}
