using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Anim
{
    [System.NonSerialized] public bool Show;
    public int id;
    public string animName;
	public int startX;
	public int startY;
	public int startIndex;
	public int endIndex;
	public bool onTouch;
	public bool onStart;
    public float[] secPerFrame;
    public Sequence[] sequences; 
}
