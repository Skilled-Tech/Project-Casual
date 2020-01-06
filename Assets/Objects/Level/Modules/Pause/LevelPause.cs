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
	public class LevelPause : Level.Module
    {
        [SerializeField]
        protected LevelPauseState state;
        public LevelPauseState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;

                OnStateChange?.Invoke(state);
            }
        }

        public delegate void StateChangeDelegate(LevelPauseState state);
        public event StateChangeDelegate OnStateChange;

        public bool IsNone => state == LevelPauseState.None;
        public bool IsSoft => state == LevelPauseState.Soft;
        public bool IsHard => state == LevelPauseState.Hard;

        public override void Configure(Level reference)
        {
            base.Configure(reference);

            OnStateChange += StateChangeCallback;
        }

        protected virtual void StateChangeCallback(LevelPauseState state)
        {
            Time.timeScale = state == LevelPauseState.Hard ? 0f : 1f;
        }

        protected virtual void OnDestroy()
        {
            OnStateChange -= StateChangeCallback;

            Time.timeScale = 1f;
        }
    }

    public enum LevelPauseState
    {
        None, Soft, Hard
    }
}