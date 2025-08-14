using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TMPro;

public class GoogleManager : MonoBehaviour
{
    public TextMeshProUGUI logText;

    void Start()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
#if UNITY_ANDROID
        SignIn();
#endif
    }

    public void SignIn()
    {
        //playgamesplatform.instance.authenticate((signinstatus success) =>
        //{
        //    if (success == signinstatus.success)
        //    {
        //        string name = playgamesplatform.instance.getuserdisplayname();
        //        string id = playgamesplatform.instance.getuserid();
        //        string imgurl = playgamesplatform.instance.getuserimageurl();

        //        logtext.text = "로그인 성공: " + name;
        //        debug.log($"[gpgs] 이름: {name}, id: {id}, 이미지url: {imgurl}");
        //    }
        //    else
        //    {
        //        logtext.text = "로그인 실패";
        //        debug.logerror("[gpgs] 로그인 실패");
        //    }
        //});

        PlayGamesPlatform.Instance.Authenticate((SignInStatus result) =>
        {
            Debug.LogError($"[GPGS] 로그인 결과: {result}");

            switch (result)
            {
                case SignInStatus.Success:
                    string name = PlayGamesPlatform.Instance.GetUserDisplayName();
                    string id = PlayGamesPlatform.Instance.GetUserId();
                    string ImgUrl = PlayGamesPlatform.Instance.GetUserImageUrl();

                    logText.text = "로그인 성공: " + name;
                    Debug.Log($"[GPGS] 이름: {name}, ID: {id}, 이미지URL: {ImgUrl}");
                    //Debug.LogError("개발자 오류 - OAuth 클라이언트 ID가 잘못되었거나 SHA-1이 안 맞음");
                    break;
                case SignInStatus.InternalError:
                    Debug.LogError("GPGS 내부 오류");
                    break;
                default:
                    Debug.LogError($"정의되지 않은 에러: {result}");
                    break;
            }
        });
    }


    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            // Continue with Play Games Services
            // Perfectly login success

            string name = PlayGamesPlatform.Instance.GetUserDisplayName();
            string id = PlayGamesPlatform.Instance.GetUserId();
            string ImgUrl = PlayGamesPlatform.Instance.GetUserImageUrl();

            logText.text = "성공입니다 \n" + name;
        }
        else
        {
            logText.text = "Sign in Failed!";
            // Disable your integration with Play Games Services or show a login button
            // to ask users to sign-in. Clicking it should call
            //PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication).
            // Login failed
        }
    }
}