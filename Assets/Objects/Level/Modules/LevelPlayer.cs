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

        public Transform SpawnPoint => transform;

        public override void Init()
        {
            base.Init();

            Instance = FindObjectOfType<Player>();

            if (Instance == null) Instance = Spawn();
        }

        public virtual Player Spawn()
        {
            var gameObject = Instantiate(prefab, SpawnPoint.position, SpawnPoint.rotation);

            gameObject.name = prefab.name;

            Initializer.Perform(gameObject);

            var instannce = gameObject.GetComponent<Player>();

            return instannce;
        }
    }
}