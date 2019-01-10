using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class HighlightRotateSequence : TweenActorPerformance{
    Vector3 endScale;
    Vector3 endRotation;
    Vector3 startScale;
    Vector3 startRotation;
    public HighlightRotateSequence Init (Vector3 i_rotation, Vector3 i_scale, float i_duration = 1, float i_speed = default(float), TweenCallback i_callback = default(TweenCallback))
    {
        HighlightRotateSequence instance = this;
        endScale = i_scale;
        endRotation = i_rotation;
        return instance;
    }

    public override bool Perform(GameObject i_rcActor)
    {
        if((i_rcActor != null) && CanPerform(i_rcActor))
        {
            startScale = i_rcActor.transform.localScale;
            startRotation = i_rcActor.transform.rotation.eulerAngles;
            //TweenSystem.CreateSequence();
            return true;
        }
        return false;
    }

    public override void UnPerform(GameObject i_rcActor)
    {
        Cancel(i_rcActor);
        i_rcActor.transform.localScale = startScale;
        TweenSystem.Rotate(i_rcActor, startRotation, 0);
    }
}
