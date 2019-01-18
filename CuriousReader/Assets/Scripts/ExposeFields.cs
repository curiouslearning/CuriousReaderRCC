namespace CuriousReader.BookBuilder
{
    using UnityEditor;
    using UnityEngine;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public static class ExposeFields
    {
        public static void Expose(PropertyField[] properties)
        {
            GUILayoutOption[] emptyOptions = new GUILayoutOption[0];

            EditorGUILayout.BeginVertical(emptyOptions);

            foreach (PropertyField field in properties)
            {
                // Let's check to see if the property field has the named parameter "InspectorLabel" set
                // and, if so, use it to set the field's label in the inspector
                string inspectorLabel = (!String.IsNullOrEmpty(field.InspectorLabel)) ? field.InspectorLabel : field.Name;

                EditorGUILayout.BeginHorizontal(emptyOptions);

                switch (field.Type)
                {
                    case SerializedPropertyType.Integer:
                        field.SetValue(EditorGUILayout.IntField(inspectorLabel, (int)field.GetValue(), emptyOptions));
                        break;

                    case SerializedPropertyType.Float:
                        field.SetValue(EditorGUILayout.FloatField(inspectorLabel, (float)field.GetValue(), emptyOptions));
                        break;

                    case SerializedPropertyType.Boolean:
                        field.SetValue(EditorGUILayout.Toggle(inspectorLabel, (bool)field.GetValue(), emptyOptions));
                        break;

                    case SerializedPropertyType.String:
                        field.SetValue(EditorGUILayout.TextField(inspectorLabel, (String)field.GetValue(), emptyOptions));
                        break;

                    case SerializedPropertyType.Vector2:
                        field.SetValue(EditorGUILayout.Vector2Field(inspectorLabel, (Vector2)field.GetValue(), emptyOptions));
                        break;

                    case SerializedPropertyType.Vector3:
                        field.SetValue(EditorGUILayout.Vector3Field(inspectorLabel, (Vector3)field.GetValue(), emptyOptions));
                        break;

                    case SerializedPropertyType.Enum:
                        {
                            if (field.HasFlag())
                            {
                                field.SetValue(EditorGUILayout.EnumMaskField(inspectorLabel, (Enum)field.GetValue(), emptyOptions));
                            }
                            else
                            {
                                field.SetValue(EditorGUILayout.EnumPopup(inspectorLabel, (Enum)field.GetValue(), emptyOptions));
                            }
                        }
                        break;

                    case SerializedPropertyType.ArraySize:

                        GUILayout.Label("Array...");

                        break;

                    default:

                        GUILayout.Label("Unknown Property Type");
                        break;

                }

                EditorGUILayout.EndHorizontal();

            }

            EditorGUILayout.EndVertical();

        }

        public static PropertyField[] GetFields(System.Object obj)
        {
            List<PropertyField> fields = new List<PropertyField>();

            FieldInfo[] infos = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo info in infos)
            {
                object[] attributes = info.GetCustomAttributes(true);

                bool isExposed = false;

                foreach (object o in attributes)
                {
                    if (o.GetType() == typeof(ExposeFieldAttribute))
                    {
                        isExposed = true;
                        break;
                    }
                }

                if (!isExposed)
                    continue;

                SerializedPropertyType type = SerializedPropertyType.Integer;

                if (PropertyField.GetPropertyType(info, out type))
                {
                    PropertyField field = new PropertyField(obj, info, type);
                    fields.Add(field);
                }

            }

            return fields.ToArray();

        }
    }
}