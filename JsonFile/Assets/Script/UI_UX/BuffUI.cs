//using MyGame;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using static UnityEngine.UI.GridLayoutGroup;

//public class BuffUI : MonoBehaviour
//{
//    //public Transform buffParent;
//    public GameObject buffIconPrefab;

//    private List<BuffIconUI> activeIcons = new();
//    public Transform playerBuffParent;
//    public Transform enemyBuffParent;

//    //버프 리스트를 UI로 표시
//    public void SetBuffs(List<BuffData> buffs,Character character)
//    {
//        //Clear();
//        Transform targetParnent = null; ;

//        Debug.Log($"버프 적용 중인 대상 {character.charaterName}");
//        if (character.charaterName == "Player")
//            targetParnent = playerBuffParent;
//        else
//        {
//            targetParnent = enemyBuffParent;
//        }
//        foreach (var buff in buffs)
//        { 
//            var icon = Instantiate(buffIconPrefab, targetParnent).GetComponent<BuffIconUI>();
//            icon.Set(buff);
//            activeIcons.Add(icon);
//        }
//    }

//    // 시간 갱신용
//    public void UpdateBuffTimers()
//    {
//        foreach (var icon in activeIcons)
//        {
//            //icon.UpdateUI();

//        }
//    }

//    public void Clear()
//    {
//        foreach (var icon in activeIcons)
//            Destroy(icon.gameObject);

//        activeIcons.Clear();
//    }
//}



//using MyGame;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class BuffUI : MonoBehaviour
//{
//    public GameObject buffIconPrefab;
//    public Transform playerBuffParent;
//    public Transform enemyBuffParent;
//    public GameObject battleGameObject; // 자동 전투 화면 캔버스
//    [SerializeField] public List<BuffIconUI> activeIcons = new();

//    private readonly Dictionary<string, BuffIconUI> iconMap = new();
//    private List<(Character ch, List<BuffData> buffs)> pending = new();

//    private string MakeKey(BuffData b)
//        => string.IsNullOrEmpty(b.SourceItemID) ? b.BuffID : $"{b.BuffID}:{b.SourceItemID}";

//    /// <summary>
//    /// 전체 버프 리스트를 받아 UI 아이콘으로 표시
//    /// </summary>
//    public void SetBuffs(List<BuffData> buffs, Character character)
//    {
//        // 전투 화면이 꺼져 있으면 나중에 그린다(안전)
//        if (battleGameObject == null || !battleGameObject.activeInHierarchy)
//        {
//            // 같은 캐릭터 건은 최신 상태로 교체
//            pending.RemoveAll(p => ReferenceEquals(p.ch, character));
//            pending.Add((character, buffs));
//            return;
//        }

//        ApplyToUI(buffs, character);
//    }

//    /// <summary>
//    /// 실제 UI 반영(아이콘 생성/갱신/제거)
//    /// </summary>
//    private void ApplyToUI(List<BuffData> buffs, Character character)
//    {
//        var parent = (character.charaterName == "Player") ? playerBuffParent : enemyBuffParent;
//        var seen = new HashSet<string>();

//        foreach (var buff in buffs)
//        {
//            string key = MakeKey(buff);
//            seen.Add(key);

//            if (iconMap.TryGetValue(key, out var ui) && ui != null)
//            {
//                // 이미 있으면 갱신(지속/수치/게이지 반영)
//                ui.Set(buff);
//            }
//            else
//            {
//                var go = Instantiate(buffIconPrefab, parent);
//                var uiNew = go.GetComponent<BuffIconUI>();
//                if (uiNew == null)
//                {
//                    Debug.LogError("[BuffUI] BuffIconUI 컴포넌트가 프리팹에 없습니다.");
//                    Destroy(go);
//                    continue;
//                }
//                uiNew.Set(buff);
//                iconMap[key] = uiNew;
//                activeIcons.Add(uiNew);
//            }
//        }

//        // 이번 상태에 없는(사라진) 버프 아이콘 제거
//        var toRemove = iconMap.Keys.Where(k => !seen.Contains(k)).ToList();
//        foreach (var k in toRemove)
//        {
//            if (iconMap[k] != null) Destroy(iconMap[k].gameObject);
//            iconMap.Remove(k);
//        }

