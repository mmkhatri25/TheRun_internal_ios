using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class BackButtonHandler : MonoBehaviour
{
    Button button;

    private Player player { get { return ReInput.players.GetPlayer(0); } }


    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Update()
    {
        if (player.GetButtonDown("UICancel"))
        {
            button?.onClick.Invoke();
        }
    }

}
