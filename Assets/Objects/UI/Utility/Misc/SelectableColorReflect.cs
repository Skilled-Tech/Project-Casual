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
    [RequireComponent(typeof(Graphic))]
	public class SelectableColorReflect : MonoBehaviour
	{
        Graphic graphic;
        public Color Color
        {
            set
            {
                if (value == graphic.color)
                    return;

                graphic.color = value;
            }
        }

        Selectable selectable;
        public CanvasRenderer CanvasRenderer => selectable.targetGraphic.canvasRenderer;

        private void Start()
        {
            graphic = GetComponent<Graphic>();

            if(graphic == null)
            {
                Debug.LogError("No Graphic component attached to " + name + ", disabling " + GetType().Name);
                enabled = false;
                return;
            }

            selectable = this.GetDependancy<Selectable>(Dependancy.Scope.Parents);
        }

        private void Update()
        {
            Color = CanvasRenderer.GetColor();
        }
    }
}