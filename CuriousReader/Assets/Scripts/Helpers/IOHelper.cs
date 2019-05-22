using System.IO;

public static class IOHelper
{

    public static void CreateDirectoryIfNotPresent(string i_strPath)
    {
        if (!Directory.Exists(i_strPath))
        {
            Directory.CreateDirectory(i_strPath);
        }
    }

}