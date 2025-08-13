using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 선택지 성공 판정 및 확률 계산 유틸리티.
/// 확률 수식 해석, 성공 판정, 결과 결정까지 담당.
/// </summary>
public static class ChoiceEvaluator
{
    /// <summary>
    /// 선택지 성공률 수식을 기반으로 확률(0~1)을 계산함.
    /// 예: "STR*10" → STR 스탯을 기준으로 10%씩 배율 계산
    /// </summary>
    public static float EvaluateFormula(string formula, PlayerState state)
    {
        if (string.IsNullOrEmpty(formula)) return 0f;

        formula = formula.Replace(" ", "").ToUpper();

        try
        {
            if (formula.StartsWith("STR*") && float.TryParse(formula.Substring(4), out float f1))
                return (state.STR * f1) / 100f;

            if (formula.StartsWith("DEX*") && float.TryParse(formula.Substring(4), out float f2))
                return (state.AGI * f2) / 100f;

            if (formula.StartsWith("INT*") && float.TryParse(formula.Substring(4), out float f3))
                return (state.INT * f3) / 100f;

            if (formula.StartsWith("CHA*") && float.TryParse(formula.Substring(4), out float f4))
                return (state.CHA * f4) / 100f;

            if (formula.StartsWith("DIV*") && float.TryParse(formula.Substring(4), out float f5))
                return (state.DIV * f5) / 100f;

            if (formula.StartsWith("MAG*") && float.TryParse(formula.Substring(4), out float f6))
                return (state.MAG * f6) / 100f;

            if (formula.StartsWith("HEALTH*") && float.TryParse(formula.Substring(7), out float f7))
                return (state.Health * f7) / 100f;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[ChoiceEvaluator] 수식 파싱 실패: {formula} → {ex.Message}");
        }

        return 0f;
    }

    /// <summary>
    /// 확률(0~1)을 기준으로 성공 여부를 랜덤으로 결정
    /// </summary>
    public static bool EvaluateSuccess(float rate01)
    {
        return UnityEngine.Random.value < Mathf.Clamp01(rate01);
    }

    /// <summary>
    /// 선택지 전체 결과 결정 (성공 여부 및 다음 코드 반환)
    /// </summary>
    public static ChoiceResult Resolve(
        string formula,
        string nextOnSuccess,
        string nextOnFail,
        PlayerState state)
    {
        float rate01 = EvaluateFormula(formula, state);
        bool isSuccess = EvaluateSuccess(rate01);

        return new ChoiceResult
        {
            SuccessRate = rate01,
            IsSuccess = isSuccess,
            NextCode = isSuccess ? nextOnSuccess : nextOnFail
        };
    }

    public static bool IsChoiceAvailable(
       List<ChoiceCondition> conditions,
       PlayerState player,
       InventoryManager inventory)
    {
        if (conditions == null || conditions.Count == 0) return true;

        foreach (var cond in conditions)
        {
            switch (cond.Type)
            {
                case "Gold":
                    if (player.Experience < cond.Value) return false;
                    break;
                case "Stat":
                    if (!HasStat(cond.StatName, cond.Value, player)) return false;
                    break;
                case "Item":
                    if (!inventory.HasItem(cond.ItemID)) // ← 여기만 수정
                        return false;
                    break;
                default:
                    Debug.LogWarning($"[조건 오류] 알 수 없는 조건 타입: {cond.Type}");
                    return false;
            }
        }

        return true;
    }

    private static bool HasStat(string stat, int min, PlayerState player)
    {
        return stat switch
        {
            "STR" => player.STR >= min,
            "AGI" => player.AGI >= min,
            "INT" => player.INT >= min,
            "CHA" => player.CHA >= min,
            "MAG" => player.MAG >= min,
            "DIV" => player.DIV >= min,
            _ => false
        };
    }
}

/// <summary>
/// 선택지 처리 결과 구조
/// </summary>
public class ChoiceResult
{
    public float SuccessRate;  // 0~1
    public bool IsSuccess;     // 성공 여부
    public string NextCode;    // 이동할 스크립트 코드
}

