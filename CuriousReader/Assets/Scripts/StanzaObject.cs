using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Elendow.SpritedowAnimator;
using CuriousReader.Performance;

public class StanzaObject : MonoBehaviour {

	[HideInInspector]
	public TextClass stanzaValue;
	public List<GTinkerText> tinkerTexts;// The minimum spacing between words
	public GStanzaManager stanzaManager;
	//public bool stanzaNarrate;

	// Time delay at end of stanza during autoplay
	public float endDelay;
    public bool m_bIsWordPlaying = false;
    private int currentWord = 0;

	private GTinkerText mouseDownTinkerText;
	private GTinkerText mouseCurrentlyDownTinkerText;

	// used as tracking to detect stanza auto play
	private int lastTinkerTextIndex = -9999;

    public void AutoPlay ()
    {
        m_bIsWordPlaying = true;
        AutoPlayInternal();
    }

    private void AutoPlayInternal ()
    {
        if(!m_bIsWordPlaying)
        {
            return;
        }
        if ((currentWord >= 0) && currentWord < tinkerTexts.Count)
        {
            GTinkerText rcWord = tinkerTexts[currentWord];
            if (!TweenSystem.IsTweening(rcWord.gameObject))
            { 
                HighlightTextPerformance autoHighlight = PerformanceSystem.GetTweenPerformance<HighlightTextPerformance>();
                autoHighlight.Init(Color.yellow, i_duration: rcWord.playTime/2, i_callback: new TweenCallback(AutoPlayInternal));
                PerformanceSystem.AddPerformance(rcWord.gameObject, autoHighlight, PromptType.AutoPlay);
                PerformanceSystem.SendPrompt(this.gameObject, rcWord.gameObject, PromptType.AutoPlay);

                if (rcWord.star)
                {
                    if ( rcWord.pairedGraphics != null )
                    {
                        foreach ( GTinkerGraphic rcGraphic in rcWord.pairedGraphics )
                        {
                            rcGraphic.OnPairedMouseDown(rcWord);
                        }
                    }

                    if ( stanzaManager.sceneManager != null )
                    {
                        stanzaManager.sceneManager.OnStarWordActivated(rcWord.gameObject);
                    }
                }

                currentWord++;
            }
        }
    }


    public void CancelAutoPlay ()
    {
        foreach (GTinkerText rcWord in tinkerTexts)
        {
            PerformanceSystem.CancelAndRemoveByType(rcWord.gameObject, PromptType.AutoPlay);
            rcWord.StopSound();
        }
        m_bIsWordPlaying = false;
    }

