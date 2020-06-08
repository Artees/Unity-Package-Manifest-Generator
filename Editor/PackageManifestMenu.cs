using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Artees.UnityPackageManifestGenerator.Editor
{
    internal static class PackageManifestMenu
    {
        [MenuItem("Assets/Create/Artees/Package Manifest")]
        private static void Create(MenuCommand menuCommand)
        {
            var selected = Selection.GetFiltered<Object>(SelectionMode.Assets).FirstOrDefault();
            var selectedPath = selected == null ? "Assets" : AssetDatabase.GetAssetPath(selected);
            var directoryPath = File.Exists(selectedPath)
                ? Path.GetDirectoryName(selectedPath)
                : selectedPath;
            if (directoryPath == null) return;
            var packagePath = directoryPath + "/package.jsonpackage";
            File.WriteAllText(packagePath, "{}");
            AssetDatabase.ImportAsset(packagePath);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(packagePath);
        }
    }
}