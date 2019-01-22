using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuriousReader.BookBuilder;
using CuriousReader.Performance;

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

    public NavigationPerformance Init (int i_nPageNumber, bool i_bDeactivateNextButton = false, List<GameObject> i_rcInvokers = null)
    {
        NavigationPerformance instance = this;
        pageNumber = i_nPageNumber;
        deactivateNextButton = i_bDeactivateNextButton;
        foreach (GameObject rcInvoker in i_rcInvokers)
        {
            instance.AddInvoker(rcInvoker);
        }
        return instance;
    }

    public NavigationPerformance Init (int i_nPageNumber, bool i_bDeactivateNextButton = false, GameObject i_rcInvoker = null)
    {
        NavigationPerformance instance = this;
        pageNumber = i_nPageNumber;
        deactivateNextButton = i_bDeactivateNextButton;
        instance.AddInvoker(i_rcInvoker);
        return instance;
    }

    public NavigationPerformance Init (NavigationParams i_rcParams)
    {
        if(i_rcParams != null)
        {
            return Init(i_rcParams.pageNumber, i_rcParams.deactivateNextButton, i_rcParams.InvokerList);

        }
        return Init(0, i_rcInvoker: null);
    }


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
