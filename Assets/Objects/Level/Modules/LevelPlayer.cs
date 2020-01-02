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
	public class LevelPlayer : Level.Module
	{
		[SerializeField]
        protected GameObject prefab;
        public GameObject Prefab { get { return prefab; } }

        public Player Instance { get; protected set; }

        public virtual void Spawn()
        {
            var gameObject = Instantiate(prefab);

            gameObject.name = prefab.name;

            Initializer.Perform(gameObject);

            Instance = gameObject.GetComponent<Player>();
        }
    }
}