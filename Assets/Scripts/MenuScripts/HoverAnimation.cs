using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{

    [SerializeField] RectTransform objToAnimate;
    [SerializeField] TextMeshProUGUI text;
    Image image;
    Button button;

    private void Awake()
    {
        objToAnimate = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        button = GetComponent<Button>();

        button.onClick.AddListener(UpPointer);

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EnterPointer();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //ExitPointer();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        UpPointer();
    }


    public void EnterPointer()
    {
        if (objToAnimate != null)
        {
            objToAnimate.DOScale(Vector3.one * 1.1f, 0.3f).SetUpdate(true);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySound("HoverSFX");

            //if (image != null)
            //    image.enabled = true;

            button.Select();
        }

        //if (text != null)
        //{
        //    text.color = Color.black;
        //}
    }

    public void ExitPointer()
    {
        this.GetComponent<Selectable>().OnPointerExit(null);

        if (objToAnimate != null)
        {
            objToAnimate.DOScale(Vector3.one, 0.3f).SetUpdate(true);

            //if (image != null)
            //    image.enabled = false;

        }

        if (text != null)
        {
            text.color = Color.white;
        }
    }

    public void UpPointer()
    {
        if (objToAnimate != null)
        {
            objToAnimate.DOScale(Vector3.one, 0f).SetUpdate(true);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySound("SelectSFX");

            //if (image != null)
            //    image.enabled = false;
        }

        if (text != null)
        {
            text.color = Color.white;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        EnterPointer();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        ExitPointer();
    }


}
