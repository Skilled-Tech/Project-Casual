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
	public class AudioChannelModule : AudioChannelsCore.Module
	{
		[SerializeField]
        protected AudioMixerGroup mixerGroup;
        public AudioMixerGroup MixerGroup { get { return mixerGroup; } }

        public AudioMixer Mixer => mixerGroup.audioMixer;

        public float Volume
        {
            get
            {
                if(Mixer.GetFloat(VolumeID, out var result))
                {
                    return GameTools.Audio.DecibelToLinear(result);
                }
                else
                {
                    Debug.LogError("Cannot read volume parameter with name: " + VolumeID + ", please ensure it exists");
                    return 0f;
                }
            }
            set
            {
                value = Mathf.Clamp01(value);

                if(Mixer.SetFloat(VolumeID, GameTools.Audio.LinearToDecibel(value)))
                {
                    PlayerPrefs.SetFloat(VolumeID, value);
                }
                else
                {
                    Debug.LogError("Cannot set volume parameter with name: " + VolumeID + ", please ensure it exists");
                }
            }
        }

        public string VolumeID => mixerGroup.name + " " + "Volume";

        public override void Configure(AudioChannelsCore reference)
        {
            base.Configure(reference);

            StartCoroutine(Procedure());

            IEnumerator Procedure()
            {
                yield return new WaitForEndOfFrame();

                if (PlayerPrefs.HasKey(VolumeID))
                    Volume = PlayerPrefs.GetFloat(VolumeID, Volume);
            }
        }
    }
}