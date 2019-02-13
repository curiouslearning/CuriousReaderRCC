using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class Extensions
{
    public static T[] RemoveAt<T>(this T[] source, int index)
    {
        T[] dest = new T[source.Length - 1];
        if (index > 0)
            Array.Copy(source, 0, dest, 0, index);

        if (index < source.Length - 1)
            Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }

    public static void Invoke(this MonoBehaviour i_rcMonoBehavior, Action i_fnDelegate, float i_fTime)
    {
        if (i_fnDelegate != null)
        {
            MemberInfo rcMemberInfo = i_fnDelegate.Method;

            if (rcMemberInfo != null)
            {
                if (!string.IsNullOrEmpty(rcMemberInfo.Name))
                {
                    i_rcMonoBehavior.Invoke(rcMemberInfo.Name, i_fTime);
                }
            }
        }
    }

    public static void InvokeRepeating(this MonoBehaviour i_rcMonoBehavior, Action i_fnDelegate, float i_fTime, float i_fRepeatRate)
    {
        if (i_fnDelegate != null)
        {
            MemberInfo rcMemberInfo = i_fnDelegate.Method;

            if (rcMemberInfo != null)
            {
                if (!string.IsNullOrEmpty(rcMemberInfo.Name))
                {
                    i_rcMonoBehavior.InvokeRepeating(rcMemberInfo.Name, i_fTime, i_fRepeatRate);
                }
            }
        }
    }

    public static void CancelInvoke(this MonoBehaviour i_rcMonoBehavior, Action i_fnDelegate)
    {
        if (i_fnDelegate != null)
        {
            MemberInfo rcMemberInfo = i_fnDelegate.Method;

            if (rcMemberInfo != null)
            {
                if (!string.IsNullOrEmpty(rcMemberInfo.Name))
                {
                    i_rcMonoBehavior.CancelInvoke(rcMemberInfo.Name);
                }
            }
        }
    }



}
