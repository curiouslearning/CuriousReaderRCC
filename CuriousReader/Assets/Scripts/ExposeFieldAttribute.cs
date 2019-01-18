using UnityEngine;
using System;
using System.Collections;
namespace CuriousReader.BookBuilder
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ExposeFieldAttribute : Attribute
    {
        public string InspectorLabel;
    }
}