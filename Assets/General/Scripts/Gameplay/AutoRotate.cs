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
	public class AutoRotate : MonoBehaviour
	{
        [SerializeField]
        float speed = 20f;

        [SerializeField]
        Vector3 axis = Vector3.up;

        [SerializeField]
        Space space = Space.World;

        void Update()
        {
            transform.Rotate(axis * speed, space);
        }
    }
}