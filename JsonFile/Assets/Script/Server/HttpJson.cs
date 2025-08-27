using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class HttpJson
{
    // POST JSON -> JSON 응답 문자열 반환
    public static IEnumerator PostJson(string url, string json, System.Action<string> onSuccess, System.Action<string> onError)
    {
        byte[] body = Encoding.UTF8.GetBytes(json);
        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

#if UNITY_2020_3_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                onError?.Invoke(req.error);
            }
            else
            {
                onSuccess?.Invoke(req.downloadHandler.text);
            }
        }
    }

    // GET -> JSON 응답 문자열 반환
    public static IEnumerator Get(string url, System.Action<string> onSuccess, System.Action<string> onError)
    {
        using (var req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

#if UNITY_2020_3_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                onError?.Invoke(req.error);
            }
            else
            {
                onSuccess?.Invoke(req.downloadHandler.text);
            }
        }
    }
}
