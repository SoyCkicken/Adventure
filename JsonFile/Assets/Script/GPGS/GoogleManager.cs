using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TMPro;
using UnityEngine.UI;

public class GoogleManager : MonoBehaviour
{
    public Canvas canvas; // 🎯 캔버스 (인스펙터 연결)
    public TextMeshProUGUI logText;
    public Button retryButton; // 🎯 재시도 버튼 (인스펙터 연결)

    private int failCount = 0;
    private const int maxFailCount = 3;


    void Start()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        
#if UNITY_EDITOR
        // 에디터에서는 자동 로그인
        //if (!canvas.gameObject.activeSelf)
        //    canvas.gameObject.SetActive(false); // 캔버스 비활성화
        //logText.text = "에디터 모드에서는 자동 로그인됩니다.";
        //failCount = 0; // 실패 횟수 초기화
#elif UNITY_ANDROID
        SignIn();
#endif


        // 재시도 버튼 숨기기
        if (retryButton != null)
            retryButton.gameObject.SetActive(false);
    }

    public void SignIn()
    {
        if (retryButton != null)
            retryButton.gameObject.SetActive(false); // 로그인 시도 중 버튼 숨김

        PlayGamesPlatform.Instance.Authenticate((SignInStatus result) =>
        {
            Debug.LogError($"[GPGS] 로그인 결과: {result}");

            if (result == SignInStatus.Success)
            {
                string name = PlayGamesPlatform.Instance.GetUserDisplayName();
                string id = PlayGamesPlatform.Instance.GetUserId();
                string imgUrl = PlayGamesPlatform.Instance.GetUserImageUrl();

                logText.text = "로그인 성공: " + name;
                Debug.Log($"[GPGS] 이름: {name}, ID: {id}, 이미지URL: {imgUrl}");
                failCount = 0; // 실패 횟수 초기화
            }
            else
            {
                if (!canvas.gameObject.activeSelf)

                    canvas.gameObject.SetActive(true); // 캔버스 활성화
                retryButton.gameObject.SetActive(true); // 재시도 버튼 활성화
                failCount++;
                logText.text = $"로그인 실패 ({failCount}/{maxFailCount})";

                if (failCount >= maxFailCount)
                {
                    logText.text = "로그인 실패 횟수 초과. 게임을 종료합니다.";
#if UNITY_EDITOR
                    //UnityEditor.EditorApplication.isPlaying = false;
                    canvas.gameObject.SetActive(false); // 캔버스 숨김
#else
                    Application.Quit();
#endif
                }
                else
                {
                    // 재시도 버튼 표시
                    if (retryButton != null)
                    {
                        retryButton.gameObject.SetActive(true);
                        retryButton.onClick.RemoveAllListeners();
                        retryButton.onClick.AddListener(SignIn);
                    }
                }
            }
        });
    }
}