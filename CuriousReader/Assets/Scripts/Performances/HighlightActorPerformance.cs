using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// A performance to make an actor highlight itself on the page
/// </summary>
public class HighlightActorPerformance : TweenActorPerformance {
    public float ScaleMultiplier;

    public ScaleActorPerformance Init (Vector3 i_values, float i_scaleMultiplier = 1.5f, float i_duration = 1f, float i_speed = default(float), TweenCallback i_callback = default(TweenCallback))
    {
        ScaleActorPerformance instance = Init (i_values, i_duration, i_speed, i_callback) as ScaleActorPerformance;
        ScaleMultiplier = i_scaleMultiplier;
        return instance;
    }


    public override bool Perform(GameObject i_rcActor)
    {
        StartValues = i_rcActor.transform.localScale;
        if((i_rcActor != null) && CanPerform(i_rcActor))
        {
            TweenSystem.Highlight(i_rcActor, ScaleMultiplier, duration, speed, OnComplete);
            return true;
        }
        return false;
    }

    public override void UnPerform (GameObject i_rcActor)
    {
        Cancel(i_rcActor);
        i_rcActor.transform.localScale = StartValues;
    }
}
