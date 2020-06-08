using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR_WIN
using Object = UnityEngine.Object;

#endif

namespace Artees.UnityPackageManifestGenerator.Editor
{
    [CustomEditor(typeof(PackageImporter))]
    internal class PackageImporterEditor : ScriptedImporterEditor
    {
        private const string JsonExtension = "json";
        private const string FirstControlName = "FirstControlName";

        protected override void Apply()
        {
            base.Apply();
            Target.Export();
            EditorUtility.SetDirty(Target);
            if (Target.AutoExportJson) ExportJsonAs(DefaultAbsoluteJsonPath);
            GUI.FocusControl(FirstControlName);
        }

        private PackageImporter Target => (PackageImporter) target;

        public override void OnInspectorGUI()
        {
            GUI.SetNextControlName(FirstControlName);
            base.OnInspectorGUI();
            CreateImportExportGui();
            CreatePublishGui();
            CreateAsmdefWarning();
        }

        private void CreateImportExportGui()
        {
            CreateSpace();
            GUILayout.BeginHorizontal();
            var autoExportContent = new GUIContent("Auto Export",
                "Automatically export the package manifest file (package.json) on apply");
            Target.AutoExportJson = GUILayout.Toggle(Target.AutoExportJson, autoExportContent);
            if (CreateButton("Import package.json...", "Import the package manifest file")) ImportJson();
            if (CreateButton("Export package.json...", "Export the package manifest file")) ExportJsonAs();
            GUILayout.EndHorizontal();
        }

        private static void CreateSpace()
        {
            GUILayout.Space(16f);
        }

        private static bool CreateButton(string text, string tooltip, params GUILayoutOption[] options)
        {
            var buttonName = GUID.Generate().ToString();
            GUI.SetNextControlName(buttonName);
            var content = new GUIContent(text, tooltip);
            var isClicked = GUILayout.Button(content, options);
            if (isClicked) GUI.FocusControl(buttonName);
            return isClicked;
        }

        private void ImportJson()
        {
            var directory = Directory.GetParent(DefaultAbsoluteJsonPath).FullName;
            var filters = new[] {JsonExtension.ToUpperInvariant(), JsonExtension};
            var absolutePath = EditorUtility.OpenFilePanelWithFilters("Open Package Manifest", directory, filters);
            if (string.IsNullOrEmpty(absolutePath)) return;
            Target.Import(absolutePath);
        }

        private void ExportJsonAs()
        {
            var directory = Directory.GetParent(DefaultAbsoluteJsonPath).FullName;
            var defaultName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this));
            var absolutePath = EditorUtility.SaveFilePanel("Export JSON", directory, defaultName, JsonExtension);
            ExportJsonAs(absolutePath);
        }

        private void ExportJsonAs(string absolutePath)
        {
            if (string.IsNullOrEmpty(absolutePath)) return;
            var relativePath = GetRelativePath(absolutePath);
            AssetDatabase.CopyAsset(Target.assetPath, relativePath);
        }

        private string DefaultAbsoluteJsonPath
        {
            get
            {
                var assetPath = AssetDatabase.GetAssetPath(target);
                var relativeJsonPath = Path.ChangeExtension(assetPath, JsonExtension);
                if (string.IsNullOrEmpty(relativeJsonPath))
                {
                    Debug.LogError($"Invalid asset path: {assetPath}", this);
                    return string.Empty;
                }

                return Path.Combine(ProjectPath, relativeJsonPath);
            }
        }

        private static string ProjectPath => Directory.GetParent(Application.dataPath).FullName;

        private static string GetRelativePath(string absolutePath)
        {
            var projectPath = ReplaceSeparators(ProjectPath);
            return ReplaceSeparators(absolutePath).Replace(projectPath, "").TrimStart('/');
        }

        private static string ReplaceSeparators(string path)
        {
            return path.Replace(Path.PathSeparator, '/').Replace('\\', '/');
        }

        private void CreatePublishGui()
        {
            CreateSpace();
            GUILayout.BeginHorizontal();
            var registry = Target.RegistryUrl;
            var width = GUILayout.Width(80f);
            if (CreateButton("Publish To", "Publish the package to the registry", width))
            {
                Publish(registry);
            }

            var newRegistry = GUILayout.TextField(registry);
            Target.RegistryUrl = newRegistry;
            GUILayout.EndHorizontal();
        }

        private void Publish(string registry)
        {
            var directory = Directory.GetParent(DefaultAbsoluteJsonPath).FullName;
#if UNITY_EDITOR_WIN
            const string publishScriptName = "PublishWin";
            const string packageFolder = "Assets/Artees/UnityPackageManifestGenerator/Editor/";
            const string publishScriptPath = packageFolder + publishScriptName + ".bat";
            CopyPublishScript(publishScriptPath, publishScriptName);
            var publishScriptFullPath = Application.dataPath + publishScriptPath.Substring(6);
            var arguments = $"/K \"{publishScriptFullPath}\" {registry}";
            var startInfo = new ProcessStartInfo("cmd", arguments)
            {
                WorkingDirectory = directory
            };
            Process.Start(startInfo);
#else
            var startInfo = new ProcessStartInfo("npm", "publish --registry " + registry)
            {
                WorkingDirectory = directory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var process = Process.Start(startInfo);
            if (process == null) return;
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(output)) Debug.Log(output);
            if (!string.IsNullOrEmpty(error)) Debug.LogError(error);
#endif
        }

#if UNITY_EDITOR_WIN
        private static void CopyPublishScript(string publishScriptPath, string publishScriptName)
        {
            if (AssetDatabase.LoadAssetAtPath<Object>(publishScriptPath) != null) return;
            var publishScripts = AssetDatabase.FindAssets(publishScriptName);
            if (publishScripts.Length > 1)
            {
                Debug.LogError($"Found multiple {publishScriptName}");
            }

            AssetDatabaseUtil.CreateFolderRecursively(publishScriptPath);
            AssetDatabase.CopyAsset(AssetDatabase.GUIDToAssetPath(publishScripts[0]), publishScriptPath);
        }
#endif

        private void CreateAsmdefWarning()
        {
            var assetPath = AssetDatabase.GetAssetPath(target);
            var assetFolder = Path.GetDirectoryName(assetPath);
            if (assetFolder == null) return;
            var af = assetFolder.Replace("\\", "/");
            if (AssetDatabase.GetAllAssetPaths().Any(s => s.EndsWith(".asmdef") && s.StartsWith(af))) return;
            CreateSpace();
            var message = "A package must have all its scripts within one or more assembly definition files. " +
                          $"Add an asmdef to {af}.";
            EditorGUILayout.HelpBox(message, MessageType.Warning);
        }
    }
}