    /// <summary>
    /// this function handles the mousedown evens on the tinkertexts of the stanza
    /// </summary>
    /// <param name="tinkerText">tinkertext that is pressed</param>
    /// <param name="suppressAnim">bool to check whether animation is to be suppressed</param>
    public void OnMouseDown(GTinkerText tinkerText, bool suppressAnim = false)
	{   
		// if we aren't already mouse down on this text
		if (mouseDownTinkerText !=null && mouseDownTinkerText != tinkerText)
		{ 
			// Then reset the old one
			mouseDownTinkerText.Reset();
		}

		// Assign this new one
		mouseDownTinkerText = tinkerText;

		// And signal the tinkerText 
		tinkerText.MyMouseDown(suppressAnim);
	}   
    /// <summary>
    /// this function handles the on paired mouse events
    /// </summary>
    /// <param name="tinkerText"></param>
	public void OnPairedMouseDown(GTinkerText tinkerText)
	{
		// if we aren't already mouse down on this text
		if (mouseDownTinkerText != null && mouseDownTinkerText != tinkerText)
		{
			// Then reset the old one
			mouseDownTinkerText.Reset();
		}

		// Assign this new one
		mouseDownTinkerText = tinkerText;

		// And trigger the tinkerText only if stanza narrate is false 
		//if(stanzaNarrate==false)
		tinkerText.OnPairedMouseDown();
	}
    /// <summary>
    /// this function handles the currently down on the tinkertext 
    /// </summary>
    /// <param name="tinkerText"></param>
	public void OnMouseCurrentlyDown(GTinkerText tinkerText)
	{
		// If this text is already marked as mouse down, clear that
		if (mouseDownTinkerText != null && mouseDownTinkerText == tinkerText)
		{
			mouseDownTinkerText = null;

			// and reassign it to currently down 
			mouseCurrentlyDownTinkerText = tinkerText;

			//DetectStanzaAutoPlay(tinkerText);
		}
		else if (mouseCurrentlyDownTinkerText != null)
		{
			// If this text isn't already marked as currently down
			if (mouseCurrentlyDownTinkerText != tinkerText)
			{
				// Then reset the old one
				mouseCurrentlyDownTinkerText.Reset();

				// Assign this new one
				mouseCurrentlyDownTinkerText = tinkerText;

				// Signal tinkerText
				tinkerText.OnMouseCurrentlyDown();

			//	DetectStanzaAutoPlay(tinkerText);
			}
		}
		else
		{
			// Assign this new one
			mouseCurrentlyDownTinkerText = tinkerText;

			// Signal tinkerText
			tinkerText.OnMouseCurrentlyDown();

			//DetectStanzaAutoPlay(tinkerText);
		}
	}
    /// <summary>
    /// this function handles the onPairedMouseCurrenltyDown event on tinkertext
    /// </summary>
    /// <param name="tinkerText"></param>
	public void OnPairedMouseCurrentlyDown(GTinkerText tinkerText)
	{
		// If this text is already marked as mouse down, clear that
		if (mouseDownTinkerText != null && mouseDownTinkerText == tinkerText)
		{
			mouseDownTinkerText = null;

			// and reassign it to currently down 
			mouseCurrentlyDownTinkerText = tinkerText;

			//DetectStanzaAutoPlay(tinkerText);
		}
		else if (mouseCurrentlyDownTinkerText != null)
		{
			// If this text isn't already marked as currently down
			if (mouseCurrentlyDownTinkerText != tinkerText)
			{
				// Then reset the old one
				mouseCurrentlyDownTinkerText.Reset();

				// Assign this new one
				mouseCurrentlyDownTinkerText = tinkerText;

				// Signal tinkerText
				tinkerText.OnPairedMouseCurrentlyDown();

				//DetectStanzaAutoPlay(tinkerText);
			}
		}
		else
		{
			// Assign this new one
			mouseCurrentlyDownTinkerText = tinkerText;

			// Signal tinkerText
			tinkerText.OnPairedMouseCurrentlyDown();

			//DetectStanzaAutoPlay(tinkerText);
		}
	}
    /// <summary>
    /// this function stops the individual sounds of all the texts of the stanza
    /// </summary>

	public void StopAllIndividualSounds()
	{
		foreach (GTinkerText tinkerText in tinkerTexts)
		{
			tinkerText.StopSound();
		}
	}
    /// <summary>
    /// this function handles the on mouse up events of text
    /// </summary>
    /// <param name="tinkerText"></param>
	public void OnMouseUp(GTinkerText tinkerText)
	{   
		// Assign this new one
		mouseDownTinkerText = tinkerText;
		// And signal the tinkerText 
		tinkerText.MyOnMouseUp();
	}


	public void ResetInputStates(GGameManager.MouseEvents mouseEvent)
	{   
		ResetMouseDownStates();
		ResetMouseCurrentlyDownStates();

		if (mouseEvent == GGameManager.MouseEvents.MouseUp)
		{
			lastTinkerTextIndex = -9999;
		}
	}

	private void ResetMouseDownStates()
	{
		if (mouseDownTinkerText != null)
		{
			mouseDownTinkerText.Reset();
		}
		mouseDownTinkerText = null;
	}

