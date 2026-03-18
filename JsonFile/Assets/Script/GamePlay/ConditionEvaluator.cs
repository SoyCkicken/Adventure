using System;
using System.Collections.Generic;
using UnityEngine;

public static class ConditionEvaluator
{
    // 프로젝트의 실제 인스턴스들을 받아서 검사
    public static bool Evaluate(
        List<ChoiceRequirement> reqs,
        PlayerState playerState,
        InventoryManager inventory,
        EquipmentSystem equipment,
        out List<string> reasons)
    {
        reasons = new List<string>();
        if (reqs == null || reqs.Count == 0) return true;

        foreach (var r in reqs)
        {
            if (r == null || string.IsNullOrEmpty(r.ID)) continue;

            var id = r.ID.Trim().ToUpperInvariant();
            var code = (r.Code ?? "").Trim();
            var val = r.Value;

            switch (id)
            {
                case "STAT":
                case "STATE":
                    if (GetStat(playerState, code) < val)
                        reasons.Add($"{code} {val}+ 필요");
                    break;

                case "GOLD":
                    if (playerState.Experience < val)
                        reasons.Add($"골드 {val}+ 필요");
                    break;

                case "ITEM":
                    // 비스택: 동일 Item_ID 객체 개수로 판단
                    if (inventory == null || inventory.CountItemInstances(code) < val)
                        reasons.Add($"{code} x{val}+ 필요");
                    break;

                case "EQUIP":
                    // 슬롯만(Weapon/Armor) 또는 특정장비(Weapon:Weapon_001)
                    if (equipment == null || !equipment.MeetsEquipRequirement(code))
                        reasons.Add($"장비 {code} 필요");
                    break;

                default:
                    Debug.LogWarning($"[ConditionEvaluator] Unknown ID: {r.ID}");
                    break;
            }
        }

        return reasons.Count == 0;
    }

    // 스탯 코드 → 값 매핑 (너희 규칙에 맞춰 필요시 수정)
    private static int GetStat(PlayerState ps, string code)
    {
        if (ps == null || string.IsNullOrEmpty(code)) return int.MinValue;
        switch (code.Trim().ToUpperInvariant())
        {
            case "STR": return ps.STR;
            case "AGI": return ps.AGI;
            case "INT": return ps.INT;
            case "MAG": return ps.MAG;
            case "DIV": return ps.DIV;
            case "CHA": return ps.CHA;
            case "HEALTH": return ps.Health;   // 필요시 CurrentHealth 등으로 변경
            case "MENTAL": return ps.INT;      // 네 용어 규칙에 맞게 교체
            default: return int.MinValue;
        }
    }
}
