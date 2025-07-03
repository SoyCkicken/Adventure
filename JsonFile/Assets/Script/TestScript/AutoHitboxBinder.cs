using Spine;
using Spine.Unity;
using System.Collections;
using UnityEngine;

/// <summary>
/// Spine 슬롯 중 "Hitbox"라는 이름이 포함된 슬롯에서 BoundingBoxAttachment가 있으면
/// PolygonCollider2D를 자동으로 생성해주는 스크립트.
/// </summary>
[RequireComponent(typeof(SkeletonRenderer))]
public class AutoHitboxBinder : MonoBehaviour
{
    private SkeletonRenderer skeletonRenderer;

    IEnumerator Start()
    {
        // Spine 초기화 이후 실행 보장
        yield return null;

        skeletonRenderer = GetComponent<SkeletonRenderer>();
        var skeleton = skeletonRenderer.Skeleton;

        foreach (Slot slot in skeleton.Slots)
        {
            string slotName = slot.Data.Name;

            // 슬롯 이름에 "Hitbox"가 포함된 경우만 처리
            if (!slotName.Contains("Hitbox")) continue;

            var attachment = slot.Attachment as BoundingBoxAttachment;
            if (attachment == null)
            {
                Debug.LogWarning($"[스킵] 슬롯 '{slotName}'에는 BoundingBoxAttachment가 없습니다.");
                continue;
            }

            Debug.Log($"[Hitbox 등록 시도] 슬롯 이름: {slotName}");

            // 히트박스 GameObject 생성
            GameObject hitboxObj = new GameObject($"{slotName}_Collider");
            hitboxObj.transform.SetParent(this.transform, false);

            // BoundingBoxFollower 컴포넌트 추가 및 설정
            var follower = hitboxObj.AddComponent<BoundingBoxFollower>();
            follower.slotName = slotName;
            //follower.followBoundingBox = true; // ⭐️ 꼭 설정
            follower.Initialize(true);

            var poly = hitboxObj.GetComponent<PolygonCollider2D>();
            if (poly != null)
            {
                poly.isTrigger = true;
                Debug.Log($"[성공] '{slotName}' → PolygonCollider2D 생성됨");
            }
            else
            {
                Debug.LogWarning($"[실패] '{slotName}' → PolygonCollider2D 생성되지 않음");
            }
        }
    }
}
