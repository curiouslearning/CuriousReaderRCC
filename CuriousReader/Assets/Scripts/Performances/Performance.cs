using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Performance - Command pattern "Command" object that performs the "work"
/// </summary>
public abstract class Performance:ScriptableObject
{ 

    /// <summary>
    /// CanPerform -- Can this performance currently be performed by this actor?  Is the performance in progress?
    /// Is the performance a one shot?  Is there a cooldown on the performance?
    /// </summary>
    /// <param name="i_rcActor"></param>
    /// <returns></returns>
    public virtual bool CanPerform(GameObject i_rcActor)
    {
        Debug.Log("Performance.CanPerform");
        return true;
    }

    /// <summary>
    /// Perform -- The Execute of the "Command".  Here is where all the work is handled for the 
    /// performance.  
    /// </summary>
    /// <param name="i_rcActor"></param>
    /// <returns></returns>
    public virtual bool Perform(GameObject i_rcActor)
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
