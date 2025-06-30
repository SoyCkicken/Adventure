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
    }
}
