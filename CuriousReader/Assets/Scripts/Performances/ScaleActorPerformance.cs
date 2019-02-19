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
        public override TweenActorPerformance Init(Vector3 i_vStartScale, Vector3 i_vEndScale, float i_duration = 1f, float i_speed = default(float), bool i_AllowInterrupt = true, TweenCallback i_callback = default(TweenCallback), bool i_yoyo = false)
        {
            base.Init(i_vStartScale, i_vEndScale, duration, speed, i_AllowInterrupt, i_callback, i_yoyo);
            return this;
        }

        public ScaleActorPerformance Init (ScaleParams i_rcParams)
        {
            if (i_rcParams != null)
            {
                Init(i_rcParams.StartValues, i_rcParams.EndValues, i_rcParams.duration, i_rcParams.speed, i_rcParams.AllowInterrupt, i_rcParams.OnComplete, i_rcParams.YoYo);
                return this;
            }
            return Init(Vector3.zero, Vector3.zero) as ScaleActorPerformance;
        }

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

        public override void UnPerform(GameObject i_rcActor)
        {
            Cancel(i_rcActor);
            i_rcActor.transform.localScale = StartValues;
        }
    }
}