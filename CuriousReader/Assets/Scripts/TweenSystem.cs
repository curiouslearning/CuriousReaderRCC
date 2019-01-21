
namespace CuriousReader.Performance
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using DG.Tweening;
    using TMPro;
    using UnityEngine.UI;


    /// <summary>
    /// A system for modifying the state of an actor over time
    /// </summary>
    public static class TweenSystem
    {
        #region Tweens 


        /// <summary>
        /// Reset <paramref name="i_rcActor"/> to its default state as specified by <paramref name="i_initialValues"/>
        /// </summary>
        /// <param name="i_rcActor">The Actor</param>
        /// <param name="i_initialValues">A record of the Actor's initial state</param>
        public static void Reset(GameObject i_rcActor, GameObjectClass i_initialValues, TweenCallback i_callBack = default(TweenCallback))
        {
            if (!ValidateArgs(i_rcActor, i_initialValues, i_callBack))
            {
                Debug.LogError("TweenSystem.Reset failed due to invalid arguments!");
                return;
            }
            DOTween.Kill(i_rcActor.transform);
            i_rcActor.gameObject.transform.position = new Vector3(i_initialValues.posX, i_initialValues.posY, i_initialValues.posZ);
            i_rcActor.gameObject.transform.rotation = Quaternion.Euler(i_initialValues.rotX, i_initialValues.rotY, i_initialValues.rotZ);
            i_rcActor.gameObject.transform.localScale = new Vector2(i_initialValues.scaleX, i_initialValues.scaleY);
            GTinkerGraphic rcGraphic = i_rcActor.GetComponent<GTinkerGraphic>();
            LoadAssetFromJSON.LoadAssetImage(rcGraphic, i_initialValues.imageName);

        }

        /// <summary>
        /// Move <paramref name="i_rcActor"/> to  <paramref name="i_endValues"/> over <paramref name="i_duration"/> OR at <paramref name="i_speed"/>
        /// </summary>
        /// <param name="i_rcActor">The Actor</param>
        /// <param name="i_endValues">The Target Position</param>
        /// <param name="i_duration">The Duration of the tween.</param>
        /// <param name="i_speed">[OPTIONAL] the speed of the tween (overrides duration)</param>
        public static void Move(GameObject i_rcActor, Vector3 i_endValues, float i_duration = 1f, float i_speed = default(float), TweenCallback i_callBack = default(TweenCallback))
        {
            if (!ValidateArgs(i_rcActor, i_endValues))
            {
                Debug.LogError("TweenSystem.Move failed due to invalid arguments!");
                return;
            }
            if (!DOTween.IsTweening(i_rcActor))
            {
                Tween tween;
                if (!i_speed.Equals(default(float)))
                {
                    tween = i_rcActor.transform.DOMove(i_endValues, i_speed).SetSpeedBased(true);
                }
                else
                {
                    tween = i_rcActor.transform.DOMove(i_endValues, i_duration);
                }
                if (i_callBack != null)
                {
                    tween.OnComplete(i_callBack);
                }
                tween.SetId(i_rcActor);
                tween.Play();
            }
        }
        #region Scale Tweens

        /// <summary>
        /// Scale actor <paramref name="i_rcActor"/>, to scale <paramref name="i_endValues"/>, either for duration <paramref name="i_duration"/> or at speed and <paramref name="i_speed"/>.
        /// </summary>
        /// <param name="i_rcActor">the actor to scale.</param>
        /// <param name="i_endValues">the scale to tween to.</param>
        /// <param name="i_duration">the duration of the tween.</param>
        /// <param name="i_speed">[OPTIONAL] the speed of the tween (overrides duration).</param>
        public static void Scale(GameObject i_rcActor, Vector3 i_endValues, float i_duration = 1f, float i_speed = default(float), TweenCallback i_callBack = default(TweenCallback))
        {
            if (!ValidateArgs(i_rcActor, i_endValues))
            {
                Debug.LogError("TweenSystem.Scale failed due to invalid arguments!");
                return;
            }
            Tweener tween;
            if (!i_speed.Equals(default(float))) //tween by speed instead of duration
            {
                tween = i_rcActor.transform.DOScale(i_endValues, i_speed).SetSpeedBased(true);
            }
            else
            {
                tween = i_rcActor.transform.DOScale(i_endValues, i_duration);
            }
            if (i_callBack != null)
            {
                tween.OnComplete(i_callBack);
            }
            tween.SetId(i_rcActor);
            tween.Play();
        }

        /// <summary>
        /// Zoom actor <paramref name="i_rcActor"/> by <paramref name="i_scaleMultiplier"/> for <paramref name="i_duration"/>, then yoyo to original scale.
        /// </summary>
        /// <param name="i_rcActor">the actor on which to perform the tween.</param>
        /// <param name="i_scaleMultiplier">the value by which to scale <paramref name="i_rcActor"/>'s size.</param>
        /// <param name="i_duration">the tween duration.</param>
        public static void Highlight(GameObject i_rcActor, float i_scaleMultiplier = 1.5f, float i_duration = 1f, float i_speed = default(float), TweenCallback i_callBack = default(TweenCallback))
        {

            if (!ValidateArgs(i_rcActor))
            {
                Debug.LogError("TweenSystem.Highlight failed due to invalid arguments!");
                return;
            }
            Vector3 endValues = i_rcActor.transform.localScale * i_scaleMultiplier;
            Tween tween;
            if (!i_speed.Equals(default(float)))
            {
                tween = i_rcActor.transform.DOScale(endValues, i_speed).SetSpeedBased(true).SetLoops(2, LoopType.Yoyo);
            }
            else
            {
                tween = i_rcActor.transform.DOScale(endValues, i_duration).SetLoops(2, LoopType.Yoyo);
            }
            if (i_callBack != null)
            {
                tween.OnComplete(i_callBack);
            }
            tween.SetId(i_rcActor);
            tween.Play();

        }

        public static void HighlightText(GameObject i_rcActor, Color i_color, float i_scaleMultiplier = 1.5f, float i_delay = 0, float i_duration = 1f, float i_speed = default(float), TweenCallback i_callBack = default(TweenCallback))
        {
            if (!ValidateArgs(i_rcActor, i_scaleMultiplier))
            {
                Debug.LogErrorFormat("HighlightText failed due to one of the following parameters being null: {0}, {1}", i_rcActor, i_scaleMultiplier);
            }
            Animator anim = i_rcActor.GetComponent<Animator>();
            if (anim != null)
            {
                anim.enabled = false;
            }
            RectTransform rect = i_rcActor.GetComponent<RectTransform>();
            Tween ScaleTween;
            Tween ColorTween;
            TextMeshProUGUI text = i_rcActor.GetComponent<TextMeshProUGUI>();
            Vector3 endValues = rect.localScale * 1.5f;
            if (rect != null)
            {
                if (!i_speed.Equals(default(float)))
                {
                    ScaleTween = rect.DOScale(endValues, i_speed).SetDelay(i_delay).SetSpeedBased(true).SetLoops(2, LoopType.Yoyo);
                    ColorTween = text.DOColor(i_color, i_speed).SetDelay(i_delay).SetSpeedBased(true).SetLoops(2, LoopType.Yoyo);
                }
                else
                {
                    ScaleTween = rect.DOScale(endValues, i_duration).SetDelay(i_delay).SetLoops(2, LoopType.Yoyo);
                    ColorTween = text.DOColor(i_color, i_duration).SetDelay(i_delay).SetLoops(2, LoopType.Yoyo);
                }
                if (i_callBack != null)
                {
                    ScaleTween.OnComplete(i_callBack);
                }
                DG.Tweening.Sequence sequence = DOTween.Sequence();
                sequence.Insert(0, ScaleTween);
                sequence.Insert(0, ColorTween);
                sequence.SetId(i_rcActor);
                sequence.Play();
            }

        }
        #endregion
        /// <summary>
        /// Rotate <paramref name="i_rcActor"/> to <paramref name="i_endValues"/>, over duration <paramref name="i_duration"/> or at <paramref name="i_speed"/>.
        /// </summary>
        /// <param name="i_rcActor">the actor to tween.</param>
        /// <param name="i_endValues">the set of Euler angles to tween to.</param>
        /// <param name="i_duration">the duration of the tween.</param>
        /// <param name="i_speed">[OPTIONAL] the speed of the tween (overrides duration).</param>
        public static void Rotate(GameObject i_rcActor, Vector3 i_endValues, float i_duration = 1f, float i_speed = default(float), TweenCallback i_callBack = default(TweenCallback))
        {
            if (!ValidateArgs(i_rcActor, i_endValues))
            {
                Debug.LogError("TweenSystem.Rotate failed due to invalid arguments!");
                return;
            }
            Tween tween;
            if (!i_speed.Equals(default(float)))
            {
                tween = i_rcActor.transform.DORotate(i_endValues, i_speed).SetSpeedBased(true);
            }
            else
            {
                tween = i_rcActor.transform.DORotate(i_endValues, i_duration);
            }
            if (i_callBack != null)
            {
                tween.OnComplete(i_callBack);
            }
            tween.SetId(i_rcActor);
            tween.Play();
        }

        /// <summary>
        /// Change the face color of this object's text to the given Color
        /// </summary>
        /// <param name="i_color">the desired color.</param>
        public static void SetFaceColor(GameObject i_rcActor, Color i_color = default(Color))
        {
            if (!ValidateArgs(i_rcActor))
            {
                Debug.LogError("TweenSystem.SetFaceColor failed due to invalid arguments!");
                return;
            }

            TextMeshProUGUI rcText = i_rcActor.GetComponent<TextMeshProUGUI>();
            if (rcText != null)
            {
                rcText.color = i_color;
                rcText.havePropertiesChanged = true;
                rcText.Rebuild(CanvasUpdate.Prelayout);
                rcText.ForceMeshUpdate();
            }
        }

        #endregion

        #region Helper Functions
        /// <summary>
        /// Validates the arguments of a tween function.
        /// </summary>
        /// <returns><c>true</c>, if no params are null, <c>false</c> otherwise.</returns>
        /// <param name="i_params">the arguments to validate.</param>
        static bool ValidateArgs(params object[] i_params)
        {
            foreach (object param in i_params)
            {
                if (param == null)
                {
                    Debug.LogWarning("One or more required arguments of this tween are null, please specify a value");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks actor <paramref name="i_rcActor"/> for any currently playing Tweens
        /// </summary>
        /// <returns><c>true</c>, if there are any currently playing Tweens, <c>false</c> otherwise.</returns>
        /// <param name="i_rcActor">the actor to inspect.</param>
        /// <param name="i_target">any specific tween target to check</param>
        public static bool IsTweening(GameObject i_rcActor)
        {
            if (DOTween.IsTweening(i_rcActor))
            {
                return true;
            }
            foreach (Component c in i_rcActor.GetComponents<Component>())
            {
                if (DOTween.IsTweening(c))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}