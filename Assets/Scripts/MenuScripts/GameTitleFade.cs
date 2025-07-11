using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTitleFade : MonoBehaviour
{
    CanvasGroup cg;

    public float fadeSpeed = 1f;
    public float startDelay = 1f;

    public GameObject mainButtons;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Fade(1, cg, () => mainButtons.SetActive(true)));
    }

    IEnumerator Fade(int direction, CanvasGroup cg, Action obj)
    {
        int fadeDir;
        float fadeSpeed = this.fadeSpeed;

        fadeDir = direction;

        yield return new WaitForSeconds(startDelay);

        if (fadeDir == -1)//Hide
        {

            while (cg.alpha > 0)
            {
                cg.alpha += fadeDir * fadeSpeed * Time.deltaTime;
                cg.alpha = Mathf.Clamp01(cg.alpha);
                yield return new WaitForSeconds(0);
            }

        }
        else//Show
        {
            while (cg.alpha < 1)
            {
                cg.alpha += fadeDir * fadeSpeed * Time.deltaTime;
                cg.alpha = Mathf.Clamp01(cg.alpha);
                yield return new WaitForSeconds(0);
            }
        }

        obj?.Invoke();
    }


}
