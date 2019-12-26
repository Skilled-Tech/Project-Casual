﻿using System;
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
	public class Initializer : MonoBehaviour
	{
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnLoad()
        {
            SceneManager.sceneLoaded += SceneLoadCallback;
        }

        static void SceneLoadCallback(Scene scene, LoadSceneMode mode)
        {
            var roots = scene.GetRootGameObjects();

            var targets = new List<IInitialize>();

            for (int i = 0; i < roots.Length; i++)
                targets.AddRange(Dependancy.GetAll<IInitialize>(roots[i]));

            Perform(targets);
        }

        static void Perform(IList<IInitialize> list)
        {
            for (int i = 0; i < list.Count; i++)
                list[i].Configure();

            for (int i = 0; i < list.Count; i++)
                list[i].Init();
        }

        public static void Perform(IInitialize target)
        {
            target.Configure();

            target.Init();
        }

        public static void Perform(GameObject gameObject)
        {
            var targets = Dependancy.GetAll<IInitialize>(gameObject);

            Perform(targets);
        }
    }

    public interface IInitialize
    {
        void Configure();

        void Init();
    }
}