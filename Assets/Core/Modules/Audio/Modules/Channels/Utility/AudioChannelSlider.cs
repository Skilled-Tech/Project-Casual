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
    [RequireComponent(typeof(Slider))]
	public class AudioChannelSlider : MonoBehaviour, IInitialize
	{
        [SerializeField]
        AudioMixerGroup mixerGroup = null;

		public Slider Slider { get; protected set; }

        public AudioChannelModule Channel { get; protected set; }

        public Core Core => Core.Instance;

        public void Configure()
        {
            Channel = Core.Audio.Channels.Find(mixerGroup);

            if (Channel == null)
            {
                Debug.LogError("Cannot find channel with mixer group: " + mixerGroup.name + ", disabling " + nameof(AudioChannelSlider) + ": " + name);
                enabled = false;
                return;
            }

            Slider = GetComponent<Slider>();
            Slider.minValue = 0f;
            Slider.maxValue = 1f;
            Slider.value = Channel.Volume;
            Slider.onValueChanged.AddListener(ValueChange);

            Channel.OnVolumeChange += VolumeChangeCallback;
        }

        public void Init()
        {

        }

        protected virtual void ValueChange(float value)
        {
            Channel.Volume = value;
        }

        private void VolumeChangeCallback(float volume)
        {
            if (Slider.value != volume) Slider.value = volume;
        }
    }
}