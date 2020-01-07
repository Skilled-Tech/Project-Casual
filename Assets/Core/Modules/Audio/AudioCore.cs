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

using UnityEngine.Audio;

namespace Game
{
	public class AudioCore : Core.Module
	{
        [SerializeField]
        protected AudioMixer mixer;
        public AudioMixer Mixer { get { return mixer; } }

        public AudioChannelsCore Channels { get; protected set; }

        public AudioPlayersCore Players { get; protected set; }

        public class Module : Core.Module<AudioCore>
        {
            public AudioCore Audio => Reference;
        }

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            Channels = this.GetDependancy<AudioChannelsCore>();

            Players = this.GetDependancy<AudioPlayersCore>();

            References.Configure(this);
        }

        public override void Init()
        {
            base.Init();

            References.Init(this);
        }
    }
}