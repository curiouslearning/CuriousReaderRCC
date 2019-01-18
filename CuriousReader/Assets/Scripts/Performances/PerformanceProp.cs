using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CuriousReader.Performance
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExposePerformancePropAttribute : Attribute
    {
        string InspectorLabel;
    }
}
