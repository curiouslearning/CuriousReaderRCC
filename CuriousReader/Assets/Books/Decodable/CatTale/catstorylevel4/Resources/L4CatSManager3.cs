using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class L4CatSManager3 : GSManager {
	

	public override void OnMouseDown(GameObject go)
	{
		if (go.name == "Cat") {
			GTinkerGraphic tinkerGraphic = GameObject.Find ("Ran").GetComponent<GTinkerGraphic> ();
			if (tinkerGraphic != null) {
//				tinkerGraphic.PlayCompleteAnim ();
			}
		} else if (go.name == "Ran") {
			GTinkerGraphic tinkerGraphic = GameObject.Find ("Cat").GetComponent<GTinkerGraphic> ();
			if (tinkerGraphic != null) {
//				tinkerGraphic.PlayCompleteAnim ();
			} 
		} else if (go.name == "Text_cat") {
			GTinkerGraphic tinkerGraphic = GameObject.Find ("Ran").GetComponent<GTinkerGraphic> ();
			if (tinkerGraphic != null) {
//				tinkerGraphic.gameObject.transform.position = new Vector3 (tinkerGraphic.dataTinkerGraphic.posX, tinkerGraphic.dataTinkerGraphic.posY, 0);
//				LoadAssetFromJSON.LoadAssetImage (tinkerGraphic, tinkerGraphic.dataTinkerGraphic.imageName);
			}
			
		}
		else if (go.name == "Text_man") {
			GTinkerGraphic tinkerGraphic = GameObject.Find ("Cat").GetComponent<GTinkerGraphic> ();
			if (tinkerGraphic != null) {
//				tinkerGraphic.gameObject.transform.position = new Vector3 (tinkerGraphic.dataTinkerGraphic.posX, tinkerGraphic.dataTinkerGraphic.posY, 0);
//				LoadAssetFromJSON.LoadAssetImage (tinkerGraphic, tinkerGraphic.dataTinkerGraphic.imageName);
			}

		}

        base.OnMouseDown (go);
	}
}