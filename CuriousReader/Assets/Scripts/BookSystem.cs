using System.Collections;
using System.Collections.Generic;
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
}
