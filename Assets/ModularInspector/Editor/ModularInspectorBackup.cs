#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ModularInspectorBackup
{
    public static string CreateBackup(IReadOnlyList<string> assetPaths)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;

        string backupRoot = Path.Combine(projectRoot, "ModularInspectorBackups", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));

        foreach (string assetPath in assetPaths)
        {
            string source = Path.Combine(projectRoot, assetPath);
            string destination = Path.Combine(backupRoot, assetPath);

            string directory = Path.GetDirectoryName(destination);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(source, destination, true);
        }

        return backupRoot;
    }

    public static void RestoreBackup(string backupPath)
    {
        if (!Directory.Exists(backupPath))
        {
            throw new DirectoryNotFoundException(backupPath);
        }

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;

        string[] files = Directory.GetFiles(backupPath, "*.cs", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            string relative = file.Substring(backupPath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string destination = Path.Combine(projectRoot, relative);

            string directory = Path.GetDirectoryName(destination);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(file, destination, true);
        }
    }
}

#endif