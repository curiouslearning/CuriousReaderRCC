using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public static class AnimateSystem { 
    /// <summary>
    /// this function bring the zoomed out text animation to its original state
    /// </summary>
    public static void ClipResume(GameObject i_rcActor)
    {
        Animator wordanimator = i_rcActor.GetComponent<Animator>();
        if (wordanimator != null)
        {
            wordanimator.Play("textzoomin");
            wordanimator.ResetTrigger("tapme");
        }
    }
    /// <summary>
    /// this function sets the tap me trigger so that the text can go from normal to zoomed out state 
    /// </summary>
    public static void ClipPlay(GameObject i_rcActor, string i_trigger = "tapme", float i_playTime = 0.21f)
    {
        if (!ValidateArgs(i_rcActor))
        {
            return;
        }

        Animator wordanimator = i_rcActor.GetComponent<Animator>();
        AudioSource source = i_rcActor.GetComponent<AudioSource>();
        if ((wordanimator != null) && (source != null))
        {
            //this.GetComponent<RectTransform>().pivot = ;
            wordanimator.speed = 1 / (i_playTime);

            //source.Play();
            wordanimator.SetTrigger(i_trigger);
        }
    }

    /// <summary>
    /// Validates the arguments.
    /// </summary>
    /// <returns><c>true</c>, if no arguments are null, <c>false</c> otherwise.</returns>
    /// <param name="i_params">the arguments to validate.</param>
    static bool ValidateArgs (params object[] i_params)
    {
        foreach(object param in i_params)
        {
            if(param == null)
            {
                Debug.LogWarning("One or more required arguments in this method is null. Please provide a value.");
                return false;
            }
        }
        return true;
    }

}
