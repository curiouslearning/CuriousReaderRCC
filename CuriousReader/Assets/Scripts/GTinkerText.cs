using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
//20.3,-18.8 (yellow), 0,-17.4 (white),blue (22.9,-0.4)
public class GTinkerText : MonoBehaviour {
    //private static bool check=false;
    public List<GTinkerGraphic>pairedGraphics=new List<GTinkerGraphic>();
	//public GTinkerGraphic pairedGraphics;
	public int pairedAnim;
	public StanzaObject stanza;
    private float startTime;//timings corresponding the timings of stanza auto narrate audio 
    private float endTime;//timings corresponding the timings of stanza auto narrate audio 
    public float playTime;//timings corresponding the timings of stanza auto narrate audio 
    private Animator wordanimator;
    private Animator graphicanimator;
    public GameObject anim;
    public GameObject anim2;
	public bool star=false;

    void Start()
    {
		AddCollider ();
        wordanimator = GetComponent<Animator>();
        if (anim2 != null)
            graphicanimator = anim2.GetComponent<Animator>();
    }
    /// <summary>
    /// it set up the timing for each tinkertext
    /// each timing corresponds to the timing of occurance of a particular text in the complete stanza audio
    /// </summary>
    /// <param name="timeStamp"></param>

    // Takes an xml word element and reads and sets the timing data
    public void SetupWordTiming(TimeStampClass timeStamp)
    {
        startTime = timeStamp.start / 1000.0f;
        endTime = timeStamp.end / 1000.0f;
        playTime = endTime - startTime;
		if (timeStamp.starWord == "Yes")
			star = true;
    }
    /// <summary>
    /// return the starting time for each word
    /// </summary>
    /// <returns></returns>
    // Returns the absolute start time
    public float GetStartTime()
    {
        return startTime;
    }
    /// <summary>
    /// return the end time for each word
    /// </summary>
    /// <returns></returns>
    // Returns the absolute end time
    public float GetEndTime()
    {
        return endTime;
    }

    /// <summary>
    /// add the box collider to the particular word
    /// </summary>
	// Adds a box collider based on initial text mesh size (and makes sure it is large enough)
	private void AddCollider()
	{
		// Setup a trigger collider at runtime so it is the same bounds as the text
		BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
		col.isTrigger = true;
		RectTransform trans= gameObject.GetComponent<RectTransform>();
		col.size = new Vector2(trans.sizeDelta.x, trans.sizeDelta.y);
		col.offset = new Vector2( (trans.rect.x +trans.sizeDelta.x/2), (trans.rect.y +trans.sizeDelta.y/2) ) ;
		// Check against a collider width that is too small (tough to tap on "I" or "1")
		if (col.size.x <= 0.055f)
		{
			// increase size x4
			Vector2 newSize = new Vector2(col.size.x * 4.0f, col.size.y);
			col.size = newSize;
		}
	}



    /// <summary>
    /// this function bring the zoomed out text animation to its original state
    /// </summary>
    public void clipResume()
    {
        wordanimator.Play("textzoomin");
		wordanimator.ResetTrigger("tapme");    
	}
    /// <summary>
    /// this function sets the tap me trigger so that the text can go from normal to zoomed out state 
    /// </summary>
    public void clipPlay()
	{   
			AudioSource source = gameObject.GetComponent<AudioSource> ();
			//this.GetComponent<RectTransform>().pivot = ;
			playTime = 0.21f;
			wordanimator.speed = 1 / (playTime);

			//source.Play();
			wordanimator.SetTrigger ("tapme");


    }
    public void iconanimPlay()

	{
		if (anim != null)
		{
			anim.SetActive(true);
		}
	}


    public void iconanimResume()
    {
        if (anim != null)
        {
            anim.SetActive(false);
		}
	}
    

    public void graphicPlay()
    {
        if (anim2 != null)
            anim2.SetActive(true);
	}

    void graphicResume()
	{if (anim2 != null)
		anim2.SetActive(false);
        
    }


