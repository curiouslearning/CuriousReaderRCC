using UnityEngine;
using System;
using System.Collections;

[AttributeUsage(AttributeTargets.Field)]
public class ExposeFieldAttribute : Attribute
{
    public string InspectorLabel;
}
