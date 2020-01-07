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
using UnityEngine.EventSystems;

namespace Game
{
    public class PlayerScoreModifierUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Player Player => Level.Instance.Player.Instance;

        public int value = 50;

        public float delay = 0.2f;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (coroutine != null) StopCoroutine(coroutine);

            coroutine = StartCoroutine(Procedure());
        }

        Coroutine coroutine;
        IEnumerator Procedure()
        {
            Player.Score.Value += value;
            yield return new WaitForSecondsRealtime(delay * 4);

            while(true)
            {
                Player.Score.Value += value;

                yield return new WaitForSecondsRealtime(delay);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
    }
}