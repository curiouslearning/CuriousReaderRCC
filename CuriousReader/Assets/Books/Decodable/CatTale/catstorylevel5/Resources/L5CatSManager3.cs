using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class L5CatSManager3 : GSManager {
    GTinkerGraphic ranman;
    GTinkerGraphic cat;
    public override void Start()
    {
        ranman= GameObject.Find("Ran").GetComponent<GTinkerGraphic>();
        cat= GameObject.Find("Cat").GetComponent<GTinkerGraphic>();
        base.Start();
    }
 
    public override void OnMouseDown(GameObject go)
	{
		if (go.name == "Cat") {
            ranman.MyOnMouseDown();
		} else if (go.name == "Ran") {
            cat.MyOnMouseDown();

		} else if (go.name == "Text_ran") {
            ranman.MyOnMouseDown();
            cat.MyOnMouseDown();

		}
        else if(go.name=="Text_cat")
        {
//            ranman.reset();
        }
        else if(go.name=="Text_ran")
        {
//            cat.reset();
        }
		base.OnMouseDown (go);
	}
}