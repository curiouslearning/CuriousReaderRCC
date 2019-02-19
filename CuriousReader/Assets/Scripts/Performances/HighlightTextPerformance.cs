using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using CuriousReader.BookBuilder;


namespace CuriousReader.Performance
{
    public class HighlightTextParams : TweenActorParams
    {
        [ExposeField]
        public float scaleMultiplier;
        [ExposeField]
        public float delay;
        [ExposeField]
        public Color color;
    }
    public class HighlightTextPerformance : TweenActorPerformance
    {
        float scaleMultiplier;
        float delay;
        Color color;
        Color startColor;
        Vector3 startScale;

        public HighlightTextPerformance Init(Color i_color, float i_scaleMultiplier = 1.5f, float i_delay = 0f, float i_duration = 1, float i_speed = 0, bool i_AllowInterrupt = true, TweenCallback i_callback = null)
        {
            base.Init(Vector3.zero, Vector3.zero, i_duration, i_speed, i_AllowInterrupt, i_callback);
            scaleMultiplier = i_scaleMultiplier;
            delay = i_delay;
            color = i_color;
            HighlightTextPerformance instance = this;
            return instance;
        }

        public HighlightTextPerformance Init (HighlightTextParams i_rcParams)
        {
            if (i_rcParams != null)
            {
                return Init(i_rcParams.color, i_rcParams.scaleMultiplier, i_rcParams.delay, i_rcParams.duration, i_rcParams.speed, i_rcParams.AllowInterrupt, i_rcParams.OnComplete);
            }
            Debug.LogWarning("Null param object passed, using default values");
            return Init(Color.yellow);
        }

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

        void ChangeText(GameObject i_rcActor, Color color)
        {
            TextMeshProUGUI text = i_rcActor.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                text.color = color;
            }
        }

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

        public override void Cancel(GameObject i_rcActor)
        {
            base.Cancel(i_rcActor);
            i_rcActor.transform.localScale = startScale;
            ChangeText(i_rcActor, startColor);

        }

        public override void UnPerform(GameObject i_rcActor)
        {
        }

    }
}