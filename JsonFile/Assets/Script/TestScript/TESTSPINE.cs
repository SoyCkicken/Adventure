using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTSPINE : MonoBehaviour
{
    // Start is called before the first frame update


    void Start()
    {
        SkeletonAnimation skeletonAnim = GetComponent<SkeletonAnimation>();
        foreach (var slot in skeletonAnim.skeleton.Slots)
        {
            Debug.Log($"Slot: {slot.Data.Name}");
        }

        var polygon = skeletonAnim.GetComponent<PolygonCollider2D>();
        if (polygon != null)
        {
            for (int i = 0; i < polygon.pathCount; i++)
            {
                Vector2[] points = polygon.GetPath(i);
                foreach (var p in points)
                {
                    Debug.DrawRay(transform.TransformPoint(p), Vector3.up * 0.2f, Color.red, 2f);
                }
            }
        }
    }
}
