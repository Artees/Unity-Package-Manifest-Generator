using System;
using System.Collections.Generic;
using UnityEngine;

namespace Artees.UnityPackageManifestGenerator.Editor
{
    /// <summary>
    /// Represents the Unity's project manifest file (manifest.json).
    /// See <a href="https://docs.unity3d.com/Manual/upm-manifestPrj.html">Unity User Manual</a> for more information. 
    /// </summary>
    [Serializable]
    internal class ProjectManifest
    {
#pragma warning disable 0649
        [SerializeField] public Dictionary<string, string> dependencies;
#pragma warning restore 0649
    }
}