using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CuriousReader.Performance;

/// <summary>
/// Script responsible for controling the scenes.
/// </summary>
public class GSManager :  MonoBehaviour {

	[HideInInspector]
	public GGameManager gameManager;
	public Canvas myCanvas;


	// Manager for all TinkerTexts and stanza
	public GStanzaManager stanzaManager;

    // Whether to allow input on text/graphics during autoplay
	public bool inputAllowedDuringAutoplay = true;

	// Whether to interrupt auto play if a single word is hit
	public bool inputInterruptsAutoplay = true;

	// Disable auto play?
	[HideInInspector]
	public bool disableAutoplay = false;

	// Disable sounds?
	[HideInInspector]
	public bool disableSounds = false;

	// Drag event active?
	[HideInInspector]
	public bool dragActive = false;

    //private int countDownEvent = 0;
    public static AudioSource[] sounds;

	//Navigation buttons
	public GameObject Lbutton;
	public GameObject Rbutton;

	/// <summary>
	/// Loads the texts timings if stanzamanager is not null.
	/// Calls on Auto stanza play.
	/// </summary>
	public virtual void Start() 
	{  

		// Reset flags
		// dragActive = false;
		// disableAutoplay = false;
		// disableSounds = false;

		// If we have a stanza manager
		if (stanzaManager != null)
		{
			// And it has an audio clip and xml defined already in the scene
			if (LoadAssetFromJSON.storyBookJson.pages[LoadAssetFromJSON.pageNumber].timestamps.Length >0)
			{
				// Then have it set the xml up
				stanzaManager.LoadStanzaJSON();
			}
		}
        if(ShelfManager.AutoNarrate == true)
			PlayStanzaAudio(); 
		    
	}

	/// <summary>
	/// Plays the stanza narration if sprite is narrateOn and stanzamanager and gamemanager are not null.
	/// </summary>
	 void PlayStanzaAudio(){
		
		if (stanzaManager != null && gameManager!= null)
		{
			stanzaManager.RequestAutoPlay (stanzaManager.stanzas [0], stanzaManager.stanzas [0].tinkerTexts [0]);
		}
	}

	/// <summary>
	/// if intruption allowed during auto play is true , return true .
	/// if stanzamanager is not null , return depending on autoplaying.
	/// return true if stanza manager is null.
	/// </summary>
	public bool IsInputAllowed()
	{
		if (inputAllowedDuringAutoplay)
		{
			return true;
		}
		else if (stanzaManager != null)
		{
			return !stanzaManager.IsAutoPlaying();   
		}

		return true; // if stanza manager is null
	}
		
    /// <summary>
    /// OnStarWordActivated -- Virtual method for what to do when a star word is activated by the
    /// page.  By default it does nothing.  However, some special pages might want to hook into 
    /// this to have fancy behaviors.
    /// </summary>
    /// <param name="i_rcWord"></param>
    public virtual void OnStarWordActivated(GameObject i_rcWord)
    {
    }

	/// <summary>
	/// If input is allowed(during stanza play / after stanza play) check if Tinkertext or Tinkergraphic 
	/// If tinkertext , call stanzamanager onmousedown(tinkrtext).
	/// If tinkergraphic , call tinkrgraphic.mymousedown.
	/// This function can be overriden by specific scene manager.
	/// </summary>
	public virtual void OnMouseDown(GameObject go)
	{
            // Lock out other input during auto play?
            if (IsInputAllowed())
            {
                // TinkerText object 
                if (go.GetComponent<GTinkerText>() != null)
                {  
                
                    GTinkerText tinkerText = go.GetComponent<GTinkerText>();

                    if (tinkerText != null)
                    {
                        if (stanzaManager != null)
                        {
                            // Is an autoplay in progress? If so, see if we should interrupt
						    if (stanzaManager.IsAutoPlaying () && inputInterruptsAutoplay) {
                            stanzaManager.CancelAutoPlay();
						}
                          stanzaManager.OnMouseDown(tinkerText);
                        }
                    }
                }
                // TinkerGraphic object
                else if (go.GetComponent<GTinkerGraphic>() != null)
			    {
                    GTinkerGraphic tinkerGraphic = go.GetComponent<GTinkerGraphic>();
                    if (tinkerGraphic != null)
                    {
                        tinkerGraphic.MyOnMouseDown();
                    }
                }
		    }
	}

