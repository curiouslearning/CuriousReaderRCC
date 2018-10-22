using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class L0CatSManager3 : GSManager {

	public override void OnMouseDown(GameObject go)
	{
		if (go.name == "Cat") {
			GTinkerGraphic tinkerGraphic = GameObject.Find ("Ran").GetComponent<GTinkerGraphic> ();
			if (tinkerGraphic != null) {
				tinkerGraphic.MyOnMouseDown ();
			}
		}
		else if (go.name == "Ran") {
			GTinkerGraphic tinkerGraphic = GameObject.Find ("Cat").GetComponent<GTinkerGraphic> ();
			if (tinkerGraphic != null ) {
				tinkerGraphic.MyOnMouseDown ();
			} 
			
		}
        /*
		 else if (go.name == "Text_ran") {
			Debug.Log ("text ran");
			GTinkerGraphic ranGraphic = GameObject.Find ("Ran").GetComponent<GTinkerGraphic> ();
			GTinkerGraphic catGraphic = GameObject.Find ("Cat").GetComponent<GTinkerGraphic> ();
			if (ranGraphic != null && catGraphic!=null) {
				catGraphic.MyOnMouseDown ();
				ranGraphic.MyOnMouseDown ();

			}

		}*/
		Debug.Log ("base");
		base.OnMouseDown (go);
	}
}
