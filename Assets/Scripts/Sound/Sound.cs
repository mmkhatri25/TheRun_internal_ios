﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Sound 
{
    public string audioName;

    //public AudioClip audioClip;

    [Range(0f,1f)]
    public float volume;

    //[Range(0.1f, 3f)]
    //public float pitch;

    //public bool loop;

    //public bool playOnAwake;

    
    public AudioSource source;
}
