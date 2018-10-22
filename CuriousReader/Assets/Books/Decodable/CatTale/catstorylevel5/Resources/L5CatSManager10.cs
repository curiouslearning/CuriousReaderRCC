using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class L5CatSManager10 : GSManager {
	GameObject fat;

	public override void OnMouseDown(GameObject go)
	{
		if (go.name == "Fat"|| go.name == "Text_fat?") 
		{  if (fat == null) {
				fat = GameObject.Find ("Fat");
			}
			fat.SetActive (false);
			GameObject seqAnim = GameObject.Find ("FatCatRun");
			GTinkerGraphic fatCat = seqAnim.GetComponent<GTinkerGraphic>();
			if (fatCat != null)
			{    
				fatCat.MyOnMouseDown();
			}
		}

		base.OnMouseDown(go);
	}
}
