using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Rewired.Integration.UnityUI;
using UnityEngine.UI;

public class DefaultButtonHighlight : MonoBehaviour
{

    public GameObject defaultButton;
    public List<GameObject> AllMenuButtons;

    private EventSystem _eventSystem;


    private void Awake()
    {
        _eventSystem = EventSystem.current;
    }

    private void OnEnable()
    {
        foreach (var item in AllMenuButtons)
        {
            item.transform.localScale = Vector3.one;
        }
        HighlightButton(defaultButton);
    }

    IEnumerator SelectButton(GameObject thisButton)
    {
        yield return new WaitForSecondsRealtime(0.1f);

        _eventSystem.SetSelectedGameObject(null);
        _eventSystem.SetSelectedGameObject(thisButton);


    }

    public void HighlightButton(GameObject thisButton)
    {
        if (thisButton != null)
            StartCoroutine(SelectButton(thisButton));
    }

    public void SetDefaultButton(GameObject thisButton)
    {
        defaultButton = thisButton;
    }

    public void HighlightDefaultButton()
    {
        if (defaultButton != null)
            StartCoroutine(SelectButton(defaultButton));
    }

}
