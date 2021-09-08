#if !UNITY_EDITOR
using Newtonsoft.Json;
#endif
using Sludge;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Analytics : MonoBehaviour
{
    public static Analytics Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SaveStats(RoundResult roundResult)
    {
#if UNITY_EDITOR
#else
        string json = JsonConvert.SerializeObject(roundResult);
        StartCoroutine(StoreEvents(json));
#endif
    }

    public static long GetWorldWideAttempts()
    {
        using var request = UnityWebRequest.Get("https://sludgefunctions.azurewebsites.net/api/world-wide-attempts");
        string response = request.downloadHandler.text;
        if (long.TryParse(response, out long totalAttempts))
            return totalAttempts;

        Debug.LogWarning($"Getting world wide attempts returned invalid data: '{response}'");
        return 0;
    }

    IEnumerator StoreEvents(string json)
    {
        using var request = new UnityWebRequest("https://sludgefunctions.azurewebsites.net/api/store-events");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.method = UnityWebRequest.kHttpVerbPOST;
        yield return request.SendWebRequest();
    }
}
