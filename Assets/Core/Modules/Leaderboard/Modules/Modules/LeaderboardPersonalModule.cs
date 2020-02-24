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
	public class LeaderboardPersonalModule : LeaderboardModule.Element<GetLeaderboardAroundPlayerRequest, GetLeaderboardAroundPlayerResult>
    {
        [SerializeField]
        protected int max = 3;
        public int Max { get { return max; } }

        public override string ID => "Personal";

        public override void Configure(LeaderboardModule reference)
        {
            base.Configure(reference);

            Core.PlayFab.Title.Leaderboards.GetAroundPlayer.OnResponse.Add(ResponseCallback);
        }

        public override void Request()
        {
            base.Request();
            
            Core.PlayFab.Title.Leaderboards.GetAroundPlayer.Request(Leaderboard.ID, max);
        }

        protected override bool CheckID(GetLeaderboardAroundPlayerRequest request) => request.StatisticName == Leaderboard.ID;

        public override IList<PlayerLeaderboardEntry> ResultToList(GetLeaderboardAroundPlayerResult result) => result.Leaderboard;
    }
}