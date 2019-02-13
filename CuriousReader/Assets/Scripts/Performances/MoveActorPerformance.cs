namespace CuriousReader.Performance
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using DG.Tweening;
    using CuriousReader.BookBuilder;

    public class MoveParams : TweenActorParams
    {
    }

    /// <summary>
    /// A performance for modifying an actor's position on the page
    /// </summary>
    public class MoveActorPerformance : TweenActorPerformance
    {

        public  MoveActorPerformance Init(Vector3 i_endPos, float i_duration = 1f, float i_speed = default(float), bool i_AllowInterrupt = true, TweenCallback i_callback = default(TweenCallback), bool i_yoyo = false)
        {
            base.Init(i_endPos, i_duration, i_speed, i_AllowInterrupt, i_callback, i_yoyo);
            return this;
        }

        public MoveActorPerformance (MoveParams i_rcParams)
        {
            if(i_rcParams != null)
            {
                Init(i_rcParams.EndValues, i_rcParams.duration, i_rcParams.speed, i_rcParams.AllowInterrupt, i_rcParams.OnComplete, i_rcParams.YoYo);
            }
        }

        public override bool Perform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            if (i_rcActor != null)
            {
                StartValues = i_rcActor.transform.position;
                TweenSystem.Move(i_rcActor, EndValues, duration, speed, OnComplete, YoYo);
                return true;
            }
            return false;
        }

        public override void UnPerform(GameObject i_rcActor)
        {
            Cancel(i_rcActor);
            i_rcActor.transform.position = StartValues;
        }

    }
}