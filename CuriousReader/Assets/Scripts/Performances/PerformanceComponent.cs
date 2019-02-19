using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


namespace CuriousReader.Performance
{
    public enum PromptType { Click, PairedClick, OnPageLoad, Collision, AutoPlay };
    public delegate bool Callback(GameObject i_rcActor);

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
                                Callback OnComplete = (i_rcActor) => rcPerformance.Perform(i_rcActor);
                                success = CancelAndUnperformAll(OnComplete);
//                                Debug.LogFormat("{0} can perform Performance of type {1} invoked by {2}", this.gameObject.name, rcPerformance.GetType(), i_rcInvokingActor.name);
                                // Need to add a callback for completion here.  
                                //success = rcPerformance.Perform(this.gameObject);
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

        public T GetPerformance <T>() where T: Performance
        {
            foreach (KeyValuePair<PromptType, List<Performance>> rcPair in Performances)
            {
                if (rcPair.Value != null)
                {
                    foreach (Performance rcPerformance in rcPair.Value)
                    {
                        if(rcPerformance.GetType() == typeof(T))
                        {
                            return rcPerformance as T;
                        }
                    }
                }
            }
            return null;
        }

        public T[] GetPerformancesOfType <T>() where T: Performance
        {
            List<T> rcPerformances = new List<T>();
            foreach (KeyValuePair<PromptType, List<Performance>> rcPair in Performances)
            {
                if(rcPair.Value != null)
                {
                    foreach(Performance rcPerformance in rcPair.Value)
                    {
                        if (rcPerformance.GetType() == typeof(T))
                            rcPerformances.Add(rcPerformance as T);
                    }
                }
            }
            return rcPerformances.ToArray();
        }

        public bool HasPerformanceOfType <T>() where T: Performance
        {
            T test = GetPerformance<T>();
            if (test != null)
            {
                return true;
            }
            return false;
        }

        public bool CancelAll(Callback OnComplete = default(Callback))
        {
            if (Performances == null)
            {
                return false;
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
            if (OnComplete != null)
            {
                return OnComplete(this.gameObject);
            }
            return true;
        }

        public bool CancelAndUnperformAll(Callback OnComplete = default(Callback))
        {
            if(Performances != null)
            {
                foreach(KeyValuePair<PromptType, List<Performance>> rcPair in Performances)
                {
                    if(rcPair.Value != null)
                    {
                        foreach(Performance p in rcPair.Value)
                        {
                            if (p.IsPerforming() && (!p.GetType().Equals(typeof(SpriteAnimationPerformance))))
                            {
                                p.Cancel(this.gameObject);
                                p.UnPerform(this.gameObject);
                            }
                        }
                    }
                }
            }
            if(OnComplete != null)
            {
                return OnComplete(this.gameObject);
            }
            return true;
        }

        public bool CancelAndRemoveAll(Callback OnComplete = default(Callback))
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
            if (OnComplete != null)
            {
                return OnComplete(this.gameObject);
            }
            return true;
        }

        public bool CancelByPromptType(PromptType i_ePromptType, Callback OnComplete = default(Callback))
        {
            if (Performances.ContainsKey(i_ePromptType))
            {
                List<Performance> pList = Performances[i_ePromptType];
                foreach (Performance p in pList)
                {
                    p.Cancel(this.gameObject);

                }
            }
            if (OnComplete != null)
            {
                return OnComplete(this.gameObject);
            }
            return true;
        }

        public bool CancelAndRemoveByType(PromptType i_ePromptType, Callback OnComplete = default(Callback))
        {
            if (!Performances.ContainsKey(i_ePromptType))
            {
                return false;
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
            if (OnComplete != null)
            {
                return OnComplete(this.gameObject);
            }
            return true;
        }
    }
}