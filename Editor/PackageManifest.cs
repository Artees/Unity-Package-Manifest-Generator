using System;
using Artees.UnitySemVer;
using UnityEngine;

namespace Artees.UnityPackageManifestGenerator.Editor
{
    /// <summary>
    /// Represents the Unity's package manifest file (package.json).
    /// See <a href="https://docs.unity3d.com/Manual/upm-manifestPkg.html">Unity User Manual</a> for more information. 
    /// </summary>
    [Serializable]
    internal class PackageManifest
    {
        /// <summary>
        /// The officially registered package name. This name must conform to the Unity Package Manager naming
        /// convention, which uses reverse domain name notation. This is a unique identifier, not the user-friendly
        /// name that appears in the list view on the Package Manager window.
        /// </summary>
        /// <example>com.company-name.package-name</example>
        [SerializeField] public string name;

        /// <summary>
        /// The package version number. This value must respect semantic versioning. Following SemVer allows the
        /// Package Manager to automatically solve conflicts (when possible), or upgrade packages
        /// to newer, backward-compatible versions.
        /// </summary>
        /// <example>3.2.1</example>
        [SerializeField, SemVer] public string version;

        /// <summary>
        /// A user-friendly name to appear in the Unity Editor (for example, in the Project Browser, the Package
        /// Manager window, etc.).
        /// </summary>
        [SerializeField] public string displayName;

        /// <summary>
        /// A brief description of the package. This is the text that appears in the details view of the Packages
        /// window. Any UTF–8 character code is supported.
        /// </summary>
        [SerializeField] public string description;

        /// <summary>
        /// Indicates the lowest Unity version the package is compatible with. If omitted, the package is considered
        /// compatible with all Unity versions. The expected format is “MAJOR.MINOR”. To point to a specific patch,
        /// use the <see cref="unityRelease"/> attribute as well. A package that is not compatible with Unity will not
        /// appear in the Packages window.
        /// </summary>
        /// <example>2018.3</example>
        [SerializeField] public string unity;

        /// <summary>
        /// Part of a Unity version indicating the specific release of Unity that the package is compatible with. You
        /// can use this attribute when an updated package requires changes made during the Unity alpha/beta
        /// development cycle (for example, if it needs newly introduced APIs, or uses existing APIs that changed in a
        /// non-backward-compatible way without API Updater rules). If you omit the unity attribute, this attribute
        /// has no effect. A package that is not compatible with Unity does not appear in the Packages window.
        /// </summary>
        /// <example>0b4</example>
        [SerializeField] public string unityRelease;

        /// <summary>
        /// A map of package dependencies. Keys are package names, and values are specific versions. They indicate
        /// other packages that this package depends on. The Package Manager does not support range syntax, only
        /// SemVer versions.
        /// </summary>
        [SerializeField] public PackageDependencies dependencies;

        /// <summary>
        /// An array of keywords used by the Package Manager search APIs. This helps users find relevant packages.
        /// </summary>
        [SerializeField] public string[] keywords;

        /// <summary>
        /// Author of the package.
        /// </summary>
        [SerializeField] public PackageAuthor author;
    }
}