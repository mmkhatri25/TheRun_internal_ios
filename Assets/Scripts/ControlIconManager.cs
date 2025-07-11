using Rewired;
using TMPro;
using UnityEngine;

public class ControlIconManager : MonoBehaviour
{
    [SerializeField] private string keyboardIcon = "";
    [SerializeField] private string psIcon = "";
    [SerializeField] private string xboxIcon = "";


    private TextMeshProUGUI _text;

    private void Awake()
    {
        SettingsCanvasManager.SetButtonTextLanguage += OnLanguageChange;
        _text = GetComponent<TextMeshProUGUI>();

        if (Application.platform == RuntimePlatform.Switch)
        {
            SpriteForSwitch();
        }
        else if (Application.platform == RuntimePlatform.PS4 || Application.platform == RuntimePlatform.PS5)
        {
            SpriteForSony();
        }
        else if (Application.platform == RuntimePlatform.XboxOne)
        {
            SpriteForXbox();
        }
        else if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
        }
        else
        {
            ReInput.ControllerConnectedEvent += OnControllerConnected;
            ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;

            //ReInput.ControllerPreDisconnectEvent += OnControllerPreDisconnect;
        }

        string[] connectedControls = ReInput.controllers.GetControllerNames(ControllerType.Joystick);

        if (connectedControls.Length > 0)
        {
            foreach (var item in connectedControls)
            {
                CheckControllerByName(item);
            }
        }
    }

    private void OnControllerConnected(ControllerStatusChangedEventArgs args)
    {
        CheckControllerByName(args.name);
    }

    private void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
    {
        SpriteForKeyboard();
    }

    private void CheckControllerByName(string args)
    {
        if (args.Contains("Sony"))
        {
            SpriteForSony();
        }
        else
        {
            SpriteForXbox();
        }

        if (Application.platform == RuntimePlatform.PS4 || Application.platform == RuntimePlatform.PS5)
        {
            SpriteForSony();
        }
        else if (Application.platform == RuntimePlatform.XboxOne)
        {
            SpriteForXbox();
        }
    }

    private void OnLanguageChange()
    {
        string[] connectedControls = ReInput.controllers.GetControllerNames(ControllerType.Joystick);
        if (connectedControls.Length > 0)
        {
            foreach (var item in connectedControls)
            {
                CheckControllerByName(item);
            }
        }
    }

    private void SpriteForSwitch()
    {
        _text.text = keyboardIcon;
    }

    private void SpriteForSony()
    {
        _text.text = psIcon;
    }

    private void SpriteForXbox()
    {
        _text.text = xboxIcon;
    }

    private void SpriteForKeyboard()
    {
        _text.text = keyboardIcon;
    }
}
