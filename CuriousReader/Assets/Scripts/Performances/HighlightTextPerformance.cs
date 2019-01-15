using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


public class HighlightTextPerformance : TweenActorPerformance
{
    float scaleMultiplier;
    Color color;
    Color startColor;

    public HighlightTextPerformance Init(Color i_color, float i_scaleMultiplier = 1.5f, float i_duration = 1, float i_speed = 0, TweenCallback i_callback = null)
    {
        base.Init(Vector3.zero, i_duration, i_speed, i_callback);
        scaleMultiplier = i_scaleMultiplier;
        color = i_color;
        HighlightTextPerformance instance = this;
        return instance;
    }

    public override bool Perform (GameObject i_rcActor, GameObject i_rcInvoker = null)
    {
        if (CanPerform(i_rcActor, i_rcInvoker))
        {
            startColor = GetActorColor(i_rcActor);
            ChangeText(i_rcActor, color); 
            TweenSystem.Highlight(i_rcActor, scaleMultiplier, duration, speed, () => ChangeText(i_rcActor, startColor));
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

    Color GetActorColor (GameObject i_rcActor)
    {
        TextMeshProUGUI text = i_rcActor.GetComponent<TextMeshProUGUI>();
        Color retVal = Color.black;
        if(text != null)
        {
            retVal = text.color;
        }
        return retVal;
    }

    public override void UnPerform(GameObject i_rcActor)
    {
    }

}