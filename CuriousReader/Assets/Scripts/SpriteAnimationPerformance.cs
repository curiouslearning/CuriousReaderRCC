﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elendow.SpritedowAnimator;

public class SpriteAnimationPerformance : Performance
{
    public string AnimationName;

    public override bool CanPerform(GameObject i_rcActor)
    {
        SpriteAnimator rcAnimator = i_rcActor.GetComponent<SpriteAnimator>();

        if (rcAnimator != null)
        {
            if (!rcAnimator.IsPlaying)
            {
                return true;
            }
        }

        return false;
    }

    public override bool Perform(GameObject i_rcActor)
    {
        if ( (i_rcActor != null) && !string.IsNullOrEmpty(AnimationName))
        {
            SpriteAnimator rcAnimator = i_rcActor.GetComponent<SpriteAnimator>();

            if (rcAnimator != null)
            {
                rcAnimator.enabled = true;
                rcAnimator.SetActiveRenderer(true);
                rcAnimator.Play(AnimationName,true);
                return true;
            }
        }

        return false;
    }

    public override void Cancel(GameObject i_rcActor)
    {
        if ( i_rcActor != null )
        {
            SpriteAnimator rcAnimator = i_rcActor.GetComponent<SpriteAnimator>();

            if (rcAnimator != null)
            {
                if (rcAnimator.IsPlaying)
                {
                    rcAnimator.Restart();
                    rcAnimator.Stop();
                }
            }
        }
    }

}
