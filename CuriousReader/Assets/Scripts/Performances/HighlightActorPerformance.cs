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

        public HighlightActorPerformance Init(float i_scaleMultiplier = 1.5f, float i_duration = 1f, float i_speed = default(float), bool i_AllowInterrupt = true, TweenCallback i_callback = default(TweenCallback))
        {
            HighlightActorPerformance instance = Init(Vector3.zero, i_duration, i_speed, i_callback) as HighlightActorPerformance;
            ScaleMultiplier = i_scaleMultiplier;
            return this;
        }

        public HighlightActorPerformance Init (HighlightParams i_rcParams)
        {
            if(i_rcParams != null)
            {
                return Init(i_rcParams.ScaleMultiplier, i_rcParams.duration, i_rcParams.speed, i_rcParams.OnComplete);
            }
            Debug.LogWarningFormat("Performance of Type {0} received Null param object,using default Values", this.GetType());
            return Init();
        }

        public void SetScaleMultiplier(float i_scaleMultiplier = 1.5f)
        {
            ScaleMultiplier = i_scaleMultiplier;
        }
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

        public override void Cancel(GameObject i_rcActor)
        {
            if (i_rcActor != null)
            {
                int restart = i_rcActor.transform.DORestart();
                DOTween.Kill(i_rcActor);
            }
        }

        public override void UnPerform(GameObject i_rcActor)
        {
            Cancel(i_rcActor);
            i_rcActor.transform.localScale = StartValues;
        }
    }
}
            base.Init(Vector3.zero, Vector3.zero, i_duration, i_speed, i_AllowInterrupt, i_callback);
                return Init( i_rcParams.ScaleMultiplier, i_rcParams.duration, i_rcParams.speed, i_rcParams.AllowInterrupt, i_rcParams.OnComplete);
        public override void Cancel(GameObject i_rcActor)
        {
            base.Cancel(i_rcActor);
        }
