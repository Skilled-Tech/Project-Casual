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
    [RequireComponent(typeof(CanvasGroup))]
	public class PopupUITemplate : UITemplate<string, PopupUITemplate>
	{
        [SerializeField]
        protected float duration = 0.5f;
        public float Duration { get { return duration; } }

        [SerializeField]
        protected AnimationCurve curve;
        public AnimationCurve Curve { get { return curve; } }

        [SerializeField]
        protected Text label;
        public Text Label { get { return label; } }

        public Color Color
        {
            get => label.color;
            set => label.color = value;
        }

        public int size
        {
            get => label.fontSize;
            set => label.fontSize = value;
        }

        public CanvasGroup CanvasGroup { get; protected set; }

        public override void Configure()
        {
            base.Configure();

            CanvasGroup = GetComponent<CanvasGroup>();
        }
        
        public override void Set(string reference)
        {
            base.Set(reference);

            label.text = reference;
        }

        public virtual void Animate()
        {
            StartCoroutine(Procedure());
        }

        IEnumerator Procedure()
        {
            var timer = duration;

            var startPosition = transform.anchoredPosition;
            var endPosition = Vector2.zero;

            while(true)
            {
                timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime);

                var rate = curve.Evaluate(timer / duration);

                CanvasGroup.alpha = rate;

                transform.anchoredPosition = Vector3.Lerp(endPosition, startPosition, rate);

                if (Mathf.Approximately(timer, 0f)) break;

                yield return new WaitForEndOfFrame();
            }

            Destroy(gameObject);
        }
    }
}