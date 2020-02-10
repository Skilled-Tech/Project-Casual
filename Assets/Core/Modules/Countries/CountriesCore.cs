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
        public Element this[string code] => From(code);

        [SerializeField]
        protected Element _default;
        public Element Default { get { return _default; } }

        public Element From(string code)
        {
            for (int i = 0; i < list.Length; i++)
                if (string.Equals(list[i].Code, code, StringComparison.OrdinalIgnoreCase))
                    return list[i];

            return null;
        }
        public Element From(PlayFab.ClientModels.CountryCode code) => From(code.ToString());

        #region Editor
        [Space]
        public EditorProperty editor;
        [Serializable]
        public class EditorProperty
        {
            public Sprite[] sprites;
        }

#if UNITY_EDITOR
        [Button("Create From Sprites")]
        void CreateFromSprites()
        {
            list = new Element[editor.sprites.Length];

            for (int i = 0; i < editor.sprites.Length; i++)
            {
                list[i] = new Element(editor.sprites[i].name, editor.sprites[i]);
            }

            EditorUtility.SetDirty(this);
        }
#endif
        #endregion
    }
}