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
	public class LinkLoginMethodOperation : Operation
	{
        [SerializeField]
        protected LoginMethod method;
        public LoginMethod Method { get { return method; } }

        public Core Core => Core.Instance;

        public AuthenticationCore.LinkProperty.Element Link => Core.Authentication.Link[method];

        public override void Execute()
        {
            Link.Require();
        }
    }
}