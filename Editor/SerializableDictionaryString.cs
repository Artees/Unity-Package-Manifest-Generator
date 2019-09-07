using System;

namespace Artees.UnityPackageManifestGenerator.Editor
{
    [Serializable]
    internal class SerializableDictionaryString : SerializableDictionary<SerializableKeyValuePairString, string, string>
    {
    }

    [Serializable]
    internal class SerializableKeyValuePairString : SerializableKeyValuePair<string, string>
    {
    }
}