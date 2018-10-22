using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class EndPage :  GSManager
{
private System.DateTime inTime;	
	
	// Use this for initialization
	public override void Start () {
		        inTime = System.DateTime.Now;   

	}
    public override void OnMouseDown(GameObject go)
    {
        if (go.name == "Home")
        {
            HomeClick();       
        }
        else if (go.name == "ReadAgain") {
            ReadAgain();
        }

    }



  public void HomeClick()
    {
        DateTime time = DateTime.Now;
        //sending data directly to firebase using "72 hours rule"! (removed local data storage)
        //DataCollection.AddInTouchData ("Button_Home", time.ToString());
        FirebaseHelper.LogInAppTouch("Button_Home_End", time.ToString());
        TimeSpan span = (time - inTime);
       
        FirebaseHelper.LogInAppSection(inTime.ToString(), span.TotalSeconds);
        SceneManager.LoadScene("shelf");

    }
	public void ReadAgain()
    {
        DateTime time = DateTime.Now;
        FirebaseHelper.LogInAppTouch("Button_Read_Again_End", time.ToString());

        TimeSpan span = (time - inTime);
       
        FirebaseHelper.LogInAppSection(inTime.ToString(), span.TotalSeconds);
        LoadAssetFromJSON l = myCanvas.GetComponent<LoadAssetFromJSON>();
        l.EmptyPage();
        l.LoadStoryData();
        
    }
}
