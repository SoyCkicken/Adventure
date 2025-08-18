/*
 * BattleImageDoTween.cs
 * - 전투 중 공격 연출(이미지 생성/이동/스케일/페이드/파괴) + 화면 흔들림을 DOTween으로 제어
 * - 적/플레이어 각각 이펙트 메서드 제공
 * - Destroy 타이밍과 흔들림 타이밍을 분리(DOVirtual.DelayedCall 사용)
 * 
 * 의존:
 *   - DOTween (using DG.Tweening)
 *   - 선택: ScreenShake (내가 이전에 준 스크립트. 없으면 shake 관련 라인만 주석 처리)
 *
 * 연결 포인트(예시):
 *   - BossPartCombatManager / 전투 매니저에서 적 공격 성공 시 → PlayEnemyAttackEffect(...)
 *   - 플레이어 공격 성공 시 → PlayPlayerAttackEffect(...)
 *   - 크리티컬/미스 → PlayCritEffect/PlayMissEffect 추가 호출
 */

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleImageDoTween : MonoBehaviour
{
    [Header("==== Targets ====")]
    [Tooltip("배경 또는 이펙트를 붙일 부모(보통 전투 화면의 Image 최상위 또는 그 상위 컨테이너)")]
    [SerializeField] private Transform fxParent;

    [Tooltip("UI 배경(RectTransform). 이걸 흔들면 자식인 적 이미지도 같이 흔들림")]
    [SerializeField] private RectTransform backgroundRect; // 배경 이미지 오브젝트(네가 말한 '배경 이미지 오브젝트')

    [Tooltip("화면 흔들림 유틸(선택). 없으면 null로 두고 흔들기 관련 코드만 주석 처리")]
    [SerializeField] private ScreenShake screenShake;

    [Header("==== Prefabs ====")]
    [Tooltip("적이 플레이어를 공격할 때 생성되는 이펙트 프리팹(예: 칼베기 이미지)")]
    [SerializeField] private GameObject enemyAttackImagePrefab;

    [Tooltip("플레이어가 적을 공격할 때 생성되는 이펙트 프리팹")]
    [SerializeField] private GameObject playerAttackImagePrefab;

    [Tooltip("크리티컬 텍스트/이미지 프리팹(선택)")]
    [SerializeField] private GameObject criticalPrefab;

    [Tooltip("MISS 텍스트/이미지 프리팹(선택)")]
    [SerializeField] private GameObject missPrefab;

    [Header("==== Common FX Settings ====")]
    [Tooltip("공격 이펙트의 시작 위치(로컬). 비워두면 배경의 중심(0,0)")]
    [SerializeField] private Vector2 startLocalPos = Vector2.zero;

    [Tooltip("공격 이펙트의 종료 위치(로컬). 예: 왼->오 이동 등")]
    [SerializeField] private Vector2 endLocalPos = new Vector2(120f, 0f);

    [Tooltip("초기 로컬 스케일(픽셀 아님, transform scale). 예: (100,100,1) 같은 비정상 수치 쓰지 말 것")]
    [SerializeField] private Vector3 startScale = Vector3.one;

    [Tooltip("히트 순간 스케일 펀치 양(상대값). 예: 0.2면 살짝 튐")]
    [SerializeField, Range(0f, 1.5f)] private float hitPunchScale = 0.2f;

    [Tooltip("이펙트 페이드 인 시간")]
    [SerializeField, Range(0f, 1f)] private float fadeIn = 0.05f;

    [Tooltip("이펙트 이동 시간")]
    [SerializeField, Range(0.05f, 2f)] private float moveDuration = 0.25f;

    [Tooltip("이펙트 총 표시 시간(파괴 시점). 흔들림도 이 타이밍에 맞출 수 있음")]
    [SerializeField, Range(0.1f, 3f)] private float lifetime = 1.0f;

    [Header("==== Shake Settings ====")]
    [Tooltip("히트 타이밍에 흔들림을 쓸지")]
    [SerializeField] private bool shakeOnHit = true;

    [Tooltip("흔들림 딜레이(초). Destroy 타이밍에 맞추고 싶으면 lifetime과 동일하게 세팅")]
    [SerializeField, Range(0f, 2f)] private float shakeDelay = 1.0f;

    [Tooltip("크리티컬 시 강하게 흔들기")]
    [SerializeField] private bool harderOnCrit = true;

    [Header("==== Optional: Anchors ====")]
    [Tooltip("플레이어/적 쪽 기준점. 비어 있으면 startLocalPos/endLocalPos 사용")]
    [SerializeField] private RectTransform playerAnchor; // 예: 플레이어 위치 근처 UI 빈 객체
    [SerializeField] private RectTransform enemyAnchor;  // 예: 적 위치 근처 UI 빈 객체

    private void Reset()
    {
        // 에디터에서 붙였을 때 자동으로 유추
        if (fxParent == null) fxParent = transform;
        if (backgroundRect == null) backgroundRect = GetComponent<RectTransform>();
        if (screenShake == null) screenShake = FindObjectOfType<ScreenShake>();
    }

    #region Public API (전투 매니저에서 호출)

    /// <summary>
    /// 적 → 플레이어 공격 연출
    /// </summary>
    public void PlayEnemyAttackEffect(bool isCrit = false)
    {
        var prefab = enemyAttackImagePrefab != null ? enemyAttackImagePrefab : playerAttackImagePrefab;
        if (prefab == null)
        {
            Debug.LogWarning("[BattleImageDoTween] enemyAttackImagePrefab가 비어있습니다.");
            return;
        }

        // 시작/끝 위치 계산 (앵커가 있으면 활용)
        Vector2 start = enemyAnchor ? enemyAnchor.anchoredPosition : startLocalPos;
        Vector2 end = playerAnchor ? playerAnchor.anchoredPosition : endLocalPos;

        SpawnAndAnimate(prefab, start, end, isCrit);
    }

    /// <summary>
    /// 플레이어 → 적 공격 연출
    /// </summary>
    public void PlayPlayerAttackEffect(bool isCrit = false)
    {
        var prefab = playerAttackImagePrefab != null ? playerAttackImagePrefab : enemyAttackImagePrefab;
        if (prefab == null)
        {
            Debug.LogWarning("[BattleImageDoTween] playerAttackImagePrefab가 비어있습니다.");
            return;
        }

        Vector2 start = playerAnchor ? playerAnchor.anchoredPosition : startLocalPos;
        Vector2 end = enemyAnchor ? enemyAnchor.anchoredPosition : endLocalPos;

        SpawnAndAnimate(prefab, start, end, isCrit);
    }

    /// <summary>
    /// 크리티컬 텍스트/이미지 (선택)
    /// </summary>
    public void PlayCritEffect(Vector2? anchoredPos = null)
    {
        if (criticalPrefab == null) return;
        ShowFloatingText(criticalPrefab, anchoredPos ?? Vector2.zero, upward: true);
    }

    /// <summary>
    /// 미스 텍스트/이미지 (선택)
    /// </summary>
    public void PlayMissEffect(Vector2? anchoredPos = null)
    {
        if (missPrefab == null) return;
        ShowFloatingText(missPrefab, anchoredPos ?? Vector2.zero, upward: false);
    }

    #endregion

    #region Core

    /// <summary>
    /// 프리팹을 생성하고, 페이드인 → 이동 → 수명 종료 시 파괴 + 흔들림 지연 호출
    /// </summary>
    private void SpawnAndAnimate(GameObject prefab, Vector2 startAnchored, Vector2 endAnchored, bool isCrit)
    {
        if (fxParent == null) fxParent = transform;

        // 프리팹 생성 (UI 기준)
        var go = Instantiate(prefab, fxParent);
        var rt = go.transform as RectTransform;
        if (rt == null)
        {
            // UI가 아니라 월드 오브젝트면 월드 좌표로 처리
            go.transform.SetParent(fxParent, worldPositionStays: false);
            go.transform.localPosition = startAnchored; // 주의: 이 경우 값이 유닛 단위
        }
        else
        {
            rt.anchoredPosition = startAnchored;
        }

        // 초기 스케일 합리화: (1,1,1) 권장. (100,100,0)은 잘못된 값이다.
        go.transform.localScale = startScale.sqrMagnitude < 0.0001f ? Vector3.one : startScale;

        // 투명도 0에서 시작(가능하면 이미지/캔버스그룹 사용)
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // 1) 페이드 인
        cg.DOFade(1f, fadeIn).SetUpdate(true);

        // 2) 이동 (UI면 anchoredPosition, 월드면 localPosition)
        if (rt != null)
        {
            rt.DOAnchorPos(endAnchored, moveDuration).SetEase(Ease.OutCubic).SetUpdate(true);
        }
        else
        {
            go.transform.DOLocalMove(new Vector3(endAnchored.x, endAnchored.y, 0f), moveDuration)
                .SetEase(Ease.OutCubic).SetUpdate(true);
        }

        // 3) 히트 순간 펀치 (이동 끝나는 시점에 가볍게)
        DOVirtual.DelayedCall(moveDuration, () =>
        {
            if (hitPunchScale > 0f)
            {
                go.transform.DOPunchScale(Vector3.one * hitPunchScale, 0.1f, vibrato: 10, elasticity: 0.8f)
                    .SetUpdate(true);
            }

            // 크리티컬 텍스트/이펙트 (선택)
            if (isCrit && criticalPrefab != null)
            {
                var pos = rt ? rt.anchoredPosition : (Vector2)go.transform.localPosition;
                PlayCritEffect(pos + new Vector2(0f, 30f));
            }
        }).SetUpdate(true);

        // 4) 수명 종료 시 파괴
        Destroy(go, lifetime);

        // 5) 흔들림: Destroy 타이밍과 맞추거나 원하는 딜레이로
        if (shakeOnHit && backgroundRect != null)
        {
            // ScreenShake가 세팅되어 있으면 그걸 쓰고, 없으면 백업으로 배경만 가볍게 흔드는 로컬 구현
            if (screenShake == null) screenShake = FindObjectOfType<ScreenShake>();

            DOVirtual.DelayedCall(shakeDelay, () =>
            {
                if (screenShake != null)
                {
                    if (isCrit && harderOnCrit)
                        screenShake.ShakeHard();
                    else
                        screenShake.Shake();
                }
                else
                {
                    // 백업: ScreenShake가 없을 때, 배경 anchoredPosition을 잠깐 흔들었다 원복
                    var origin = backgroundRect.anchoredPosition;
                    backgroundRect.DOShakeAnchorPos(0.2f, 20f, 30, 90f, true, true)
                                  .SetUpdate(true)
                                  .OnComplete(() => backgroundRect.anchoredPosition = origin);
                }
            }).SetUpdate(true);
        }

        // 6) 페이드 아웃(선택) – 수명 끝나기 살짝 전에 서서히 사라지게
        if (lifetime > 0.2f)
        {
            var fadeOutStart = Mathf.Max(0.05f, lifetime - 0.15f);
            DOVirtual.DelayedCall(fadeOutStart, () =>
            {
                if (cg != null) cg.DOFade(0f, 0.12f).SetUpdate(true);
            }).SetUpdate(true);
        }
    }

    /// <summary>
    /// 떠오르는 텍스트/이미지 연출 (크리/미스 등). 생성 → 위로 이동 → 페이드아웃 → 파괴
    /// </summary>
    private void ShowFloatingText(GameObject prefab, Vector2 anchoredPos, bool upward)
    {
        if (prefab == null) return;

        var go = Instantiate(prefab, fxParent);
        var rt = go.transform as RectTransform;
        if (rt != null)
        {
            rt.anchoredPosition = anchoredPos;
        }
        else
        {
            go.transform.localPosition = anchoredPos;
        }
        go.transform.localScale = Vector3.one;

        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        float upDist = upward ? 60f : -40f;

        // 페이드 인
        cg.DOFade(1f, 0.05f).SetUpdate(true);

        // 위로(또는 아래로) 살짝 이동
        if (rt != null)
        {
            rt.DOAnchorPos(anchoredPos + new Vector2(0f, upDist), 0.25f)
              .SetEase(Ease.OutCubic).SetUpdate(true);
        }
        else
        {
            go.transform.DOLocalMove(anchoredPos + new Vector2(0f, upDist), 0.25f)
              .SetEase(Ease.OutCubic).SetUpdate(true);
        }

        // 잠깐 유지 후 페이드 아웃
        DOVirtual.DelayedCall(0.35f, () =>
        {
            cg.DOFade(0f, 0.15f).SetUpdate(true);
        }).SetUpdate(true);

        Destroy(go, 0.55f);
    }

    #endregion

    #region Editor Helpers

#if UNITY_EDITOR
    [ContextMenu("Test Enemy Attack")]
    private void _TestEnemy()
    {
        PlayEnemyAttackEffect(isCrit: false);
    }

    [ContextMenu("Test Enemy Attack (Crit)")]
    private void _TestEnemyCrit()
    {
        PlayEnemyAttackEffect(isCrit: true);
    }

    [ContextMenu("Test Player Attack")]
    private void _TestPlayer()
    {
        PlayPlayerAttackEffect(isCrit: false);
    }

    [ContextMenu("Test Player Attack (Crit)")]
    private void _TestPlayerCrit()
    {
        PlayPlayerAttackEffect(isCrit: true);
    }
#endif

    #endregion
}
