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

                StateChangeCallback(state);

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

            Level.OnExit += ExitCallback;
        }

        public override void Init()
        {
            base.Init();

            State = LevelPauseState.None;
        }

        protected virtual void StateChangeCallback(LevelPauseState state)
        {
            Time.timeScale = StateToTimeScale(state);
        }

        void ExitCallback()
        {
            Level.OnExit -= ExitCallback;

            Time.timeScale = 1f;
        }

        //Static Utility
        public static float StateToTimeScale(LevelPauseState state)
        {
            switch (state)
            {
                case LevelPauseState.None:
                    return 1f;
                case LevelPauseState.Soft:
                    return 0f;
                case LevelPauseState.Hard:
                    return 0f;
            }

            return 1f;
        }
    }

    public enum LevelPauseState
    {
        None, Soft, Hard
    }
}