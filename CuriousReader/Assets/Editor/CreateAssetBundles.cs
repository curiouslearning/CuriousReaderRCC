using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class CreateAssetBundles {

	[MenuItem("Curious Reader/Build AssetBundles")]
	static void BuildAllAssetBundles()
	{
		Debug.Log("Building all Asset Bundles!");
		DateTimeTimer timer = new DateTimeTimer();
		timer.Start();
		AssetBundleMaker.BuildAllAssetBundles();
		Debug.Log("Building all Asset Bundles finished in: " + timer.GetElapsedTime() + " minutes.");
	}

}