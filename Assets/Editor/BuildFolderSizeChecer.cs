using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildFolderSizeChecker
{
    [MenuItem("Tools/Check WebGL Build Folder Size")]
    public static void CheckBuildFolderSize()
    {
        string path = "Builds/WebGL/Build";
        if (!Directory.Exists(path))
        {
            Debug.LogWarning($"No se encontró la carpeta: {path}");
            return;
        }

        string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        double totalSize = 0;

        Debug.Log("===== WebGL Build Files =====");
        foreach (string file in files)
        {
            FileInfo info = new FileInfo(file);
            double sizeMB = info.Length / (1024.0 * 1024.0);
            totalSize += sizeMB;
            Debug.Log($"{Path.GetFileName(file)} : {sizeMB:F2} MB");
        }

        Debug.Log($"=============================\nTOTAL: {totalSize:F2} MB");
    }
}
