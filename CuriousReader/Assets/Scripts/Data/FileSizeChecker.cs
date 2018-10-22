using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileSizeChecker : MonoBehaviour {

	long size10MB =10485760;   //10 MB size for the 
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void CheckAndReduceFileSize(){
		if(DataCollection.CheckSize() >= size10MB){
		    //delete data from DataCollection.

		}
	}
}
