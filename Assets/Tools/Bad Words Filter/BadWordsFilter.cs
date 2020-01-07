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

using System.Text;
using System.Text.RegularExpressions;

namespace Game
{
	public static class BadWordsFilter
	{
        public const string FileName = "badwords";

        public static string FilePath => FileName;

        static TextAsset asset;

        public static List<string> List { get; private set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnLoad()
        {
            asset = Resources.Load<TextAsset>(FilePath);

            List = new List<string>();

            if(asset == null)
            {
                Debug.LogWarning("No " + FilePath + " found in a resources folder, " + nameof(BadWordsFilter) + " functionality won't work");
                return;
            }

            using (var reader = new StringReader(asset.text))
            {
                while (true)
                {
                    var line = reader.ReadLine();

                    if (line == null) break;

                    if (line == string.Empty) continue;

                    line = line.ToLower();

                    List.Add(line);
                }
            }
        }

        public static bool IsClean(string word) => !Contains(word);

        public static bool Contains(string word)
        {
            word = word.ToLower();

            for (int i = 0; i < List.Count; i++)
                if (word.Contains(List[i]))
                    return true;

            return false;
        }
    }
}