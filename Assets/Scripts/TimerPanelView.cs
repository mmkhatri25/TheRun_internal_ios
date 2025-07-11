using UnityEngine;
using UnityEngine.UI;

public class TimerPanelView : MonoBehaviour
{

    public Sprite pink_bg;
    public Sprite pink_fill;
    [Space]
    public Sprite blue_bg;
    public Sprite blue_fill;
    [Space]
    public Sprite red_bg;
    public Sprite red_fill;

    public Image timer_bg;
    public Image timer_fill;

    public void SetTimerColor(string color)
    {
        switch (color)
        {
            case "red":
                timer_bg.sprite = red_bg;
                timer_fill.sprite = red_fill;
                break;
            case "blue":
                timer_bg.sprite = blue_bg;
                timer_fill.sprite = blue_bg;
                break;
            case "pink":
                timer_bg.sprite = pink_bg;
                timer_fill.sprite = pink_fill;
                break;
        }

    }
}
