using System.Collections.Generic;
using UnityEngine;

public class ActivateDebugPanel : MonoBehaviour
{
    [SerializeField] private GameConfig buildType;
    [SerializeField] private GameObject activateButton;
    [SerializeField] private List<GameObject> objectToActivate = new List<GameObject>();
    // Start is called before the first frame update

    int clickCount;

    private void Start()
    {
        if (buildType.gameType == GameType.Release)
        {
            Destroy(gameObject);
        }
    }

    public void ShowPanel()
    {
        clickCount++;

        if (clickCount >= 5)
        {

            ShowHideObjects(true);
            clickCount = 0;
        }
    }


    public void ShowHideObjects(bool show)
    {
        foreach (GameObject obj in objectToActivate)
        {
            obj.SetActive(show);
        }

        if (!show)
        {
            clickCount = 0;
            activateButton.SetActive(true);
        }
        else
        {
            activateButton.SetActive(false);
        }

    }

    public void ClearData()
    {
        PlayerPrefs.DeleteAll();
    }
}
