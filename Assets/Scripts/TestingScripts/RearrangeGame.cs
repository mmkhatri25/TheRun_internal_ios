using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RearrangeGame : MonoBehaviour
{
    public List<TextMeshProUGUI> displayText = new List<TextMeshProUGUI>();
    public Button[] optionButtons;
    public string[] textSequence;

    private int currentIndex = 0;

    void Start()
    {

    }

    public void OnButtonClick(int index)
    {
        int buttonIndex = index;

        if (buttonIndex == currentIndex)
        {
            currentIndex++;
            optionButtons[buttonIndex].gameObject.SetActive(false);

            if (currentIndex < textSequence.Length)
            {
                UpdateDisplayText();
            }
            else
            {
                // Game completed
                Debug.Log("Game completed!");
            }
        }
    }

    void UpdateDisplayText()
    {
        displayText[currentIndex].text = textSequence[currentIndex];
    }
}
