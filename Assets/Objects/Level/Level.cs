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
    [DefaultExecutionOrder(ExecutionOrder)]
	public class Level : MonoBehaviour
	{
        public const int ExecutionOrder = -200;

		public static Level Instance { get; protected set; }

        public LevelPause Pause { get; protected set; }
        public LevelMenu Menu { get; protected set; }

        public class Module : MonoBehaviour, IReference<Level>
        {
            public Level Level { get; protected set; }
            public virtual void Configure(Level reference)
            {
                Level = reference;
            }

            public virtual void Init()
            {
                
            }
        }

        public Core Core => Core.Instance;

        protected virtual void Awake()
        {
            Instance = this;

            Pause = this.GetDependancy<LevelPause>();
            Menu = FindObjectOfType<LevelMenu>();

            References.Configure(this);
            References.Configure(this, Menu);
        }

        protected virtual void Start()
        {
            References.Init(this);
            References.Init(this, Menu);

            Core.Ads.Placements.Common.RewardedVideo.Show();
        }

        public virtual void Retry()
        {
            Core.Scenes.Load(gameObject.scene.name);
        }

        public virtual void Quit()
        {
            Core.Scenes.Load(Core.Scenes.MainMenu);
        }
    }
}