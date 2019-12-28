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
	public class Relay : MonoBehaviour, IInitialize
	{
        protected IOperation[] targets;

        public RelayScope scope = RelayScope.GameObject;

        public virtual void Configure()
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

        public virtual void Init()
        {
            
        }

        public event Action OnInvoke;
        public virtual void Invoke()
        {
            Operation.ExecuteAll(targets);

            OnInvoke?.Invoke();
        }
    }

    public enum RelayScope
    {
        GameObject, GameObjectAndChildern
    }
}