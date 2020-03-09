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
	public class SinBob : MonoBehaviour
	{
        [SerializeField]
        float speed;

        [SerializeField]
        float scale;

        [SerializeField]
        Vector3 axis = Vector3.up;

        float offset;

        private void Update()
        {
            var position = transform.position;

            position -= axis * offset;

            offset = scale * Mathf.Sin(speed * Time.time);

            position += axis * offset;

            transform.position = position;
        }
    }
}