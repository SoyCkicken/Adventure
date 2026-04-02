// ScreenShake.cs
// DOTween ��� ȭ�� ��鸲 ��ƿ��Ƽ
// - RectTransform(��� UI) ����
// - Transform(ī�޶�/���� ������Ʈ) ����
// �ߺ� ȣ�� �� ���� Ʈ���� �����ϰ� �����ϰ� ������Ѵ�.

using UnityEngine;
using DG.Tweening;

public class ScreenShake : MonoBehaviour
{
    [Header("��� ��� (�� �� �ϳ��� ����)")]
    [Tooltip("UI ���ó�� RectTransform�� ���� ���� �� ����")]
    public RectTransform uiTarget;

    [Tooltip("ī�޶� �� GameObject(ī�޶��� �θ�)�� ���� ���� �� ����")]
    public Transform worldTarget;

    [Header("�⺻ �Ķ����")]
    [Tooltip("��鸮�� �ð�(��)")]
    public float defaultDuration = 0.2f;

    [Tooltip("��鸲 ���� (UI: anchoredPosition �ȼ� ���� / ����: ���� ����)")]
    public float defaultStrength = 20f;

    [Tooltip("���� Ƚ��(���� �������� ������ �и�)")]
    public int defaultVibrato = 30;

    [Tooltip("��鸮�� ������ �������� (0~180)")]
    [Range(0f, 180f)] public float defaultRandomness = 90f;

    [Tooltip("Time.timeScale ���� ���� (������ ���� true ��õ)")]
    public bool ignoreTimeScale = true;

    // ���� ����
    private Tweener _activeTweener;
    private Vector3 _uiOriginalAnchoredPos;
    private Vector3 _worldOriginalLocalPos;

    void Awake()
    {
        // ����ġ ���� (�� ���� ���� ����)
        if (uiTarget != null)
            _uiOriginalAnchoredPos = uiTarget.anchoredPosition;

        if (worldTarget != null)
            _worldOriginalLocalPos = worldTarget.localPosition;
    }

    /// <summary>
    /// �⺻ �Ķ���ͷ� ����
    /// </summary>
    public void Shake() => Shake(defaultDuration, defaultStrength, defaultVibrato, defaultRandomness);

    /// <summary>
    /// Ŀ���� �Ķ���ͷ� ����
    /// </summary>
    public void Shake(float duration, float strength, int vibrato = 30, float randomness = 90f)
    {
        // ���� ���� Ʈ�� ���� �� ����ġ ����
        KillActiveTweenerAndRestore();

        if (uiTarget != null)
        {
            // UI�� ���: anchoredPosition�� ���� (RectTransform ����)
            // DOShakeAnchorPos : ������, �ȼ� ����
            _activeTweener = uiTarget.DOShakeAnchorPos(duration, strength, vibrato, randomness, true, true)
                .SetUpdate(ignoreTimeScale) // Ÿ�ӽ����� ����(����� ��õ)
                .OnComplete(() => uiTarget.anchoredPosition = _uiOriginalAnchoredPos);
        }
        else if (worldTarget != null)
        {
            // ����/ī�޶��� ���: localPosition ����
            // DOShakePosition �� ����/���� ��ȯ �Ұ� -> DoLocalMove�� �̿��� ������ ���
            // DOTween�� DOShakePosition(transform) �����ε�� ���� ��ǥ ���.
            // ī�޶� ��Ʈ(�� ������Ʈ)�� ���� �� ��õ. ���� ���� ��ó�� ī�޶�(Post)�� �浹 ����.
            _activeTweener = worldTarget.DOShakePosition(duration, strength, vibrato, randomness, false, true)
                .SetUpdate(ignoreTimeScale)
                .OnComplete(() => worldTarget.localPosition = _worldOriginalLocalPos);
        }
        else
        {
            Debug.LogWarning("[ScreenShake] uiTarget/worldTarget �� �� �������. �ƹ��͵� ��� �� ����.");
        }
    }

    /// <summary>
    /// ���ϰ�(�ǰ�/ũ�� ��) ���� ���� ������
    /// </summary>
    public void ShakeHard()
    {
        Shake(duration: 0.25f, strength: defaultStrength * 1.5f, vibrato: defaultVibrato + 10, randomness: defaultRandomness);
    }

    /// <summary>
    /// Ʈ�� ���� + ����ġ ����
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
    /// �ܺο��� �����ϰ� ���󺹱��� �� ȣ�� (�� ��ȯ ��)
    /// </summary>
    public void Restore()
    {
        KillActiveTweenerAndRestore();
    }
}
