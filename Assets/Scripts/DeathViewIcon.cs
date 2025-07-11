using UnityEngine;

public class DeathViewIcon : MonoBehaviour
{
    [SerializeField] private GameObject lockObject;
    [SerializeField] private GameObject unlockObject;

    public void Lock()
    {
        lockObject.SetActive(true);
        unlockObject.SetActive(false);
    }

    public void UnLock()
    {
        lockObject.SetActive(false);
        unlockObject.SetActive(true);
    }
}
