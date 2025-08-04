using Spine.Unity;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public string logicalPartName; // "머리", "오른쪽 팔" 등

    public SkeletonAnimation skeletonAnimation;
    public string slotName = "RightArm_Hitbox";

    void Update()
    {
        var skeleton = skeletonAnimation.Skeleton;
        var slot = skeleton.FindSlot(slotName);
        if (slot == null) return;

        var bone = slot.Bone;
        Vector3 localPos = new Vector3(bone.WorldX, bone.WorldY, 0);
        Vector3 worldPos = skeletonAnimation.transform.TransformPoint(localPos);

        Debug.Log($"[Slot: {slotName}] 월드 위치: {worldPos}");
        Debug.DrawRay(worldPos, Vector3.up * 0.3f, Color.yellow); // Scene에서 보이게
    }
}