using System.Linq;
using UnityEngine;

public class MouseRaycaster2D : MonoBehaviour
{
    public LayerMask targetLayer; // 레이캐스트 대상 레이어
    public BossPartCombatManager manager; // 부위 선택 처리용
    //public TESTBoss testboss; // TESTBoss 인스턴스
                             
    //void Update()                       
    //{               
    //    if (Input.GetMouseButtonDown(0)) // 마우스 클릭 or 터치               
    //    {                
    //        // 1. 화면 기준 위치 → 월드 위치로 변환                    
    //        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //        // 2. 해당 위치에 2D 레이캐스트 (점 기준이므로 방향벡터는 Vector2.zero)
    //        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.up, 0.1f, targetLayer);
    //        foreach (var hit in hits)
    //        {
    //            Debug.Log($"[RayHit] {hit.collider.name} 맞춤");
    //            var hb = hit.collider.GetComponent<EnemyHitbox>();
    //            string partName = hb.logicalPartName;
    //            Debug.Log($"[Raycast] {partName} 클릭됨");
    //            manager.SetSelectedPart(partName); // 타겟으로 지정
    //        }
    //    }
    //}

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.zero, 0.1f, targetLayer);

            // EnemyHitbox가 있는 대상만 필터링
            var hitboxes = hits
                .Select(hit => hit.collider.GetComponent<EnemyHitbox>())
                .Where(hb => hb != null)
                .ToList();

            if (hitboxes.Count == 0)
                return; // EnemyHitbox가 없으면 아무것도 하지 않음

            // 우선순위: Head 부위가 있는가?
            var headHit = hitboxes.FirstOrDefault(hb => hb.logicalPartName == "머리");
            var LarmHit = hitboxes.FirstOrDefault(hb => hb.logicalPartName == "왼쪽 팔");
            var RarmHit = hitboxes.FirstOrDefault(hb => hb.logicalPartName == "오른쪽 팔");

            if (headHit != null)
            {
                Debug.Log("[Raycast] Head 부위가 우선적으로 감지됨");
                manager.SetSelectedPart(headHit.logicalPartName);
            }
           else if (LarmHit != null)
            {
                Debug.Log("[Raycast] 왼쪽 팔 부위가 우선적으로 감지됨");
                manager.SetSelectedPart(LarmHit.logicalPartName);
            }
           else if (RarmHit != null)
            {
                Debug.Log("[Raycast] 오른쪽 팔 부위가 우선적으로 감지됨");
                manager.SetSelectedPart(RarmHit.logicalPartName);
            }
            else
            {
                Debug.Log($"[Raycast] Head 없음, 첫 번째 부위 선택: {hitboxes[0].logicalPartName}");
                manager.SetSelectedPart(hitboxes[0].logicalPartName);
            }
        }
    }
}
