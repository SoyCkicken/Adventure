// TMPShakeEffect.cs
// 전체 텍스트에 약간의 랜덤 흔들림
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TMPShakeEffect : MonoBehaviour
{
    public float intensity = 0.7f;
    public float speed = 20f;

    TMP_Text txt;

    void Awake() { txt = GetComponent<TMP_Text>(); }

    void LateUpdate()
    {
        txt.ForceMeshUpdate();
        var ti = txt.textInfo;

        for (int i = 0; i < ti.characterCount; i++)
        {
            var ch = ti.characterInfo[i];
            if (!ch.isVisible) continue;

            int vIndex = ch.vertexIndex;
            int mIndex = ch.materialReferenceIndex;

            var verts = ti.meshInfo[mIndex].vertices;

            float t = Time.time * speed + i * 0.3f;
            Vector2 jitter = new Vector2(Mathf.PerlinNoise(t, 0f) - 0.5f, Mathf.PerlinNoise(0f, t) - 0.5f) * intensity;

            verts[vIndex + 0] += (Vector3)jitter;
            verts[vIndex + 1] += (Vector3)jitter;
            verts[vIndex + 2] += (Vector3)jitter;
            verts[vIndex + 3] += (Vector3)jitter;
        }

        for (int m = 0; m < ti.meshInfo.Length; m++)
        {
            var mi = ti.meshInfo[m];
            mi.mesh.vertices = mi.vertices;
            txt.UpdateGeometry(mi.mesh, m);
        }
    }
}
