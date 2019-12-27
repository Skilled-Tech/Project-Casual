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
	public class SingleBehaviourEnable : MonoBehaviour
	{
		[SerializeField]
        protected MonoBehaviour behaviour;
        public MonoBehaviour Behaviour { get { return behaviour; } }

        public static Dictionary<Type, List<MonoBehaviour>> Dictionary = new Dictionary<Type, List<MonoBehaviour>>();
        public static void Add(MonoBehaviour behaviour)
        {
            var type = behaviour.GetType();
            List<MonoBehaviour> list;

            if(Dictionary.ContainsKey(type))
            {
                list = Dictionary[type];

                if(list.Count > 0) list.Last().enabled = false;

                list.Add(behaviour);
            }
            else
            {
                list = new List<MonoBehaviour>();

                list.Add(behaviour);

                Dictionary.Add(type, list);
            }
        }
        public static void Remove(MonoBehaviour behaviour)
        {
            var type = behaviour.GetType();
            List<MonoBehaviour> list;

            if (Dictionary.ContainsKey(type))
            {
                list = Dictionary[type];

                list.Remove(behaviour);

                if(list.Count > 0) list.Last().enabled = true;
            }
        }

        private void OnEnable()
        {
            if(behaviour == null)
            {
                Debug.LogError("No behaviour defined for " + nameof(SingleBehaviourEnable), gameObject);
            }
            else
            {
                Add(behaviour);
            }
        }

        private void OnDisable()
        {
            if (behaviour == null)
            {

            }
            else
            {
                Remove(behaviour);
            }
        }
    }
}