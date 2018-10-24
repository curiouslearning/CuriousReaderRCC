using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Sequence
{
    [System.NonSerialized] public bool Show;
    public int startFrame;
	public int endFrame;
	public int noOfLoops;
    public Movable movable;
}
