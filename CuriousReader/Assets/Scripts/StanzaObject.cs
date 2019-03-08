using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Elendow.SpritedowAnimator;
using CuriousReader.Performance;
 
public class StanzaObject : MonoBehaviour
{
	[HideInInspector]
	public TextClass stanzaValue;
	public List<GTinkerText> tinkerTexts;
	public GStanzaManager stanzaManager;

	private GTinkerText mouseDownTinkerText;
	private GTinkerText mouseCurrentlyDownTinkerText;

    public float width;

    public void AutoPlay ()
    {
        foreach ( GTinkerText rcWord in tinkerTexts)
        {
            rcWord.Invoke(rcWord.AutoPlay, rcWord.startTime);
        }
    }

    public void CancelAutoPlay ()
    {
        foreach (GTinkerText rcWord in tinkerTexts)
        {
            rcWord.CancelInvoke(rcWord.AutoPlay);
            PerformanceSystem.CancelAndRemoveByType(rcWord.gameObject, PromptType.AutoPlay);
            rcWord.StopSound();
        }
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

	public void ResetInputStates(GGameManager.MouseEvents mouseEvent)
	{   
		ResetMouseDownStates();
		ResetMouseCurrentlyDownStates();
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
}
