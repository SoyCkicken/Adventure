using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 체력 수치가 정수형(1~5) 기준일 때, 바 길이를 스탯에 따라 조절하는 컴포넌트
/// </summary>
[RequireComponent(typeof(Slider))]
public class IntegerHPBarScaler : MonoBehaviour
{
    [Header("슬라이더 연결")]
    public Slider slider;
    public RectTransform BackGroundHPBar;
    public RectTransform BackGroundHPBar2;

    [Header("기준 설정")]
    public int maxStatHP = 5;         // 최대 체력 수치 (예: 5)
    public float baseWidth = 200f;    // 체력이 5일 때 바 길이

    [Header("최소 길이 보정")]
    public float minWidth = 120f;

    private RectTransform barTransform;

    void Awake()
    {
        //barTransform = GetComponent<RectTransform>();
        if (slider == null)
            slider = GetComponent<Slider>();

        // 정수형 슬라이더로 설정
        slider.wholeNumbers = true;
        //Debug.Log($"RectTransform 대상: {barTransform.gameObject.name}");
    }

    /// <summary>
    /// 체력 수치에 맞춰 체력바 길이 및 최대값 설정
    /// </summary>
    public void SetHPMax(int maxHP)
    {
        float ratio = (float)maxHP / maxStatHP;
        float newWidth = baseWidth * ratio;
        newWidth = Mathf.Clamp(newWidth, minWidth, baseWidth);

        Debug.Log($"[HPBar or MPBar] maxHP: {maxHP}, 계산된 길이: {newWidth}");

        Vector2 size = BackGroundHPBar.sizeDelta;
        size.x = newWidth;
        BackGroundHPBar.sizeDelta = size;
        Vector2 size2 = BackGroundHPBar2.sizeDelta;
        size2.x = size.x +5;
        BackGroundHPBar2.sizeDelta = size2;


        slider.maxValue = maxHP;
        slider.value = maxHP;
    }
    public void SetMPMax(int maxMP)
    {
        float ratio = (float)maxMP / maxStatHP;
        float newWidth = baseWidth * ratio;
        newWidth = Mathf.Clamp(newWidth, minWidth, baseWidth);

        Debug.Log($"[MPBar] maxHP: {maxMP}, 계산된 길이: {newWidth}");

        Vector2 size = BackGroundHPBar.sizeDelta;
        size.x = newWidth;
        BackGroundHPBar.sizeDelta = size;
        Vector2 size2 = BackGroundHPBar2.sizeDelta;
        size2.x = size.x + 5;
        BackGroundHPBar2.sizeDelta = size2;


        slider.maxValue = maxMP;
        slider.value = maxMP;
    }

    /// <summary>
    /// 현재 체력값 설정
    /// </summary>
    public void SetCurrent(int currentHP)
    {
        slider.value = currentHP;
    }
}
