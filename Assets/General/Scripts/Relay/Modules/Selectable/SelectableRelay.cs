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
    public class SelectableRelay : Relay
    {
        public Selectable Selectable { get; protected set; }

        public bool Interactable
        {
            get => Selectable.interactable;
            set => Selectable.interactable = value;
        }

        public override void Configure()
        {
            base.Configure();

            Selectable = GetComponent<Selectable>();
        }
    }

    public class SelectableRelay<TComponent> : SelectableRelay
        where TComponent : Selectable
    {
        public TComponent Component { get; protected set; }

        public override void Configure()
        {
            base.Configure();

            Component = Selectable as TComponent;
        }
    }
}