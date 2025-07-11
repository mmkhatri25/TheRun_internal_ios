using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChoiceButtonSelection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] TextMeshProUGUI text;
    Button thisButton;

    public UnityAction OnButtonSelected;
    public UnityAction OnButtonDeSelected;


    private void Awake()
    {
        thisButton = gameObject.GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnButtonSelected?.Invoke();
        thisButton.Select();
        thisButton.OnSelect(null);

        //if (text != null)
        //{
        //    text.color = Color.black;
        //}
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //if (text != null)
        //{
        //    text.color = Color.white;
        //}
    }

    public void OnSelect(BaseEventData eventData)
    {
        OnButtonSelected?.Invoke();
        thisButton.Select();
        thisButton.OnSelect(null);

        //if (text != null)
        //{
        //    text.color = Color.black;
        //}
    }

    public void OnDeselect(BaseEventData eventData)
    {
        OnButtonDeSelected?.Invoke();
        //if (text != null)
        //{
        //    text.color = Color.white;
        //}
    }

}
