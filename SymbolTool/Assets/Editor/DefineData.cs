using System;
using System.Collections.Generic;
using UnityEngine;

namespace AntonMakesGames.Tools
{
    [Serializable]
    public class DefineEntity
    {
        public string Name;
        public bool IsActive;
    }

    public class DefineData : ScriptableObject
    {
        public List<DefineEntity> Collection = new List<DefineEntity>();
        public bool Contains(string s)
        {
            return Collection.FindLastIndex(
                (DefineEntity e) =>
                {
                    return e.Name == s;
                }
            ) > -1;
        }
    }
}
