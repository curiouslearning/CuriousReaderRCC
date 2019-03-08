using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class TextClass
{ 
    [System.NonSerialized] public bool Show;
    public int id;
    public string text;

    // Though I'd love to use a full transform set with position, rotation, and scale here.  I don't think it will
    // be that helpful in the end and I think there is value in not letting too much control get out there... for example
    // the depth Z level of the text should be relatively the same for authoring IMHO.  So I'm just going to expose X, Y
    // and a default value of (0.0f, 0.0f) should be considered for autoplacement.

    public bool customPosition = false;

    public float x = 0.0f;
    public float y = 0.0f;

}
