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
	public class LevelMenu : Level.Module
	{
        [SerializeField]
        protected HoldUI hold;
        public HoldUI Hold { get { return hold; } }

        public LevelEndMenu End { get; protected set; }

        public override void Configure(Level reference)
        {
            base.Configure(reference);

            End = this.GetDependancy<LevelEndMenu>();
        }
    }
}