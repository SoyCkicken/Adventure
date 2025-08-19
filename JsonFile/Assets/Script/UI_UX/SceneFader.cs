/*
 * SceneFader.cs
 * - 전역 화면 페이드 인/아웃 컨트롤러
 * - 런타임에 자체 Canvas + Image를 생성하여 오버레이로 덮음
 * - DOTween으로 CanvasGroup 알파만 제어 (가볍고 확실함)
 * - 페이드 중 입력 차단(GraphicRaycaster + Image raycastTarget = true)
 * - SetUpdate(true)로 타임스케일 무시 → 로딩/일시정지 중에도 부드럽게 동작
 *
 * 사용 예:
 *   await SceneFader.Instance.LoadSceneWithFade("Battle", 0.35f, 0.25f);
 *   // 또는 코루틴 버전: StartCoroutine(SceneFader.Instance.LoadSceneWithFadeRoutine("Battle"));
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }

    [Header("색/알파")]
    [SerializeField] private Color overlayColor = Color.black; // 기본 블랙 페이드
    [SerializeField, Range(0f, 1f)] private float startAlpha = 0f; // 시작 시 알파(0=투명)

    [Header("Z 순서/캔버스")]
    [SerializeField] private int sortOrder = 10000; // UI 최상단
    [SerializeField] private bool createOnAwakeIfMissing = true;

    [Header("안전장치")]
    [SerializeField] private bool blockInputDuringFade = true;

    [Header("기타")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    private Canvas _canvas;
    private CanvasGroup _cg;
    private Image _img;
    private bool _isBusy;
    private Tweener _activeTween;

    void Awake()
    {
        // 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        if (createOnAwakeIfMissing)
            EnsureOverlay();
    }

    /// <summary>
    /// 오버레이 캔버스/이미지/캔버스그룹 생성 또는 참조 보장
    /// </summary>
    public void EnsureOverlay()
    {
        if (_canvas == null)
        {
            var goCanvas = new GameObject("SceneFaderCanvas", typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
            goCanvas.transform.SetParent(transform, false);

            _canvas = goCanvas.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = sortOrder;

            _cg = goCanvas.GetComponent<CanvasGroup>();
            _cg.alpha = startAlpha;
        }

        if (_img == null)
        {
            var goImg = new GameObject("Overlay", typeof(Image));
            goImg.transform.SetParent(_canvas.transform, false);

            _img = goImg.GetComponent<Image>();
            _img.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 1f); // 실제 가시 알파는 CanvasGroup이 담당
            _img.raycastTarget = blockInputDuringFade;

            // 풀스크린 스트레치
            var rt = _img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }

    /// <summary>
    /// 색상 교체(화이트 페이드/컬러 페이드 등)
    /// </summary>
    public void SetOverlayColor(Color color)
    {
        EnsureOverlay();
        overlayColor = color;
        _img.color = new Color(color.r, color.g, color.b, 1f);
    }

    /// <summary>
    /// 페이드 인(검은 화면 → 투명)
    /// </summary>
    public IEnumerator FadeIn(float duration = 0.3f, bool ignoreTimeScale = true)
    {
        EnsureOverlay();

        if (_isBusy) yield return null;
        _isBusy = true;
        KillActiveTween();

        _img.raycastTarget = blockInputDuringFade; // 입력차단 on
        _activeTween = _cg.DOFade(0f, duration)
                         .SetUpdate(ignoreTimeScale)
                         .OnComplete(() =>
                         {
                             _img.raycastTarget = false; // 완료 후 입력차단 off
                             _isBusy = false;
                         });
        yield return _activeTween.WaitForCompletion();
    }

    /// <summary>
    /// 페이드 아웃(투명 → 검은 화면)
    /// </summary>
    public IEnumerator FadeOut(float duration = 0.3f, bool ignoreTimeScale = true)
    {
        EnsureOverlay();

        if (_isBusy) yield return null;
        _isBusy = true;
        KillActiveTween();

        _img.raycastTarget = blockInputDuringFade;
        _activeTween = _cg.DOFade(1f, duration)
                         .SetUpdate(ignoreTimeScale)
                         .OnComplete(() =>
                         {
                             // 어차피 어두운 상태라 차단 유지해도 무방. 필요 시 off로 바꿔도 됨.
                             _isBusy = false;
                         });
        yield return _activeTween.WaitForCompletion();
    }

    /// <summary>
    /// 씬 전환 전체 루틴: 페이드아웃 → 씬 로드 → (옵션) 초기화 콜백 → 페이드인
    /// </summary>
    public IEnumerator LoadSceneWithFadeRoutine(
        string sceneName,
        float fadeOut = 0.3f,
        float fadeIn = 0.25f,
        System.Action onBeforeUnload = null,
        System.Action onAfterLoad = null,
        LoadSceneMode mode = LoadSceneMode.Single)
    {
        EnsureOverlay();

        // 1) 페이드 아웃
        yield return FadeOut(fadeOut);

        // 2) 언로드 직전 훅
        onBeforeUnload?.Invoke();

        // 3) 비동기 씬 로드
        var op = SceneManager.LoadSceneAsync(sceneName, mode);
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        // 4) 로드 완료 직후 훅 (참조 재바인딩/초기화 지점)
        onAfterLoad?.Invoke();

        // 5) 페이드 인
        yield return FadeIn(fadeIn);
    }

    /// <summary>
    /// 단순 호출용(코루틴 시작까지 포함)
    /// </summary>
    public void LoadSceneWithFade(
        string sceneName,
        float fadeOut = 0.3f,
        float fadeIn = 0.25f,
        System.Action onBeforeUnload = null,
        System.Action onAfterLoad = null,
        LoadSceneMode mode = LoadSceneMode.Single,
        MonoBehaviour runner = null)
    {
        EnsureOverlay();
        (runner ?? this).StartCoroutine(LoadSceneWithFadeRoutine(sceneName, fadeOut, fadeIn, onBeforeUnload, onAfterLoad, mode));
    }

    /// <summary>
    /// 현재 화면을 즉시 특정 알파로 고정(0=투명, 1=완전 가림)
    /// </summary>
    public void SetAlphaImmediate(float a)
    {
        EnsureOverlay();
        KillActiveTween();
        _cg.alpha = Mathf.Clamp01(a);
        _img.raycastTarget = blockInputDuringFade && a > 0.001f;
    }

    private void KillActiveTween()
    {
        if (_activeTween != null && _activeTween.IsActive())
        {
            _activeTween.Kill(false);
            _activeTween = null;
        }
    }
}

