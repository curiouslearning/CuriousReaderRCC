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
        [ExposeField]
        public Vector3 EndValues;
        [ExposeField]
        protected Vector3 StartValues;
        [ExposeField]
        public float duration;
        [ExposeField]
        public float speed;
        [ExposeField]
        public TweenCallback OnComplete;

    }
    /// <summary>
    /// An abstract class for creating performances that change an actor's sta abstract class TweenActorPerformance : Performance{
    /// </summary>
    public abstract class TweenActorPerformance : Performance
    {
        public Vector3 EndValues;
        protected Vector3 StartValues;
        public float duration;
        public float speed;
        public TweenCallback OnComplete;
        public virtual TweenActorPerformance Init(Vector3 i_values, float i_duration = 1f, float i_speed = default(float), TweenCallback i_callback = default(TweenCallback))
        {
            TweenActorPerformance instance = this;
            DOTween.Init();
            EndValues = i_values;
            duration = i_duration;
            speed = i_speed;
            OnComplete = i_callback;
            return instance;
        }

        /// <summary>
        /// Check to see if this Performance can currently be performed
        /// </summary>
        /// <returns><c>true</c>, if we can perform right now, <c>false</c> otherwise.</returns>
        /// <param name="i_rcActor">The actor to perform the Performance</param>
        public override bool CanPerform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            if (!base.CanPerform(i_rcActor, i_rcInvoker))
            {
                return false;
            }
            if (i_rcActor != null && !TweenSystem.IsTweening(i_rcActor))
            {
                return true;
            }
            return false;
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
                int kill = DOTween.Kill(i_rcActor);
                Debug.LogFormat("Killed {1} tween(s) on {0}", i_rcActor.name, kill);
            }
        }

        /// <summary>
        /// return the actor to its original state before the execution of this performance
        /// </summary>
        /// <param name="i_rcActor">the actor to reset.</param>
        public abstract void UnPerform(GameObject i_rcActor);


    }
}