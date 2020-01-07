using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Game
{
    [Serializable]
    public class TextValidator
    {
        public delegate bool Delegate(string input);

        public List<Delegate> List { get; protected set; }

        public virtual void Add(Delegate validator)
        {
            List.Add(validator);
        }
        public virtual void Remove(Delegate validator)
        {
            List.Remove(validator);
        }

        public virtual bool Check(string input)
        {
            for (int i = 0; i < List.Count; i++)
                if (List[i](input) == false)
                    return false;

            return true;
        }
        
        public TextValidator()
        {
            List = new List<Delegate>();
        }

        public static class Defaults
        {
            public static bool NotEmpty(string input) => !string.IsNullOrEmpty(input);
        }
    }
}