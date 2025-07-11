using UnityEngine;
using UnityEngine.EventSystems;
public class SetButtonSelection : MonoBehaviour
{

    [SerializeField] GameObject objectToSelect;


    private void OnEnable()
    {
        SetGameObjectSelection();
    }

    public void SetGameObjectSelection()
    {
        EventSystem.current.SetSelectedGameObject(null);

        EventSystem.current.SetSelectedGameObject(objectToSelect);
    }

    
}
