using CuriousReader.BookBuilder;
namespace CuriousReader.Performance
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using DG.Tweening;

    public class HighlightParams : TweenActorParams
    {
        [ExposeField]
        public float ScaleMultiplier = 1.5f;
    }

    /// <summary>
    /// A performance to make an actor highlight itself on the page
    /// </summary>
    public class HighlightActorPerformance : TweenActorPerformance
    {
        public float ScaleMultiplier;


        /// <summary>
        /// Initialize the performance with the specified parameters
        /// </summary>
        /// <returns>The initialized performance.</returns>
        /// <param name="i_scaleMultiplier">I scale multiplier.</param>
        /// <param name="i_duration">I duration.</param>
        /// <param name="i_speed">OPTIONAL: speed (overrides duration).</param>
        /// <param name="i_AllowInterrupt">If set to <c>true</c> allows this performance to interrupt other performances.</param>
        /// <param name="i_callback">Method to call on completion.</param>
        public HighlightActorPerformance Init(float i_scaleMultiplier = 1.5f, float i_duration = 1f, float i_speed = default(float), bool i_AllowInterrupt = true, TweenCallback i_callback = default(TweenCallback), List<GameObject> i_rcInvokers = null)
        {
            base.Init(Vector3.zero, Vector3.zero, i_duration, i_speed, i_AllowInterrupt, i_callback, i_rcInvokers: i_rcInvokers);
            ScaleMultiplier = i_scaleMultiplier;
            return this;
        }

        /// <summary>
        /// Initialize the performances using a HiglightParams object
        /// </summary>
        /// <returns>the initialized performance</returns>
        /// <param name="i_rcParams">the parameters to initialize the performance with</param>
        public HighlightActorPerformance Init (HighlightParams i_rcParams)
        {
            if(i_rcParams != null)
            {
                return Init( i_rcParams.ScaleMultiplier, i_rcParams.duration, i_rcParams.speed, i_rcParams.AllowInterrupt, i_rcParams.OnComplete, i_rcInvokers: i_rcParams.InvokerList);
            }
            Debug.LogWarningFormat("Performance of Type {0} received Null param object,using default Values", this.GetType());
            return Init();
        }

        /// <summary>
        /// Sets the scale multiplier.
        /// </summary>
        /// <param name="i_scaleMultiplier">scale multiplier.</param>
        public void SetScaleMultiplier(float i_scaleMultiplier = 1.5f)
        {
            ScaleMultiplier = i_scaleMultiplier;
        }

        /// <summary>
        /// Perform this performance on i_rcActor
        /// </summary>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise</returns>
        /// <param name="i_rcActor">I rc actor.</param>
        /// <param name="i_rcInvoker">I rc invoker.</param>
        public override bool Perform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            StartValues = i_rcActor.transform.localScale;
            if (i_rcActor != null)
            {
                TweenSystem.Highlight(i_rcActor, ScaleMultiplier, duration, speed, OnComplete);
                Performing = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Cancel the instance of this performance on i_rcActor.
        /// </summary>
        /// <param name="i_rcActor">the performing actor.</param>
        public override void Cancel(GameObject i_rcActor)
        {
            base.Cancel(i_rcActor);
        }

        /// <summary>
        /// Rewind and reset this performance on <paramref name="i_rcActor"/>
        /// </summary>
        /// <param name="i_rcActor">the performing actor.</param>
        public override void UnPerform(GameObject i_rcActor)
        {
            Cancel(i_rcActor);
            i_rcActor.transform.localScale = StartValues;
        }
    }
}

