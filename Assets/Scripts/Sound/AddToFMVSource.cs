using UnityEngine;

public class AddToFMVSource : MonoBehaviour
{

    private AudioSource _source;
    private string _sourceName;


    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _sourceName = gameObject.name + StaticStrings.GUID;
    }

    private void Start()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.AddSound(_sourceName, _source);
            AudioManager.Instance.SetVolume(GameManager.ins.savePrefs.VOLUME);
        }
    }


    private void OnDestroy()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.RemoveSound(_sourceName);
        }
    }
}
