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
using PlayFab.SharedModels;

namespace Game
{
    public class LeaderboardModule : LeaderboardsCore.Module
    {
        public string ID => gameObject.name;

        #region List
        public List<LeaderboardElement> List { get; protected set; }

        public int Count => List.Count;

        public LeaderboardElement this[int index] => List[index];

        public bool ContainsID(string ID)
        {
            for (int i = 0; i < Count; i++)
                if (this[i].ID == ID)
                    return true;

            return false;
        }
        #endregion

        #region Element
        public TopElement Top { get; protected set; }
        public class TopElement : Element<GetLeaderboardRequest, GetLeaderboardResult>
        {
            public override PlayFabCore.Request<GetLeaderboardRequest, GetLeaderboardResult> PlayFabRequest
                => PlayFab.Title.Leaderboards.Get;

            public override IList<PlayerLeaderboardEntry> ExtractEntries(GetLeaderboardResult result)
                => result.Leaderboard;
            public override string ExtractStatisitcName(GetLeaderboardRequest request)
                => request.StatisticName;

            public override void Request()
            {
                base.Request();

                PlayFab.Title.Leaderboards.Get.Request(Leaderboard.ID);
            }
        }

        public AroundPlayerElement AroundPlayer { get; protected set; }
        public class AroundPlayerElement : Element<GetLeaderboardAroundPlayerRequest, GetLeaderboardAroundPlayerResult>
        {
            public override PlayFabCore.Request<GetLeaderboardAroundPlayerRequest, GetLeaderboardAroundPlayerResult> PlayFabRequest
                => PlayFab.Title.Leaderboards.GetAroundPlayer;

            public override IList<PlayerLeaderboardEntry> ExtractEntries(GetLeaderboardAroundPlayerResult result)
                => result.Leaderboard;
            public override string ExtractStatisitcName(GetLeaderboardAroundPlayerRequest request)
                => request.StatisticName;

            public override void Request()
            {
                base.Request();

                PlayFab.Title.Leaderboards.GetAroundPlayer.Request(Leaderboard.ID, 1);
            }
        }

        public class Element : Property
        {
            #region List
            public List<LeaderboardElement> List { get; protected set; }

            public int Count => List.Count;

            public LeaderboardElement this[int index] => List[index];
            #endregion

            public event RestDelegates.ResultCallback<IList<LeaderboardElement>> OnUpdate;
            protected virtual void Update(IList<PlayerLeaderboardEntry> results)
            {
                for (int i = 0; i < results.Count; i++)
                {
                    var element = new LeaderboardElement(results[i]);

                    List.Add(element);
                }

                List.Sort(LeaderboardElement.Comparisons.Position.Instance); //Just incase ¯\_(ツ)_/¯ ?

                OnUpdate?.Invoke(List);
            }

            public event RestDelegates.ErrorCallback<PlayFabError> OnError;
            protected virtual void ErrorCallback(PlayFabError error)
            {
                OnError?.Invoke(error);
            }
        }
        public abstract class Element<TRequest, TResult> : Element
            where TRequest :  PlayFabRequestCommon, new()
            where TResult :  PlayFabResultCommon
        {
            public abstract string ExtractStatisitcName(TRequest request);
            public abstract IList<PlayerLeaderboardEntry> ExtractEntries(TResult result);

            public abstract PlayFabCore.Request<TRequest, TResult> PlayFabRequest { get; }

            public override void Configure(LeaderboardModule reference)
            {
                base.Configure(reference);

                List = new List<LeaderboardElement>();

                PlayFabRequest.OnResponse += ResponseCallback;
            }

            public virtual void Request()
            {

            }

            #region Events
            private void ResponseCallback(TResult result, PlayFabError error)
            {
                var request = result.Request as TRequest;

                if (ExtractStatisitcName(request) == Leaderboard.ID)
                    ResponseAction(request, result, error);
            }

            public event RestDelegates.ResponseCallback<IList<LeaderboardElement>, PlayFabError> OnResponse;
            protected virtual void ResponseAction(TRequest request, TResult result, PlayFabError error)
            {
                if (error == null)
                    ResultAction(request, result);
                else
                    ErrorCallback(error);

                OnResponse?.Invoke(List, error);
            }

            protected virtual void ResultAction(TRequest request, TResult result)
            {
                List.Clear();

                Update(ExtractEntries(result));
            }
            #endregion
        }
        #endregion

        public class Property : IReference<LeaderboardModule>
        {
            public LeaderboardModule Leaderboard { get; protected set; }

            public Core Core => Core.Instance;

            public PlayFabCore PlayFab => Core.PlayFab;

            public virtual void Configure(LeaderboardModule reference)
            {
                Leaderboard = reference;
            }

            public virtual void Init()
            {

            }
        }

        public PlayFabCore PlayFab => Core.PlayFab;

        public override void Configure(LeaderboardsCore reference)
        {
            base.Configure(reference);

            List = new List<LeaderboardElement>();

            Top = new TopElement();
            Register(Top);

            AroundPlayer = new AroundPlayerElement();
            Register(AroundPlayer);

            PlayFab.Login.OnResult += LoginResultCallback;
        }

        public virtual void Register(Element element)
        {
            base.Register(this, element);

            element.OnUpdate += (IList<LeaderboardElement> result) => ElementUpdateCallback(element, result);
        }
        
        public virtual void Request()
        {
            List.Clear();

            Top.Request();
            AroundPlayer.Request();
        }

        #region Callbacks
        private void LoginResultCallback(LoginResult result) => Request();

        private void ElementUpdateCallback(Element element, IList<LeaderboardElement> list)
        {
            UpdateAction(list);
        }

        public event RestDelegates.ErrorCallback<PlayFabError> OnError;
        protected virtual void ErrorCallback(PlayFabError error)
        {
            OnError?.Invoke(error);
        }
        #endregion

        public event RestDelegates.ResultCallback<LeaderboardModule> OnUpdate;
        protected virtual void UpdateAction(IList<LeaderboardElement> elements)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (ContainsID(elements[i].ID)) continue;

                List.Add(elements[i]);
            }

            List.Sort(LeaderboardElement.Comparisons.Position.Instance);

            OnUpdate?.Invoke(this);
        }
    }

    [Serializable]
    public class LeaderboardElement
    {
        public string ID { get; protected set; }

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

        public LeaderboardElement(PlayerLeaderboardEntry entry) : this(entry.PlayFabId, entry.Position, entry.DisplayName, entry.StatValue)
        {

        }
        public LeaderboardElement(string ID, int position, string displayName, int value)
        {
            this.ID = ID;
            this.Position = position;
            this.DisplayName = displayName;
            this.Value = value;
        }
    }
}