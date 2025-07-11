using Rewired;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Fading : MonoBehaviour
{

    [SerializeField] private Image fadeOutTexture;                  // the texture taht will overlay the screen . This can be black or a loading Graphics
    [SerializeField] public float fadeSpeed = 1.8f;                // fading speed

    private float alpha = 1.0f;                    // the texture aplha value B/W 0 and 1;
    private int fadeDir = -1;                      // the direction on fade in = -1 or out = 1

    public static Fading Instance;
    public static Action OnCompleteFade;

    private Player player { get { return ReInput.players.GetPlayer(0); } }

    IEnumerator Start()
    {
        gameObject.SetActive(true);
        //Loading.gameObject.SetActive(false);
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            yield return new WaitForSeconds(.5f);
            //float temp = BeginFade(-1);
        }
        else
            Destroy(this.gameObject);

    }



    public float BeginFade(int direction)
    {
        if (direction == -1)
        {
            fadeOutTexture.color = new Color32(0, 0, 0, 255);
            alpha = 1f;
        }
        else
        {
            fadeOutTexture.color = new Color32(0, 0, 0, 0);
            alpha = 0f;
        }

        StartCoroutine(Fade(direction));
        return (fadeSpeed);
    }

    IEnumerator Fade(int direction)
    {
        fadeDir = direction;

        if (fadeDir == -1)
        {

            while (alpha > 0)
            {
                alpha += fadeDir * fadeSpeed * Time.deltaTime;

                alpha = Mathf.Clamp01(alpha);
                fadeOutTexture.color = new Color(fadeOutTexture.color.r, fadeOutTexture.color.b, fadeOutTexture.color.b, alpha);
                yield return new WaitForSeconds(0);
            }

        }
        else
        {
            while (alpha < 1)
            {
                alpha += fadeDir * fadeSpeed * Time.deltaTime;

                alpha = Mathf.Clamp01(alpha);
                fadeOutTexture.color = new Color(fadeOutTexture.color.r, fadeOutTexture.color.b, fadeOutTexture.color.b, alpha);
                yield return new WaitForSeconds(0);
            }
        }

    }


    public void FadeInOut(Action obj) //Call this function and pass the method that needs to be called on fade completion
    {
        OnCompleteFade += obj;
        StartCoroutine(_FadeInOut(obj));
    }

    IEnumerator _FadeInOut(Action obj)
    {
        player.controllers.maps.SetAllMapsEnabled(false);
        BeginFade(1);

        yield return new WaitForSeconds(fadeSpeed / 3f);

        BeginFade(-1);

        OnCompleteFade?.Invoke();
        player.controllers.maps.SetAllMapsEnabled(true);

        OnCompleteFade -= obj;
    }

}

