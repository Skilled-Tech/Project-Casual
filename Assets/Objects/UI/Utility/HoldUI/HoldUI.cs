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

using UnityEngine.EventSystems;

namespace Game
{
    public class HoldUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public bool IsDown { get; protected set; }

        public event Action OnClick;
        public void OnPointerDown(PointerEventData eventData)
        {
            IsDown = true;

            OnClick?.Invoke();
        }

        public event Action OnRelease;
        public void OnPointerUp(PointerEventData eventData)
        {
            IsDown = false;

            OnRelease?.Invoke();
        }
    }
}