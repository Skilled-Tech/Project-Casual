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
	public partial class AuthenticationCore : Core.Module
	{
        [SerializeField]
        protected LoginProperty login;
        public LoginProperty Login { get { return login; } }

        [SerializeField]
        protected LinkProperty link;
        public LinkProperty Link { get { return link; } }

        public PlayFabCore PlayFab => Core.PlayFab;

        public class Property : Core.Property<AuthenticationCore>
        {
            public AuthenticationCore Authentication => Reference;
        }

        public override void Configure(Core reference)
        {
            base.Configure(reference);

            Register(this, login);
            Register(this, link);
        }
    }

    public enum AuthenticationMethod
    {
        CustomID, Facebook
    }
}