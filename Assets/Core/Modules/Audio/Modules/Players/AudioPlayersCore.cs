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
	public class AudioPlayersCore : AudioCore.Module
	{
        public SFXPlayer SFX { get; protected set; }

        [RequireComponent(typeof(AudioSource))]
        public class Element : Module
        {
            public AudioSource Source { get; protected set; }

            public override void Configure(AudioPlayersCore reference)
            {
                base.Configure(reference);

                Source = GetComponent<AudioSource>();
            }
        }

        public class Module : Core.Module<AudioPlayersCore>
        {
            public AudioPlayersCore Players => Reference;
        }

        public override void Configure(AudioCore reference)
        {
            base.Configure(reference);

            SFX = this.GetDependancy<SFXPlayer>();

            References.Configure(this);
        }

        public override void Init()
        {
            base.Init();

            References.Init(this);
        }
    }
}