	// Mouse Down Event
    /// <summary>
    /// this function performs the text_animation(zoomin + zoomout) for the each tinkertext on mousedown event
    /// this function also play the corresponding graphical animation if there is paired graphic 
    /// </summary>
    /// <param name="suppressAnim"></param>
	public void MyMouseDown(bool suppressAnim = false)
	{   
			System.DateTime time = System.DateTime.Now;
			//sending data directly to firebase using "72 hours rule"! (removed local data storage)
			//DataCollection.AddInTouchData( ("Text_"+gameObject.GetComponent<Text>().text) , time.ToString());

			FirebaseHelper.LogInAppTouch (("Text_" + gameObject.GetComponent<TextMeshPro> ().text), time.ToString ());

			if (!stanza.stanzaManager.sceneManager.disableSounds) {
				PlaySound();
			}
			clipPlay ();
			iconanimPlay ();

			if (!suppressAnim) {
				graphicPlay ();
			}
        CheckPairedGraphic();

        // Is there a TinkerGraphic paired with this TinkerText?
        
	}
    public void CheckPairedGraphic()
	{   
		for (int i = 0; i < pairedGraphics.Count; i++){
		if (pairedGraphics[i]!= null) 
		    {
            
                //if animation is present
			if (pairedAnim>=0) 
				{ 
				pairedGraphics [i].OnPairedMouseDown (this);
		
			} 
		    else 
				{   //StopCoroutine ("Animate");
					//StopCoroutine (pairedGraphics[i].Animdelay ());
				    pairedGraphics [i].ResetandZoom ();
			}
			}
        
		  }
	}


	// Paired Mouse Down Event
    /// <summary>
    /// this function is called when the tinkertext is linked to particular tinkergraphic and we need to perform the the zooming animation for that text 
    /// </summary>
	public void OnPairedMouseDown()
	{
		if (!stanza.stanzaManager.sceneManager.disableSounds)
      		{   
			PlaySound();
		}

		clipPlay();
		iconanimPlay();
	}

	// Mouse Currently Down Event
	public void OnMouseCurrentlyDown()
	{
		if (!stanza.stanzaManager.sceneManager.disableSounds)
		{   
			PlaySound();
		}

		clipPlay();
		iconanimPlay();

		// Is there a TinkerGraphic paired with this TinkerText?
		/*if (pairedGraphic)
		{
			// Then send the event along!
		//	pairedGraphic.OnPairedMouseCurrentlyDown(this);
		}*/
	}

	// Paired Mouse Currently Down Event
	public void OnPairedMouseCurrentlyDown()
	{
		if (!stanza.stanzaManager.sceneManager.disableSounds)
		{    
			PlaySound();
		} 

		clipPlay();
		iconanimPlay();
	}

	// Mouse Up Event
    /// <summary>
    /// this function is called when there is mouse up event
    /// on this event the text gos from zoomed out to the normal state
    /// </summary>
	public void MyOnMouseUp()
	{
		// Is there a TinkerGraphic paired with this TinkerText?
		/*if (pairedGraphic)
		{
			// Then send the event along!
			//pairedGraphic.OnPairedMouseUp(this);
		}*/

        clipResume();
		iconanimResume();
		graphicResume();
	}
		
	// Plays any sound that is attached
    /// <summary>
    /// this function plays the sound for each tinkrtext
    /// </summary>
	public void PlaySound()
	{   
		if (!GetComponent<AudioSource>().isPlaying)
		{
			GetComponent<AudioSource>().Play();
		}
	}
    /// <summary>
    /// this function stops the sound for each tinkrtext
    /// </summary>
	// Stops any sound that is attached 
	public void StopSound()
	{
		if (GetComponent<AudioSource>().isPlaying)
		{
			GetComponent<AudioSource>().Stop();
		}
	}


	// Resets the state
	public void Reset()
	{

        // If there is an anim attached, stop it from playing and hide it
        clipResume();
		iconanimResume();

		/*if (pairedGraphic != null)
		{

			Renderer[] list;
			list = this.pairedGraphic.gameObject.GetComponentsInChildren<Renderer>();
			foreach(Renderer item in list){   //color all the components
				item.material.color = this.pairedGraphic.resetColor;
			}
		}*/
	}

}