// Spritedow Animation Plugin by Elendow
// http://elendow.com

#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System;
using System.Reflection;

namespace Elendow.SpritedowAnimator
{
    [CustomPropertyDrawer(typeof(SpriteAnimationFieldAttribute))]
    public class SpriteAnimationFieldDrawer : PropertyDrawer
    {
        private int selectedAnim;
        private string[] animations;
        private BaseAnimator animator;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
			EditorGUI.BeginProperty(position, label, property);

            if (animator == null)
            {
                MonoBehaviour go = (MonoBehaviour)GetParent(property);
                animator = go.GetComponentInChildren<BaseAnimator>();

                if (animator == null)
                {
                    EditorGUI.LabelField(position, "No animator detected in this object.");
                }
            }
            else
            {
                if (animator.animations == null || animator.animations.Count == 0)
                {
                    EditorGUI.LabelField(position, "Animator without animations");
                }
                else
                {
                    bool refresh = animations == null || animations.Length != animator.animations.Count;

                    if (!refresh)
                    {
                        if (animations.Length > 0 && animations.Length == animator.animations.Count)
                        {
                            for (int i = 0; i < animator.animations.Count; i++)
                            {
                                if (!animations[i].Equals(animator.animations[i]))
                                {
                                    refresh = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (refresh)
                    {
                        animations = new string[animator.animations.Count];
                        for (int i = 0; i < animations.Length; i++)
                        {
                            if (animator.animations[i] != null)
                            {
                                animations[i] = animator.animations[i].Name;
                                if (animations[i] == property.stringValue)
                                    selectedAnim = i;
                            }
                            else
                            {
                                animations[i] = "{null}";
                            }
                        }
                    }

                    if (animations.Length > 0)
                    {
                        selectedAnim = EditorGUI.Popup(position, label.text, selectedAnim, animations);
                        property.stringValue = animations[selectedAnim];
                    }
                    else
                    {
                        selectedAnim = -1;
                        EditorGUI.LabelField(position, "Animator without animations");
                    }
                }
            }
            EditorGUI.EndProperty();
		}

		private object GetParent(SerializedProperty prop)
		{
			var path = prop.propertyPath.Replace(".Array.data[", "[");
			object obj = prop.serializedObject.targetObject;
			var elements = path.Split('.');
			foreach (var element in elements.Take(elements.Length - 1))
			{
				if (element.Contains("["))
				{
					var elementName = element.Substring(0, element.IndexOf("["));
					var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
					obj = GetValue(obj, elementName, index);
				}
				else
				{
					obj = GetValue(obj, element);
				}
			}
			return obj;
		}

        private object GetValue(object source, string name)
		{
			if (source == null)
				return null;
			var type = source.GetType();
			var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			if (f == null)
			{
				var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (p == null)
					return null;
				return p.GetValue(source, null);
			}
			return f.GetValue(source);
		}

        private object GetValue(object source, string name, int index)
		{
			var enumerable = GetValue(source, name) as IEnumerable;
			var enm = enumerable.GetEnumerator();
			while (index-- >= 0)
				enm.MoveNext();
			return enm.Current;
		}
    }
}
#endif