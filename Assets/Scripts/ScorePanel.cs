using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScorePanel : MonoBehaviour
{

    [Header("REF")]
    [SerializeField] GameUiManager gameUi;
    [SerializeField] CanvasGroup canvasGroup;


    [Header("Player")]
    [SerializeField] Image playerImage;
    [SerializeField] TextMeshProUGUI playerScoreText;

    [Header("Player")]
    [SerializeField] Image oppImage;
    [SerializeField] Image oppRingImage;
    [SerializeField] TextMeshProUGUI oppScoreText;

    float fadeSpeed = 1.2f;

    int playerScore;
    int oppScore;

    private void Start()
    {
        playerScoreText.text = GameManager.ins.SCORE_A.ToString();
        oppScoreText.text = GameManager.ins.SCORE_B.ToString();
    }

    public void SetScoresTextOnSkip()
    {
        playerScoreText.text = GameManager.ins.SCORE_A.ToString();
        oppScoreText.text = GameManager.ins.SCORE_B.ToString();
    }

    public void SetPlayerImages(Sprite pSprite, Sprite oSprite)
    {
        playerImage.sprite = pSprite;
        oppImage.sprite = oSprite;
    }

    public void ShowHideScorePanel(int playerScore, int oppScore, float waitDuration, string oppScoreGraphic = "")
    {
        this.playerScore = playerScore;
        this.oppScore = oppScore;

        if (!string.IsNullOrEmpty(oppScoreGraphic))
        {
            oppImage.sprite = Resources.Load<Sprite>("ScoreUI/" + oppScoreGraphic.ToLower());
        }

        if (this.playerScore == 0 || this.oppScore == 0)
        {
            playerScoreText.text = "0";
            oppScoreText.text = "0";
        }
        else
        {
            StartCoroutine(_ShowHideScorePanel(waitDuration));
        }

    }


    IEnumerator _ShowHideScorePanel(float waitDuration)
    {
        StartCoroutine(Fade(1, this.playerScore, this.oppScore));

        yield return new WaitForSeconds(waitDuration);

        StartCoroutine(Fade(-1, this.playerScore, this.oppScore));
    }

    IEnumerator Fade(int direction, int playerScore, int oppScore)
    {
        int fadeDir;
        float fadeSpeed = this.fadeSpeed;

        fadeDir = direction;

        yield return null;


        while (gameUi.isGamePause)
            yield return null;


        if (fadeDir == -1)// Hide 
        {
            while (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha += fadeDir * fadeSpeed * Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha);
                yield return new WaitForSeconds(0);
            }

        }
        else
        {
            while (canvasGroup.alpha < 1)// show 
            {
                canvasGroup.alpha += fadeDir * fadeSpeed * Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha);
                yield return new WaitForSeconds(0);
            }
        }

        if (fadeDir == 1)
        {
            if (this.playerScore != -1)
            {
                playerScoreText.transform.DOPunchScale(Vector3.one * 1.2f, 0.2f).OnComplete(() =>
                {
                    playerScoreText.text = this.playerScore.ToString();
                });
            }

            if (this.oppScore != -1)
            {
                oppScoreText.transform.DOPunchScale(Vector3.one * 1.2f, 0.2f).OnComplete(() =>
                {
                    oppScoreText.text = this.oppScore.ToString();
                });
            }
        }

    }
}
