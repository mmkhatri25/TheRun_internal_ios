using ArabicSupport;
using I2.Loc;
using Rewired;
using TMPro;
using UnityEngine;

public class ControllerIconManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI enterText;
    [SerializeField] private TextMeshProUGUI backText;
    [SerializeField] private TextMeshProUGUI skipText;

    readonly string enterTranslateKey = "select_text";
    readonly string backTranslateKey = "back";
    readonly string skipTranslateKey = "SkipScreen";


    void Awake()
    {
        // Subscribe to events
        //ArabicFixer.Fix(currentSubtitle.Text, true, false);


        SettingsCanvasManager.SetButtonTextLanguage += OnLanguageChange;

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
            enterText.gameObject.SetActive(false);
            backText.gameObject.SetActive(false);
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
        else
        {
            SkipButtonText(ControlKeySprites.TAB);
        }
    }

    private void Start()
    {
        OnLanguageChange();
    }

    // This function will be called when a controller is connected
    // You can get information about the controller that was connected via the args parameter
    void OnControllerConnected(ControllerStatusChangedEventArgs args)
    {
        Debug.Log("A controller was connected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);

        CheckControllerByName(args.name);
    }


    void CheckControllerByName(string args)
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

        DisableCursor();
    }

    // This function will be called when a controller is fully disconnected
    // You can get information about the controller that was disconnected via the args parameter
    void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
    {
        Debug.Log("A controller was disconnected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);

        SpriteForKeyboard();
        EnableCursor();
    }

    // This function will be called when a controller is about to be disconnected
    // You can get information about the controller that is being disconnected via the args parameter
    // You can use this event to save the controller's maps before it's disconnected
    //void OnControllerPreDisconnect(ControllerStatusChangedEventArgs args)
    //{
    //    Debug.Log("A controller is being disconnected! Name = " + args.name + " Id = " + args.controllerId + " Type = " + args.controllerType);
    //}

    void OnDestroy()
    {
        ReInput.ControllerConnectedEvent -= OnControllerConnected;
        ReInput.ControllerDisconnectedEvent -= OnControllerDisconnected;
        SettingsCanvasManager.SetButtonTextLanguage -= OnLanguageChange;
        //ReInput.ControllerPreDisconnectEvent -= OnControllerPreDisconnect;
    }


    void OnLanguageChange()
    {
        string temp2 = "";

        //temp = enterText.text.Split(' ');
        temp2 = LocalizationManager.GetTranslation(enterTranslateKey);

        if (LocalizationManager.CurrentLanguage == "Arabic")
        {
            temp2 = ArabicFixer.Fix(temp2, true, false);
        }

        enterText.text = ControlKeySprites.ENTER + " " + temp2;
        //temp = backText.text.Split(' ');
        temp2 = LocalizationManager.GetTranslation(backTranslateKey);

        if (LocalizationManager.CurrentLanguage == "Arabic")
        {
            temp2 = ArabicFixer.Fix(temp2, true, false);
        }

        backText.text = ControlKeySprites.ESCAPE + " " + temp2;

        if (Application.platform == RuntimePlatform.Switch)
        {
            SkipButtonText(ControlKeySprites.SW_RT);
        }
        else if (Application.platform == RuntimePlatform.PS4 || Application.platform == RuntimePlatform.PS5)
        {
            SkipButtonText(ControlKeySprites.PS_R2);
        }
        else if (Application.platform == RuntimePlatform.XboxOne)
        {
            SkipButtonText(ControlKeySprites.XB_RT);
        }
        else
        {
            string[] connectedControls = ReInput.controllers.GetControllerNames(ControllerType.Joystick);
            if (connectedControls.Length > 0)
            {
                foreach (var item in connectedControls)
                {
                    CheckControllerByName(item);
                }
            }
            else
            {
                SkipButtonText(ControlKeySprites.TAB);
            }
        }

    }


    void SpriteForSwitch()
    {
        enterText.text = ControlKeySprites.SW_A + LocalizationManager.GetTranslation(enterTranslateKey);
        backText.text = ControlKeySprites.SW_B + LocalizationManager.GetTranslation(backTranslateKey);

        SkipButtonText(ControlKeySprites.SW_RT);
    }

    void SpriteForSony()
    {
        enterText.text = ControlKeySprites.PS_X + LocalizationManager.GetTranslation(enterTranslateKey);
        backText.text = ControlKeySprites.PS_O + LocalizationManager.GetTranslation(backTranslateKey);

        SkipButtonText(ControlKeySprites.PS_R2);
    }

    void SpriteForXbox()
    {
        enterText.text = ControlKeySprites.XB_A + LocalizationManager.GetTranslation(enterTranslateKey);
        backText.text = ControlKeySprites.XB_B + LocalizationManager.GetTranslation(backTranslateKey);

        SkipButtonText(ControlKeySprites.XB_RT);
    }

    void SpriteForKeyboard()
    {
        enterText.text = ControlKeySprites.ENTER + LocalizationManager.GetTranslation(enterTranslateKey);
        backText.text = ControlKeySprites.ESCAPE + LocalizationManager.GetTranslation(backTranslateKey);

        SkipButtonText(ControlKeySprites.TAB);
    }


    void SkipButtonText(string spriteValue = null)
    {
        string skipTextTemp = LocalizationManager.GetTranslation(skipTranslateKey);

        if (spriteValue != null)
            skipTextTemp = skipTextTemp.Replace("TAB", spriteValue);

        skipText.text = skipTextTemp;
    }

    void DisableCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void EnableCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

}

