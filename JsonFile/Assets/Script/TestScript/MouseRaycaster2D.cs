using UnityEngine;

public class MouseRaycaster2D : MonoBehaviour
{
    public Transform rayOriginTransform; // 마우스 좌표를 시각화할 오브젝트
    public LayerMask targetLayer;        // EnemyHitbox가 포함된 Layer
    public BossPartCombatManager manager; // 부위 선택 처리용

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 1. 마우스 월드 위치 계산
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = -10f; // Spine이 있는 거리 (카메라 기준 Z = -10 이면 Spine은 z = 0에 있음)
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            mouseWorldPos.z = 0f;

            if (rayOriginTransform != null)
                rayOriginTransform.position = mouseWorldPos;

            // 2. RaycastAll로 모든 충돌 결과 가져오기
            RaycastHit2D[] hits = Physics2D.RaycastAll(mouseWorldPos, Vector2.zero, Mathf.Infinity);

            bool hitTarget = false;

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;

                GameObject hitObj = hit.collider.gameObject;

                // 3. 해당 오브젝트가 원하는 LayerMask에 포함되어 있는지 검사
                if (((1 << hitObj.layer) & targetLayer.value) != 0)
                {
                    EnemyHitbox hitbox = hitObj.GetComponent<EnemyHitbox>();
                    if (hitbox != null)
                    {
                        string partName = hitbox.logicalPartName;
                        Debug.Log($"✅ 관통 후 타격 성공: {partName} ({hitObj.name})");
                        manager.SetSelectedPart(partName); // 부위 선택 처리
                        hitTarget = true;
                        Debug.DrawRay(mouseWorldPos, Vector3.forward * 10f, Color.green, 1.0f);
                        Debug.Log("🎯 클릭 좌표: " + mouseWorldPos);
                        break; // 첫 타겟 오브젝트만 처리
                    }
                    else
                    {
                        Debug.Log($"⚠️ EnemyHitbox가 없음: {hitObj.name}");
                    }
                }
                else
                {
                    Debug.Log($"⛔ 패스된 오브젝트: {hitObj.name} (Layer: {LayerMask.LayerToName(hitObj.layer)})");
                }
            }

            if (!hitTarget)
                Debug.Log("❌ 유효한 부위에 타격 실패");

            // 디버그용 Ray 표시
            Debug.DrawRay(mouseWorldPos, Vector3.forward * 10f, Color.red, 1.5f);
        }
    }
}
