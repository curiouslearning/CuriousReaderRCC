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
        public RotateActorPerformance Init(int i_degrees, float i_duration = 1f, float i_speed = default(float), TweenCallback i_callback = default(TweenCallback), bool i_yoyo = false)
        {
            RotateActorPerformance instance = base.Init(Vector3.zero, i_duration, i_speed, i_callback, i_yoyo) as RotateActorPerformance;
            return instance;
        }

        public override bool Perform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            if (i_rcActor != null)
            {
                StartValues = i_rcActor.transform.rotation.eulerAngles;
                TweenSystem.Rotate(i_rcActor, EndValues, RotateMode.LocalAxisAdd, duration, speed, OnComplete, YoYo);
                return true;
            }
            return false;
        }


        public override void UnPerform(GameObject i_rcActor)
        {
            Cancel(i_rcActor);
            TweenSystem.Rotate(i_rcActor, StartValues, i_duration: 0f);
        }
    }
}