using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class L4CatSManager9 : GSManager {
    GameObject jam;
	public override void Start ()
	{   jam = GameObject.Find ("Jam");
		base.Start ();
	}

	public override void OnMouseDown(GameObject go)
	{
		if (go.name == "CatJam" || go.name == "Text_cat" || go.name == "Text_wants") {
			jam.SetActive (true);
		}
		else if ( go.name == "Text_jam."){
			jam.SetActive (true);
			GTinkerGraphic catJam = GameObject.Find ("CatJam").GetComponent<GTinkerGraphic> ();
			if (catJam != null)
			{
//                catJam.reset();
			}


		}

		base.OnMouseDown (go);
	}


}
