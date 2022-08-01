using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;


namespace Text2Scene
{
    /// <summary>
    /// Configurates the request for the TI
    /// </summary>
    public static class TICommunicator
    {
        public const string baseURL = "https://textimager.hucompute.org/rest";
        public const string languageURL = "/language";
        public const string processURL = "/process";

        public static IEnumerator TIRequest(string url, Action<string> onComplete, Action<string> onError)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();
                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    onError.Invoke(webRequest.error);
                    yield break;
                }
                if (webRequest.responseCode == 200L)
                    onComplete.Invoke(webRequest.downloadHandler.text);
            }
        }
    }
}