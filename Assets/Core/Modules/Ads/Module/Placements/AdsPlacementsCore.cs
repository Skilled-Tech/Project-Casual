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
	public class AdsPlacementsCore : AdsCore.Module
	{
        #region List
        public List<AdsPlacementModule> List { get; protected set; }

        public int Count => List.Count;
        public AdsPlacementModule this[int index] => List[index];

        public AdsPlacementModule Find(string ID)
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
            protected AdsPlacementModule video;
            public AdsPlacementModule Video { get { return video; } }

            [SerializeField]
            protected AdsPlacementModule rewardedVideo;
            public AdsPlacementModule RewardedVideo { get { return rewardedVideo; } }
        }

        public class Module : Core.Module<AdsPlacementsCore>
        {
            public AdsPlacementsCore Placements => Reference;

            public AdsCore Ads => Placements.Ads;
        }

        public override void Configure(AdsCore reference)
        {
            base.Configure(reference);

            List = this.GetAllDependancies<AdsPlacementModule>();

            References.Configure(this);
        }

        public override void Init()
        {
            base.Init();

            References.Init(this);
        }
    }
}