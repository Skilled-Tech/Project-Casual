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

                protected int _value = 0;
                public virtual int Value
                {
                    get => _value;
                    set
                    {
                        _value = value;

                        if (Core.PlayFab.IsLoggedIn) Core.PlayFab.Player.Statistics.Update.Request(ID, value);

                        OnValueChanged?.Invoke(value);
                    }
                }

                public delegate void ValueChangeDelegate(float value);
                public event ValueChangeDelegate OnValueChanged;

                public virtual int DefaultValue => 0;

                public override void Configure(StatisticsProperty reference)
                {
                    base.Configure(reference);

                    Core.PlayFab.Player.Profile.OnUpdate += ProfileUpdateCallback;
                }

                public override void Init()
                {
                    base.Init();

                    Load();
                }

                public virtual void Load()
                {
                    Value = PlayerPrefs.GetInt(ID, DefaultValue);
                }

                protected virtual void ProfileUpdateCallback()
                {
                    //if(Core.PlayFab.Player.Profile.)
                }
            }

            [Serializable]
            public class Property : Core.Property<StatisticsProperty>
            {
                public StatisticsProperty Statistics => Reference;

                public PlayerCore Player => Statistics.Player;
            }
        }
        
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

            References.Configure(this);
        }

        public override void Init()
        {
            base.Init();

            References.Init(this);
        }
    }
}