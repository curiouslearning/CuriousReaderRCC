using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System;

public class PngToAnim : MonoBehaviour {
    private SpriteRenderer spr;
   // public String animPath;
    public static Sprite[] sprites;
    private int currentframe = 0;
    private float SecPerFrame = 0.25f;
    private bool isLooping = false;
    private void Awake()
    {
       
        spr = GetComponent<SpriteRenderer>();
        currentframe = 0;
        //PlayAnimation(0, 0.25f, "loop");

    }
   
    public void PlayAnimation(int ID,float secPerFrame,bool isLooping)
    {
        SecPerFrame = secPerFrame;
        this.isLooping = isLooping;
        StopCoroutine("AnimateSprite");
        switch(ID)
        {
            default:
                currentframe = 0;
                StartCoroutine("AnimateSprite",ID);
                break;
        }
    }
    IEnumerator AnimateSprite(int ID)
    {
        switch(ID)
        {
            default:
                yield return new WaitForSeconds(SecPerFrame);
                spr.sprite = sprites[currentframe];
                currentframe++;
                if(currentframe>=sprites.Length)
                {
                    if (isLooping)
                    {
                        currentframe = 0;
                    }
                    else
                    {
                        break;
                    }
                }
                StartCoroutine("AnimateSprite", ID);
                break;
        }
    }
    /*void Start()
   {

      AnimationClip animClip = new AnimationClip();
       animClip.frameRate = 25;
       EditorCurveBinding spriteBinding = new EditorCurveBinding();
       spriteBinding.type = typeof(SpriteRenderer);
       spriteBinding.path = "";
       spriteBinding.propertyName = "m_sprite";
       ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Length];
       for(int i=0;i<(sprites.Length);i++)
       {
           spriteKeyFrames[i] = new ObjectReferenceKeyframe();
           spriteKeyFrames[i].time = i;
           spriteKeyFrames[i].value = sprites[i];
       }
       AnimationUtility.SetObjectReferenceCurve(animClip,spriteBinding,spriteKeyFrames);
       AssetDatabase.CreateAsset(animClip,"Assets/anims/clock.anim");
       AssetDatabase.SaveAssets();
       AssetDatabase.Refresh();
   }*/

    /*void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        loopanim();
    }

    // Update is called once per frame
    void nonloopanim () {
        count = 0;
        while(count<sprites.Length)
        {
            spr.sprite = sprites[count];
            StartCoroutine(Wait(25f));
            count++;
        }
        
	}
    void loopanim()
    {
        int i = 1;
        count = 0;
        while(i==1)
        {
            spr.sprite =sprites[count];
            StartCoroutine(Wait(25f));
            count = (count + 1) % sprites.Length;

        }
    }
    
    IEnumerator Wait(float sec)
    {
        yield return new WaitForSeconds(sec);
    }*/

}
