using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using CuriousReader.BookBuilder;

namespace CuriousReader.Performance
{
    public class TweenActorParams : PerformanceParams
    {
        public Vector3 StartValues;
        [ExposeField]
        public float duration;
        [ExposeField]
        public float speed;
        [ExposeField]
        public TweenCallback OnComplete;
        [ExposeField]
        public bool Reset;
        [ExposeField]
        public bool YoYo;
        [ExposeField]
        public Vector3 EndValues;

    }
    /// <summary>
    /// An abstract class for creating performances that change an actor's sta abstract class TweenActorPerformance : Performance{
    /// </summary>
    public abstract class TweenActorPerformance : Performance
    {
        public Vector3 EndValues;
        protected Vector3 StartValues;
        public float duration = 1f;
        public float speed;
        public TweenCallback OnComplete;
        public bool YoYo;
        public virtual TweenActorPerformance Init(Vector3 i_vStartPosition, Vector3 i_vEndPosition, float i_duration = 1f, float i_speed = default(float), bool i_AllowInterrupt = true, TweenCallback i_callback = default(TweenCallback), bool i_yoyo = false)
        {
            StartValues = i_vStartPosition;
            EndValues = i_vEndPosition;
            duration = i_duration;
            speed = i_speed;
            AllowInterrupt = i_AllowInterrupt;
            OnComplete = i_callback;
            YoYo = i_yoyo;
            return this;
        }

        public TweenActorPerformance Init(TweenActorParams i_rcParams)
        {
            if (i_rcParams != null)
            {
                return Init(i_rcParams.StartValues, i_rcParams.EndValues, i_rcParams.duration, i_rcParams.speed, i_rcParams.AllowInterrupt, i_rcParams.OnComplete, i_rcParams.YoYo);
            }
            Debug.LogWarningFormat("Performance of Type {0} received Null param object,using default Values", this.GetType());
            return Init(Vector3.zero,Vector3.zero);
        }


        /// <summary>
        /// Excecute this performance with the specified actor
        /// </summary>
        /// <returns><c>true</c> if the Peformance was successfully completed, <c>false</c> otherwise </returns>
        /// <param name="i_rcActor">the actor that will perform the performance</param>
        public override bool Perform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            base.Perform(i_rcActor, i_rcInvoker);
            return true;
        }

        /// <summary>
        /// Cancel this performance
        /// </summary>
        /// <param name="i_rcActor">The actor currenly performing this performance</param>

        public override void Cancel(GameObject i_rcActor)
        {
            if (i_rcActor != null)
            {
                DOTween.Restart(i_rcActor);
                int kill = DOTween.Kill(i_rcActor);
            }
        }
    }
}