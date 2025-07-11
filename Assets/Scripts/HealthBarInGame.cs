using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarInGame : MonoBehaviour
{

    CanvasGroup cg;

    public float fadeSpeed;//Higher the value faster the fade speed

    public List<CanvasGroup> healthPoints = new List<CanvasGroup>();

    public Sprite redBar;
    public Sprite greenBar;

    RectTransform rect;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
    }


    IEnumerator Fade(int direction, CanvasGroup cg, Action obj)
    {
        int fadeDir;
        float fadeSpeed = this.fadeSpeed;

        fadeDir = direction;

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

        yield return new WaitForSeconds(0.1f);

        obj?.Invoke();

    }



    public void Show(Vector3 pos, int oldValue, int newValue, float waitDuration)
    {
        rect.anchoredPosition = pos;

        foreach (CanvasGroup item in healthPoints)
        {
            item.alpha = 0;
        }

        oldValue = Mathf.Clamp(oldValue, 0, 6);
        newValue = Mathf.Clamp(newValue, 0, 6);

        SetSpriteColor(oldValue);

        StartCoroutine(Fade(1, cg, () => SetHealth(oldValue, newValue)));

        StartCoroutine(Hide(waitDuration));
    }


    IEnumerator Hide(float waitDuration)
    {
        yield return new WaitForSeconds(waitDuration);

        StartCoroutine(Fade(-1, cg, null));
    }

    public void SetHealth(int oldValue, int newValue)//Set Health Bar UI based on Relationship points. Red <=3, Green >=4
    {

        if (newValue > oldValue)
        {
            CanvasGroup topCG = healthPoints[oldValue].GetComponent<CanvasGroup>();

            StartCoroutine(Fade(1, topCG, () => SetSpriteColor(newValue)));//() => SetSpriteColor(newValue)
        }
        else if (newValue < oldValue)
        {

            CanvasGroup topCG = healthPoints[newValue].GetComponent<CanvasGroup>();

            StartCoroutine(Fade(-1, topCG, () => SetSpriteColor(newValue)));
        }
        else
        {
            SetSpriteColor(newValue);
        }
    }


    public void HideInstantly()
    {
        StopAllCoroutines();
        cg.alpha = 0;
    }


    void SetSpriteColor(int value)
    {
        for (int i = 0; i < value; i++)
        {
            healthPoints[i].alpha = 1;

            if (value > 3)
            {
                healthPoints[i].GetComponent<Image>().sprite = greenBar;
            }
            else
            {
                healthPoints[i].GetComponent<Image>().sprite = redBar;
            }

        }
    }
}
