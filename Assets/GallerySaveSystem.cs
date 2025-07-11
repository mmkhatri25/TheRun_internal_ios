using Bayat.SaveSystem;
using System.Threading.Tasks;
using UnityEngine;

public abstract class GallerySaveSystem : MonoBehaviour
{
    [SerializeField]
    protected SaveSystemSettingsPreset settings;

    public virtual void Awake()
    {
        if (settings == null)
        {
            return;
        }
        settings.ApplyTo(SaveSystemSettings.DefaultSettings);
    }

    public abstract void Save(string value);
    public abstract Task<string> GetString(string ID);

    public abstract void Load();

    //public abstract void Delete();
}
