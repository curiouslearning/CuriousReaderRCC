namespace CuriousReader.Performance
{
    using UnityEngine;
    using System.Collections;
    using UnityEngine.UI;
    using DG.Tweening;

    public class ScaleParams : TweenActorParams
    {
    }
    /// <summary>
    /// A performance for modifying the size of an actor
    /// </summary>
    public class ScaleActorPerformance : TweenActorPerformance
    {
        /// <summary>
        /// Initialize the performance with the given values
        /// </summary>
        /// <returns>The initialized performance.</returns>
        /// <param name="i_vStartScale">start scale.</param>
        /// <param name="i_vEndScale">end scale.</param>
        /// <param name="i_duration">duration of the performance.</param>
        /// <param name="i_speed">OPTIONAL: speed of the performance (overrides rotation).</param>
        /// <param name="i_AllowInterrupt">If set to <c>true</c> allow this performance interrupt to interrupt other performances.</param>
        /// <param name="i_callback">I callback.</param>
        /// <param name="i_yoyo">If set to <c>true</c> interpolate between <paramref name="i_vStartScale"/> and <paramref name="i_vEndScale"/> one full cycle (forwards and backwards).</param>
        public override TweenActorPerformance Init(Vector3 i_vStartScale, Vector3 i_vEndScale, float i_duration = 1f, float i_speed = default(float), bool i_AllowInterrupt = true, TweenCallback i_callback = default(TweenCallback), bool i_yoyo = false)
        {
            base.Init(i_vStartScale, i_vEndScale, duration, speed, i_AllowInterrupt, i_callback, i_yoyo);
            return this;
        }

        /// <summary>
        /// Initialize the performances using a PerformanceParams object
        /// </summary>
        /// <returns>The initialized performance</returns>
        /// <param name="i_rcParams">the Params class.</param>
        public ScaleActorPerformance Init (ScaleParams i_rcParams)
        {
            if (i_rcParams != null)
            {
                Init(i_rcParams.StartValues, i_rcParams.EndValues, i_rcParams.duration, i_rcParams.speed, i_rcParams.AllowInterrupt, i_rcParams.OnComplete, i_rcParams.YoYo);
                return this;
            }
            return Init(Vector3.zero, Vector3.zero) as ScaleActorPerformance;
        }

        /// <summary>
        /// Perform this performance on <paramref name="i_rcActor"/>
        /// </summary>
        /// <returns><c>true</c> if performance was successful, <c>false</c>otherwise.</returns>
        /// <param name="i_rcActor">the Performing Actor.</param>
        /// <param name="i_rcInvoker">the Invoking Actor.</param>
        public override bool Perform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            if (i_rcActor != null)
            {
                i_rcActor.transform.localScale = StartValues;
                TweenSystem.Scale(i_rcActor, EndValues, duration, speed, OnComplete);
                Performing = true;
                return true;
            }
            return false;
        }


        /// <summary>
        /// Rewind and reset this performance on <paramref name="i_rcActor"/>
        /// </summary>
        /// <param name="i_rcActor">I rc actor.</param>
        public override void UnPerform(GameObject i_rcActor)
        {
            Cancel(i_rcActor);
            i_rcActor.transform.localScale = StartValues;
        }
    }
}