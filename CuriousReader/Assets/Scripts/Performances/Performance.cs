using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PerformanceParams
{
    public static string OnBookGUI<T>(TriggerClass i_rcTrigger) where T : PerformanceParams, new()
    {
        string strParams = "";

        if (i_rcTrigger.EditorFields == null)
        {
            T rcParams = new T();
            if (!string.IsNullOrEmpty(i_rcTrigger.Params))
            {
                rcParams = JsonUtility.FromJson<T>(i_rcTrigger.Params);
            }

            i_rcTrigger.EditorFields = ExposeFields.GetFields(rcParams);
            i_rcTrigger.PerformanceParams = (PerformanceParams)rcParams;
        }

        if (i_rcTrigger.EditorFields != null)
        {
            ExposeFields.Expose(i_rcTrigger.EditorFields);
        }

        if (i_rcTrigger.PerformanceParams != null)
        {
            T rcParams = (T)i_rcTrigger.PerformanceParams;
            strParams = JsonUtility.ToJson(rcParams);
        }

        return strParams;

    }
}

public class HighlightPerformanceParams: PerformanceParams
{
    [ExposeField]
    public float ScaleMultiplier;
}

namespace CuriousReader.Performance
{
    /// <summary>
    /// Performance - Command pattern "Command" object that performs the "work"
    /// </summary>
    public abstract class Performance : ScriptableObject
    {
        [ExposePerformancePropAttribute]
        protected List<GameObject> InvokerList
        {
            get
            {
                return InvokerList;
            }

            set
            {
                InvokerList = value;
            }
        }

        public virtual void AddInvoker(GameObject i_rcInvoker)
        {
            if ((InvokerList != null) && !InvokerList.Contains(i_rcInvoker))
            {
                InvokerList.Add(i_rcInvoker);
            }
        }

        public virtual void RemoveInvoker(GameObject i_rcInvoker)
        {
            if ((i_rcInvoker != null) && InvokerList.Contains(i_rcInvoker))
            {
                InvokerList.Remove(i_rcInvoker);
            }
        }
        /// <summary>
        /// CanPerform -- Can this performance currently be performed by this actor?  Is the performance in progress?
        /// Is the performance a one shot?  Is there a cooldown on the performance?
        /// </summary>
        /// <param name="i_rcActor"></param>
        /// <returns></returns>
        public virtual bool CanPerform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            if (InvokerList != null && InvokerList.Count > 0)
            {
                if (!InvokerList.Contains(i_rcInvoker))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Perform -- The Execute of the "Command".  Here is where all the work is handled for the 
        /// performance.  
        /// </summary>
        /// <param name="i_rcActor"></param>
        /// <returns></returns>
        public virtual bool Perform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            Debug.Log("Performance.Perform entered.");
            return true;
        }

        /// <summary>
        /// In order to stop a performance that is currently being executed we need a method to do so.
        /// </summary>
        /// <param name="i_rcActor"></param>
        public virtual void Cancel(GameObject i_rcActor)
        {
            Debug.Log("Performance was just cancelled.");
        }

    }
}