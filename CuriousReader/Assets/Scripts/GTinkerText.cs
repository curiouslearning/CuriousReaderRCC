using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using CuriousReader.Performance;

public class GTinkerText : MonoBehaviour
{
    public List<GTinkerGraphic>pairedGraphics=new List<GTinkerGraphic>();
	public int pairedAnim;
	public StanzaObject stanza;
    public float startTime;//timings corresponding the timings of stanza auto narrate audio 
    private float endTime;//timings corresponding the timings of stanza auto narrate audio 
    public float playTime;//timings corresponding the timings of stanza auto narrate audio 
	public bool star=false;
    public GSManager sceneManager;

    void Start()
    {
		AddCollider ();
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

		FirebaseHelper.LogInAppTouch (("Text_" + gameObject.GetComponent<TextMeshProUGUI> ().text), time.ToString ());

		if (!stanza.stanzaManager.sceneManager.disableSounds)
        {
			PlaySound();
		}

        PerformanceSystem.SendPrompt(this.gameObject, this.gameObject, PromptType.Click);

        CheckPairedGraphic();
	}

    public void CheckPairedGraphic()
    {
        for (int i = 0; i < pairedGraphics.Count; i++)
        {
            if (pairedGraphics[i] != null)
            {
                pairedGraphics[i].OnPairedMouseDown(gameObject);
            }
        }
    }

	// Paired Mouse Down Event
    /// <summary>
    /// this function is called when the tinkertext is linked to particular tinkergraphic and we need to perform the the zooming animation for that text 
    /// </summary>
	public void OnPairedMouseDown(GameObject i_rcInvoker = null)
	{
		if (!stanza.stanzaManager.sceneManager.disableSounds)
        {   
			  PlaySound();
		}

        PerformanceSystem.SendPrompt(i_rcInvoker, this.gameObject, PromptType.PairedClick);
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
        AudioSource a = GetComponent<AudioSource>();
		if ((a != null) && a.isPlaying)
		{
			a.Stop();
		}
	}


	// Resets the state
	public void Reset()
	{

        // If there is an anim attached, stop it from playing and hide it
        //clipResume();
		//iconanimResume();

		/*if (pairedGraphic != null)
		{

			Renderer[] list;
			list = this.pairedGraphic.gameObject.GetComponentsInChildren<Renderer>();
			foreach(Renderer item in list){   //color all the components
				item.material.color = this.pairedGraphic.resetColor;
			}
		}*/
	}

    /// <summary>
    /// AutoPlay -- AutoPlay is invoked when a word is highlighting itself by AutoNarration
    /// This method is called via invocation timer in StanzaObject.
    /// </summary>
    public void AutoPlay()
    {
        if (!TweenSystem.IsTweening(gameObject))
        {
            HighlightTextPerformance autoHighlight = PerformanceSystem.GetTweenPerformance<HighlightTextPerformance>();
            autoHighlight.Init(Color.yellow, i_duration: playTime / 2);
            PerformanceSystem.AddPerformance(gameObject, autoHighlight, PromptType.AutoPlay);
            PerformanceSystem.SendPrompt(gameObject, gameObject, PromptType.AutoPlay);

            if (star)
            {
                // Make paired gameobjects do their thing as though we clicked the word actively.
                if (pairedGraphics != null)
                {
                    foreach (GTinkerGraphic rcGraphic in pairedGraphics)
                    {
                        rcGraphic.OnPairedMouseDown(gameObject);
                    }
                }

                // Notify the scene that a starword was hit in case the scene manager 
                // is custom and cares about this.
                if (stanza != null)
                {
                    if (stanza.stanzaManager != null)
                    {
                        if (stanza.stanzaManager.sceneManager != null)
                        {
                            stanza.stanzaManager.sceneManager.OnStarWordActivated(gameObject);
                        }
                    }
                }
            }
        }

    }

}