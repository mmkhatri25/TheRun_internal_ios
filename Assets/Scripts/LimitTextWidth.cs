using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LimitTextWidth : MonoBehaviour
{
    [SerializeField] private int characterLimit = 300;
    [SerializeField] private RectTransform layoutToRefresh;

    private TextMeshProUGUI textBox;
    private LayoutElement layoutElement;

    void Start()
    {
        textBox = GetComponent<TextMeshProUGUI>();
        layoutElement = GetComponent<LayoutElement>();
    }

    void Update()
    {
        if (layoutElement != null)
        {

            layoutElement.enabled = (textBox.text.Length > characterLimit) ? true : false;
            if (layoutToRefresh != null) LayoutRebuilder.ForceRebuildLayoutImmediate(layoutToRefresh);
        }
    }

}

