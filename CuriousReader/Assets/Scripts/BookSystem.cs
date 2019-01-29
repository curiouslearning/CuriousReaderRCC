using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class BookSystem
{
    /// <summary>
    /// GetPageLoader
    /// </summary>
    /// <returns>The object that loads pages.</returns>
    public static LoadAssetFromJSON GetPageLoader()
    {
        GameObject rcCanvas = GameObject.Find("Canvas");

        if ( rcCanvas != null )
        {
            return rcCanvas.GetComponent<LoadAssetFromJSON>();
        }

        return null;
    }

    public static void FadeIn(GameObject i_rcObject, float i_fTime)
    {
        if (i_rcObject != null)
        {
            SpriteRenderer rcRenderer = i_rcObject.GetComponent<SpriteRenderer>();

            if (rcRenderer != null)
            {
                rcRenderer.material.DOColor(new Color(1.0f, 1.0f, 1.0f, 1.0f), i_fTime);
            }
        }
    }

    public static void FadeOut(GameObject i_rcObject, float i_fTime)
    {
        if (i_rcObject != null)
        {
            SpriteRenderer rcRenderer = i_rcObject.GetComponent<SpriteRenderer>();

            if (rcRenderer != null)
            {
                rcRenderer.material.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), i_fTime);
            }
        }
    }

    public static void Darken(GameObject i_rcObject, float i_fTime, float i_fValue)
    {
        if (i_rcObject != null)
        {
            SpriteRenderer rcRenderer = i_rcObject.GetComponent<SpriteRenderer>();

            if (rcRenderer != null)
            {
                rcRenderer.material.DOColor(new Color(i_fValue,i_fValue,i_fValue, 1.0f), i_fTime);
            }
        }
    }

}
