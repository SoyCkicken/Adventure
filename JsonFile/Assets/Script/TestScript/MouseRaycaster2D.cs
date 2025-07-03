using System.Collections.Generic;
using UnityEngine;

public class MouseRaycaster2D : MonoBehaviour
{
    public Transform rayOriginTransform;
    public LayerMask targetLayer;
    public BossPartCombatManager manager; // 공격 처리용

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            mouseWorldPos.z = 0f;

            rayOriginTransform.position = mouseWorldPos;

            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, targetLayer);

            if (hit.collider != null)
            {
                EnemyHitbox hitbox = hit.collider.GetComponent<EnemyHitbox>();
                if (hitbox != null)
                {
                    string partName = hitbox.logicalPartName;
                    Debug.Log($"🎯 터치 감지 → 논리 부위: {partName}");

                    manager.SetSelectedPart(partName); // <-- 부위만 선택
                }
                else
                {
                    Debug.LogWarning("⚠️ EnemyHitbox 스크립트가 없습니다. collider 이름: " + hit.collider.name);
                }
            }
            else
            {
                Debug.Log("❌ 공격할 부위 없음");
            }

            // 디버그 레이 표시
            Debug.DrawRay(mouseWorldPos, Vector3.forward * 0.1f, Color.green, 0.5f);
        }
    }
}