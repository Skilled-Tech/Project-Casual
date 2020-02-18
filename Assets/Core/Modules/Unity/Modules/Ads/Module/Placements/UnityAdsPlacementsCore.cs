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
	public class UnityAdsPlacementsCore : UnityAdsCore.Module
	{
        #region List
        public List<UnityAdsPlacementModule> List { get; protected set; }

        public int Count => List.Count;
        public UnityAdsPlacementModule this[int index] => List[index];

        public UnityAdsPlacementModule Find(string ID)
        {
            for (int i = 0; i < List.Count; i++)
                if (List[i].ID == ID)
                    return List[i];

            return null;
        }
        #endregion

        [SerializeField]
        protected CommonData common;
        public CommonData Common { get { return common; } }
        [Serializable]
        public class CommonData
        {
            [SerializeField]
            protected UnityAdsPlacementModule video;
            public UnityAdsPlacementModule Video { get { return video; } }

            [SerializeField]
            protected UnityAdsPlacementModule rewardedVideo;
            public UnityAdsPlacementModule RewardedVideo { get { return rewardedVideo; } }
        }

        public class Module : Core.Module<UnityAdsPlacementsCore>
        {
            public UnityAdsPlacementsCore Placements => Reference;

            public UnityAdsCore Ads => Placements.Ads;
        }

        public override void Configure(UnityAdsCore reference)
        {
            base.Configure(reference);

            List = this.GetAllDependancies<UnityAdsPlacementModule>();

            References.Configure(this);
        }

        public override void Init()
        {
            base.Init();

            References.Init(this);
        }
    }
}