//        // activeIcons 정리(파괴된 아이콘 제거)
//        activeIcons.RemoveAll(x => x == null);
//    }

//    private void OnEnable()
//    {
//        // 전투 화면이 켜질 때 대기열 플러시
//        FlushPending();
//    }

//    private void FlushPending()
//    {
//        if (battleGameObject == null || !battleGameObject.activeInHierarchy) return;

//        foreach (var (ch, buffs) in pending)
//            ApplyToUI(buffs, ch);

//        pending.Clear();
//    }

//    /// <summary>특정 BuffID 아이콘만 제거(장비 해제 등)</summary>
//    public void ClearBuffByID(string buffID)
//    {
//        // 1) iconMap에서 해당 BuffID 계열(복합키 포함) 제거
//        var keys = iconMap.Keys
//            .Where(k => k == buffID || k.StartsWith(buffID + ":"))
//            .ToList();

//        foreach (var key in keys)
//        {
//            if (iconMap[key] != null)
//                Destroy(iconMap[key].gameObject);
//            iconMap.Remove(key);
//        }

//        // 2) activeIcons 리스트에서도 제거 (안전망)
//        for (int i = activeIcons.Count - 1; i >= 0; i--)
//        {
//            var ui = activeIcons[i];
//            if (ui == null) { activeIcons.RemoveAt(i); continue; }
//            var bd = ui.buffData;
//            if (bd != null && bd.BuffID == buffID)
//            {
//                Destroy(ui.gameObject);
//                activeIcons.RemoveAt(i);
//            }
//        }

//        // 3) 아직 화면이 꺼져 있어 pending에 쌓인 항목들에서도 제거
//        for (int i = 0; i < pending.Count; i++)
//        {
//            var (ch, list) = pending[i];
//            pending[i] = (ch, list.Where(b => b.BuffID != buffID).ToList());
//        }
//    }

//    /// <summary>디버그/장면 리셋 용</summary>
//    public void ClearAll()
//    {
//        foreach (var icon in activeIcons)
//            if (icon != null) Destroy(icon.gameObject);

//        activeIcons.Clear();
//        iconMap.Clear();
//        pending.Clear();
//    }
//}

using MyGame;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Buff UI 총괄
/// - 플레이어/적 아이콘 맵을 분리해 상호 삭제 버그 방지
/// - 전투 캔버스가 비활성일 땐 pending에만 쌓고, 활성 전이에 Flush
/// - 아이콘은 재사용(Set 갱신), 이번 상태에 없는 것만 제거
/// </summary>
public class BuffUI : MonoBehaviour
{
    [Header("Prefab & Parents")]
    public GameObject buffIconPrefab;
    public Transform playerBuffParent;
    public Transform enemyBuffParent;

    [Header("Battle Canvas Root")]
    public GameObject battleGameObject; // 전투 화면 캔버스 루트(비활성일 수 있음)

    // 디버깅 확인용
    [SerializeField] public List<BuffIconUI> activeIcons = new();

    // ★ 핵심: 캐릭터별 아이콘 맵 분리
    private readonly Dictionary<string, BuffIconUI> playerMap = new(); // key = BuffID[:SourceItemID]
    private readonly Dictionary<string, BuffIconUI> enemyMap = new();

    // 전투 캔버스가 꺼져 있을 때 들어온 요청을 모아두는 큐
    private readonly List<(Character ch, List<BuffData> buffs)> pending = new();

    private bool lastBattleActive = false;

    private static string MakeKey(BuffData b)
        => string.IsNullOrEmpty(b.SourceItemID) ? b.BuffID : $"{b.BuffID}:{b.SourceItemID}";

    private Dictionary<string, BuffIconUI> GetMap(Character character)
        => (character.charaterName == "Player") ? playerMap : enemyMap;

    private Transform GetParent(Character character)
        => (character.charaterName == "Player") ? playerBuffParent : enemyBuffParent;

    private void Awake()
    {
        lastBattleActive = battleGameObject != null && battleGameObject.activeInHierarchy;
    }

