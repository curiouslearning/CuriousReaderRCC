using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class GameObjectClass
{
    [System.NonSerialized] public bool Show;
    public int id;
	public string tag;
    public float posX;
    public float posY;
    public float scaleX;
    public float scaleY;
    public int orderInLayer;
    public string imageName;
    public bool inText;
    public string label;
	public string destroyOnCollision;
    public bool draggable;
    public Anim[] anim;

}
