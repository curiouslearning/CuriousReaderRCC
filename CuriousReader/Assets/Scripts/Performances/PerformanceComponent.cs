using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


namespace CuriousReader.Performance
{
    public enum PromptType { Click, PairedClick, OnPageLoad, Collision, AutoPlay };

    public class PerformanceComponent : MonoBehaviour
    {
        Dictionary<PromptType, List<Performance>> Performances = new Dictionary<PromptType, List<Performance>>();

        public void AddPerformance(Performance i_rcPerformance, PromptType i_ePromptType, GameObject i_rcInvoker = null)
        {
            if (i_rcPerformance != null)
            {
                if (i_rcInvoker != null)
                {
                    i_rcPerformance.AddInvoker(i_rcInvoker);
                }
                if (!Performances.ContainsKey(i_ePromptType))
                {
                    Performances[i_ePromptType] = new List<Performance>();
                }
                if (!Performances[i_ePromptType].Contains(i_rcPerformance))
                {
                    Performances[i_ePromptType].Add(i_rcPerformance);
                }
            }
        }

        public void Prompt(GameObject i_rcInvokingActor, PromptType i_ePromptType)
        {
            if (Performances != null)
            {
                foreach (KeyValuePair<PromptType, List<Performance>> rcPair in Performances)
                {
                    if (rcPair.Key.Equals(i_ePromptType))
                    {
                        foreach (Performance rcPerformance in rcPair.Value)
                        {
                            bool success = false;
                            if (rcPerformance.CanPerform(this.gameObject, i_rcInvokingActor))
                            {
//                                Debug.LogFormat("{0} can perform Performance of type {1} invoked by {2}", this.gameObject.name, rcPerformance.GetType(), i_rcInvokingActor.name);
                                // Need to add a callback for completion here.  
                                success = rcPerformance.Perform(this.gameObject);
                                if (!success)
                                {
                                    Debug.LogWarningFormat("({0}) failed to perform Performance of type ({1})!", this.gameObject.name, rcPerformance.GetType());
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CancelAll()
        {
            if (Performances == null)
            {
                return;
            }
            foreach (KeyValuePair<PromptType, List<Performance>> rcPair in Performances)
            {
                if (rcPair.Value != null)
                {
                    foreach (Performance p in rcPair.Value)
                    {
                        p.Cancel(this.gameObject);
                    }
                }
            }

        }

        public void CancelAndRemoveAll()
        {
            if (Performances != null)
            {
                foreach (KeyValuePair<PromptType, List<Performance>> rcPair in Performances)
                {
                    if (rcPair.Value != null)
                        foreach (Performance p in rcPair.Value)
                        {
                            p.Cancel(this.gameObject);
                        }
                    rcPair.Value.Clear();
                }
            }
        }

        public void CancelByPromptType(PromptType i_ePromptType)
        {
            if (Performances.ContainsKey(i_ePromptType))
            {
                List<Performance> pList = Performances[i_ePromptType];
                foreach (Performance p in pList)
                {
                    p.Cancel(this.gameObject);

                }
            }
        }

        public void CancelAndRemoveByType(PromptType i_ePromptType)
        {
            if (!Performances.ContainsKey(i_ePromptType))
            {
                return;
            }
            List<Performance> pList = Performances[i_ePromptType];
            for (int i = 0; i < pList.Count; i++)
            {
                if (pList[i] != null)
                {
                    pList[i].Cancel(this.gameObject);
                }
            }
            pList.Clear();
        }
    }
}