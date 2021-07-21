using Sludge.Shared;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Networking;

namespace Sludge.Backend
{
    public static class Server
    {
        const string ServerUrl = "http://localhost:7071/api/mylevels/api"; // localhost
        //const string ServerUrl = "https://sludge-backend.azurewebsites.net/api"; // live

        public static string Token;
        public static string LatestResponse;
        public static string LatestError;

        public static List<LevelData> GetMyLevels()
        {
            //string json = Get("mylevels");
            return null;
        }

        public static void GetToken()
        {

        }

        public static IEnumerator Get(string function, params (string key, string value)[] param)
        {
            // ?user=123&
            string queryParam = string.Join("&", param.Select(p => $"{p.key}={p.value}"));

            LatestError = null;
            var req = UnityWebRequest.Get($"{ServerUrl}/{function}?token={Token}&{queryParam}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
                LatestError = req.error;
            else
                LatestResponse = req.downloadHandler.text;
        }

        public static void test()
        {
            Get("mylevels");
        }
    }
}
