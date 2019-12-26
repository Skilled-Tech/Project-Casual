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
	public class AudioChannelSlider : MonoBehaviour
	{
        [SerializeField]
        AudioMixerGroup mixerGroup = null;

		public Slider Slider { get; protected set; }

        public AudioChannelsCore Channels => Core.Instance.Audio.Channels;

        public AudioChannelModule Channel { get; protected set; }

        protected virtual void Awake()
        {
            Channel = Channels.Find(mixerGroup);

            if(Channel == null)
            {
                Debug.LogError("Cannot find channel with mixer group: " + mixerGroup.name + ", disabling " + nameof(AudioChannelSlider) + ": " + name);
                enabled = false;
            }

            Slider = GetComponent<Slider>();
        }

        protected virtual void Start()
        {
            Slider.minValue = 0f;
            Slider.maxValue = 1f;
            Slider.value = Channel.Volume;

            Slider.onValueChanged.AddListener(ValueChange);
        }

        protected virtual void ValueChange(float value)
        {
            Channel.Volume = value;
        }
    }
}