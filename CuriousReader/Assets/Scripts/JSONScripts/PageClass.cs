using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class PageClass
{
    [System.NonSerialized] public bool Show;
    public int pageNumber;
	public string script;
    public AudioClass audio;
    public GameObjectClass[] gameObjects;
    public TextClass[] texts;
    public string audioFile;
    public TimeStampClass[] timestamps;
    public TriggerClass[] triggers;
    //public bool isTitle;
    //public string fontColor;
    //public int textStartPosition;
}
