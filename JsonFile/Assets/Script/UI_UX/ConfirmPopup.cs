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

    public static void Show(string message, Action onConfirm)
    {
        if (Instance == null)
        {
            Debug.LogError("[ConfirmPopup] 프리팹이 씬에 존재하지 않습니다.");
            return;
        }

        Instance.gameObject.SetActive(true);
        Instance.messageText.text = message;

        // 기존 리스너 제거
        Instance.yesButton.onClick.RemoveAllListeners();
        Instance.noButton.onClick.RemoveAllListeners();

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
}
