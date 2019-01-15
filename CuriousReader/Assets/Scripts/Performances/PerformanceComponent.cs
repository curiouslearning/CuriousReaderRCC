using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum PromptType { Click, PairedClick, OnPageLoad, Collision };  

public class PerformanceComponent : MonoBehaviour
{
    Dictionary<PromptType, List<Performance>> Performances = new Dictionary<PromptType,List<Performance>>();

    public void AddPerformance(Performance i_rcPerformance, PromptType i_ePromptType, GameObject i_rcInvoker = null)
    {
        if ( i_rcPerformance != null )
        {
            if(i_rcInvoker != null)
            {
                i_rcPerformance.AddInvoker(i_rcInvoker);
            }
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
                        bool success  = rcPerformance.Perform(this.gameObject, i_rcInvokingActor);
                        if (success)
                        {
                            Debug.Log(this.gameObject.name + " can perform " + rcPerformance.name + ".");
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
