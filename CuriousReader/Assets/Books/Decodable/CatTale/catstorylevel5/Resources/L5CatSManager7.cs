using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class L5CatSManager7 : GSManager {

	GameObject ham;
    GTinkerGraphic catHam;
    public override void Start()
    {
        ham = GameObject.Find("Ham");
        catHam = GameObject.Find("CatHam").GetComponent<GTinkerGraphic>();
        base.Start();
    }

    public override void OnMouseDown(GameObject go)
	{
		if (go.name == "CatHam" || go.name == "Text_cat") {
			
			ham.SetActive (true);
		} else if (go.name == "Text_eat") 
		{  ham.SetActive (false);
		}
		else if ( go.name == "Text_ham."){
			ham.SetActive (true);
            catHam.reset();
		}
		/*else if (go.name == "Ham") 
		{   
			catHam.MyOnMouseDown();
		}*/
        else if(go.name=="Text_eat")
        {
            ham.SetActive(false);
        }
		base.OnMouseDown (go);
	}

}