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
        public List<Element> List { get; protected set; }

        public int Count => List.Count;

        public Element this[int index] => List[index];
        #endregion

        public bool IsProcessing
        {
            get
            {
                for (int i = 0; i < Count; i++)
                    if (this[i].IsProcessing)
                        return true;

                return false;
            }
        }

        public abstract class Element : Module
        {
            public abstract string ID { get; }

            #region List
            public List<LeaderboardEntry> List { get; protected set; }

            public int Count => List.Count;

            public LeaderboardEntry this[int index] => List[index];
            #endregion

            public bool IsProcessing { get; protected set; }

            public override void Configure(LeaderboardModule reference)
            {
                base.Configure(reference);

                IsProcessing = false;

                List = new List<LeaderboardEntry>();
            }

            public virtual void Clear()
            {
                List.Clear();

                InvokeUpdate();
            }

            public virtual void Request()
            {
                IsProcessing = true;
            }

            protected virtual void Process(IList<PlayerLeaderboardEntry> result)
            {
                List.Clear();

                for (int i = 0; i < result.Count; i++)
                {
                    var instance = new LeaderboardEntry(result[i]);

                    List.Add(instance);
                }

                IsProcessing = false;

                InvokeUpdate();
            }

            protected virtual void InvokeError(PlayFabError error)
            {
                IsProcessing = false;
            }

            public MoeEvent<Element> OnUpdate { get; protected set; } = new MoeEvent<Element>();
            protected virtual void InvokeUpdate()
            {
                OnUpdate.Invoke(this);
            }
        }
        public abstract class Element<TRequest, TResult> : Element
            where TRequest : PlayFabRequestCommon
            where TResult : PlayFabResultCommon
        {
            public abstract IList<PlayerLeaderboardEntry> ResultToList(TResult result);

            protected virtual void ResponseCallback(TResult result, PlayFabError error)
            {
                if (error == null)
                {
                    var request = result.Request as TRequest;

                    if(CheckID(request))
                    {
                        var list = ResultToList(result);

                        Process(list);
                    }
                }
                else
                    InvokeError(error);
            }

            protected abstract bool CheckID(TRequest request);
        }

        public IEnumerable<LeaderboardEntry> IEnumerate() => IEnumerate(true);
        public IEnumerable<LeaderboardEntry> IEnumerate(bool ignoreDuplicates)
        {
            var hash = new HashSet<string>();

            for (int x = 0; x < Count; x++)
            {
                for (int y = 0; y < this[x].Count; y++)
                {
                    if (ignoreDuplicates && hash.Contains(this[x][y].ID)) continue;

                    hash.Add(this[x][y].ID);

                    yield return this[x][y];
                }
            }
        }

        public class Module : Core.Module<LeaderboardModule>
        {
            public LeaderboardModule Leaderboard => Reference;
        }

        public PlayFabCore PlayFab => Core.PlayFab;
        
        public override void Configure(LeaderboardsCore reference)
        {
            base.Configure(reference);

            List = this.GetAllDependancies<Element>();

            PlayFab.Login.OnResult.Add(LoginResultCallback);
            PlayFab.OnLogout.Add(LogoutCallback);
            PlayFab.Player.Statistics.Update.OnResult.Add(PlayerUpdateStatisticCallback);

            References.Configure(this);
        }

        public override void Init()
        {
            base.Init();

            for (int i = 0; i < Count; i++)
            {
                this[i].OnUpdate.Add(ElementUpdateCalllback);
            }

            References.Init(this);
        }

        public virtual void Request()
        {
            if (PlayFab.IsLoggedIn == false)
            {
                Debug.LogWarning("Can't request leaderboard " + ID + " when the player isn't logged in, ignoring");
                return;
            }

            for (int i = 0; i < Count; i++)
                this[i].Request();
        }

        public virtual void Clear()
        {
            for (int i = 0; i < Count; i++)
                this[i].Clear();
        }

        #region Callbacks
        private void LoginResultCallback(LoginResult result)
        {
            Request();
        }

        private void LogoutCallback()
        {
            InvokeUpdate();
        }

        Coroutine PlayerUpdateStatisticCoroutine;
        void PlayerUpdateStatisticCallback(UpdatePlayerStatisticsResult result)
        {
            var request = result.Request as UpdatePlayerStatisticsRequest;

            bool HasID(StatisticUpdate update) => update.StatisticName == ID;

            if (request.Statistics.Any(HasID))
            {
                if (PlayerUpdateStatisticCoroutine != null) StopCoroutine(PlayerUpdateStatisticCoroutine);

                PlayerUpdateStatisticCoroutine = StartCoroutine(Procedure());

                IEnumerator Procedure()
                {
                    yield return new WaitForSecondsRealtime(1f); //Just to make sure that the playfab server has updated the leaderboard

                    if(PlayFab.IsLoggedIn) Request();

                    PlayerUpdateStatisticCoroutine = null;
                }
            }
        }

        void ElementUpdateCalllback(Element element)
        {
            if(IsProcessing)
            {

            }
            else
            {
                InvokeUpdate();
            }
        }

        public event RestDelegates.Error<PlayFabError> OnError;
        protected virtual void ErrorCallback(PlayFabError error)
        {
            OnError?.Invoke(error);
        }
        #endregion

        public event RestDelegates.Result<LeaderboardModule> OnUpdate;
        protected virtual void InvokeUpdate()
        {
            OnUpdate?.Invoke(this);
        }
    }

    [Serializable]
    public class LeaderboardEntry
    {
        public string ID { get; protected set; }

        public int Position { get; protected set; }

        public string DisplayName { get; protected set; }

        public int Value { get; protected set; }

        public LocationModel Location { get; protected set; }

        public static class Comparisons
        {
            public class Position : IComparer<LeaderboardEntry>
            {
                public static Position Instance = new Position();

                public int Compare(LeaderboardEntry x, LeaderboardEntry y)
                {
                    return x.Position - y.Position;
                }
            }
        }

        public LeaderboardEntry(PlayerLeaderboardEntry entry) : this(entry.PlayFabId, entry.Position, entry.DisplayName, entry.StatValue, entry.Profile.Locations.FirstOrDefault())
        {

        }
        public LeaderboardEntry(string ID, int position, string displayName, int value, LocationModel location)
        {
            this.ID = ID;
            this.Position = position;
            this.DisplayName = displayName;
            this.Value = value;
            this.Location = location;
        }
    }
}