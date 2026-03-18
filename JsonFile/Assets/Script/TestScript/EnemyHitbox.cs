using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public string logicalPartName; // ex) "¿ÞÆÈ", "¸Ó¸®", "¿À¸¥´Ù¸®" µî

    private void Reset()
    {
        logicalPartName = gameObject.name;
    }
}