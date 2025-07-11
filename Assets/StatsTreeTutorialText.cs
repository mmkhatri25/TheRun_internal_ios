using ArabicSupport;
using I2.Loc;
using Rewired;
using TMPro;
using UnityEngine;

public class StatsTreeTutorialText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI zoomInText;
    [SerializeField] private TextMeshProUGUI zoomOutText;
    [SerializeField] private TextMeshProUGUI panText;

    readonly string zoomInTranslateKey = "zoom_in";
    readonly string zoomOutTranslateKey = "zoom_out";
    readonly string panTranslateKey = "pan_key";

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
            zoomInText.gameObject.SetActive(false);
            zoomOutText.gameObject.SetActive(false);
            panText.gameObject.SetActive(false);
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
        Vector2 sizeUpdate = panText.GetComponent<RectTransform>().sizeDelta;
        sizeUpdate.x = 200;
        panText.GetComponent<RectTransform>().sizeDelta = sizeUpdate;

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
        string[] connectedControls = ReInput.controllers.GetControllerNames(ControllerType.Joystick);
        if (connectedControls.Length > 0)
        {
            foreach (var item in connectedControls)
            {
                CheckControllerByName(item);
            }
        }

    }

    void FixArabicText(TextMeshProUGUI textMeshProUGUI, string translateKey)
    {
        string temp2 = "";

        //temp = textMeshProUGUI.text.Split(' ');
        temp2 = LocalizationManager.GetTranslation(translateKey);

        if (LocalizationManager.CurrentLanguage == "Arabic")
        {
            temp2 = ArabicFixer.Fix(temp2, true, false);
        }

        textMeshProUGUI.text = ControlKeySprites.W.TrimEnd() + ControlKeySprites.A.TrimEnd() + ControlKeySprites.S.TrimEnd() + ControlKeySprites.D
                             + temp2;
    }


    void SpriteForSwitch()
    {
        zoomInText.text = ControlKeySprites.SW_RT + LocalizationManager.GetTranslation(zoomInTranslateKey);
        zoomOutText.text = ControlKeySprites.SW_LT + LocalizationManager.GetTranslation(zoomOutTranslateKey);
        panText.text = ControlKeySprites.SW_R_JOYSTICK + LocalizationManager.GetTranslation(panTranslateKey);

    }

    void SpriteForSony()
    {
        zoomInText.text = ControlKeySprites.PS_R2 + LocalizationManager.GetTranslation(zoomInTranslateKey);
        zoomOutText.text = ControlKeySprites.PS_L2 + LocalizationManager.GetTranslation(zoomOutTranslateKey);
        panText.text = ControlKeySprites.PS_R_JOYSTICK + LocalizationManager.GetTranslation(panTranslateKey);

    }

    void SpriteForXbox()
    {
        zoomInText.text = ControlKeySprites.XB_RT + LocalizationManager.GetTranslation(zoomInTranslateKey);
        zoomOutText.text = ControlKeySprites.XB_LT + LocalizationManager.GetTranslation(zoomOutTranslateKey);
        panText.text = ControlKeySprites.XB_R_JOYSTICK + LocalizationManager.GetTranslation(panTranslateKey);

    }

    void SpriteForKeyboard()
    {
        zoomInText.text = ControlKeySprites.Q + LocalizationManager.GetTranslation(zoomInTranslateKey);
        zoomOutText.text = ControlKeySprites.E + LocalizationManager.GetTranslation(zoomOutTranslateKey);
        panText.text = ControlKeySprites.W + ControlKeySprites.A + ControlKeySprites.S + ControlKeySprites.D
                        + LocalizationManager.GetTranslation(panTranslateKey);

        Vector2 sizeUpdate = panText.GetComponent<RectTransform>().sizeDelta;
        sizeUpdate.x = 300;
        panText.GetComponent<RectTransform>().sizeDelta = sizeUpdate;
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
