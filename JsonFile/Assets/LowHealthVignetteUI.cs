using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HP/MP(정신력) 저하 시 화면 가장자리에 경고 비네트 표시.
/// - HP 경고(빨강)가 있으면 HP를 우선 표시.
/// - HP에 문제가 없을 때만 MP 경고(검정)를 표시.
/// - 중앙 투명/가장자리 불투명 라디얼 알파 스프라이트를 사용(없으면 런타임 생성).
/// - PlayerState에 HealthRatio/MentalRatio 프로퍼티가 있으면 최우선 사용.
///   없으면 필드 조합(CurrentHealth/Health, CurrentMental/MP 등)을 자동 추정.
/// </summary>
public class LowHealthVignetteUI : MonoBehaviour
{
    [Header("References")]
    public Image vignetteImage;      // 전체를 덮는 UI Image
    public PlayerState playerState;  // 씬의 PlayerState 참조(없으면 디폴트 1.0 취급)

    [Header("HP 경고 설정")]
    [Range(0f, 1f)] public float hpStartAt = 0.6f;   // 이 비율 아래부터 서서히 표시
    [Range(0f, 1f)] public float hpMaxAlpha = 0.55f; // HP=0에서의 최대 알파
    public bool hpPulse = true;
    [Range(0f, 1f)] public float hpCritical = 0.25f; // 이 이하면 맥동
    public float hpPulseSpeed = 3.5f;
    public AnimationCurve hpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Color hpColor = new Color(1f, 0f, 0f, 1f); // 빨강

    [Header("MP(정신력) 경고 설정")]
    [Range(0f, 1f)] public float mpStartAt = 0.6f;
    [Range(0f, 1f)] public float mpMaxAlpha = 0.45f;
    public bool mpPulse = true;
    [Range(0f, 1f)] public float mpCritical = 0.25f;
    public float mpPulseSpeed = 2.5f;
    public AnimationCurve mpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Color mpColor = new Color(0f, 0f, 0f, 1f); // 검정

    [Header("Debug(테스트용 수동 주입)")]
    [Tooltip("0~1로 직접 주입(-1이면 무시). 우선순위: 디버그 > SetRatio > PlayerState")]
    [Range(-1f, 1f)] public float debugHpRatio = -1f;
    [Range(-1f, 1f)] public float debugMpRatio = -1f;

    // 외부에서 직접 비율 주입하고 싶을 때(데미지/정신 소모 이벤트 등)
    public void SetHealthRatio(float r) { _hpInjected = Mathf.Clamp01(r); _hpUseInjected = true; }
    public void SetMentalRatio(float r) { _mpInjected = Mathf.Clamp01(r); _mpUseInjected = true; }
    private float _hpInjected = 1f, _mpInjected = 1f;
    private bool _hpUseInjected = false, _mpUseInjected = false;

    void Awake()
    {
        playerState = PlayerState.Instance; // PlayerState 싱글톤 참조

        if (!vignetteImage) vignetteImage = GetComponentInChildren<Image>(true);

        // 비네트 스프라이트가 비어 있으면 런타임 생성(라디얼 알파)
        if (vignetteImage && vignetteImage.sprite == null)
        {
            vignetteImage.sprite = CreateRadialGradientSprite(512);
        }

        // 풀스크린 정상 덮기 + 입력 방해 금지 + 부모 마스크 무시
        if (vignetteImage)
        {
            vignetteImage.type = Image.Type.Simple;
            vignetteImage.preserveAspect = false; // 화면 비율과 무관하게 꽉 채우기
            vignetteImage.raycastTarget = false;  // 버튼 막지 않기
            vignetteImage.maskable = false;       // 부모 Mask/RectMask2D 무시
        }
    }

    void Update()
    {
        // 1) 현재 비율 가져오기
        float hp = GetHpRatio();
        float mp = GetMpRatio();

        // 2) 각 채널의 알파 계산
        float hpAlpha = EvalAlpha(hp, hpStartAt, hpMaxAlpha, hpCurve, hpPulse, hpCritical, hpPulseSpeed);
        float mpAlpha = EvalAlpha(mp, mpStartAt, mpMaxAlpha, mpCurve, mpPulse, mpCritical, mpPulseSpeed);

        // 3) 우선순위 결정: HP > MP
        Color outColor;
        float outAlpha;
        if (hpAlpha > 0.001f)
        {
            outColor = hpColor;
            outAlpha = hpAlpha;
        }
        else if (mpAlpha > 0.001f)
        {
            outColor = mpColor;
            outAlpha = mpAlpha;
        }
        else
        {
            outColor = hpColor; // 아무거나 상관없음
            outAlpha = 0f;
        }

        // 4) 적용
        if (vignetteImage)
        {
            var c = outColor;
            c.a = outAlpha;
            vignetteImage.color = c;
        }

        // 다음 프레임부터는 주입 플래그 해제(1회성 푸시 사용 시)
        _hpUseInjected = false;
        _mpUseInjected = false;
    }

