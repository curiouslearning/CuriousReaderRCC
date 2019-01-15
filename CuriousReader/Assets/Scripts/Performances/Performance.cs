using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Performance - Command pattern "Command" object that performs the "work"
/// </summary>
public abstract class Performance : ScriptableObject
{
    protected GameObject rcInvoker;

    public virtual void AddInvoker(GameObject i_rcInvoker)
    {
        rcInvoker = i_rcInvoker;
    }
    /// <summary>
    /// CanPerform -- Can this performance currently be performed by this actor?  Is the performance in progress?
    /// Is the performance a one shot?  Is there a cooldown on the performance?
    /// </summary>
    /// <param name="i_rcActor"></param>
    /// <returns></returns>
    public virtual bool CanPerform(GameObject i_rcActor, GameObject i_rcInvoker = null)
    {
        Debug.Log("Performance.CanPerform"); 
        if ((rcInvoker != null))
        {
            if (!rcInvoker.Equals(i_rcInvoker))
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
        if(CanPerform(i_rcActor, i_rcInvoker))
        {
            Debug.Log("Performance.Perform entered.");
            return true;
        }
        return false;

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
