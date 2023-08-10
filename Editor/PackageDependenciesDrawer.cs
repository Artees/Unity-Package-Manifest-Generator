using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LitJson;
using UnityEditor;
using UnityEngine;

namespace Artees.UnityPackageManifestGenerator.Editor
{
    [CustomPropertyDrawer(typeof(PackageDependencies))]
    internal class PackageDependenciesDrawer : PropertyDrawer
    {
        private static readonly Regex SemVerRegex = SemVerRegex =
            new Regex(
                @"(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label.tooltip = "A map of package dependencies. They indicate other packages that this package depends on.";
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);
            if (!property.isExpanded) return;
            var packagesDirectory =
                Directory.GetParent(Application.dataPath).GetDirectories("Packages").FirstOrDefault();
            var manifestFile = packagesDirectory?.GetFiles("manifest.json").FirstOrDefault();
            if (manifestFile == null) return;
            var manifestJson = File.ReadAllText(manifestFile.FullName);
            var manifest = JsonMapper.ToObject<ProjectManifest>(manifestJson);
            EditorGUI.indentLevel++;
            var targetObject = property.serializedObject.targetObject;
            if (!(fieldInfo.GetValue(targetObject) is PackageDependencies target)) return;
            var newTarget = new PackageDependencies();
            foreach (var (key, value) in manifest.dependencies)
            {
                CreateToggle(key, SemVerRegex.Match(value).Value, target, newTarget);
            }

            var targetPackage = Path.ChangeExtension(AssetDatabase.GetAssetPath(targetObject), "json");
            var packages = AssetDatabase.GetAllAssetPaths()
                .Where(s => s != targetPackage && s.StartsWith("Assets/") && s.EndsWith("/package.json"));
            foreach (var packagePath in packages)
            {
                var packageJson = File.ReadAllText(packagePath);
                var package = JsonMapper.ToObject<PackageManifest>(packageJson);
                CreateToggle(package.name, package.version, target, newTarget);
            }

            if (!target.SequenceEqual(newTarget))
            {
                fieldInfo.SetValue(targetObject, newTarget);
                newTarget.OnBeforeSerialize();
                var keyValuePairs = property.FindPropertyRelative("keyValuePairs");
                keyValuePairs.arraySize = newTarget.Count;
                for (var i = 0; i < newTarget.Count; i++)
                {
                    var arrayElement = keyValuePairs.GetArrayElementAtIndex(i);
                    var newArrayElement = newTarget.keyValuePairs[i];
                    arrayElement.FindPropertyRelative("key").stringValue = newArrayElement.key;
                    arrayElement.FindPropertyRelative("value").stringValue = newArrayElement.value;
                }
            }

            EditorGUI.indentLevel--;
        }

        private static void CreateToggle(string dependencyName, string dependencyVersion, PackageDependencies target,
            PackageDependencies newTarget)
        {
            if (dependencyName.StartsWith("com.unity.modules.")) return;
            EditorGUILayout.BeginHorizontal();
            var contains = target.ContainsKey(dependencyName);
            var newContains = EditorGUILayout.Toggle(contains, GUILayout.Width(30));
            if (newContains)
            {
                newTarget[dependencyName] = dependencyVersion;
            }

            var content = new GUIContent(dependencyName, $"\"{dependencyName}\" : \"{dependencyVersion}\"");
            EditorGUILayout.LabelField(content);
            EditorGUILayout.EndHorizontal();
        }
    }
}