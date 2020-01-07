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
        public const string ID = "Score";

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

                var change = value - _value;

                this._value = value;

                OnValueChange?.Invoke(Value);
                OnValueModified?.Invoke(Value, change);
            }
        }

        public delegate void ValueModifiedDelegate(int value, int change);
        public event ValueModifiedDelegate OnValueModified;

        public delegate void ValueChangeDelegate(int value);
        public event ValueChangeDelegate OnValueChange;

        [SerializeField]
        protected AudioClip sfx;
        public AudioClip SFX { get { return sfx; } }

        public override void Configure(Player reference)
        {
            base.Configure(reference);

            OnValueModified += ValueModifiedCallback;
        }

        private void ValueModifiedCallback(int value, int change)
        {
            Core.Audio.Players.SFX.Play(sfx);
        }

        public virtual void UpdateStatistic()
        {
            if(Core.PlayFab.IsLoggedIn)
            {
                Core.PlayFab.Player.Statistics.Update.Request(ID, Value);
            }
            else
            {

            }
        }
    }
}