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
        public override TweenActorPerformance Init(Vector3 i_endPos, float i_duration = 1f, float i_speed = default(float), TweenCallback i_callback = default(TweenCallback), bool i_yoyo = false)
        {
            ScaleActorPerformance instance = base.Init(i_endPos, duration, speed, i_callback, i_yoyo) as ScaleActorPerformance;
            return instance;
        }

        public ScaleActorPerformance Init (ScaleParams i_rcParams)
        {
            if (i_rcParams != null)
            {
                ScaleActorPerformance instance = Init(i_rcParams.EndValues, i_rcParams.duration, i_rcParams.speed, i_rcParams.OnComplete, i_rcParams.YoYo) as ScaleActorPerformance;
                return instance;
            }
            return Init(Vector3.zero) as ScaleActorPerformance;
        }
        public override bool Perform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            if (i_rcActor != null)
            {
                StartValues = i_rcActor.transform.localScale;
                TweenSystem.Scale(i_rcActor, EndValues, duration, speed, OnComplete);
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