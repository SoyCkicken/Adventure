using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 한 TMP_Text 안에서 지정된 문자 범위들만 웨이브 적용
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TMPWaveEffectRange : MonoBehaviour
{
    [System.Serializable]
    public struct WaveRange { public int start; public int length; }

    public float amplitude = 5f;     // 파고
    public float frequency = 8f;     // 문자 간 위상 차이
    public float speed = 4f;         // 시간 속도

    public TMP_Text txt;
    public List<WaveRange> ranges = new List<WaveRange>();

    void Reset()
    {
        txt = GetComponent<TMP_Text>();
    }

    void Awake()
    {
        if (!txt) txt = GetComponent<TMP_Text>();
        // TMP 내부 메쉬 갱신을 우리가 컨트롤하기 위해 필요
        txt.ForceMeshUpdate();
    }

    /// <summary> 웨이브 적용 범위를 추가 (start: 포함, length: 문자 수) </summary>
    public void AddRange(int start, int length)
    {
        if (length <= 0) return;
        ranges.Add(new WaveRange { start = start, length = length });
    }

    /// <summary> 마지막으로 추가한 범위의 길이를 갱신(타이핑 중 실시간 늘릴 때) </summary>
    public void UpdateLastRangeLength(int newLength)
    {
        if (ranges.Count == 0) return;
        var r = ranges[ranges.Count - 1];
        r.length = Mathf.Max(0, newLength);
        ranges[ranges.Count - 1] = r;
    }

    void LateUpdate()
    {
        if (txt == null || txt.textInfo == null) return;
        txt.ForceMeshUpdate();

        var textInfo = txt.textInfo;
        float time = Time.unscaledTime * speed;

        // 머티리얼(서브메시) 단위 루프
        for (int mi = 0; mi < textInfo.meshInfo.Length; mi++)
        {
            var meshInfo = textInfo.meshInfo[mi];
            var vertices = meshInfo.vertices;

            // 문자 단위 루프
            for (int ci = 0; ci < textInfo.characterCount; ci++)
            {
                var ch = textInfo.characterInfo[ci];
                if (!ch.isVisible) continue;
                if (ch.materialReferenceIndex != mi) continue;

                int charIndex = ch.index;

                if (!IsInAnyRange(charIndex)) continue;

                // 문자의 4개 버텍스 인덱스
                int v0 = ch.vertexIndex + 0;
                int v1 = ch.vertexIndex + 1;
                int v2 = ch.vertexIndex + 2;
                int v3 = ch.vertexIndex + 3;

                // 위상은 문자 인덱스로 약간씩 차이를 둔다
                float offsetY = Mathf.Sin(time + charIndex * (frequency * 0.05f)) * amplitude;

                Vector3 oy = new Vector3(0, offsetY, 0);
                vertices[v0] += oy;
                vertices[v1] += oy;
                vertices[v2] += oy;
                vertices[v3] += oy;
            }

            // 변경된 버텍스 적용
            meshInfo.mesh.vertices = vertices;
            txt.UpdateGeometry(meshInfo.mesh, mi);
        }
    }

    bool IsInAnyRange(int charIndex)
    {
        for (int i = 0; i < ranges.Count; i++)
        {
            int s = ranges[i].start;
            int e = s + ranges[i].length; // e는 제외
            if (charIndex >= s && charIndex < e) return true;
        }
        return false;
    }
}
