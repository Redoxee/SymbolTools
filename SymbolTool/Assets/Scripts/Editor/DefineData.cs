using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AntonMakesGames.Tools
{
    [Serializable]
    public class DefineEntity
    {
        public string Name;
        public List<BuildTargetGroup> Targets = new List<BuildTargetGroup>();
        public bool IsActiveOnTarget(BuildTargetGroup target)
        {
            return Targets.Contains(target);
        }
        public void SetValueOnTarget(BuildTargetGroup t, bool value)
        {
            if (value)
            {
                if (!Targets.Contains(t))
                    Targets.Add(t);
            }
            else
            {
                if (Targets.Contains(t))
                    Targets.Remove(t);
            }
        }

    }

    public class DefineData : ScriptableObject
    {
        public List<DefineEntity> Collection = new List<DefineEntity>();
        public BuildTargetGroup LastTargetSelected = BuildTargetGroup.Unknown;

        public bool Contains(string s)
        {
            return GetIndex(s) > -1;
        }

        private int GetIndex(string s)
        {
            return Collection.FindLastIndex(
                (DefineEntity e) =>
                {
                    return e.Name == s;
                }
            );
        }

        public DefineEntity Get(string s)
        {
            int index = GetIndex(s);
            if (index > -1)
                return Collection[index];
            return null;
        }

        public void Add(DefineEntity e)
        {
            if (!Contains(e.Name))
            {
                Collection.Add(e);
            }
            else
            {
                DefineEntity oldEntry = Get(e.Name);
                foreach (var target in e.Targets)
                {
                    oldEntry.SetValueOnTarget(target, true);
                }
            }
        }
        
    }
}
