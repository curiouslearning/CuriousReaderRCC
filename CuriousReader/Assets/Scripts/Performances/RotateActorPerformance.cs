namespace CuriousReader.Performance
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using DG.Tweening;

    public class RotateParams : TweenActorParams
    {
    }
    /// <summary>
    /// A performance to rotate or flip an actor
    /// </summary>
    public class RotateActorPerformance : TweenActorPerformance
    {
        /// <summary>
        /// Initialize this performance with the specified parameters
        /// </summary>
        /// <returns>The initialized performance.</returns>
        /// <param name="i_vStartValues">the starting Euler Angles of the Actor.</param>
        /// <param name="i_vEndValues">the magnitude and axes of the desired rotation.</param>
        /// <param name="i_duration">the duration of the performance.</param>
        /// <param name="i_speed">OPTIONAL: the rotation speed (overrides duration).</param>
        /// <param name="i_AllowInterrupt">If set to <c>true</c> allow this performance to interrupt other performances.</param>
        /// <param name="i_callback">The method(s) to call OnComplete.</param>
        /// <param name="i_yoyo">If set to <c>true</c>.interpolate between <paramref name="i_vStartValues"/> and <paramref name="i_vEndValues"/> one full cycle (forwards and backwards)</param>
        public RotateActorPerformance Init(Vector3 i_vStartValues, Vector3 i_vEndValues, float i_duration = 1f, float i_speed = default(float), bool i_AllowInterrupt = true, TweenCallback i_callback = default(TweenCallback), bool i_yoyo = false, List<GameObject> i_rcInvokers = null)
        {
            base.Init(i_vStartValues, i_vEndValues, i_duration, i_speed, i_AllowInterrupt, i_callback, i_yoyo, i_rcInvokers);
            return this;
        }

        /// <summary>
        /// initialize this performance using a params class
        /// </summary>
        /// <returns>The initalized performance.</returns>
        /// <param name="i_rcParams">the class containing the params.</param>
        public RotateActorPerformance Init (RotateParams i_rcParams)
        {
            if(i_rcParams != null)
            {
                return Init(i_rcParams.StartValues, i_rcParams.EndValues, i_rcParams.duration, i_rcParams.speed, i_rcParams.AllowInterrupt, i_rcParams.OnComplete, i_rcParams.YoYo, i_rcParams.InvokerList);
            }
            return Init(Vector3.zero, Vector3.zero);
        }

        /// <summary>
        /// Perform this performance on <paramref name="i_rcActor"/>
        /// </summary>
        /// <returns><c>true</c> if performance was successful, <c>false</c> otherwise.</returns>
        /// <param name="i_rcActor">the performing Actor</param>
        /// <param name="i_rcInvoker">the invoking Actor.</param>
        public override bool Perform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            if (i_rcActor != null)
            {
                i_rcActor.transform.localRotation = Quaternion.Euler(StartValues);
                TweenSystem.Rotate(i_rcActor, EndValues, RotateMode.LocalAxisAdd, duration, speed, OnComplete, YoYo);
                Performing = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Rewind and reset this performance on <paramref name="i_rcActor"/>
        /// </summary>
        /// <param name="i_rcActor">the performing Actor.</param>
        public override void UnPerform(GameObject i_rcActor)
        {
            Cancel(i_rcActor);
            i_rcActor.transform.rotation = Quaternion.Euler(StartValues);
            // TweenSystem.Rotate(i_rcActor, StartValues, RotateMode.Fast, 0f);
        }
    }
}