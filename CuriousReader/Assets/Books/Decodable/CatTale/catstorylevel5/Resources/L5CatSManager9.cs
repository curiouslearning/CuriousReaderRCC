using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class L5CatSManager9 : GSManager {

	GameObject jam;
    GTinkerGraphic catJam;
    public override void Start()
    {
        jam = GameObject.Find("Jam");
        catJam = GameObject.Find("CatJam").GetComponent<GTinkerGraphic>();
        base.Start();
    }
    public override void OnMouseDown(GameObject go)
	{
		if (go.name == "CatJam"|| go.name=="Text_cat"|| go.name=="Text_eat") {
            jam.SetActive (true);
		}
		else if ( go.name == "Text_jam."){
            jam.SetActive (true);
            catJam.reset();
         }
		/*else if (go.name == "Jam") 
		{   
			jam = go;
			GTinkerGraphic catJam = GameObject.Find("CatJam").GetComponent<GTinkerGraphic>();
			if (catJam != null)
			{ 
				catJam.MyOnMouseDown();
			}
		}*/

		base.OnMouseDown (go);
	}


}
