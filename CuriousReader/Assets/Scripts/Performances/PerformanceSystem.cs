using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using Elendow.SpritedowAnimator;

public static class PerformanceSystem
{

    /// <summary>
    /// Initialize a Performance Component on <paramref name="rcActor"/>, or returns it if it exists.
    /// </summary>
    /// <returns>The Performance Component.</returns>
    /// <param name="rcActor">The Actor to initialize.</param>
    public static PerformanceComponent InitializeComponent(GameObject rcActor)
    {
        if (rcActor == null)
        {
            Debug.LogError("passed a null Actor!");
            return null;
        }
        PerformanceComponent rcComponent = rcActor.GetComponent<PerformanceComponent>();
        if (rcComponent == null)
        {
            rcComponent = rcActor.AddComponent<PerformanceComponent>();
        }
        return rcComponent;
    }

    /// <summary>
    /// Return a SpriteAnimationPerformance initialized with the name of the animation in <paramref name="rcAnimator"/>'s animation list specified in <paramref name="trigger"/>'s
    /// </summary>
    /// <returns>A new SpriteAnimationPerformance.</returns>
    /// <param name="rcAnimator">The SpriteAnimator to reference</param>
    /// <param name="trigger">the TriggerClass containing the id number of the desired animation</param>
    public static SpriteAnimationPerformance GetSpriteAnimationPerformance(SpriteAnimator rcAnimator, TriggerClass trigger) {

        SpriteAnimationPerformance rcPerformance = SpriteAnimationPerformance.CreateInstance<SpriteAnimationPerformance>();

        if ((rcPerformance) != null && (rcAnimator != null))
        {
            if ((trigger.animId >= 0) && (trigger.animId < rcAnimator.animations.Count))
            {
                rcPerformance.AnimationName = rcAnimator.animations[trigger.animId].Name;
                Debug.Log("Animation name is: " + rcAnimator.animations[trigger.animId].Name);
            }
            else { Debug.LogWarning("trigger is out of bounds!");}
        }
        else { Debug.LogWarning("Performance and/or animator is null!"); }
        return rcPerformance;
    }

    /// <summary>
    /// Returns an Instance of the desired TweenActorPerformance, initialized with optional parameters 
    /// </summary>
    /// <returns>The tween performance.</returns>
    /// <param name="doInit">If set to <c>true</c> Initialize the performance with the given parameters.</param>
    /// <param name="endValues">the end values to tween to.</param>
    /// <param name="duration">Duration of the tween.</param>
    /// <param name="speed">Speed of the tween (overrides duration).</param>
    /// <param name="OnComplete">optional method to call on completion of tween.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public static T GetTweenPerformance<T>(bool doInit = false, Vector3 endValues = default(Vector3), float duration = 1f, float speed = default(float), TweenCallback OnComplete = default(TweenCallback)) where T : TweenActorPerformance
    {
        T rcPerformance = ScriptableObject.CreateInstance<T>();
        if (rcPerformance != null && doInit)
        {
            rcPerformance.Init(endValues, duration, speed, OnComplete);
        }
        return rcPerformance;
    }

    /// <summary>
    /// Attaches <paramref name="rcPerformance"/> to <paramref name="rcActor"/>, and sets its prompt to <paramref name="promptType"/>
    /// </summary>
    /// <returns><c>true</c>, if performance was successfully added, <c>false</c> otherwise.</returns>
    /// <param name="rcActor">The Performing Actor.</param>
    /// <param name="rcPerformance">The desired Performance.</param>
    /// <param name="promptType">The type of input that prompts this performance</param>
    public static bool AddPerformance (GameObject rcActor, Performance rcPerformance, PromptType promptType, GameObject rcInvoker = null)
    {
        PerformanceComponent rcComponent = InitializeComponent(rcActor);
        if(rcPerformance != null)
        {
            rcComponent.AddPerformance(rcPerformance, promptType, rcInvoker);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Sends a prompt of type <paramref name="promptType"/> from <paramref name="rcInvoker"/> to <paramref name="rcActor"/>.
    /// </summary>
    /// <param name="rcInvoker">The Actor invoking the Prompt.</param>
    /// <param name="rcActor">The Actor being prompted.</param>
    /// <param name="promptType">The Prompt Type.</param>
    public static void SendPrompt (GameObject rcInvoker, GameObject rcActor, PromptType promptType)
    {
        PerformanceComponent rcComponent = InitializeComponent(rcActor);
        if (rcComponent != null)
        {
            rcComponent.Prompt(rcInvoker, promptType);
        }
    }

}
