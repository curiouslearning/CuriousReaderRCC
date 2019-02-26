using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using CuriousReader.BookBuilder;


namespace CuriousReader.Performance
{
    /// <summary>
    /// A serializable class for storing performance parameters
    /// </summary>
    public class HighlightTextParams : TweenActorParams
    {
        [ExposeField]
        public float scaleMultiplier;
        [ExposeField]
        public float delay;
        [ExposeField]
        public Color color;
    }

    /// <summary>
    /// A performance used to highlight text on a page
    /// </summary>
    public class HighlightTextPerformance : TweenActorPerformance
    {
        float scaleMultiplier;
        float delay;
        Color color;
        Color startColor;
        Vector3 startScale;

        /// <summary>
        /// Initialize the performance with the specified parameters
        /// </summary>
        /// <returns>The initialized performance.</returns>
        /// <param name="i_color">the desired color</param>
        /// <param name="i_scaleMultiplier">I scale multiplier.</param>
        /// <param name="i_duration">I duration.</param>
        /// <param name="i_delay"> optional delay before performing</param>
        /// <param name="i_speed">OPTIONAL: speed (overrides duration).</param>
        /// <param name="i_AllowInterrupt">If set to <c>true</c> allows this performance to interrupt other performances.</param>
        /// <param name="i_callback">Method to call on completion.</param>
        public HighlightTextPerformance Init(Color i_color, float i_scaleMultiplier = 1.5f, float i_delay = 0f, float i_duration = 1, float i_speed = 0, bool i_AllowInterrupt = true, TweenCallback i_callback = null)
        {
            base.Init(Vector3.zero, Vector3.zero, i_duration, i_speed, i_AllowInterrupt, i_callback);
            scaleMultiplier = i_scaleMultiplier;
            delay = i_delay;
            color = i_color;
            HighlightTextPerformance instance = this;
            return instance;
        }

        /// <summary>
        /// Initialize this performance using a params class
        /// </summary>
        /// <returns>The initialized performance.</returns>
        /// <param name="i_rcParams">I rc parameters.</param>
        public HighlightTextPerformance Init (HighlightTextParams i_rcParams)
        {
            if (i_rcParams != null)
            {
                return Init(i_rcParams.color, i_rcParams.scaleMultiplier, i_rcParams.delay, i_rcParams.duration, i_rcParams.speed, i_rcParams.AllowInterrupt, i_rcParams.OnComplete);
            }
            Debug.LogWarning("Null param object passed, using default values");
            return Init(Color.yellow);
        }

        /// <summary>
        /// Perform this performance on <paramref name="i_rcActor"/>
        /// </summary>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise</returns>
        /// <param name="i_rcActor">the performing actor</param>
        /// <param name="i_rcInvoker">the invoking actor</param>
        public override bool Perform(GameObject i_rcActor, GameObject i_rcInvoker = null)
        {
            if (i_rcActor != null)
            {
                startColor = GetActorColor(i_rcActor);
                startScale = i_rcActor.transform.localScale;
                //ChangeText(i_rcActor, color);
                TweenSystem.HighlightText(i_rcActor, color, scaleMultiplier, delay, duration, speed, OnComplete);
                Performing = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Changes the text color.
        /// </summary>
        /// <param name="i_rcActor">the text to perform on.</param>
        /// <param name="color">the target color</param>
        void ChangeText(GameObject i_rcActor, Color color)
        {
            TextMeshProUGUI text = i_rcActor.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.color = color;
            }
        }

        /// <summary>
        /// Gets the color of the text.
        /// </summary>
        /// <returns>The text color.</returns>
        /// <param name="i_rcActor">the performing actor.</param>
        Color GetActorColor(GameObject i_rcActor)
        {
            TextMeshProUGUI text = i_rcActor.GetComponent<TextMeshProUGUI>();
            Color retVal = Color.black;
            if (text != null)
            {
                retVal = text.color;
            }
            return retVal;
        }

        /// <summary>
        /// Cancel this performance on the specified actor.
        /// </summary>
        /// <param name="i_rcActor">I rc actor.</param>
        public override void Cancel(GameObject i_rcActor)
        {
            base.Cancel(i_rcActor);
            i_rcActor.transform.localScale = startScale;
            ChangeText(i_rcActor, startColor);

        }

        /// <summary>
        /// Rewind and reset this peformance on <paramref name="i_rcActor"/>
        /// </summary>
        /// <param name="i_rcActor">I rc actor.</param>
        public override void UnPerform(GameObject i_rcActor)
        {
        }

    }
}