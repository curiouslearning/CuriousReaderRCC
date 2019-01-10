using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// A performance for modifying an actor's position on the page
/// </summary>
public class MoveActorPerformance : TweenActorPerformance{


    public override TweenActorPerformance Init(Vector3 i_endPos, float i_duration = 1f, float i_speed = default(float), TweenCallback i_callback = default(TweenCallback)) 
    {
        MoveActorPerformance instance = base.Init(i_endPos, duration, speed, i_callback) as MoveActorPerformance;
        return instance;

    }

    public override bool Perform (GameObject i_rcActor)
    {
        if(CanPerform(i_rcActor))
        {
            StartValues = i_rcActor.transform.position;
            TweenSystem.Move(i_rcActor, EndValues, duration, speed, OnComplete);
            return true;
        }
        return false;
    }

    public override void UnPerform(GameObject i_rcActor)
    {
        Cancel(i_rcActor);
        i_rcActor.transform.position = StartValues;
    }

}
