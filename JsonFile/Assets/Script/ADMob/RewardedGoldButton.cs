// RewardedWithPopupButton.cs
// 버튼 클릭 → "광고 보시겠습니까?" 확인창 → 시청 성공 시 보상 지급
using UnityEngine;
using UnityEngine.UI;

public class RewardedWithPopupButton : MonoBehaviour
{
    [Header("보상 설정")]
    public int rewardAmount = 1000;

    [Header("UI")]
    public Button button;

    bool busy;

    void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (button) button.onClick.AddListener(OnClick);
    }

    void OnDestroy()
    {
        if (button) button.onClick.RemoveListener(OnClick);
    }

    void OnClick()
    {
        if (busy) return;

        ConfirmPopup.Show(
            message: $"광고를 시청하고\n<color=#FFD54F>{rewardAmount}</color> 골드를 받으시겠습니까?",
            onConfirm: TryShowRewarded,
            showNoButton: true,
            yesLabel: "시청", noLabel: "취소"
        );
    }

    void TryShowRewarded()
    {
        if (busy) return;
        busy = true;
        if (button) button.interactable = false;

        bool shown = AdMobManager.Instance.ShowRewarded(success =>
        {
            busy = false;
            if (button) button.interactable = true;

            if (success)
            {
                // TODO: 네 게임의 보상 지급 로직
                // 예) PlayerState.Instance.AddGold(rewardAmount);
                Debug.Log($"[Rewarded] +{rewardAmount} 지급");
                ConfirmPopup.ShowInfo($"보상이 지급되었습니다.\n+{rewardAmount}");
                PlayerState.Instance.AddGold(rewardAmount); // 플레이어 상태에 골드 추가
            }
            else
            {
                ConfirmPopup.ShowInfo("광고 시청이 완료되지 않아 보상이 지급되지 않았습니다.");
            }
        });

        if (!shown)
        {
            busy = false;
            if (button) button.interactable = true;
            ConfirmPopup.ShowInfo("광고가 아직 준비되지 않았습니다.\n잠시 후 다시 시도해 주세요.");
        }
    }
}
