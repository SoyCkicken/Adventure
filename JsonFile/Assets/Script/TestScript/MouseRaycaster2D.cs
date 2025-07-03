using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseRaycaster2D : MonoBehaviour
{
    //public LayerMask targetLayer;
    //public Transform rayOriginTransform;

    //void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        // [1] 스크린 좌표를 가져온다
    //        Vector3 mouseScreenPos = Input.mousePosition;

    //        // [2] Z 거리 보정 (카메라와 오브젝트 간 거리)
    //        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);

    //        // [3] 월드 좌표로 변환
    //        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
    //        mouseWorldPos.z = 0f;

    //        // [4] 깡통 오브젝트 이동
    //        rayOriginTransform.position = mouseWorldPos;

    //        // [5] Ray 쏘기
    //        RaycastHit2D hit = Physics2D.Raycast(rayOriginTransform.position, Vector2.zero, Mathf.Infinity, targetLayer);

    //        if (hit.collider != null)
    //        {
    //            Debug.Log($"[🎯 적중] 부위: {hit.collider.name}");
    //            // combatManager.AttackPart(hit.collider.name);
    //        }
    //        else
    //        {
    //            Debug.Log("[❌ 미스] 충돌 없음");
    //        }

    //        // [6] 디버그 레이 표시
    //        Debug.DrawRay(rayOriginTransform.position, Vector3.forward * 500f, Color.green, 0.5f);
    //    }
    //}
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
                string partName = hit.collider.name; // 또는 커스텀 컴포넌트에서 가져오기
                Debug.Log($"🎯 터치 감지: {partName}");

                manager.testPlayer.PerformAttack(manager.testBoss,partName); // 매니저에 공격 요청
            }
            else
            {
                Debug.Log("❌ 공격할 부위 없음");
            }
        }
    }
}

