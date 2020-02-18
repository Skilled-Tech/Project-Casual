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
	public class UnityIAPPurchaseOperation : Operation
	{
        [SerializeField]
        protected string _ID;
        public string ID { get { return _ID; } }

        public override void Execute()
        {
            Core.Instance.Unity.IAP.Purchase(ID);
        }
    }
}