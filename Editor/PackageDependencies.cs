using System;

namespace Artees.UnityPackageManifestGenerator.Editor
{
    /// <summary>
    /// A map of package dependencies. Keys are package names, and values are specific versions. They indicate other
    /// packages that this package depends on. The Package Manager does not support range syntax, only SemVer versions.
    /// </summary>
    /// <seealso cref="PackageManifest"/>
    [Serializable]
    internal class PackageDependencies : SerializableDictionaryString
    {
    }
}