using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// GTinkerTriggerBehavior -- This is virtual class that will get acted upon when an object gets
/// triggered.  We will want to allow for a delegate or callback when the trigger behavior is complete
/// if we ever want to make a sequence of events.
/// </summary>
public class GTinkerTriggerBehavior
{
    public virtual void Trigger()
    {
        
    }
}
