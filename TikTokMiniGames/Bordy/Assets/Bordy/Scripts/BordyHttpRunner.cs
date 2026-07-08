using System;
using System.Collections;
using UnityEngine;

namespace Bordy
{
    /// <summary>Runs coroutines for static services (cloud HTTP). / 为静态服务跑协程。</summary>
    internal class BordyHttpRunner : MonoBehaviour
    {
        private static BordyHttpRunner _instance;

        public static void Run(IEnumerator routine)
        {
            if (_instance == null)
            {
                var go = new GameObject("BordyHttpRunner");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<BordyHttpRunner>();
            }
            _instance.StartCoroutine(routine);
        }
    }
}
