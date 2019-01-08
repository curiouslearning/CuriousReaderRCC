using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateAssetBundles {

	[MenuItem("Curious Reader/Build AssetBundles")]
	static void BuildAllAssetBundles()
	{
		string assetBundleDirectory = "Assets/StreamingAssets/AssetBundles/";
		if(!Directory.Exists(assetBundleDirectory))
		{
			Directory.CreateDirectory(assetBundleDirectory);
		}
		BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.Android);
	}
}