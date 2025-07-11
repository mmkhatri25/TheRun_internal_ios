using Rewired;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArrangeGameOptionSelectManager : MonoBehaviour
{

    [SerializeField] List<Button> buttonsList = new List<Button>();


    PointerEventData pointer;

    bool isSelected;

    private Player player { get { return ReInput.players.GetPlayer(0); } }

    private void Awake()
    {
        pointer = new PointerEventData(EventSystem.current);
    }

    private void OnEnable()
    {
        //ClearPointer();

        //buttonsList[0].Select();
        //buttonsList[0].OnSelect(null);

        ClearPointer();
    }


    private void Update()
    {
        if (player.GetButtonDown("UIHorizontal"))//Right Button
        {
            ClearPointer();

            if (isSelected)
            {
                return;
            }

            foreach (var item in buttonsList)
            {
                if (item.gameObject.activeSelf)
                {
                    isSelected = true;
                    item.Select();
                    item.OnSelect(null);
                    break;
                }
            }

        }

        if (player.GetNegativeButton("UIHorizontal"))//Left Button
        {
            ClearPointer();


            if (isSelected)
            {
                return;
            }

            foreach (var item in buttonsList)
            {
                if (item.gameObject.activeSelf)
                {
                    isSelected = true;
                    item.Select();
                    item.OnSelect(null);
                    break;
                }
            }
        }
    }

    void ClearPointer()
    {
        foreach (var item in buttonsList)
        {
            item.OnPointerExit(pointer);
        }
    }

    public void OnClickButton()
    {
        isSelected = false;

        foreach (var item in buttonsList)
        {
            if (item.gameObject.activeSelf)
            {
                isSelected = true;
                item.Select();
                item.OnSelect(null);
                break;
            }
        }
    }
}
