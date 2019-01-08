using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum PromptType { Click, PairedClick, OnPageLoad, Collision };  

public class PerformanceComponent : MonoBehaviour
{
    Dictionary<PromptType, List<Performance>> Performances = new Dictionary<PromptType,List<Performance>>();

    public void AddPerformance(Performance i_rcPerformance, PromptType i_ePromptType)
    {
        if ( i_rcPerformance != null )
        {
            if ( !Performances.ContainsKey(i_ePromptType))
            {
                Performances[i_ePromptType] = new List<Performance>();
            }

            Performances[i_ePromptType].Add(i_rcPerformance);
        }
    }

    public void Prompt( GameObject i_rcInvokingActor, PromptType i_ePromptType)
    {
        if ( Performances != null )
        {
            foreach (KeyValuePair<PromptType, List<Performance>> rcPair in Performances)
            {
                if (rcPair.Key.Equals(i_ePromptType))
                {
                    foreach (Performance rcPerformance in rcPair.Value)
                    {
                        if (rcPerformance.CanPerform(this.gameObject))
                        {
                            Debug.Log(this.gameObject.name + " can perform " + rcPerformance.name + ".");
                            rcPerformance.Perform(this.gameObject);
                            // Need to add a callback for completion here.  
                        }
                        else
                        {
                            Debug.Log(this.gameObject.name + " can NOT perform " + rcPerformance.name + ".");
                        }
                    }
                }
            }
        }
    }
}
