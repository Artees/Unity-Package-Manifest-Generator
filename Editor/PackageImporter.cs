using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Artees.UnitySemVer;
using LitJson;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Artees.UnityPackageManifestGenerator.Editor
{
    [ScriptedImporter(0, "jsonpackage")]
    internal class PackageImporter : ScriptedImporter
    {
#pragma warning disable 649
        [Tooltip("The officially registered package name. This name must conform to the Unity Package Manager naming " +
                 "convention, which uses reverse domain name notation. This is a unique identifier, not the " +
                 "user-friendly name that appears in the list view on the Package Manager window.")]
        [SerializeField]
        private new string name;

        [SerializeField, SemVer] private string version;

        [Tooltip("A user-friendly name to appear in the Unity Editor (for example, in the Project Browser, " +
                 "the Package Manager window, etc.).")]
        [SerializeField]
        private string displayName;

        [Tooltip("A brief description of the package. This is the text that appears in the details view of the " +
                 "Packages window. Any UTF–8 character code is supported.")]
        [SerializeField, TextArea]
        private string description;

        [Tooltip("Indicates the lowest Unity version the package is compatible with. If omitted, the package is " +
                 "considered compatible with all Unity versions. The expected format is “MAJOR.MINOR” (for example, " +
                 "2018.3). To point to a specific patch, use the unityRelease attribute  as well. A package that is " +
                 "not compatible with Unity will not appear in the Packages window.")]
        [SerializeField]
        private string unity;

        [Tooltip("Part of a Unity version indicating the specific release of Unity that the package is compatible " +
                 "with. You can use this attribute when an updated package requires changes made during the Unity " +
                 "alpha/beta development cycle (for example, if it needs newly introduced APIs, or uses existing " +
                 "APIs that changed in a non-backward-compatible way without API Updater rules). The expected format " +
                 "is “<UPDATE><RELEASE>” (for example, 0b4). If you omit the unity attribute, this attribute has no " +
                 "effect. A package that is not compatible with Unity does not appear in the Packages window.")]
        [SerializeField]
        private string unityRelease;

        [SerializeField] private PackageDependencies dependencies;

        [Tooltip("An array of keywords used by the Package Manager search APIs. " +
                 "This helps users find relevant packages.")]
        [SerializeField]
        private string[] keywords;

        [Tooltip("Author of the package.")] [SerializeField]
        private PackageAuthor author = new PackageAuthor();

        [SerializeField, HideInInspector] private bool autoExportJson = true;
        [SerializeField, HideInInspector] private bool overrideExportJsonPath = false;
        [SerializeField, HideInInspector] private string exportJsonPath = string.Empty;

        [SerializeField, HideInInspector] private string registryUrl = "https://registry.my-company.com";
#pragma warning restore 649

        [SuppressMessage("ReSharper", "ConvertToAutoProperty",
            Justification = "Serialized backing field")]
        public bool AutoExportJson
        {
            get => autoExportJson;
            set => autoExportJson = value;
        }

        [SuppressMessage("ReSharper", "ConvertToAutoProperty",
            Justification = "Serialized backing field")]
        public bool OverrideExportJsonPath
        {
            get => overrideExportJsonPath;
            set => overrideExportJsonPath = value;
        }

        [SuppressMessage("ReSharper", "ConvertToAutoProperty",
            Justification = "Serialized backing field")]
        public string AutoExportJsonPath
        {
            get => exportJsonPath;
            set => exportJsonPath = value;
        }

        [SuppressMessage("ReSharper", "ConvertToAutoProperty",
            Justification = "Serialized backing field")]
        public string RegistryUrl
        {
            get => registryUrl;
            set => registryUrl = value;
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            Import(ctx.assetPath);
        }

        public void Import(string absolutePath)
        {
            var json = File.ReadAllText(absolutePath);
            if (string.IsNullOrEmpty(json)) return;
            if (!json.StartsWith("%"))
            {
                try
                {
                    var package = JsonMapper.ToObject<PackageManifest>(json);
                    name = package.name;
                    version = package.version;
                    displayName = package.displayName;
                    description = package.description;
                    unity = package.unity;
                    unityRelease = package.unityRelease;
                    dependencies = package.dependencies;
                    keywords = package.keywords;
                    author = package.author;
                }
                catch (ArgumentException)
                {
                }
            }
        }

        public void Export()
        {
            Validate();
            var package = new PackageManifest
            {
                name = new DomainName(name).ToString(),
                version = version,
                displayName = displayName,
                description = description,
                unity = unity,
                unityRelease = unityRelease,
                dependencies = dependencies,
                keywords = keywords,
                author = author
            };
            var stringBuilder = new StringBuilder();
            var jsonWriter = new JsonWriter(stringBuilder) {PrettyPrint = true};
            JsonMapper.ToJson(package, jsonWriter);
            var json = stringBuilder.ToString();
            File.WriteAllText(assetPath, json);
        }

        private void Validate()
        {
            if (string.IsNullOrEmpty(name))
            {
                name = new DomainName(Application.productName, Application.companyName).Reverse();
            }

            const int maxNameLength = 50;
            if (name.Length > maxNameLength)
            {
                Debug.LogWarning($"The package name {name} is longer than {maxNameLength} characters. " +
                                 "It will not appear in the Editor.");
            }

            if (string.IsNullOrEmpty(displayName))
            {
                displayName = new DomainName(name).Subdomains.Last();
            }

            ValidateUnity();
            if (string.IsNullOrEmpty(author.name))
            {
                author.name = Application.companyName;
            }
        }

        private void ValidateUnity()
        {
            if (string.IsNullOrEmpty(unity))
            {
                unity = Application.unityVersion;
            }

            const char separator = '.';
            var unitySplitted = unity.Split(new[] {separator}, StringSplitOptions.RemoveEmptyEntries);
            var unityUints = new uint[2];
            if (unitySplitted.Length > 0)
            {
                uint.TryParse(unitySplitted[0], out unityUints[0]);
                if (unitySplitted.Length > 1)
                {
                    uint.TryParse(unitySplitted[1], out unityUints[1]);
                }
            }

            unity = string.Join(separator.ToString(), unityUints.Select(u => u.ToString()).ToArray());
        }
    }
}