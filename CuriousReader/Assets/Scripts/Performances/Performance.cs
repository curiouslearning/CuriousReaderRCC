using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Performance - Command pattern "Command" object that performs the "work"
/// </summary>
public abstract class Performance : ScriptableObject
{
    protected List<GameObject> InvokerList = new List<GameObject>();

    public virtual void AddInvoker(GameObject i_rcInvoker)
    {
        if((InvokerList != null) && !InvokerList.Contains(i_rcInvoker))
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
