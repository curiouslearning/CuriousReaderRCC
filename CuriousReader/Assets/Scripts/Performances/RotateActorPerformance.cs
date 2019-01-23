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

        public override TweenActorPerformance Init(Vector3 i_values, float i_duration = 1f, float i_speed = default(float), TweenCallback i_callback = default(TweenCallback))
        {
            RotateActorPerformance instance = base.Init(i_values, i_duration, i_speed, i_callback) as RotateActorPerformance;
            return instance;
        }

        public override bool Perform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            if (i_rcActor != null)
            {
                StartValues = i_rcActor.transform.rotation.eulerAngles;
                TweenSystem.Rotate(i_rcActor, EndValues, duration, speed, OnComplete);
                return true;
            }
            return false;
        }

        public override void UnPerform(GameObject i_rcActor)
        {
            Cancel(i_rcActor);
            TweenSystem.Rotate(i_rcActor, StartValues, 0f);
        }
    }
}