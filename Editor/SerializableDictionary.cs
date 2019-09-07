using System;
using System.Collections.Generic;
using UnityEngine;

namespace Artees.UnityPackageManifestGenerator.Editor
{
    [Serializable]
    internal abstract class SerializableDictionary<TPair, TKey, TValue> :
        Dictionary<TKey, TValue>, ISerializationCallbackReceiver
        where TPair : SerializableKeyValuePair<TKey, TValue>, new()
    {
        [SerializeField] public List<TPair> keyValuePairs = new List<TPair>();

        public void OnBeforeSerialize()
        {
            keyValuePairs.Clear();
            foreach (var keyValuePair in this)
            {
                keyValuePairs.Add(new TPair {key = keyValuePair.Key, value = keyValuePair.Value});
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            foreach (var keyValuePair in keyValuePairs)
            {
                this[keyValuePair.key] = keyValuePair.value;
            }
        }
    }

    [Serializable]
    internal abstract class SerializableKeyValuePair<TKey, TValue>
    {
        [SerializeField] public TKey key;
        [SerializeField] public TValue value;
    }
}