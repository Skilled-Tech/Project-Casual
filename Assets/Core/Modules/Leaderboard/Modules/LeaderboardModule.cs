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
	public class LeaderboardModule : LeaderboardsCore.Module
	{
        public string ID => gameObject.name;

        #region List
        public List<LeaderboardElement> List { get; protected set; }

        public int Count => List.Count;

        public LeaderboardElement this[int index] => List[index];
        #endregion

        public PlayFabCore PlayFab => Core.PlayFab;

        public override void Configure(LeaderboardsCore reference)
        {
            base.Configure(reference);

            List = new List<LeaderboardElement>();

            PlayFab.Login.OnResult += LoginResultCallback;

            PlayFab.Title.Leaderboards.Get.OnResponse += ResponseCallback;
        }

        private void LoginResultCallback(LoginResult result)
        {
            Request();
        }

        public virtual void Request()
        {
            PlayFab.Title.Leaderboards.Get.Request(ID);
        }

        #region Events
        private void ResponseCallback(GetLeaderboardResult result, PlayFabError error)
        {
            var request = result.Request as GetLeaderboardRequest;

            if (request.StatisticName == ID)
                ResponseAction(request, result, error);
        }

        public event RestDelegates.ResponseCallback<LeaderboardModule, PlayFabError> OnResponse;
        protected virtual void ResponseAction(GetLeaderboardRequest request, GetLeaderboardResult result, PlayFabError error)
        {
            if (error == null)
                ResultAction(request, result);
            else
                ErrorCallback(error);

            OnResponse?.Invoke(this, error);
        }

        public event RestDelegates.ResultCallback<LeaderboardModule> OnUpdate;
        protected virtual void ResultAction(GetLeaderboardRequest request, GetLeaderboardResult result)
        {
            Debug.Log("Retrieved " + request.StatisticName + " Leaderboard");

            List.Clear();

            for (int i = 0; i < result.Leaderboard.Count; i++)
            {
                var element = new LeaderboardElement(result.Leaderboard[i]);

                List.Add(element);
            }

            List.Sort(LeaderboardElement.Comparisons.Position.Instance); //Just incase ¯\_(ツ)_/¯ ?

            OnUpdate?.Invoke(this);
        }

        public event RestDelegates.ErrorCallback<PlayFabError> OnError;
        protected virtual void ErrorCallback(PlayFabError error)
        {
            OnError?.Invoke(error);
        }
        #endregion
    }

    [Serializable]
    public class LeaderboardElement
    {
        public int Position { get; protected set; }

        public string DisplayName { get; protected set; }

        public int Value { get; protected set; }

        public static class Comparisons
        {
            public class Position : IComparer<LeaderboardElement>
            {
                public static Position Instance = new Position();

                public int Compare(LeaderboardElement x, LeaderboardElement y)
                {
                    return x.Position - y.Position;
                }
            }
        }

        public LeaderboardElement(PlayerLeaderboardEntry entry) : this(entry.Position, entry.DisplayName, entry.StatValue)
        {

        }
        public LeaderboardElement(int position, string displayName, int value)
        {
            this.Position = position;
            this.DisplayName = displayName;
            this.Value = value;
        }
    }
}