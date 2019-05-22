using System.IO;

public static class IOHelper
{

    /// <summary>
    /// Creates a directory if it doesn't exist
    /// </summary>
    /// <param name="i_strPath">Directory path</param>
    public static void CreateDirectoryIfNotPresent(string i_strPath)
    {
        if (!Directory.Exists(i_strPath))
        {
            Directory.CreateDirectory(i_strPath);
        }
    }

}