using Rewired;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionSelectManager : MonoBehaviour
{
    [SerializeField] Button secondButton;
    [SerializeField] GameUiManager gameUiManager;
    [SerializeField] List<Button> allButtons = new List<Button>();

    PointerEventData pointer;

    private Player player { get { return ReInput.players.GetPlayer(0); } }

    public bool isSelected;


    private void Awake()
    {
        pointer = new PointerEventData(EventSystem.current);
    }

    private void Update()
    {
        if (player.GetButtonDown("UIHorizontal"))//Right Button
        {
            ClearPointer();

            if (!isSelected && !gameUiManager.isGamePause)
            {
                Button lastButton = GetLastButton();
                lastButton.Select();
                lastButton.OnSelect(null);
                isSelected = true;
            }
        }

        if (player.GetNegativeButtonDown("UIHorizontal"))//Left Button
        {
            ClearPointer();

            if (!isSelected && !gameUiManager.isGamePause)
            {
                Button firstButton = GetFirstButton();
                firstButton.Select();
                firstButton.OnSelect(null);
                isSelected = true;
            }
        }
    }

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    void ClearPointer()
    {
        foreach (var item in gameUiManager.gameVideoManager.optionButtons)
        {
            item.OnPointerExit(pointer);
        }
    }

    private Button GetFirstButton()
    {
        foreach (Button item in allButtons)
        {
            if (item.gameObject.activeInHierarchy)
            {
                return item;
            }
        }
        return allButtons[0];
    }

    private Button GetLastButton()
    {
        for (int i = (allButtons.Count - 1); i >= 0; i--)
        {
            if (allButtons[i].gameObject.activeInHierarchy)
            {
                return allButtons[i];
            }
        }
        return allButtons[0];
    }

    void OnDisable()
    {
        isSelected = false;
    }
}
