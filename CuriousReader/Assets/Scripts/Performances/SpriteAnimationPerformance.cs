
using CuriousReader.BookBuilder;

namespace CuriousReader.Performance
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Elendow.SpritedowAnimator;
    public class SpriteAnimationParams : PerformanceParams
    {
        [ExposeField][CustomField(CustomFieldType = typeof(SpriteAnimation))]
        public string AnimationName;
    }

    /// <summary>
    /// A performance for animating an Actor's SpriteRenderer
    /// </summary>
    public class SpriteAnimationPerformance : Performance
    {
        public string AnimationName;

        /// <summary>
        /// checks <paramref name="i_rcActor"/> to see if <paramref name="i_rcActor"/> can prompt this performance.
        /// </summary>
        /// <returns><c>true</c>, if we canperform, <c>false</c> otherwise.</returns>
        /// <param name="i_rcActor">the performing Actor.</param>
        /// <param name="i_rcInvoker">the Invoking Actor.</param>
        public override bool CanPerform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            if (InvokerList != null && InvokerList.Count > 0)
            {
                if (!InvokerList.Contains(i_rcInvoker))
                {
                    return false;
                }
            }

            SpriteAnimator rcAnimator = i_rcActor.GetComponent<SpriteAnimator>();

            if (rcAnimator != null)
            {
                if (!rcAnimator.IsPlaying)
                {
                    return true;
                }
                else
                {
                    if ( AllowInterrupt )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Perform the specified performance on <paramref name="i_rcActor"/>
        /// </summary>
        /// <returns><c>true</c> if performance was successful, <c>false</c> otherwise.</returns>
        /// <param name="i_rcActor">the performing Actor.</param>
        /// <param name="i_rcInvoker">the invoking Actor.</param>
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
                    Performing = true;
                    return true;
                }
                Debug.LogError("SpriteAnimator is null!");
            }
            Debug.LogError("Actor and/or Anim is null");
            return false;
        }

        /// <summary>
        /// Cancel the performance on <paramref name="i_rcActor"/>.
        /// </summary>
        /// <param name="i_rcActor">the performing Actor.</param>
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

        /// <summary>
        /// Rewind and reset this performance on <paramref name="i_rcActor"/>
        /// </summary>
        /// <param name="i_rcActor">I rc actor.</param>
        public override void UnPerform(GameObject i_rcActor)
        {
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
            base.UnPerform(i_rcActor);
        }

    }
}