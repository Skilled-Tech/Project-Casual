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

using UnityEngine.Events;

namespace Game
{
	public class UnityEventOperation : Operation
	{
		[SerializeField]
        protected UnityEvent onExecute;
        public UnityEvent OnExecute { get { return onExecute; } }

        public override void Execute()
        {
            OnExecute.Invoke();
        }
    }
}