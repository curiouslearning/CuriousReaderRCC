using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuriousReader.BookBuilder;
using CuriousReader.Performance;

/// <summary>
/// a class for serializing performance state
/// </summary>
public class NavigationParams : PerformanceParams
{
    [ExposeField]
    public int pageNumber;
    [ExposeField]
    public bool deactivateNextButton;
}
public class NavigationPerformance : Performance{
    int pageNumber;
    int startPage;
    bool deactivateNextButton;

    /// <summary>
    /// initializes this performance instance with the specified parameters
    /// </summary>
    /// <returns>the specified performance.</returns>
    /// <param name="i_nPageNumber">the page number to navigate to.</param>
    /// <param name="i_bDeactivateNextButton">If set to <c>true</c> deactivate the default next page button.</param>
    /// <param name="i_rcInvokers">the list of actors that can invoke this performance</param>
    /// <param name="i_AllowInterrupt">If set to <c>true</c> allow this performance to interrupt other performances.</param>
    public NavigationPerformance Init (int i_nPageNumber, bool i_bDeactivateNextButton = false, List<GameObject> i_rcInvokers = null, bool i_AllowInterrupt = true)
    {
        base.Init(i_rcInvokers, i_AllowInterrupt);
        pageNumber = i_nPageNumber;
        deactivateNextButton = i_bDeactivateNextButton;
        return this;
    }

    /// <summary>
    /// initializes this performance instance with the specified parameters
    /// </summary>
    /// <returns>the specified performance.</returns>
    /// <param name="i_nPageNumber">the page number to navigate to.</param>
    /// <param name="i_bDeactivateNextButton">If set to <c>true</c> deactivate the default next page button.</param>
    /// <param name="i_rcInvoker">the actor that can invoke this performance</param>
    /// <param name="i_AllowInterrupt">If set to <c>true</c> allow this performance to interrupt other performances.</param>
    public NavigationPerformance Init (int i_nPageNumber, bool i_bDeactivateNextButton = false, GameObject i_rcInvoker = null, bool i_AllowInterrupt = true)
    {
        pageNumber = i_nPageNumber;
        deactivateNextButton = i_bDeactivateNextButton;
        AddInvoker(i_rcInvoker);
        return this;
    }

    /// <summary>
    /// initializes this performance using the provided params class
    /// </summary>
    /// <returns>The initialized performance</returns>
    /// <param name="i_rcParams">the class containing the initalization parameters.</param>
    public NavigationPerformance Init (NavigationParams i_rcParams)
    {
        if(i_rcParams != null)
        {
            return Init(i_rcParams.pageNumber, i_rcParams.deactivateNextButton, i_rcParams.InvokerList);

        }
        return Init(0, i_rcInvoker: null);
    }

    /// <summary>
    /// checks to see if <paramref name="i_rcInvoker"/>can invoke this performance on <paramref name="i_rcActor"/>
    /// </summary>
    /// <returns><c>true</c>, if we can perform, <c>false</c> otherwise.</returns>
    /// <param name="i_rcActor">the performing actor.</param>
    /// <param name="i_rcInvoker">the invoking actor.</param>
    public override bool CanPerform(GameObject i_rcActor, GameObject i_rcInvoker = null)
    {
        if(!base.CanPerform(i_rcActor, i_rcInvoker))
        {
            return false;
        }
        if(pageNumber != default(int))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Perform this performance on <paramref name="i_rcActor"/>
    /// </summary>
    /// <returns><c>true</c> if performance was successful, <c>false</c>otherwise</returns>
    /// <param name="i_rcActor">I rc actor.</param>
    /// <param name="i_rcInvoker">I rc invoker.</param>
    public override bool Perform(GameObject i_rcActor, GameObject i_rcInvoker = null)
    {
        if (i_rcActor == null)
        {
            return false;
        }
        LoadAssetFromJSON rcPageLoader = BookSystem.GetPageLoader();

        if (rcPageLoader != null)
        {
            startPage = LoadAssetFromJSON.pageNumber;
            rcPageLoader.RecordPageHistory();
            rcPageLoader.LoadPage(pageNumber, i_rcActor.gameObject);
            return true;
        }
        return false;
    }
/// <summary>
/// Rewind and reset this performance on <paramref name="i_rcActor"/>
/// </summary>
/// <param name="i_rcActor">I rc actor.</param>
    public void UnPerform (GameObject i_rcActor)
    {
        if(i_rcActor != null)
        {
            LoadAssetFromJSON rcPageLoader = BookSystem.GetPageLoader();
            if (rcPageLoader != null && startPage != default(int))
            {
                rcPageLoader.LoadPage(startPage, i_rcActor);
            }
        }
    }



}
