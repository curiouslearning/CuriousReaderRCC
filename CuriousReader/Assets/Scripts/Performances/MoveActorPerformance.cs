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
    /// MoveActorPerformance -- One of the truest extensions of a TweenActorPerformance, the move actor performance is 
    /// used to move from StartingValue vector (interpreted as a position on the screen) to EndValue over a specific duration
    /// or using a specific speed.
    /// </summary>
    public class MoveActorPerformance : TweenActorPerformance
    {
        /// <summary>
        /// MoveActorPerformance constructor.  
        /// </summary>
        /// <param name="i_rcParams">Parameters that get translated to the local members via Init.</param>
        public MoveActorPerformance(MoveParams i_rcParams)
        {
            if (i_rcParams != null)
            {
                Init(i_rcParams.StartValues, i_rcParams.EndValues, i_rcParams.duration, i_rcParams.speed, i_rcParams.AllowInterrupt, i_rcParams.OnComplete, i_rcParams.YoYo);
            }
        }

        /// <summary>
        /// Init.  This method initializes a performance allowing us to transform parameters to variables on this performance.
        /// </summary>
        /// <param name="i_vStartPosition">Starting location of the tween.</param>
        /// <param name="i_vEndPosition">Ending location of the tween.</param>
        /// <param name="i_duration">Amount of time in seconds to perform the tween.</param>
        /// <param name="i_speed">Speed in unity transform units/second</param>
        /// <param name="i_AllowInterrupt">Is this performance allowed to be interupted?</param>
        /// <param name="i_callback">Completion Callback</param>
        /// <param name="i_yoyo">Does this movement yo-yo back to the start position?</param>
        /// <returns></returns>
        public new MoveActorPerformance Init(Vector3 i_vStartPosition, Vector3 i_vEndPosition, float i_duration = 1f, float i_speed = default(float), bool i_AllowInterrupt = true, TweenCallback i_callback = default(TweenCallback), bool i_yoyo = false)
        {
            base.Init(i_vStartPosition, i_vEndPosition, i_duration, i_speed, i_AllowInterrupt, i_callback, i_yoyo);
            return this;
        }

        /// <summary>
        /// Perform.  Lights, camera, action... Actor i_rcActor moves stage left to EndPosition starting at 
        /// StartPosition which is currently set during initialization in LoadAssetFromJSON.  The params get overridden 
        /// shortly after initialization since we can't trust the current position of actor because it could be in 
        /// the middle of a tween.
        /// </summary>
        /// <param name="i_rcActor"></param>
        /// <param name="i_rcInvoker"></param>
        /// <returns></returns>
        public override bool Perform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            if (i_rcActor != null)
            {
                // TODO -- We may want a form of movement that allows relative movement.
                // We would ignore the start value here and use the current position as the starting point.
                i_rcActor.transform.position = StartValues;
                TweenSystem.Move(i_rcActor, EndValues, duration, speed, OnComplete, YoYo);
                Performing = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// UnPerform -- This method is used to "undo" the command of Performance.  In the case of movement, we need
        /// to cancel the tween that is in progress and reset the position.  We may want to add a parameter that smooths 
        /// us back to the start position instead of the current abrupt untransitioned position set.
        /// </summary>
        /// <param name="i_rcActor"></param>
        public override void UnPerform(GameObject i_rcActor)
        {
            Cancel(i_rcActor);
            i_rcActor.transform.position = StartValues;
        }
    }
}