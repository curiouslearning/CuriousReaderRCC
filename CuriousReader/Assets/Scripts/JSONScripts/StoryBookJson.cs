using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class StoryBookJson{
	public int id;
    public string fontColor;
	public float textStartPositionX;
	public float textStartPositionY;
	public int textFontSize;
    public string storyImageFile;
    public string backgroundColor;
    public string arrowColor;
    public string fontFamily;
    public string language;
   public PageClass[] pages;
    
}
