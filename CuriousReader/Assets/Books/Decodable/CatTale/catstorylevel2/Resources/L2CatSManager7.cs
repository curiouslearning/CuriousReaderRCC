using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class L2CatSManager7 : GSManager {

	GameObject ham;

	public override void Start ()
	{   ham = GameObject.Find ("Ham");
		
		base.Start ();
	}

	public override void OnMouseDown(GameObject go)
	{
		if (go.name == "CatHam" || go.name=="Text_cat's") {

			ham.SetActive(true);
		} else if (go.name == "Text_ham") {
			ham.SetActive (true);
			GTinkerGraphic catHam = GameObject.Find ("CatHam").GetComponent<GTinkerGraphic> ();
			if (catHam != null) {
				//catHam.reset();
				catHam.MyOnMouseDown ();

			}

		}
		//		else if (go.name == "Ham") 
		//		{   
		//			ham = go;
		//			GTinkerGraphic catHam = GameObject.Find("CatHam").GetComponent<GTinkerGraphic>();
		//			if (catHam != null)
		//			{ 
		//				catHam.MyOnMouseDown();
		//			}
		//		}

		base.OnMouseDown (go);
	}

}