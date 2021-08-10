using Newtonsoft.Json;
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
        string json = JsonConvert.SerializeObject(roundResult);
        StartCoroutine(StoreEvents(json));
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
