using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingButtonNavigation : Selectable, ISelectHandler, IDeselectHandler, IMoveHandler
{

    public GameObject highlighter;
    public Button previous_button;
    public Button next_button;

    public override void OnDeselect(BaseEventData eventData)
    {
        ExitPointerCustom();
    }

    public override void OnSelect(BaseEventData eventData)
    {
        EnterPointerCustom();
    }

    public void EnterPointerCustom()
    {
        highlighter.SetActive(true);
        transform.DOScale(Vector3.one * 1.1f, 0.3f).SetUpdate(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound("HoverSFX");
    }

    public void ExitPointerCustom()
    {
        highlighter.SetActive(false);
        transform.DOScale(Vector3.one, 0.3f).SetUpdate(true);
    }

    public override void OnMove(AxisEventData eventData)
    {
        switch (eventData.moveDir)
        {
            case MoveDirection.Left:
                if (previous_button.IsInteractable())
                    previous_button.onClick.Invoke();
                break;
            case MoveDirection.Right:
                if (next_button.IsInteractable())
                    next_button.onClick.Invoke();
                break;
            case MoveDirection.Up:
                base.OnMove(eventData);
                break;
            case MoveDirection.Down:
                base.OnMove(eventData);
                break;
        }
    }
}
