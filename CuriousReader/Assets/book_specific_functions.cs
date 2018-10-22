using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class book_specific_functions : MonoBehaviour {
	static public Color blue = new Color(246.0f,233.0f,255.0f,74.0f);
	SpriteRenderer m_spriterenderer;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public virtual void OnMouseDown(GameObject go)
	{   Debug.Log (go);
		if (go.name == "blue") {
			Debug.Log (go);
			var changecolour = GameObject.Find ("cat");
			m_spriterenderer=changecolour.GetComponent<SpriteRenderer>();
			m_spriterenderer.color = blue;
		}
	}
}
