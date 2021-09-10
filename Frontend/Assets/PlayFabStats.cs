using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabStats : MonoBehaviour
{
    [DllImport("__Internal")]
    public static extern string GetURLFromPage();

    public static PlayFabStats Instance;

    public bool LoginWhenInEditor = true;

    [NonSerialized] public object LastResult;
    [NonSerialized] public string LastError;
    [NonSerialized] public bool LoginProcessComplete;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (Application.isEditor && !LoginWhenInEditor)
        {
            return;
        }

        StartCoroutine(InitializePlayFab());
    }

    public IEnumerator InitializePlayFab()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            LoginProcessComplete = true;
            yield break; // Already logged in
        }

        Debug.Log("PlayFab: Login...");
        yield return DoLoginCo();
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            LoginProcessComplete = true;
            yield break;
        }

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            yield return LogPlayerData(
                new Dictionary<string, string> {
            { "hosting_url", Application.absoluteURL },
            { "page_top_url", GetURLFromPage() }
            });
        }

        LoginProcessComplete = true;
    }

    public IEnumerator StorePlayerProgression(double value)
    {
        if (!LoginProcessComplete)
            yield break;

        yield return LogPlayerData(
            new Dictionary<string, string> {
            { "progression", $"{value:0.00}" },
        });
    }

    string GetUserId()
    {
        var userId = GameManager.ClientId;
        return userId;
    }

    void DoCustomLogin(Action<LoginResult> onsuccess, Action<PlayFabError> onError)
    {
        LoginWithCustomIDRequest request = new LoginWithCustomIDRequest
        {
            TitleId = PlayFabSettings.TitleId,
            CustomId = GetUserId(),
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, onsuccess, onError);
    }

    public void LogPlayerDataAsync(string key, string value, UserDataPermission permission = UserDataPermission.Private)
    {
        StartCoroutine(LogPlayerData(new Dictionary<string, string> { { key, value } }));
    }

    public IEnumerator LogPlayerData(Dictionary<string, string> pairs, UserDataPermission permission = UserDataPermission.Private)
    {
        UpdateUserDataRequest req = new UpdateUserDataRequest();
        {
            req.Data = pairs;
            req.Permission = permission;
        };

        Action<Action<UpdateUserDataResult>, Action<PlayFabError>> apiCall = (onsuccess, onError) =>
        {
            PlayFabClientAPI.UpdateUserData(req, onsuccess, onError);
        };

        yield return ExecuteApiCallWithRetry(apiCall);
    }

    public IEnumerator DoLoginCo()
    {
        Action<Action<LoginResult>, Action<PlayFabError>> apiCall;

        switch (Application.platform)
        {
            case RuntimePlatform.WebGLPlayer:
                apiCall = DoCustomLogin;
                break;

            default:
                apiCall = DoCustomLogin;
                break;
        }

        yield return ExecuteApiCallWithRetry(apiCall);
    }

    IEnumerator ExecuteApiCallWithRetry<TResult>(Action<Action<TResult>, Action<PlayFabError>> apiAction)
    {
        LastResult = null;
        LastError = null;

        string name = typeof(TResult).Name;
        float startTime = Time.realtimeSinceStartup;
        float timeWaited = 0;
        TResult result = default;

        bool callComplete = false;

        Action<TResult> onSuccess = callResult =>
        {
            float timeTotal = Time.realtimeSinceStartup - startTime;
            Debug.Log($"PlayFab: Request succesful ({name}), ms = " + timeTotal * 1000);
            result = callResult;
            callComplete = true;
        };

        Action<PlayFabError> onError = error =>
        {
            string fullMsg = error.ErrorMessage;
            if (error.ErrorDetails != null)
                foreach (var pair in error.ErrorDetails)
                    foreach (var eachMsg in pair.Value)
                        fullMsg += "\n" + pair.Key + ": " + eachMsg;

            Debug.LogError($"PlayFab: Request ({name}) failed: {fullMsg}");
            LastError = fullMsg;
            callComplete = true;
        };

        Debug.Log($"PlayFab: Sending request...{name}");
        apiAction(onSuccess, onError);

        while (!callComplete)
        {
            yield return null;
            timeWaited = Time.realtimeSinceStartup - startTime;
        }

        timeWaited = Time.realtimeSinceStartup - startTime;
        LastResult = result;
    }
}