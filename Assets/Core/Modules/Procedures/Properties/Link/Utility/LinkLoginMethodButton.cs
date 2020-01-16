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
	public class LinkLoginMethodButton : MonoBehaviour
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
        }

        void Action()
        {
            if (Core.Procedures.Login.IsComplete == false)
                Login();
            else
                Link();
        }

        void Login()
        {
            Interactable = false;

            SingleSubscribe.Execute(Core.Procedures.Login.OnResponse, Callback);
            Core.Procedures.Login.Request();

            void Callback(ProceduresCore.LoginProperty.Element element, string error)
            {
                if(error == null)
                {
                    Link();
                }
                else
                {
                    //TODO provide some feedback ?
                }
            }
        }

        void Link()
        {
            Interactable = false;

            SingleSubscribe.Execute(Core.Procedures.Link.OnResponse, Callback);
            Core.Procedures.Link[method].Request();

            void Callback(ProceduresCore.LinkProperty.Element element, string error)
            {
                //TODO provide some feedback here too ?

                Interactable = true;

                if (error == null)
                {
                    
                }
                else
                {

                }
            }
        }
    }
}