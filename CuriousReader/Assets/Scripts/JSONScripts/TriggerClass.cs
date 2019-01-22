using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuriousReader.Performance;
using CuriousReader.BookBuilder;
using UnityEditor;

public enum TriggerType
{
    Activate = 1,
    Deactivate = 2, 
//    BiDirectional = 3,
    FadeIn = 4,
    FadeOut = 5,
    Navigation = 6,
    Animation = 7,
    Highlight = 8,
    Move = 9,
    Rotate = 10,
    Scale = 11
}

[System.Serializable]
public class TriggerClass
{
    [System.NonSerialized] public bool Show;
    public int stanzaID;
    public TriggerType type;
    public float timing;
    public int textId;
    public int animId;
    public int timestamp;
    public int sceneObjectId;
    public string Params;
    [System.NonSerialized] public PropertyField[] EditorFields;
    [System.NonSerialized] public PerformanceParams PerformanceParams;
}

