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

using NaughtyAttributes;

namespace Game
{
	public class CountriesCore : Core.Module
	{
        public Sprite[] sprites;

        [Button("Extract")]
        void Extract()
        {
            list = new Element[sprites.Length];

            for (int i = 0; i < sprites.Length; i++)
            {
                list[i] = new Element(sprites[i].name, sprites[i]);
            }

            EditorUtility.SetDirty(this);
        }

        [SerializeField]
        protected Element[] list;
        public Element[] List { get { return list; } }
        [Serializable]
        public class Element
        {
            [SerializeField]
            protected string code;
            public string Code { get { return code; } }

            [SerializeField]
            protected Sprite sprite;
            public Sprite Sprite { get { return sprite; } }

            public Element(string code, Sprite sprite)
            {
                this.code = code;
                this.sprite = sprite;
            }
        }

        public int Count => list.Length;

        public Element this[int index] => list[index];
        public Element this[string code] => Get(code);

        public Element Get(string code)
        {
            for (int i = 0; i < list.Length; i++)
                if (string.Equals(list[i].Code, code, StringComparison.OrdinalIgnoreCase))
                    return list[i];

            return null;
        }
    }
}