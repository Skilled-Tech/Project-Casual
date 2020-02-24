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

using NaughtyAttributes;

namespace Game
{
	public class PlayerCore : Core.Module
	{
        [SerializeField]
        protected StatisticsProperty statistics;
        public StatisticsProperty Statistics { get { return statistics; } }
        [Serializable]
        public class StatisticsProperty : Property
        {
            [SerializeField]
            protected ScoreProperty score;
            public ScoreProperty Score { get { return score; } }
            [Serializable]
            public class ScoreProperty : Element
            {
                public override string ID => "Score";
            }

            [SerializeField]
            protected HighScoreProperty highScore;
            public HighScoreProperty HighScore { get { return highScore; } }
            [Serializable]
            public class HighScoreProperty : Element
            {
                public override string ID => "High Score";
            }

            [Serializable]
            public abstract class Element : Property
            {
                public abstract string ID { get; }
                
                public virtual int Value
                {
                    get => PlayerPrefs.GetInt(ID, DefaultValue);
                    set
                    {
                        Save(value);

                        Upload(value);

                        OnValueChanged?.Invoke(value);
                    }
                }

                public delegate void ValueChangeDelegate(float value);
                public event ValueChangeDelegate OnValueChanged;

                public virtual int DefaultValue => 0;

                public PlayFabCore PlayFab => Core.PlayFab;

                public override void Init()
                {
                    base.Init();

                    PlayFab.Player.Profile.Statistics.OnSet += PlayFabStatisticSetCallback;
                }

                protected virtual void Save(int value)
                {
                    if(value != DefaultValue) PlayerPrefs.SetInt(ID, value);
                }

                protected virtual void Upload(int value)
                {
                    if(PlayFab.IsLoggedIn) PlayFab.Player.Statistics.Update.Request(ID, value);
                }

                public virtual void Clear()
                {
                    PlayerPrefs.DeleteKey(ID);
                }

                private void PlayFabStatisticSetCallback(string name, int cloudValue)
                {
                    if(name == ID)
                    {
                        //Values match, nothing to change here
                        if (Value == cloudValue) 
                        {

                        }

                        //Our local value is higher than the cloud value
                        //Upload local to cloud
                        if (Value > cloudValue)
                        {
                            Upload(Value);
                        }

                        //Our local value is lower than the cloud value
                        //Store cloud value
                        if(Value < cloudValue)
                        {
                            Save(cloudValue);
                        }
                    }
                }
            }

            [Serializable]
            public class Property : Core.Property<StatisticsProperty>
            {
                public StatisticsProperty Statistics => Reference;

                public PlayerCore Player => Statistics.Player;
            }

            public override void Configure(PlayerCore reference)
            {
                base.Configure(reference);

                Register(this, score);
                Register(this, highScore);
            }

            public virtual void Clear()
            {
                score.Clear();
                highScore.Clear();
            }
        }
        
        [Button("Clear Statistics")]
        void ClearStatistics() => statistics.Clear();

        [Serializable]
        public class Property : Core.Property<PlayerCore>
        {
            public PlayerCore Player => Reference;
        }

        public class Module : Core.Module<PlayerCore>
        {
            public PlayerCore Player => Reference;
        }

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            Register(this, statistics);

            References.Configure(this);
        }

        public override void Init()
        {
            base.Init();

            References.Init(this);
        }
    }
}