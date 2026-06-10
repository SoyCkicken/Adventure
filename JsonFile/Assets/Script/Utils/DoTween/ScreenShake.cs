// ScreenShake.cs
// DOTween 기반 화면 흔들림 유틸리티
// - RectTransform(배경 UI) 흔들기
// - Transform(카메라/월드 오브젝트) 흔들기
// 중복 호출 시 이전 트윈을 안전하게 종료하고 재시작한다.

using UnityEngine;
using DG.Tweening;

public class ScreenShake : MonoBehaviour
{
    [Header("흔들 대상 (둘 중 하나만 세팅)")]
    [Tooltip("UI 배경처럼 RectTransform을 흔들고 싶을 때 지정")]
    public RectTransform uiTarget;

    [Tooltip("카메라나 빈 GameObject(카메라의 부모)를 흔들고 싶을 때 지정")]
    public Transform worldTarget;

    [Header("기본 파라미터")]
    [Tooltip("흔들리는 시간(초)")]
    public float defaultDuration = 0.2f;

    [Tooltip("흔들림 강도 (UI: anchoredPosition 픽셀 단위 / 월드: 유닛 단위)")]
    public float defaultStrength = 20f;

    [Tooltip("진동 횟수(값이 높을수록 촘촘히 털림)")]
    public int defaultVibrato = 30;

    [Tooltip("흔들리는 방향의 무작위성 (0~180)")]
    [Range(0f, 180f)] public float defaultRandomness = 90f;

    [Tooltip("Time.timeScale 무시 여부 (연출은 보통 true 추천)")]
    public bool ignoreTimeScale = true;

    // 내부 상태
    private Tweener _activeTweener;
    private Vector3 _uiOriginalAnchoredPos;
    private Vector3 _worldOriginalLocalPos;

    void Awake()
    {
        // 원위치 저장 (씬 시작 시점 기준)
        if (uiTarget != null)
            _uiOriginalAnchoredPos = uiTarget.anchoredPosition;

        if (worldTarget != null)
            _worldOriginalLocalPos = worldTarget.localPosition;
    }

    /// <summary>
    /// 기본 파라미터로 흔들기
    /// </summary>
    public void Shake() => Shake(defaultDuration, defaultStrength, defaultVibrato, defaultRandomness);

    /// <summary>
    /// 커스텀 파라미터로 흔들기
    /// </summary>
    public void Shake(float duration, float strength, int vibrato = 30, float randomness = 90f)
    {
        // 진행 중인 트윈 종료 및 원위치 보정
        KillActiveTweenerAndRestore();

        if (uiTarget != null)
        {
            // UI인 경우: anchoredPosition을 흔든다 (RectTransform 전용)
            // DOShakeAnchorPos : 직관적, 픽셀 단위
            _activeTweener = uiTarget.DOShakeAnchorPos(duration, strength, vibrato, randomness, true, true)
                .SetUpdate(ignoreTimeScale) // 타임스케일 무시(연출용 추천)
                .OnComplete(() => uiTarget.anchoredPosition = _uiOriginalAnchoredPos);
        }
        else if (worldTarget != null)
        {
            // 월드/카메라인 경우: localPosition 흔들기
            // DOShakePosition 은 월드/로컬 전환 불가 -> DoLocalMove를 이용한 프리셋 대신
            // DOTween의 DOShakePosition(transform) 오버로드는 월드 좌표 기반.
            // 카메라 루트(빈 오브젝트)를 흔드는 걸 추천. 직접 흔들면 후처리 카메라(Post)와 충돌 적음.
            _activeTweener = worldTarget.DOShakePosition(duration, strength, vibrato, randomness, false, true)
                .SetUpdate(ignoreTimeScale)
                .OnComplete(() => worldTarget.localPosition = _worldOriginalLocalPos);
        }
        else
        {
            Debug.LogWarning("[ScreenShake] uiTarget/worldTarget 둘 다 비어있음. 아무것도 흔들 수 없음.");
        }
    }

    /// <summary>
    /// 강하게(피격/크리 등) 흔들기 예시 프리셋
    /// </summary>
    public void ShakeHard()
    {
        Shake(duration: 0.25f, strength: defaultStrength * 1.5f, vibrato: defaultVibrato + 10, randomness: defaultRandomness);
    }

    /// <summary>
    /// 트윈 정리 + 원위치 복구
    /// </summary>
    private void KillActiveTweenerAndRestore()
    {
        if (_activeTweener != null && _activeTweener.IsActive())
        {
            _activeTweener.Kill(false);
            _activeTweener = null;

            if (uiTarget != null) uiTarget.anchoredPosition = _uiOriginalAnchoredPos;
            if (worldTarget != null) worldTarget.localPosition = _worldOriginalLocalPos;
        }
    }

    /// <summary>
    /// 외부에서 안전하게 원상복귀할 때 호출 (씬 전환 등)
    /// </summary>
    public void Restore()
    {
        KillActiveTweenerAndRestore();
    }
}
