using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Space(5)]
    public List<Sound> sounds;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

    }

    public void PlaySound(string Soundname)
    {
        //Sound s = Array.Find(sounds, sound => sound.audioName == Soundname);
        Sound s = sounds.Find(p => p.audioName == Soundname);
        if (s == null)
        {
            Debug.LogWarning("Sound with " + Soundname + " not found!!");
            return;
        }

        s.source.Play();
    }

    public void PlayOneShot(string Soundname)
    {
        //Sound s = Array.Find(sounds, sound => sound.audioName == Soundname);
        Sound s = sounds.Find(p => p.audioName == Soundname);
        if (s == null)
        {
            Debug.LogWarning("Sound with " + Soundname + " not found!!");
            return;
        }

        s.source.Play();
    }


    public AudioSource GetAudioSource(string Soundname)
    {
        Sound s = sounds.Find(p => p.audioName == Soundname);
        if (s == null)
        {
            Debug.LogWarning("Sound with " + Soundname + " not found!!");
            return null;
        }

        return s.source;
    }


    public void StopSound(string soundName)
    {
        //Sound s = Array.Find(sounds, sound => sound.audioName == soundName);
        Sound s = sounds.Find(p => p.audioName == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound with " + soundName + " not found!!");
            return;
        }

        if (s.source.isPlaying)
            s.source.Stop();


    }

    public void AddSound(string soundName, AudioSource audioSource)
    {
        //Sound s = Array.Find(sounds, sound => sound.audioName == soundName);
        Sound s = sounds.Find(p => p.audioName == soundName);
        if (s == null)
        {
            Sound newSound = new Sound();
            newSound.audioName = soundName;
            newSound.source = audioSource;
            sounds.Add(newSound);
            return;
        }
        else
        {
            s.source = audioSource;
        }
    }

    public void RemoveSound(string soundName)
    {
        Sound s = sounds.Find(p => p.audioName == soundName);
        if (s != null)
        {
            sounds.Remove(s);
            return;
        }
    }

    public void StopAllSound()
    {
        foreach (Sound s in sounds)
        {
            if (s.source.isPlaying)
                s.source.Stop();
        }
    }


    public void SetVolume(Slider slider)
    {
        foreach (Sound s in sounds)
        {
            if (s.source != null)
                s.source.volume = slider.value;
        }

        GameManager.ins.SetFloat(out GameManager.ins.savePrefs.VOLUME, slider.value);
        GameManager.ins.SaveAllPref();
    }


    public void SetVolume(float volume)
    {
        foreach (Sound s in sounds)
        {
            if (s.source != null)
            {
                s.volume = volume;
                s.source.volume = volume;
            }
        }
    }


    public void ReduceVolumeForSound(string Soundname, float volume)
    {

        //Sound s = Array.Find(sounds, sound => sound.audioName == Soundname);
        Sound s = sounds.Find(p => p.audioName == Soundname);
        if (s == null)
        {
            Debug.LogWarning("Sound with " + Soundname + " not found!!");
            return;
        }
        else
            s.source.volume = volume;
    }

    public void MuteAllAudio(string[] skipParticulars)
    {
        //Use this to mute audio source expect the list of sources provided in "skipParticulars" list

        foreach (Sound s in sounds)
        {
            if (s.source != null)
                s.source.mute = true;
        }

        foreach (var item in skipParticulars)
        {
            Sound s = sounds.Find(p => p.audioName == item);
            if (s.source != null)
                s.source.mute = false;
        }
    }

    public void MuteAllAudio()
    {
        foreach (Sound s in sounds)
        {
            if (s.source != null)
                s.source.mute = true;
        }
    }

    public void UnMuteAllAudio()
    {
        foreach (Sound s in sounds)
        {
            if (s.source != null)
                s.source.mute = false;
        }
    }

}