	/// <summary>
	/// Here we have a superclass intercept for catching global TinkerGraphic mouse down events.
	/// If graphic is paired to text ,this fuction is called.
	/// </summary>
    /// <param name="tinkerGraphic">Tinker graphic.</param>
	public virtual void OnMouseDown(GTinkerGraphic tinkerGraphic)
	{
        if (tinkerGraphic != null)
        {
            bool textPrompted = false;
            foreach (GameObject pairedObject in tinkerGraphic.PairedObjects)
            {
                GTinkerText rcTinkerText = pairedObject.GetComponent<GTinkerText>();
                if (rcTinkerText != null)
                {
                    if (!textPrompted)
                    {
                        stanzaManager.OnPairedMouseDown(pairedObject);
                        textPrompted = true;
                    }
                }
            }
        }
    }

	/// <summary>
	/// Here we have a superclass intercept for catching global TinkerText paired mouse down events.
	/// </summary>
	/// <param name="tinkerText">Tinker text.</param>
	public virtual void OnPairedMouseDown(GTinkerText tinkerText)
	{
		
	}

	/// <summary>
	/// Here we have a superclass intercept for catching global GameObject mouse currently down events.
	/// If input is allowed(during stanza play / after stanza play) check if Tinkertext or Tinkergraphic 
	/// If tinkertext , call stanzamanager onmousecurrentlydown(tinkrtext).
	/// If tinkergraphic , call tinkrgraphic onmousecurrentlydown.
	/// This function can be overriden by specific scene manager.
	/// </summary>
	/// <param name="go">Game object clicked.</param>
	public virtual void OnMouseCurrentlyDown(GameObject go)
	{
		// Lock out other input during auto play?
		if (IsInputAllowed())
		{
			// TinkerGraphic object
			if (go.GetComponent<GTinkerGraphic>() != null)
			{
				GTinkerGraphic tinkerGraphic = go.GetComponent<GTinkerGraphic>();

				if (tinkerGraphic != null)
				{
					tinkerGraphic.OnMouseCurrentlyDown();
				}
			}
		}
	}

	/// <summary>
	/// Here we have a superclass intercept for catching global TinkerGraphic mouse currently down events.
	/// Move the graphic if draggable.
	/// </summary>
	/// <param name="tinkerGraphic">Tinker graphic.</param>
	public virtual void OnMouseCurrentlyDown(GTinkerGraphic tinkerGraphic)
	{
        if (tinkerGraphic.GetDraggable())
        {
            tinkerGraphic.MoveObject();
        }
        
    }

	/// <summary>
	/// Here we have a superclass intercept for catching global GameObject mouse up events.
	/// If object not draggable and tinker text is not null , call stanza manager onMouseUp(tinkertext). 
	/// If tinkergraphic , call tinkrgraphic.MyOnMouseUp.
	/// This function can be overriden by specific scene manager.
	/// </summary>
	/// <param name="go">Game Object selected.</param>
	public virtual void OnMouseUp(GameObject go)
	{
		if (go.GetComponent<GTinkerGraphic>() != null)
		{
			GTinkerGraphic tinkerGraphic = go.GetComponent<GTinkerGraphic>();

			if (tinkerGraphic != null)
			{
				tinkerGraphic.MyOnMouseUp();
			}
		}
	}

	/// <summary>
	/// Here we have a superclass intercept for catching global TinkerGraphic mouse up events.
	/// </summary>
	/// <param name="tinkerGraphic">Tinker graphic.</param>
	public virtual void OnMouseUp(GTinkerGraphic tinkerGraphic)
	{
		// override me
	}

	/// <summary>
	/// Override if a scene manager subclass needs a hint manager.
	/// </summary>
	public virtual IEnumerator StartHintManager()
	{
		yield break;
	}


	/// <summary>
	/// Override if a scene manager subclass needs a graphic hint
	/// </summary>
	public virtual IEnumerator PlayHintAnimation()
	{
		yield break;
	}

	/// <summary>
	/// Resets the states on mouse up.
	/// Reset all the Tinker graphics to normal state
	/// </summary>
	/// <param name="mouseEvent">Mouse event.</param>
	public virtual void ResetInputStates(GGameManager.MouseEvents mouseEvent)
	{
        if (stanzaManager != null)
		{
			stanzaManager.ResetInputStates(mouseEvent);
		}

		GTinkerGraphic[] list;

        if (this != null)
        {
            list = this.GetComponentsInChildren<GTinkerGraphic>();

            foreach (GTinkerGraphic tinkerGraphic in list)
            {
                tinkerGraphic.MyOnMouseUp();
            }
        }
	}

}