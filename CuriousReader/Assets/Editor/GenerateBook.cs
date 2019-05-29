using System.IO;
using UnityEditor;
using UnityEngine;

public class GenerateBook
{
    private static readonly string m_strNewBookParentDirectory  = "Assets/Books/Decodable/UbongoKids/";
    private static readonly string m_strNewBookDefaultTitle     = "NewBook";

    private static readonly string m_strBookInfosPath = "Assets/BookInfo/";
    private static readonly string m_strNewBookInfoDefaultName = "NewBook.asset";

    [MenuItem("Curious Reader/Generate Book")]
    public static void GenerateBookFilesAndBookScriptableObject()
    {
        generateBookDirectories();
        generateBookScriptableObject();

        // Refresh when we are done
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }

    private static void generateBookDirectories()
    {
        // Create book directory itself
        string newBookPath = Path.Combine(m_strNewBookParentDirectory, m_strNewBookDefaultTitle);
        IOHelper.CreateDirectoryIfNotPresent(newBookPath);

        // Create Common/Objects directory
        IOHelper.CreateDirectoryIfNotPresent(Path.Combine(newBookPath, "Common/Objects"));

        // Create Language Directories Based on ReaderLanguage enumeration
        for (ReaderLanguage i = ReaderLanguage.English; i < ReaderLanguage.Count; i++)
        {
            string languageDirectoryName = i.ToString();
            string languageDirectoryPath = Path.Combine(newBookPath, languageDirectoryName);
            // Create Language Directory
            IOHelper.CreateDirectoryIfNotPresent(languageDirectoryPath);
            IOHelper.CreateDirectoryIfNotPresent(Path.Combine(languageDirectoryPath, "Level2/Audio/Stanza"));
            IOHelper.CreateDirectoryIfNotPresent(Path.Combine(languageDirectoryPath, "Level2/Resources"));
            IOHelper.CreateDirectoryIfNotPresent(Path.Combine(languageDirectoryPath, "Level4/Audio/Stanza"));
            IOHelper.CreateDirectoryIfNotPresent(Path.Combine(languageDirectoryPath, "Level4/Resources"));
            IOHelper.CreateDirectoryIfNotPresent(Path.Combine(languageDirectoryPath, "Level6/Audio/Stanza"));
            IOHelper.CreateDirectoryIfNotPresent(Path.Combine(languageDirectoryPath, "Level6/Resources"));
            IOHelper.CreateDirectoryIfNotPresent(Path.Combine(languageDirectoryPath, "Words"));
        }
    }

    private static void generateBookScriptableObject()
    {
        BookInfo asset = ScriptableObject.CreateInstance<BookInfo>();

        IOHelper.CreateDirectoryIfNotPresent(m_strBookInfosPath);

        AssetDatabase.CreateAsset(asset, $"{m_strBookInfosPath}{m_strNewBookInfoDefaultName}");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
}