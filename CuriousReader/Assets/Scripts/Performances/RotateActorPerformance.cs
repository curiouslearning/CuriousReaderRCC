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
        public RotateActorPerformance Init(Vector3 i_vStartValues, Vector3 i_vEndValues, float i_duration = 1f, float i_speed = default(float), bool i_AllowInterrupt = true, TweenCallback i_callback = default(TweenCallback), bool i_yoyo = false)
        {
            base.Init(i_vStartValues, i_vEndValues, i_duration, i_speed, i_AllowInterrupt, i_callback, i_yoyo);
            return this;
        }

        public RotateActorPerformance Init (RotateParams i_rcParams)
        {
            if(i_rcParams != null)
            {
                return Init(i_rcParams.StartValues, i_rcParams.EndValues, i_rcParams.duration, i_rcParams.speed, i_rcParams.AllowInterrupt, i_rcParams.OnComplete, i_rcParams.YoYo);
            }
            return Init(Vector3.zero, Vector3.zero);
        }

        public override bool Perform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            if (i_rcActor != null)
            {
                i_rcActor.transform.rotation = Quaternion.Euler(StartValues);
                TweenSystem.Rotate(i_rcActor, EndValues, RotateMode.LocalAxisAdd, duration, speed, OnComplete, YoYo);
                Performing = true;
                return true;
            }
            return false;
        }

        public override void UnPerform(GameObject i_rcActor)
        {
            Cancel(i_rcActor);
            i_rcActor.transform.rotation = Quaternion.Euler(StartValues);
            // TweenSystem.Rotate(i_rcActor, StartValues, RotateMode.Fast, 0f);
        }
    }
}