    // -------------------- 내부 구현 --------------------

    private float EvalAlpha(float ratio, float start, float maxA, AnimationCurve curve, bool pulse, float crit, float speed)
    {
        float t = Mathf.InverseLerp(start, 0f, Mathf.Clamp01(ratio)); // start→0으로 내려갈수록 0→1
        t = Mathf.Clamp01(curve.Evaluate(t));
        float a = Mathf.Lerp(0f, maxA, t);

        if (pulse && ratio <= crit && a > 0f)
        {
            // 일시정지에도 동작하도록 unscaledTime 사용
            float p = 0.75f + 0.25f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * speed));
            a *= p;
        }
        return a;
    }

    private float GetHpRatio()
    {
        if (debugHpRatio >= 0f) return Mathf.Clamp01(debugHpRatio);
        if (_hpUseInjected) return _hpInjected;
        if (!playerState) return 1f;

        // 1) PlayerState.HealthRatio 프로퍼티가 있으면 그것 사용
        var prop = playerState.GetType().GetProperty("HealthRatio");
        if (prop != null && prop.PropertyType == typeof(float))
            return Mathf.Clamp01((float)prop.GetValue(playerState));

        // 2) 일반적인 필드 조합 시도: CurrentHealth / Health, HP / Health, CurrentHealth / MaxHP
        int cur = 0, max = 0;
        var fCur = playerState.GetType().GetField("CurrentHealth");
        var fHealth = playerState.GetType().GetField("Health");
        var fHP = playerState.GetType().GetField("HP");
        var fMaxHP = playerState.GetType().GetField("MaxHP");

        if (fCur != null && fHealth != null)
        {
            cur = (int)fCur.GetValue(playerState);
            max = Mathf.Max(1, (int)fHealth.GetValue(playerState));
            return Mathf.Clamp01((float)cur / max);
        }
        if (fHP != null && fHealth != null)
        {
            cur = (int)fHP.GetValue(playerState);
            max = Mathf.Max(1, (int)fHealth.GetValue(playerState));
            return Mathf.Clamp01((float)cur / max);
        }
        if (fCur != null && fMaxHP != null)
        {
            cur = (int)fCur.GetValue(playerState);
            max = Mathf.Max(1, (int)fMaxHP.GetValue(playerState));
            return Mathf.Clamp01((float)cur / max);
        }

        return 1f;
    }

    private float GetMpRatio()
    {
        if (debugMpRatio >= 0f) return Mathf.Clamp01(debugMpRatio);
        if (_mpUseInjected) return _mpInjected;
        if (!playerState) return 1f;

        // 1) PlayerState.MentalRatio 프로퍼티가 있으면 그것 사용
        var prop = playerState.GetType().GetProperty("MentalRatio");
        if (prop != null && prop.PropertyType == typeof(float))
            return Mathf.Clamp01((float)prop.GetValue(playerState));

        // 2) 일반적인 필드 조합 시도: CurrentMental / MP, CurrentMental / MaxMP
        //   (네 프로젝트에 CurrentMental, MP 필드가 존재)
        int cur = 0, max = 0;
        var fCur = playerState.GetType().GetField("CurrentMental");
        var fMP = playerState.GetType().GetField("MP");
        var fMaxMP = playerState.GetType().GetField("MaxMP");

        if (fCur != null && fMP != null)
        {
            cur = (int)fCur.GetValue(playerState);
            max = Mathf.Max(1, (int)fMP.GetValue(playerState));   // MP가 최대치라면 이 조합이 맞다
            return Mathf.Clamp01((float)cur / max);
        }
        if (fCur != null && fMaxMP != null)
        {
            cur = (int)fCur.GetValue(playerState);
            max = Mathf.Max(1, (int)fMaxMP.GetValue(playerState));
            return Mathf.Clamp01((float)cur / max);
        }

        // 실패 시 안전하게 비활성
        return 1f;
    }

    /// <summary>
    /// 중앙 투명 / 가장자리 불투명 라디얼 알파 스프라이트 생성.
    /// 흰색 알파만 가진 텍스처이므로 Image.color로 틴트(빨강/검정)를 바꾼다.
    /// </summary>
    private Sprite CreateRadialGradientSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        float cx = (size - 1) * 0.5f, cy = (size - 1) * 0.5f;
        float maxR = Mathf.Sqrt(cx * cx + cy * cy);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx, dy = y - cy;
                float r = Mathf.Sqrt(dx * dx + dy * dy) / maxR; // 0=center, 1=edge
                float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(r));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
