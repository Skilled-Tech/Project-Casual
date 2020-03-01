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

            if (Core.Authentication.Login.IsProcessing)
                WaitForCurrentLogin();
        }

        void WaitForCurrentLogin()
        {
            Interactable = false;

            Core.Authentication.Login.OnResponse.Enque(Callback);

            void Callback(AuthenticationCore.LoginProperty.Element element, Core.Procedure.Response response)
            {
                if (this == null) return; //Counter measure if the gameobject got destroyed mid procedure

                Interactable = true;
            }
        }

        void Action()
        {
            if (Core.Authentication.Login.IsProcessing)
            {
                Debug.LogWarning("Cannot process new login request untill the old one is finished");

                //TODO Provide Feedback ?
            }
            if (Core.Authentication.Login.IsComplete == false)
                Login();
            else
                Link();
        }

        void Login()
        {
            Interactable = false;

            Core.Authentication.Login.OnResponse.Enque(Callback);
            Core.Authentication.Login[method].Require();

            void Callback(AuthenticationCore.LoginProperty.Element element, Core.Procedure.Response response)
            {
                if (this == null) return; //Counter measure if the gameobject got destroyed mid procedure

                Interactable = true;
                //TODO Provide Feedback
            }
        }

        void Link()
        {
            Interactable = false;

            Core.Authentication.Link.OnResponse.Enque(Callback);
            Core.Authentication.Link[method].Require();

            void Callback(AuthenticationCore.LinkProperty.Element element, Core.Procedure.Response response)
            {
                if (this == null) return; //Counter measure if the gameobject got destroyed mid procedure

                Interactable = true;
                //TODO Provide Feedback
            }
        }
    }
}