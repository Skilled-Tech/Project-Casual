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
	public class SetScrollRectPositionOperation : Operation
	{
        [SerializeField]
        ScrollRect scroll;

        public Vector3 position;

        protected virtual void Reset()
        {
            scroll = this.GetDependancy<ScrollRect>(Dependancy.Scope.CurrentToParents);
        }

        public override void Execute()
        {
            scroll.content.anchoredPosition3D = position;
        }
    }
}