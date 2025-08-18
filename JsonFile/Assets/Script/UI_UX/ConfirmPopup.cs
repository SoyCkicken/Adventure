using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmPopup : MonoBehaviour
{
    public static ConfirmPopup Instance;

    [Header("UI Components")]
    public TextMeshProUGUI messageText;
    public Button yesButton;
    public Button noButton;

    private void Awake()
    {
        gameObject.SetActive(true);
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 기본 확인 팝업. yes/no 라벨을 지정할 수 있게 확장.
    /// </summary>
    public static void Show(string message, Action onConfirm, bool showNoButton = true,
                            string yesLabel = "예", string noLabel = "아니오") // ← 라벨 파라미터 추가
    {
        if (Instance == null)
        {
            Debug.LogError("[ConfirmPopup] 프리팹이 씬에 존재하지 않습니다.");
            return;
        }

        Instance.gameObject.SetActive(true);
        Instance.messageText.text = message;

        // 라벨 설정 (버튼의 자식 TMP 텍스트 찾아서 변경)
        var yesText = Instance.yesButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (yesText) yesText.text = yesLabel;
        var noText = Instance.noButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (noText) noText.text = noLabel;

        // 기존 리스너 제거
        Instance.yesButton.onClick.RemoveAllListeners();
        Instance.noButton.onClick.RemoveAllListeners();

        Instance.noButton.gameObject.SetActive(showNoButton);

        // 예 버튼
        Instance.yesButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            Instance.gameObject.SetActive(false);
        });

        // 아니오 버튼
        Instance.noButton.onClick.AddListener(() =>
        {
            Instance.gameObject.SetActive(false);
        });
    }

    /// <summary>
    /// OK 한 개만 있는 정보 팝업(안내/에러 메시지 등).
    /// </summary>
    public static void ShowInfo(string message, string okLabel = "확인")
    {
        Show(message, null, false, okLabel, ""); // No 버튼 숨기고 Yes 라벨만 바꿔서 사용
        Instance.yesButton.onClick.RemoveAllListeners();
        Instance.yesButton.onClick.AddListener(() => Instance.gameObject.SetActive(false));
    }
}
