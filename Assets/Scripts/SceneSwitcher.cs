using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] private string scenName = string.Empty;
    [SerializeField] private bool switchOnStart;
    [SerializeField] private float delayToSwitch = 0;

    private void Start()
    {
        if (switchOnStart)
        {
            SwitchScene();
        }
    }

    public void SwitchScene()
    {
        StartCoroutine(SceneSwitchStart());
    }

    private IEnumerator SceneSwitchStart()
    {
        yield return new WaitForSeconds(delayToSwitch);

        SceneManager.LoadScene(scenName);
    }
}
