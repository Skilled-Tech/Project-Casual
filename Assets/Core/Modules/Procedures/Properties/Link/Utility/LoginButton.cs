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
    [RequireComponent(typeof(Button))]
	public class LoginButton : MonoBehaviour
	{
        [SerializeField]
        protected LoginMethod method;
        public LoginMethod Method { get { return method; } }

        Button button;

        public bool Interactable
        {
            get => button.interactable;
            set => button.interactable = value;
        }

        public Core Core => Core.Instance;

        public PlayFabCore PlayFab => Core.PlayFab;

        private void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(Action);

            if (Core.Procedures.Login.IsProcessing)
                WaitForCurrentLogin();
        }

        void WaitForCurrentLogin()
        {
            Interactable = false;

            Core.Procedures.Login.OnResponse.Enque(Callback);

            void Callback(ProceduresCore.LoginProperty.Element element, Procedure.Response response)
            {
                Interactable = true;
            }
        }

        void Action()
        {
            if (Core.Procedures.Login.IsProcessing)
            {
                Debug.LogWarning("Cannot process new login request untill the old one is finished");

                //TODO Provide Feedback ?
            }
            if (Core.Procedures.Login.IsComplete == false)
                Login();
            else
                Link();
        }

        void Login()
        {
            Interactable = false;

            Core.Procedures.Login.OnResponse.Enque(Callback);
            Core.Procedures.Login[method].Require();

            void Callback(ProceduresCore.LoginProperty.Element element, Procedure.Response response)
            {
                Interactable = true;
                //TODO Provide Feedback
            }
        }

        void Link()
        {
            Interactable = false;

            Core.Procedures.Link.OnResponse.Enque(Callback);
            Core.Procedures.Link[method].Require();

            void Callback(ProceduresCore.LinkProperty.Element element, Procedure.Response response)
            {
                Interactable = true;
                //TODO Provide Feedback
            }
        }
    }
}