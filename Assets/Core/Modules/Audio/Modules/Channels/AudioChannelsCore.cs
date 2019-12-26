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
	public class AudioChannelsCore : AudioCore.Module
	{
        public List<AudioChannelModule> List { get; protected set; }

        public int Count => List.Count;
        public AudioChannelModule this[int index] => List[index];

		public class Module : Core.Module<AudioChannelsCore>
        {
            public AudioChannelsCore Channels => Reference;
        }

        public override void Configure(AudioCore reference)
        {
            List = this.GetAllDependancies<AudioChannelModule>();

            base.Configure(reference);

            References.Configure(this);
        }

        public override void Init()
        {
            base.Init();

            References.Init(this);
        }

        public AudioChannelModule Find(AudioMixerGroup mixerGroup)
        {
            for (int i = 0; i < Count; i++)
                if (this[i].MixerGroup == mixerGroup)
                    return this[i];

            return null;
        }
    }
}