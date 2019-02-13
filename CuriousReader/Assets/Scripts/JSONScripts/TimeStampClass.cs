using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class TimeStampClass
{
    [System.NonSerialized] public bool Show;
    [System.NonSerialized] public AudioClip SubClip;
    [System.NonSerialized] public float[] AudioData;
    [System.NonSerialized] public bool IsPlaying;
    public int start;
    public int wordIdx;
    public string audio;
    public int end;
	public string starWord;
}
