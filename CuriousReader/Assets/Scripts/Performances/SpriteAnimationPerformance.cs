
using CuriousReader.BookBuilder;

namespace CuriousReader.Performance
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Elendow.SpritedowAnimator;
    public class SpriteAnimationParams : PerformanceParams
    {
        [ExposeField]
        public string AnimationName;
    }
    public class SpriteAnimationPerformance : Performance
    {
        public string AnimationName;

        public override bool CanPerform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            if (!base.CanPerform(i_rcActor, i_rcInvoker))
            {
                return false;
            }
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

        public override bool Perform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            if ((i_rcActor != null) && !string.IsNullOrEmpty(AnimationName))
            {
                SpriteAnimator rcAnimator = i_rcActor.GetComponent<SpriteAnimator>();

                if (rcAnimator != null)
                {
                    rcAnimator.enabled = true;
                    rcAnimator.SetActiveRenderer(true);
                    rcAnimator.Play(AnimationName, true);
                    return true;
                }
                Debug.LogError("SpriteAnimator is null!");
            }
            Debug.LogError("Actor and/or Anim is null");
            return false;
        }

        public override void Cancel(GameObject i_rcActor)
        {
            if (i_rcActor != null)
            {
                SpriteAnimator rcAnimator = i_rcActor.GetComponent<SpriteAnimator>();

                if (rcAnimator != null)
                {
                    if (rcAnimator.IsPlaying)
                    {
                        rcAnimator.Stop();
                    }
                }
            }
        }

        public override void UnPerform(GameObject i_rcActor)
        {
            base.UnPerform(i_rcActor);
            SpriteAnimator rcAnim = i_rcActor.GetComponent<SpriteAnimator>();
            if (rcAnim != null)
            {
                if (!rcAnim.IsPlaying)
                {
                    rcAnim.Play();
                }
                rcAnim.Restart();
                rcAnim.Stop();
            }
        }

    }
}