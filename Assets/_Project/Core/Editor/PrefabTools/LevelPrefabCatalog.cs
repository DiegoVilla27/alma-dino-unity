using System;
using System.Collections.Generic;
using UnityEngine;

namespace AlmaDino.Core.Editor
{
    public sealed class LevelPrefabCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public string Signature;
            public GameObject Prefab;
            public string SourceScene;
            public string SourceObject;
        }

        public List<Entry> Pieces = new();
        public List<GameObject> Assemblies = new();
    }
}
