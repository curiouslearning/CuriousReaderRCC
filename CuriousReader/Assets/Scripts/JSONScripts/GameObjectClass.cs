using System.Collections;
using System.Collections.Generic;
using Elendow.SpritedowAnimator;
using UnityEngine;
[System.Serializable]
public class GameObjectClass
{
    [System.NonSerialized] public bool Show;
    [System.NonSerialized] public int ImageIndex;
    public int id;
	public string tag;
    public float posX;
    public float posY;
    public float posZ;
    public float scaleX;
    public float scaleY;
    public float rotX;
    public float rotY;
    public float rotZ;
    public int orderInLayer;
    public string imageName;
    [System.NonSerialized] public GameObject editorImageObject;
    public bool inText;
    public string label;
	public string destroyOnCollision;
    public bool draggable;
    public Anim[] anim;
    public string[] Animations;
    [System.NonSerialized] public int[] AnimationsID;
}
