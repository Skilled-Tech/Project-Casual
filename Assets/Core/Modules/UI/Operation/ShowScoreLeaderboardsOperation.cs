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
    public class ShowScoreLeaderboardsOperation : Operation
    {
        public Core Core => Core.Instance;

        public LeaderboardUI Leaderboard => Core.UI.Leaderboards.Score;

        public override void Execute()
        {
            Leaderboard.Show();

            Leaderboard.Submit.Element.SetActive(false);
        }
    }
}