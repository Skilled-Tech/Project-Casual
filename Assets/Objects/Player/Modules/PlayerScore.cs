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
	public class PlayerScore : Player.Module
	{
        [SerializeField]
        protected int _value;
        public int Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value < 0) value = 0;

                var change = value - this._value;

                this._value = value;

                OnValueChange?.Invoke(Value, change);
            }
        }

        public PlayerCore.StatisticsProperty Statistics => Core.Player.Statistics;

        public delegate void ValueChangeDelegate(int value, int change);
        public event ValueChangeDelegate OnValueChange;

        [SerializeField]
        protected AudioClip sfx;
        public AudioClip SFX { get { return sfx; } }

        public override void Configure(Player reference)
        {
            base.Configure(reference);

            OnValueChange += ValueModifiedCallback;
        }

        private void ValueModifiedCallback(int value, int change)
        {
            Core.Audio.Players.SFX.Play(sfx);
        }

        public virtual void UpdateStatistic()
        {
            if(Value > Statistics.HighScore.Value)
            {
                Statistics.HighScore.Value = Value;
                Statistics.HighScore.Upload();
            }
        }

        public virtual void Roundup()
        {
            Statistics.Score.Value += Value;
            Statistics.Score.Upload();
        }
    }
}