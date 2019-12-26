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
    public class References
    {
        public static void Configure<TReference>(TReference reference)
            where TReference : Component
        {
            var targets = Dependancy.GetAll<IReference<TReference>>(reference.gameObject);

            Configure(reference, targets);
        }
        public static void Configure<TReference>(TReference reference, IList<IReference<TReference>> targets)
        {
            for (int i = 0; i < targets.Count; i++)
                Configure(reference, targets[i]);
        }
        public static void Configure<TReference>(TReference reference, IReference<TReference> target)
        {
            target.Configure(reference);
        }

        public static void Init<TReference>(TReference reference)
            where TReference : Component
        {
            var targets = Dependancy.GetAll<IReference<TReference>>(reference.gameObject);

            Init<TReference>(targets);
        }
        public static void Init<TReference>(IList<IReference<TReference>> targets)
        {
            for (int i = 0; i < targets.Count; i++)
                Init(targets[i]);
        }
        public static void Init<TReference>(IReference<TReference> target)
        {
            target.Init();
        }
    }

    public interface IReference<T>
    {
        void Configure(T reference);

        void Init();
    }
}