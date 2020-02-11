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

using PlayFab;
using PlayFab.ClientModels;

namespace Game
{
	public class LeaderboardPersonalModule : LeaderboardModule.Element<GetLeaderboardAroundPlayerResult>
    {
        [SerializeField]
        protected int max = 3;
        public int Max { get { return max; } }

        public override string ID => "Personal";

        public override void Request()
        {
            base.Request();

            Core.PlayFab.Title.Leaderboards.GetAroundPlayer.OnResponse.Enque(ResponseCallback);
            Core.PlayFab.Title.Leaderboards.GetAroundPlayer.Request(Leaderboard.ID, max);
        }

        public override IList<PlayerLeaderboardEntry> ResultToList(GetLeaderboardAroundPlayerResult result) => result.Leaderboard;
    }
}