using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuriousReader.BookBuilder;

namespace CuriousReader.Performance
{
    /// <summary>
    /// A class for serializing performance state
    /// </summary>
    public class PerformanceParams
    {
        [ExposeField]
        public List<GameObject> InvokerList = new List<GameObject>();
        [ExposeField]
        public bool AllowInterrupt = true;
    }


    /// <summary>
    /// Performance: The basic unit of action an object (Actor) can execute (Perform). Movement, resizing, animation, and sound effects are all examples of performances
    /// </summary>
    public abstract class Performance : ScriptableObject
    {
        protected List<GameObject> InvokerList;
        protected bool Performing;
        public bool AllowInterrupt;
        public bool AllowConcurrent = false;

        /// <summary>
        /// Initialize this performance with the desired parameters
        /// </summary>
        /// <returns>The initialized performance.</returns>
        /// <param name="i_InvokerList">the list of actors allowed to invoke this performance.</param>
        /// <param name="i_AllowInterrupt">If set to <c>true</c> allow this performance to interrupt other performances on its actor.</param>
        public virtual Performance Init(List<GameObject> i_InvokerList = null, bool i_AllowInterrupt = true)
        {

            if(i_InvokerList!= null)
            { 
                foreach (GameObject invoker in i_InvokerList)
                {
                    if(invoker != null)
                    {
                        AddInvoker(invoker);
                    }
                }
            }
            AllowInterrupt = i_AllowInterrupt;
            return this;
        }

        /// <summary>
        /// Adds the given actor to this performance's list of allowed invokers.
        /// </summary>
        /// <param name="i_rcInvoker">I rc invoker.</param>
        public virtual void AddInvoker(GameObject i_rcInvoker)
        {
            if (InvokerList != null)
            {
                if (!InvokerList.Contains(i_rcInvoker))
                {
                    InvokerList.Add(i_rcInvoker);
                }
            }
            else
            {
                InvokerList = new List<GameObject>();
                InvokerList.Add(i_rcInvoker);
            }
        }

        /// <summary>
        /// Removes the given actor from this performance's list of allowed invokers.
        /// </summary>
        /// <param name="i_rcInvoker">I rc invoker.</param>
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
                {
                    return false;
                }
            }

            if (PerformanceSystem.IsPerforming(i_rcActor))
            {
                if (AllowInterrupt)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        public virtual bool IsPerforming()
        {
            return Performing;
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
            Performing = true;
            return true;
        }

        /// <summary>
        /// In order to stop a performance that is currently being executed we need a method to do so.
        /// </summary>
        /// <param name="i_rcActor"></param>
        public virtual void Cancel(GameObject i_rcActor)
        {
            Performing = false; 
            Debug.Log("Performance was just cancelled.");
        }

        /// <summary>
        /// Rewind and reset this performance on <paramref name="i_rcActor"/>
        /// </summary>
        /// <param name="i_rcActor">I rc actor.</param>
        public virtual void UnPerform (GameObject i_rcActor)
        {
            Performing = false;
            Debug.Log("undoing performance!");
        }
    }
}