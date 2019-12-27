#if TextMeshPro
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

using TMPro;

namespace Game
{
    public static partial class GameTools
    {
        public static class TextMeshPro
        {
            public static class Font
            {
                public static class Style
                {
                    public static void SetStrikeThrough(TMP_Text label, bool isOn)
                    {
                        if (isOn)
                            label.fontStyle = label.fontStyle & ~FontStyles.Strikethrough;
                        else
                            label.fontStyle = label.fontStyle | FontStyles.Strikethrough;
                    }
                }
            }
        }
    }

    public static class TMP_Extensions
    {
        public static void SetStrikeThrough(this TMP_Text label, bool isOn)
        {
            GameTools.TextMeshPro.Font.Style.SetStrikeThrough(label, isOn);
        }
    }
}
#endif