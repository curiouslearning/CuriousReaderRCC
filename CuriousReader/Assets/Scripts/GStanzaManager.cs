using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CuriousReader.Performance;

public class GStanzaManager : MonoBehaviour {

	public GSManager sceneManager;
	public List<StanzaObject> stanzas;

	private bool autoPlaying = false;
	private bool cancelAutoPlay = false;

	void Start(){
	}



	public void LoadStanzaJSON()
	{
		SetupWordTimings(LoadAssetFromJSON.storyBookJson.pages[LoadAssetFromJSON.pageNumber].timestamps);

	}

	// Loads up a custom stanza audio mp3 and xml timing data
	public void LoadStanzaAudioAndXML(string audioFilename, string xmlFilename)
	{
		// Load our audio
		AudioClip stanzaClip = (AudioClip)Resources.Load(audioFilename) as AudioClip;
		GetComponent<AudioSource>().clip = stanzaClip;


		SetupWordTimings(LoadAssetFromJSON.storyBookJson.pages[LoadAssetFromJSON.pageNumber].timestamps);
	}

	// Goes through all stanzas and tinkertexts assigning the word timings
	private void SetupWordTimings(TimeStampClass[] timeStamps)
	{
		int stanzaIndex = 0;
		int wordIndex = 0;
		int relativeWordIndex = 0;

		while (wordIndex < timeStamps.Length)
		{
			stanzas[stanzaIndex].tinkerTexts[relativeWordIndex].SetupWordTiming(timeStamps[wordIndex]);
			wordIndex++;
			relativeWordIndex++;

			// Hit end of stanza yet?
			if (relativeWordIndex > stanzas[stanzaIndex].tinkerTexts.Count - 1)
			{
				relativeWordIndex = 0;
				stanzaIndex++;

				// If we are in a new valid stanza
				if (stanzaIndex < stanzas.Count)
				{
					// Calculate and set our end delay based on when last word in stanza ends and when first word of next stanza begins
					float firstWordStartTime = timeStamps[wordIndex].start / 1000.0f;
					float lastWordEndTime = timeStamps[wordIndex-1].end / 1000.0f;
					stanzas[stanzaIndex - 1].endDelay = firstWordStartTime - lastWordEndTime;
				}
			}
		}
	}



	// Whether we are currently autoplaying stanzas
	public bool IsAutoPlaying()
	{
		return autoPlaying;
	}


	// Method to request an auto play starting w/ a stanza
	public void RequestAutoPlay(StanzaObject startingStanza, GTinkerText startingTinkerText = null)
	{
//        Debug.Log ("request auto play");
		if (!autoPlaying)  // && !sceneManager.disableAutoplay)
		{
//            Debug.Log("not auto playing");
			autoPlaying = true;
			cancelAutoPlay = false; // reset our cancel flag
			StartAutoPlay(startingStanza, startingTinkerText);
		}
	}

	// Method to request cancelling an autoplay
	public void RequestCancelAutoPlay()
	{
		if (autoPlaying)
		{
			cancelAutoPlay = true;
            CancelAutoPlay();
		}
	}

	// Whether there is a cancel request in progress
	public void CancelAutoPlay()
	{
        autoPlaying = false;
        foreach(StanzaObject sObject in stanzas)
        {
            sObject.CancelAutoPlay();
        }
	}


	// Begins an auto play starting w/ a stanza
	private void StartAutoPlay(StanzaObject startingStanza, GTinkerText startingTinkerText)
	{

		// If we aren't starting from the beginning, read the audio progress from the startingTinkerText
		GetComponent<AudioSource>().time = startingTinkerText.GetStartTime();
		// Start playing the full stanza audio
		GetComponent<AudioSource>().Play();
        autoPlaying = true;
		int startingStanzaIndex = stanzas.IndexOf(startingStanza);
		for (int i = startingStanzaIndex; i < stanzas.Count; i++)
		{
			if (i == startingStanzaIndex)
			{
                stanzas[i].AutoPlay();
			}
		}
	}

	public void OnMouseDown(GTinkerText tinkerText, bool suppressAnim = false)
	{   Debug.Log ("mousedown2");
		if (tinkerText.stanza != null && stanzas.Contains(tinkerText.stanza))
		{  
			tinkerText.stanza.OnMouseDown(tinkerText, suppressAnim);
		}

	}

	public void OnPairedMouseDown(GTinkerText tinkerText)
	{
		if (tinkerText.stanza != null && stanzas.Contains(tinkerText.stanza))
		{
			tinkerText.stanza.OnPairedMouseDown(tinkerText);
		}
	}

	public void OnMouseCurrentlyDown(GTinkerText tinkerText)
	{
		if (tinkerText.stanza != null && stanzas.Contains(tinkerText.stanza))
		{
			tinkerText.stanza.OnMouseCurrentlyDown(tinkerText);
		}
	}

	public void OnPairedMouseCurrentlyDown(GTinkerText tinkerText)
	{
		if (tinkerText.stanza != null && stanzas.Contains(tinkerText.stanza))
		{
			tinkerText.stanza.OnPairedMouseCurrentlyDown(tinkerText);
		}
	}

	public void OnMouseUp(GTinkerText tinkerText)
	{
		if (tinkerText.stanza != null && stanzas.Contains(tinkerText.stanza))
		{
			tinkerText.stanza.OnMouseUp(tinkerText);
		}

	}


	public void ResetInputStates(GGameManager.MouseEvents mouseEvent)
	{
		foreach (StanzaObject stanza in stanzas)
		{
			stanza.ResetInputStates(mouseEvent);
		}
	}


	/*public void OnMouseDown(GTinkerGraphic tinkerGraphic)
	{
		if (tinkerGraphic != null)
		{
			tinkerGraphic.MyOnMouseDown();
		}

	}

	public void OnMouseUp(GTinkerGraphic tinkerGraphic)
	{
		tinkerGraphic.MyOnMouseUp();
	}
      */

}
