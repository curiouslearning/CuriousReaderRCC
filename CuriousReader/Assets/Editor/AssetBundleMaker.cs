using System.Linq;
using System.Collections.Generic;
using UnityEditor;

public static class AssetBundleMaker 
{
    
    private static readonly string m_strBundlePath = "Assets/StreamingAssets/AssetBundles/"; 

    /// <summary>
    /// Builds all the asset bundles
    /// </summary>
    public static void BuildAllAssetBundles() 
    {
        createAssetBundlesDirectoryIfNotPresent();
        BuildPipeline.BuildAssetBundles(m_strBundlePath, BuildAssetBundleOptions.None, BuildTarget.Android);
    }

    /// <summary>
    /// Builds a specific asset bundle with a name
    /// </summary>
    /// <param name="i_strBundleName">Name of the bundle</param>
    public static void BuildAssetBundle(string i_strBundleName)
    {
        createAssetBundlesDirectoryIfNotPresent();
        List<string> allAssets = AssetDatabase.GetAllAssetPaths().ToList();
        List<string> bundleAssetPaths = allAssets.FindAll((assetPath) =>
        {
            return AssetDatabase.GetImplicitAssetBundleName(assetPath) == i_strBundleName;
        }).ToList();

        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        buildMap[0].assetBundleName = i_strBundleName;
        buildMap[0].assetNames = bundleAssetPaths.ToArray();
        BuildPipeline.BuildAssetBundles(m_strBundlePath, buildMap, BuildAssetBundleOptions.None, BuildTarget.Android);
    }

    /// <summary>
    /// Creates the asset bundles directory if it doesn't exist
    /// </summary>
    private static void createAssetBundlesDirectoryIfNotPresent()
    {
        IOHelper.CreateDirectoryIfNotPresent(m_strBundlePath);
    }

}