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
        // 1) 정책 체크
        if (!AdRewardPolicy.Instance.CanShow(out var reason, out var wait))
        {
            ConfirmPopup.ShowInfo(reason ?? "광고를 지금은 볼 수 없습니다.");
            return;
        }

        // 2) 사용자 확인
        ConfirmPopup.Show(
            message: $"광고 시청으로 <color=#FFD54F>{rewardAmount}</color> 골드를 받으시겠습니까?",
            onConfirm: TryShowRewarded,
            showNoButton: true, yesLabel: "시청", noLabel: "취소"
        );
    }


    void TryShowRewarded()
    {
        bool shown = AdMobManager.Instance.ShowRewarded(success =>
        {
            if (success)
            {
                AdRewardPolicy.Instance.RecordShown(); // ★ 성공 시 기록
                                                       // 보상 지급 로직…
            }
            else
            {
                ConfirmPopup.ShowInfo("광고 시청이 완료되지 않았습니다.");
            }
        });

        if (!shown)
            ConfirmPopup.ShowInfo("광고가 아직 준비되지 않았습니다. 잠시 후 다시 시도해 주세요.");
    }
}
