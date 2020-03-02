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

using Newtonsoft.Json;

namespace Game
{
    public partial class PlayFabCore
    {
        [Serializable]
        public class TitleProperty : Property
        {
            [SerializeField]
            protected LeaderboardsProperty leaderboards;
            public LeaderboardsProperty Leaderboards { get { return leaderboards; } }
            [Serializable]
            public class LeaderboardsProperty : Property
            {
                public GetRequest Get { get; protected set; }
                public class GetRequest : Request<GetLeaderboardRequest, GetLeaderboardResult>
                {
                    public override MethodDelegate Method => PlayFabClientAPI.GetLeaderboard;

                    protected override GetLeaderboardRequest GenerateRequest()
                    {
                        var request = base.GenerateRequest();

                        request.ProfileConstraints = DefaultPlayerProfileConstraints;

                        return request;
                    }

                    public virtual void Request(string statistic) => Request(statistic, 5);
                    public virtual void Request(string statistic, int count)
                    {
                        var request = GenerateRequest();

                        request.StatisticName = statistic;
                        request.MaxResultsCount = count;

                        Send(request);
                    }

                    protected override void ApplyDefaults(ref GetLeaderboardRequest request)
                    {
                        request.ProfileConstraints = DefaultPlayerProfileConstraints;
                    }
                }

                public GetAroundPlayerRequest GetAroundPlayer { get; protected set; }
                public class GetAroundPlayerRequest : Request<GetLeaderboardAroundPlayerRequest, GetLeaderboardAroundPlayerResult>
                {
                    public override MethodDelegate Method => PlayFabClientAPI.GetLeaderboardAroundPlayer;

                    protected override GetLeaderboardAroundPlayerRequest GenerateRequest()
                    {
                        var request = base.GenerateRequest();

                        request.ProfileConstraints = DefaultPlayerProfileConstraints;

                        return request;
                    }

                    public virtual void Request(string statistic, int count)
                    {
                        var request = GenerateRequest();

                        request.StatisticName = statistic;
                        request.MaxResultsCount = count;

                        Send(request);
                    }

                    protected override void ApplyDefaults(ref GetLeaderboardAroundPlayerRequest request)
                    {
                        request.ProfileConstraints = DefaultPlayerProfileConstraints;
                    }
                }

                public abstract class Request<TRequest, TResult> : PlayFabCore.Request<TRequest, TResult>
                    where TRequest : PlayFabRequestCommon, new()
                    where TResult : PlayFabResultCommon
                {
                    protected override TRequest GenerateRequest()
                    {
                        var request = base.GenerateRequest();

                        ApplyDefaults(ref request);

                        return request;
                    }

                    protected abstract void ApplyDefaults(ref TRequest request);
                }

                public static readonly PlayerProfileViewConstraints DefaultPlayerProfileConstraints = new PlayerProfileViewConstraints()
                {
                    ShowDisplayName = true,
                    ShowLocations = true,
                };

                public override void Configure(TitleProperty reference)
                {
                    base.Configure(reference);

                    Get = new GetRequest();
                    GetAroundPlayer = new GetAroundPlayerRequest();
                }
            }

            [SerializeField]
            protected CatalogProperty catalog;
            public CatalogProperty Catalog { get { return catalog; } }
            [Serializable]
            public class CatalogProperty : Property
            {
                [SerializeField]
                protected string name = "Default";
                public string Name { get { return name; } }

                [SerializeField]
                protected List<CatalogItem> items;
                public List<CatalogItem> Items { get { return items; } }

                public RetrieveRequest Retrieve { get; protected set; }
                public class RetrieveRequest : Request<GetCatalogItemsRequest, GetCatalogItemsResult>
                {
                    public override MethodDelegate Method => PlayFabClientAPI.GetCatalogItems;

                    public CatalogProperty Catalog { get; protected set; }

                    public virtual void Request()
                    {
                        var request = GenerateRequest();

                        request.CatalogVersion = Catalog.Name;

                        Send(request);
                    }

                    public RetrieveRequest(CatalogProperty catalog)
                    {
                        this.Catalog = catalog;
                    }
                }

                public override void Configure(TitleProperty reference)
                {
                    base.Configure(reference);

                    items = new List<CatalogItem>();

                    Retrieve = new RetrieveRequest(this);
                    Retrieve.OnResult.Add(RetrieveCallback);
                }

                private void RetrieveCallback(GetCatalogItemsResult result)
                {
                    items.Clear();

                    items.AddRange(result.Catalog);
                }
            }

            public DataRequest Data { get; protected set; }
            public class DataRequest : Request<GetTitleDataRequest, GetTitleDataResult>
            {
                public override MethodDelegate Method => PlayFabClientAPI.GetTitleData;

                public virtual void Request() => Query(null);

                public virtual void Request(params string[] keys)
                {
                    var list = keys.ToList();

                    Query(list);
                }

                public virtual void Request(IList<string> keys)
                {
                    var list = keys.ToList();

                    Query(list);
                }

                protected virtual void Query(List<string> keys)
                {
                    var request = GenerateRequest();

                    request.Keys = keys;

                    Send(request);
                }
            }

            public NewsRequest News { get; protected set; }
            public class NewsRequest : Request<GetTitleNewsRequest, GetTitleNewsResult>
            {
                public override MethodDelegate Method => PlayFabClientAPI.GetTitleNews;

                public virtual void Request() => Request(10);
                public virtual void Request(int count)
                {
                    var request = GenerateRequest();

                    request.Count = count;

                    Send(request);
                }
            }

            [Serializable]
            public class Property : Core.Property<TitleProperty>
            {
                public TitleProperty Title => Reference;

                public PlayFabCore PlayFab => Title.PlayFab;
            }

            public override void Configure(PlayFabCore reference)
            {
                base.Configure(reference);

                Register(this, leaderboards);
                Register(this, catalog);

                Data = new DataRequest();
                News = new NewsRequest();
            }
        }
    }
}