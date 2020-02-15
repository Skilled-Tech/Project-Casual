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
    [RequireComponent(typeof(UIElement))]
    [RequireComponent(typeof(Animator))]
    public class BellUI : MonoBehaviour, IInitialize
	{
        public UIElement Element { get; protected set; }

        public bool IsOn => Element.IsOn;

        public Animator Animator { get; protected set; }

        [SerializeField]
        protected AnimationsData animations;
        public AnimationsData Animations { get { return animations; } }
        [Serializable]
        public class AnimationsData
        {
            [SerializeField]
            protected AnimationClip popIn;
            public AnimationClip PopIn { get { return popIn; } }

            [SerializeField]
            protected AnimationClip chime;
            public AnimationClip Chime { get { return chime; } }

            [SerializeField]
            protected AnimationClip popOut;
            public AnimationClip PopOut { get { return popOut; } }
        }

        public virtual void Configure()
        {
            Element = GetComponent<UIElement>();

            Animator = GetComponent<Animator>();
        }
        public virtual void Init()
        {
            
        }

        protected virtual void OnEnable()
        {
            StartCoroutine(Procedure());

            IEnumerator Procedure()
            {
                yield return new WaitForEndOfFrame();

                if (Element.IsOn) Show();
            }
        }

        Coroutine state;
        protected virtual void Transition(IEnumerator ienumerator)
        {
            if (Element.IsOn == false) Element.Show();

            if (state != null) StopCoroutine(state);

            state = StartCoroutine(ienumerator);
        }

        public virtual void Show()
        {
            Transition(Procedure());

            IEnumerator Procedure()
            {
                Animator.Play(animations.PopIn.name);

                Transition(Idle());

                yield break;
            }
        }

        IEnumerator Idle()
        {
            while(true)
            {
                yield return new WaitForSeconds(Random.Range(2f, 4f));

                Chime();
            }
        }

        public void Chime()
        {
            Transition(Procedure());

            IEnumerator Procedure()
            {
                Animator.Play(animations.Chime.name);

                yield break;
            }
        }

        public virtual void Hide()
        {
            Transition(Procedure());

            IEnumerator Procedure()
            {
                Animator.Play(animations.PopOut.name);

                yield return new WaitForSecondsRealtime(Animations.PopOut.length);

                Element.Hide();
            }
        }
	}
}