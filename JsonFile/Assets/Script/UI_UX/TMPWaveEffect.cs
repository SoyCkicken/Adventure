using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class TMPWaveEffect : MonoBehaviour
{
    public float amplitude = 2f;
    public float frequency = 8f;
    public float speed = 2f;

    public TMP_Text txt;
    private Mesh mesh;
    private Vector3[] verts;

    void Awake()
    {
        txt = GetComponent<TMP_Text>();
    }

    void LateUpdate()
    {
        if (txt == null || txt.textInfo.characterCount == 0) return;

        txt.ForceMeshUpdate();
        var ti = txt.textInfo;

        for (int i = 0; i < ti.characterCount; i++)
        {
            var ch = ti.characterInfo[i];
            if (!ch.isVisible) continue;

            int vIndex = ch.vertexIndex;
            int mIndex = ch.materialReferenceIndex;
            verts = ti.meshInfo[mIndex].vertices;

            float t = Time.time * speed + i * 0.1f;
            float offset = Mathf.Sin(t * frequency) * amplitude;

            verts[vIndex + 0].y += offset;
            verts[vIndex + 1].y += offset;
            verts[vIndex + 2].y += offset;
            verts[vIndex + 3].y += offset;
        }

        for (int m = 0; m < ti.meshInfo.Length; m++)
        {
            var mi = ti.meshInfo[m];
            mi.mesh.vertices = mi.vertices;
            txt.UpdateGeometry(mi.mesh, m);
        }
    }
}