	private void ResetMouseCurrentlyDownStates()
	{
		if (mouseCurrentlyDownTinkerText != null)
		{
			mouseCurrentlyDownTinkerText.Reset();
		}
		mouseCurrentlyDownTinkerText = null;
	}

	/*

	// Attempts to detect when player has swiped across two words left to right in a stanza to begin autoplay
	private void DetectStanzaAutoPlay(GTinkerText tinkerText)
	{
		int currentTinkerTextIndex = tinkerTexts.IndexOf(tinkerText);

		if (lastTinkerTextIndex == -9999)
		{
			lastTinkerTextIndex = currentTinkerTextIndex;
		}
		else
		{

			if (currentTinkerTextIndex == lastTinkerTextIndex + 1)
			{

				if (currentTinkerTextIndex < tinkerTexts.Count - 1)     //less than last index of this stanza.
				{
					float pauseDelay = tinkerTexts[currentTinkerTextIndex].GetStartTime() - tinkerTexts[lastTinkerTextIndex].GetEndTime();
					StartCoroutine(RequestToPlay(pauseDelay, this, currentTinkerTextIndex + 1));    //start from next TinkerText of this stanza (avoid overlap)

				}
				else
				{
					int nextStanzaIndex = stanzaManager.stanzas.IndexOf(this) + 1;
					//start from next stanza's first TinkerText (avoid overlap)
					StartCoroutine(RequestToPlay(endDelay, stanzaManager.stanzas[nextStanzaIndex], 0));

				}

				//stanzaManager.RequestAutoPlay (this, tinkerTexts [lastTinkerTextIndex]);
				lastTinkerTextIndex = -9999;
			}
			else if (currentTinkerTextIndex < lastTinkerTextIndex)
			{
				lastTinkerTextIndex = -9999;
			}
			else
			{
				lastTinkerTextIndex = currentTinkerTextIndex;
			}
		}

	}

	private IEnumerator RequestToPlay(float delay, StanzaObject stanza, int tinkerTextIndex)
	{
		yield return new WaitForSeconds(delay + .5f);
		stanzaManager.RequestAutoPlay(stanza, stanza.tinkerTexts[tinkerTextIndex]);
	}

	/* Spaces the TinkerTexts in this stanza (called after all texts are setup)
	public void AutoSpace()
	{
		TinkerText anchorText = tinkerTexts[0];
		RectTransform trans = anchorText.GetComponent<RectTransform>();

		for (int i = 1; i < tinkerTexts.Count; i++)
		{
			TinkerText currentText = tinkerTexts[i];
			float newXPos = trans.anchoredPosition.x  + trans.sizeDelta.x + minWordSpace;
			trans.anchoredPosition = new Vector2(newXPos, trans.anchoredPosition.y);
			anchorText = currentText;
			trans = anchorText.GetComponent<RectTransform>();
		}
	}

     //Spaces the TinkerTexts in this stanza (called after all texts are setup)
	public void AutoSpace()
	{
		TinkerText anchorText = tinkerTexts[0];
		Bounds anchorBounds = anchorText.GetComponent<Text>().GetComponent<Renderer>().bounds;

		for (int i = 1; i < tinkerTexts.Count; i++)
		{
			TinkerText currentText = tinkerTexts[i];
			Bounds currentBounds = currentText.GetComponent<Text>().GetComponent<Renderer>().bounds;

			float newXPos = anchorText.gameObject.transform.position.x + minWordSpace + (anchorBounds.size.x / 2.0f) + (currentBounds.size.x / 2.0f);

			Vector3 newPos = new Vector3(newXPos, currentText.gameObject.transform.position.y, currentText.gameObject.transform.position.z);
			currentText.transform.position = newPos;

			anchorText = currentText;
			anchorBounds = anchorText.GetComponent<Text>().GetComponent<Renderer>().bounds;
		}
	}*/
}
