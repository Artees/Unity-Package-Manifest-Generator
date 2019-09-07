using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Artees.UnityPackageManifestGenerator.Editor
{
    /// <summary>
    /// Author of the package.
    /// </summary>
    /// <seealso cref="PackageManifest"/>
    [Serializable]
    [SuppressMessage("ReSharper", "NotAccessedField.Global", Justification = "JSON")]
    internal class PackageAuthor
    {
#pragma warning disable 0649
        [SerializeField] public string name;
        [SerializeField] public string email;
        [SerializeField] public string url;
#pragma warning restore 0649
    }
}