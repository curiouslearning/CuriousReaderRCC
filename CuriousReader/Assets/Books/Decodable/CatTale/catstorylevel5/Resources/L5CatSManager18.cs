using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class L5CatSManager18 : GSManager {


	public override void OnMouseDown(GameObject go)
	{
		if (go.name =="Jam"|| go.name == "Text_jam.") 
		{   
			GTinkerGraphic cat = GameObject.Find ("Cat").GetComponent<GTinkerGraphic>();
			if (cat != null)
			{   
				cat.MyOnMouseDown();

			}
		}
		base.OnMouseDown(go);

	}
}
