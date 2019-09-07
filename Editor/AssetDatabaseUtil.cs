using UnityEditor;

namespace Artees.UnityPackageManifestGenerator.Editor
{
    internal static class AssetDatabaseUtil
    {
        public static void CreateFolderRecursively(string filePath)
        {
            var folders = filePath.Split('/');
            for (var i = 0; i < folders.Length - 1; i++)
            {
                var folderName = folders[i];
                var folderPath = string.Join("/", folders, 0, i + 1);
                if (AssetDatabase.IsValidFolder(folderPath)) continue;
                var parentFolder = string.Join("/", folders, 0, i);
                AssetDatabase.CreateFolder(parentFolder, folderName);
            }
        }
    }
}