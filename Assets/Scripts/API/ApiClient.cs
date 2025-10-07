using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class ApiClient
{
    public static string BaseUrl = "https://wemwcwblxrzqrohaganp.supabase.co/functions/v1/make-server-5f58455d";

    public static IEnumerator Post(string endpoint, string jsonBody, string token, Action<string> onSuccess, Action<string> onError)
    {
        using var request = new UnityWebRequest(BaseUrl + endpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(token))
            request.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(request.downloadHandler.text);
        else
            onError?.Invoke(request.error + ": " + request.downloadHandler.text);
    }
}
