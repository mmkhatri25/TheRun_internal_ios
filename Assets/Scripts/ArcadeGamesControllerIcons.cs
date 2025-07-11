using Rewired;
using System.Collections.Generic;
using UnityEngine;

public class ArcadeGamesControllerIcons : MonoBehaviour
{
    [SerializeField] private List<GameControllerIconType> uiIconLists;

    string[] connectedControls;

    private void Awake()
    {
        ReInput.ControllerConnectedEvent += OnControllerConnected;
        ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
    }

    private void Start()
    {
        connectedControls = ReInput.controllers.GetControllerNames(ControllerType.Joystick);
        DisableAllUI();
        if (connectedControls.Length > 0)
        {
            foreach (var item in connectedControls)
            {
                CheckControllerByName(item);
            }
        }
        else
        {
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
            {
                EnableUIForPlatform(PLATFORM_TYPE.STEAM, CONTROLLER_TYPE.KEYBOARD);
            }
        }
    }

    void CheckControllerByName(string args)
    {
        DisableAllUI();

        if (args.Contains("Sony"))
        {
            EnableUIForPlatform(PLATFORM_TYPE.STEAM, CONTROLLER_TYPE.PS);
        }
        else
        {
            EnableUIForPlatform(PLATFORM_TYPE.STEAM, CONTROLLER_TYPE.XBOX);
        }

        if (Application.platform == RuntimePlatform.PS4 || Application.platform == RuntimePlatform.PS5)
        {
            EnableUIForPlatform(PLATFORM_TYPE.PS, CONTROLLER_TYPE.PS);
        }
        else if (Application.platform == RuntimePlatform.XboxOne)
        {
            EnableUIForPlatform(PLATFORM_TYPE.XBOX, CONTROLLER_TYPE.XBOX);
        }

    }

    void EnableUIForPlatform(PLATFORM_TYPE pType, CONTROLLER_TYPE cType)
    {
        GameControllerIconType gameControllerIconType = uiIconLists.Find(p => p.platform_Type == pType && p.controller_Type == cType);

        if (gameControllerIconType != null)
        {
            gameControllerIconType.Enable();
        }
    }

    private void DisableAllUI()
    {
        foreach (var item in uiIconLists)
        {
            item.Disable();
        }
    }


    private void OnDestroy()
    {
        ReInput.ControllerConnectedEvent -= OnControllerConnected;
        ReInput.ControllerDisconnectedEvent -= OnControllerDisconnected;
    }



    void OnControllerConnected(ControllerStatusChangedEventArgs args)
    {
        connectedControls = ReInput.controllers.GetControllerNames(ControllerType.Joystick);

        CheckControllerByName(args.name);
    }

    void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
    {
        connectedControls = ReInput.controllers.GetControllerNames(ControllerType.Joystick);

        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
        {
            DisableAllUI();
            EnableUIForPlatform(PLATFORM_TYPE.STEAM, CONTROLLER_TYPE.KEYBOARD);
        }
    }
}

[System.Serializable]
public class GameControllerIconType
{
    public PLATFORM_TYPE platform_Type;
    public CONTROLLER_TYPE controller_Type;
    public List<GameObject> gameObjects;

    public void Disable()
    {
        foreach (var item in gameObjects)
        {
            item.SetActive(false);
        }
    }

    public void Enable()
    {
        foreach (var item in gameObjects)
        {
            item.SetActive(true);
        }
    }
}

public enum PLATFORM_TYPE
{
    STEAM,
    PS,
    XBOX,
    SWITCH
}

public enum CONTROLLER_TYPE
{
    PS,
    XBOX,
    SWITCH,
    KEYBOARD
}