    /// <summary>캐릭터의 현재 버프 목록으로 UI 동기화</summary>
    public void SetBuffs(List<BuffData> buffs, Character character)
    {
        // 전투 화면이 꺼져 있으면 큐에 적재만
        if (battleGameObject == null || !battleGameObject.activeInHierarchy)
        {
            pending.RemoveAll(p => ReferenceEquals(p.ch, character)); // 최신상태로 치환
            pending.Add((character, buffs));
            return;
        }

        ApplyToUI(buffs, character);
    }

    /// <summary>실제 UI 반영(아이콘 생성/갱신/제거) — 캐릭터별 맵만 건드림</summary>
    private void ApplyToUI(List<BuffData> buffs, Character character)
    {
        var parent = GetParent(character);
        var map = GetMap(character);

        var seen = new HashSet<string>();

        foreach (var buff in buffs)
        {
            string key = MakeKey(buff);
            seen.Add(key);

            if (map.TryGetValue(key, out var ui) && ui != null)
            {
                ui.Set(buff); // 기존 아이콘 갱신
            }
            else
            {
                var go = Instantiate(buffIconPrefab, parent);
                var uiNew = go.GetComponent<BuffIconUI>();
                if (uiNew == null)
                {
                    Debug.LogError("[BuffUI] BuffIconUI 컴포넌트가 프리팹에 없습니다.");
                    Destroy(go);
                    continue;
                }
                uiNew.Set(buff);
                map[key] = uiNew;
                activeIcons.Add(uiNew);
            }
        }

        // 이번 상태에 없는(사라진) 키만 제거 — ★ 해당 캐릭터 맵만 대상으로
        var toRemove = map.Keys.Where(k => !seen.Contains(k)).ToList();
        foreach (var k in toRemove)
        {
            if (map[k] != null) Destroy(map[k].gameObject);
            map.Remove(k);
        }

        // 죽은 참조 정리
        activeIcons.RemoveAll(x => x == null);
    }

    // 전투 캔버스 활성 전이 감지 → pending 플러시
    private void LateUpdate()
    {
        bool nowActive = (battleGameObject != null && battleGameObject.activeInHierarchy);
        if (!lastBattleActive && nowActive) // false → true
            FlushPending();
        lastBattleActive = nowActive;
    }

    private void OnEnable() => FlushPending();

    private void FlushPending()
    {
        if (battleGameObject == null || !battleGameObject.activeInHierarchy) return;

        foreach (var (ch, buffs) in pending)
            ApplyToUI(buffs, ch);

        pending.Clear();
    }

    /// <summary>특정 BuffID 아이콘만 제거(장비 해제/만료)</summary>
    public void ClearBuffByID(string buffID)
    {
        // 플레이어/적 맵 모두에서 해당 ID(복합키 포함) 제거
        void ClearFromMap(Dictionary<string, BuffIconUI> map)
        {
            var keys = map.Keys.Where(k => k == buffID || k.StartsWith(buffID + ":")).ToList();
            foreach (var key in keys)
            {
                if (map[key] != null) Destroy(map[key].gameObject);
                map.Remove(key);
            }
        }

        ClearFromMap(playerMap);
        ClearFromMap(enemyMap);

        // activeIcons 리스트에서도 제거 (안전망)
        for (int i = activeIcons.Count - 1; i >= 0; i--)
        {
            var ui = activeIcons[i];
            if (ui == null) { activeIcons.RemoveAt(i); continue; }
            var bd = ui.buffData;
            if (bd != null && bd.BuffID == buffID)
            {
                Destroy(ui.gameObject);
                activeIcons.RemoveAt(i);
            }
        }

        // pending에서도 제거
        for (int i = 0; i < pending.Count; i++)
        {
            var (ch, list) = pending[i];
            pending[i] = (ch, list.Where(b => b.BuffID != buffID).ToList());
        }
    }

    /// <summary>디버그/장면 리셋</summary>
    public void ClearAll()
    {
        foreach (var ui in activeIcons)
            if (ui != null) Destroy(ui.gameObject);

        activeIcons.Clear();
        playerMap.Clear();
        enemyMap.Clear();
        pending.Clear();
    }
}
