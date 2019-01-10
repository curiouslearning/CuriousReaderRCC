using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerType
{
    Activate = 1,
    Deactivate = 2, 
//    BiDirectional = 3,
    FadeIn = 4,
    FadeOut = 5,
    Navigation = 6,
    Animation = 7
}

[System.Serializable]
public class TriggerClass
{
    [System.NonSerialized] public bool Show;
    public int stanzaID;
    public TriggerType type;
    public float timing;
    public int textId;
    public int timestamp;
    public int sceneObjectId;
	public int animId;
    public int typeOfLinking;
    public bool DeactivateNextButton;
    public int NavigationPage;
}

