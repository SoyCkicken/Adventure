using Spine;
using Spine.Unity;
using System.Collections;
using UnityEngine;

/// <summary>
/// Spine 슬롯 중 "Hitbox"라는 이름이 포함된 슬롯에서 BoundingBoxAttachment가 있으면
/// PolygonCollider2D를 자동으로 생성하고 EnemyHitbox를 붙여주는 스크립트.
/// </summary>
[RequireComponent(typeof(SkeletonRenderer))]
public class AutoHitboxBinder : MonoBehaviour
{
    private SkeletonRenderer skeletonRenderer;

    IEnumerator Start()
    {
        yield return null; // Spine 초기화 보장

        skeletonRenderer = GetComponent<SkeletonRenderer>();
        var skeleton = skeletonRenderer.Skeleton;

        foreach (Slot slot in skeleton.Slots)
        {
            string slotName = slot.Data.Name;

            // 슬롯 이름에 "Hitbox"가 포함되어 있어야 처리
            if (!slotName.Contains("Hitbox")) continue;

            var attachment = slot.Attachment as BoundingBoxAttachment;
            if (attachment == null)
            {
                Debug.LogWarning($"[스킵] '{slotName}' → BoundingBoxAttachment 없음");
                continue;
            }

            Debug.Log($"[Hitbox 등록 시도] 슬롯: {slotName}");

            // 히트박스 오브젝트 생성
            GameObject hitboxObj = new GameObject($"{slotName}_Collider");
            hitboxObj.transform.SetParent(this.transform, false);

            // BoundingBoxFollower 부착
            var follower = hitboxObj.AddComponent<BoundingBoxFollower>();
            follower.slotName = slotName;
            follower.Initialize(true);

            var poly = hitboxObj.GetComponent<PolygonCollider2D>();
            if (poly != null)
            {
                poly.isTrigger = true;

                // 🔥 EnemyHitbox 자동 부착 + 논리 부위 이름 추출
                var hitbox = hitboxObj.AddComponent<EnemyHitbox>();
                hitbox.logicalPartName = ExtractLogicalPartName(slotName);

                Debug.Log($"[등록 완료] {slotName} → PolygonCollider2D + EnemyHitbox({hitbox.logicalPartName})");
            }
            else
            {
                Debug.LogWarning($"[실패] '{slotName}' → PolygonCollider2D 생성 실패");
            }
        }
    }

    /// <summary>
    /// Spine 슬롯 이름에서 논리 부위명 추출. 예: "Arm_Hitbox" → "Arm"
    /// </summary>
    private string ExtractLogicalPartName(string slotName)
    {
        return slotName.Replace("_Hitbox", "")
                       .Replace("Hitbox", "")
                       .Replace("M_jombie_", "") // 필요 시 제거
                       .Trim();
    }
}
