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
	public class Relay : MonoBehaviour
	{
        protected IOperation[] targets;

        public RelayScope scope = RelayScope.GameObject;

        protected virtual void Awake()
        {
            switch (scope)
            {
                case RelayScope.GameObject:
                    targets = GetComponents<IOperation>();
                    break;

                case RelayScope.GameObjectAndChildern:
                    targets = GetComponentsInChildren<IOperation>();
                    break;
            }
        }

        public virtual void Invoke()
        {
            Operation.ExecuteAll(targets);
        }
	}

    public enum RelayScope
    {
        GameObject, GameObjectAndChildern
    }
}