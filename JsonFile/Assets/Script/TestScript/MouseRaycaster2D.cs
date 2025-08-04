using Unity.Burst.CompilerServices;
using UnityEngine;

public class MouseRaycaster2D : MonoBehaviour
{
    public LayerMask targetLayer; // 레이캐스트 대상 레이어
    public BossPartCombatManager manager; // 부위 선택 처리용
    public TESTBoss testboss; // TESTBoss 인스턴스
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 클릭 or 터치
        {
            // 1. 화면 기준 위치 → 월드 위치로 변환
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 2. 해당 위치에 2D 레이캐스트 (점 기준이므로 방향벡터는 Vector2.zero)
            RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.up, 0.1f, targetLayer);

           
            foreach (var hit in hits)
            {
                Debug.Log($"[RayHit] {hit.collider.name} 맞춤");
                var hb = hit.collider.GetComponent<EnemyHitbox>();
                // 3. 맞은 오브젝트가 CombatPart라면
                if (hit.collider.TryGetComponent<BossPartCombatManager>(out BossPartCombatManager part))
                {
                    // 4. CombatPart의 부위 이름을 가져와서
                    string partName = hb.logicalPartName;
                    Debug.Log($"[Raycast] {partName} 클릭됨");
                    manager.SetSelectedPart(partName); // 데미지는 상황에 따라 조절
                    return; // 첫 번째 맞은 오브젝트만 처리하고 종료
                }
            }
        }
    }
}
