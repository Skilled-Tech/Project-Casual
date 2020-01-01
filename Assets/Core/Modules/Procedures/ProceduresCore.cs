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

using PlayFab;
using PlayFab.ClientModels;

namespace Game
{
	public class ProceduresCore : Core.Module
	{
        [SerializeField]
        protected LoginProcedure login;
        public LoginProcedure Login { get { return login; } }
        [Serializable]
        public class LoginProcedure : Element
        {
            public override bool Complete => PlayFabClientAPI.IsClientLoggedIn();

            public override IEnumerator<Element> Requirements()
            {
                yield break;
            }

            public PlayFabCore PlayFab => Core.PlayFab;

            public override void Start()
            {
                base.Start();

                PlayFab.Login.OnResponse += ResponseCallback;
            }

            private void ResponseCallback(LoginResult result, PlayFabError error)
            {
                if(error == null)
                {
                    End();
                }
                else
                {
                    InvokeError(error.ErrorMessage);
                }
            }
        }

        [Serializable]
        public abstract class Element : Property
        {
            public abstract bool Complete { get; }

            public bool IsProcessing { get; protected set; }

            public abstract IEnumerator<Element> Requirements();

            public event Action OnStart;
            public virtual void Start()
            {
                IsProcessing = true;

                OnStart?.Invoke();
            }

            public event RestDelegates.ErrorCallback<string> OnError;
            public virtual void InvokeError(string error)
            {
                IsProcessing = false;

                OnError?.Invoke(error);
            }

            public event Action OnEnd;
            protected virtual void End()
            {
                IsProcessing = false;

                OnEnd?.Invoke();
            }
        }

        [Serializable]
        public class Property : Core.Property<ProceduresCore>
        {
            public ProceduresCore Procedures => Reference;
        }

        public override void Init()
        {
            base.Init();

            login.Start();
        }
    }
}