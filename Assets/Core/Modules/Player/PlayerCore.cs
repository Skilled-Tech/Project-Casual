﻿using System;
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

using PlayFab;
using PlayFab.ClientModels;

namespace Game
{
	public class PlayerCore : Core.Module
	{
        public UpdateDisplayNameProcedure UpdateDisplayName { get; protected set; }
        [Serializable]
        public class UpdateDisplayNameProcedure : Procedure
        {
            public PlayFabCore PlayFab => Core.PlayFab;

            public TextInputUI TextInput => Core.UI.TextInput;

            public virtual void Require() => Require("Updating Display Name");

            public override void Request()
            {
                base.Request();

                DisplayInput();
            }

            void DisplayInput()
            {
                Popup.Hide();
                TextInput.Show("Enter Display Name", Callback);

                void Callback(TextInputUI.Response response)
                {
                    if (response.Success)
                        PlayFabRequest(response.Text);
                    else if (response.Canceled)
                        Cancel();
                    else
                    {
                        Debug.LogError("Unknown Condition Met");
                        InvokeError("Unknown Error");
                    }
                }
            }

            void PlayFabRequest(string name)
            {
                Popup.Lock("Updating Profile Info");

                PlayFab.Player.Profile.DisplayName.Update.OnResponse.Enque(Callback);
                PlayFab.Player.Profile.DisplayName.Update.Request(name);

                void Callback(UpdateUserTitleDisplayNameResult result, PlayFabError error)
                {
                    if (error == null)
                        End();
                    else
                    {
                        if (error.Error == PlayFabErrorCode.NameNotAvailable)
                            Popup.Show("Name Not Available, Please Try A Different Name");
                        else
                            InvokeError(error.ErrorMessage);
                    }
                }
            }

            protected override void Stop()
            {
                base.Stop();

                Popup.Hide();

                TextInput.Hide();
            }
        }

        public class Procedure : Core.Procedure<PlayerCore>
        {
            public PlayerCore Player => Reference;
        }

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

                [SerializeField]
                protected int _value;
                public virtual int Value
                {
                    get => _value;
                    set
                    {
                        _value = value;

                        Save(value);

                        OnValueChanged?.Invoke(Value);
                    }
                }

                public delegate void ValueChangeDelegate(float value);
                public event ValueChangeDelegate OnValueChanged;

                public virtual int DefaultValue => 0;

                public PlayFabCore PlayFab => Core.PlayFab;

                public override void Configure(StatisticsProperty reference)
                {
                    base.Configure(reference);

                    Value = PlayerPrefs.GetInt(ID, DefaultValue);
                }

                public override void Init()
                {
                    base.Init();

                    PlayFab.Player.Profile.Statistics.OnSet += PlayFabStatisticSetCallback;

                    PlayFab.Player.Profile.Statistics.OnLoad += PlayFabStatisticsLoadCallback;
                }

                protected virtual void Save(int value)
                {
                    if (value != DefaultValue) PlayerPrefs.SetInt(ID, value);
                }

                public virtual void Upload() => Upload(Value);
                protected virtual void Upload(int value)
                {
                    if(PlayFab.IsLoggedIn) PlayFab.Player.Profile.Statistics.Update.Request(ID, value);
                }

                public virtual void Clear()
                {
                    PlayerPrefs.DeleteKey(ID);
                }

                protected virtual void PlayFabStatisticsLoadCallback()
                {
                    var eval = PlayFab.Player.Profile.Statistics.Evalute(ID);

                    if (Value != eval) Value = eval;
                }

                protected virtual void PlayFabStatisticSetCallback(string name, int value)
                {
                    if (name == ID) PlayFabStatisticSetCallback(value);
                }
                protected virtual void PlayFabStatisticSetCallback(int cloudValue)
                {
                    if (Value != cloudValue) Value = cloudValue;
                }
            }

            [Serializable]
            public class Property : Core.Property<StatisticsProperty>
            {
                public StatisticsProperty Statistics => Reference;

                public PlayerCore Player => Statistics.Player;
            }

            public PlayFabCore PlayFab => Core.PlayFab;

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

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            UpdateDisplayName = new UpdateDisplayNameProcedure();

            Register(this, UpdateDisplayName);
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