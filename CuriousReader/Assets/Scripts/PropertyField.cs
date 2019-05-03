    namespace CuriousReader.BookBuilder
    {
    using UnityEditor;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;
    using System.Reflection;
    public class PropertyField
        {
            System.Object m_Instance;
            FieldInfo m_Info;
#if UNITY_EDITOR
        SerializedPropertyType m_Type;

            public FieldInfo FieldInfo
            {
                get
                {
                    return m_Info;
                }
            }

            public SerializedPropertyType Type
            {
                get
                {
                    return m_Type;
                }
            }

            public String Name
            {
                get
                {
                    return ObjectNames.NicifyVariableName(m_Info.Name);
                }
            }

            public string InspectorLabel
            {
                get
                {
                    string returnValue = null;
                    object[] attributes = m_Info.GetCustomAttributes(true);
                    foreach (object o in attributes)
                    {
                        if (o.GetType() == typeof(ExposeFieldAttribute))
                        {
                            ExposeFieldAttribute exposeProperty = o as ExposeFieldAttribute;
                            if (exposeProperty != null)
                                returnValue = exposeProperty.InspectorLabel;
                        }
                    }

                    return returnValue;
                }
            }

            public PropertyField(System.Object instance, FieldInfo info, SerializedPropertyType type)
            {
                m_Instance = instance;
                m_Info = info;
                m_Type = type;
            }
#endif
        public bool HasFlag()
            {
                return (m_Info.FieldType.GetCustomAttributes(typeof(FlagsAttribute), true).Length > 0);
            }

            public System.Object GetValue()
            {
                return m_Info.GetValue(m_Instance);
            }

            public void SetValue(System.Object value)
            {
                m_Info.SetValue(m_Instance, value);
            }
#if UNITY_EDITOR
        public static bool GetPropertyType(FieldInfo info, out SerializedPropertyType propertyType)
            {
                propertyType = SerializedPropertyType.Generic;

                Type type = info.FieldType;

                if (type == typeof(int))
                {
                    propertyType = SerializedPropertyType.Integer;
                    return true;
                }

                if (type == typeof(float))
                {
                    propertyType = SerializedPropertyType.Float;
                    return true;
                }

                if (type == typeof(bool))
                {
                    propertyType = SerializedPropertyType.Boolean;
                    return true;
                }

                if (type == typeof(string))
                {
                    propertyType = SerializedPropertyType.String;
                    return true;
                }

                if (type == typeof(Vector2))
                {
                    propertyType = SerializedPropertyType.Vector2;
                    return true;
                }

                if (type == typeof(Vector3))
                {
                    propertyType = SerializedPropertyType.Vector3;
                    return true;
                }

                if (type.IsEnum)
                {
                    propertyType = SerializedPropertyType.Enum;
                    return true;
                }

                if (type.IsArray)
                {
                    propertyType = SerializedPropertyType.ArraySize;
                    return true;
                }

                return false;
            }
#endif
    }
}