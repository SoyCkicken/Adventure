//using System.Collections.Generic;
//using UnityEngine;
//using TMPro;

///// 한 TMP_Text 안에서 "지정 범위에만" 웨이브/색상을 적용
//[RequireComponent(typeof(TMP_Text))]
//public class TMPTextRangeEffects : MonoBehaviour
//{
//    [System.Serializable]
//    public struct ShakeRange { public int start; public int length; }
//    [System.Serializable] public struct WaveRange { public int start; public int length; }
//    [System.Serializable] public struct ColorRange { public int start; public int length; public Color32 color; }

//    [Header("Wave")]
//    public float amplitude = 5f;
//    public float frequency = 8f;
//    public float speed = 4f;

//    [Header("Refs")]
//    public TMP_Text txt;

//    // 등록된 범위
//    public List<WaveRange> waves = new();
//    public List<ColorRange> colors = new();
//    public List<ShakeRange> shakes = new();

//    void Reset() { txt = GetComponent<TMP_Text>(); }
//    void Awake() { if (!txt) txt = GetComponent<TMP_Text>(); txt.ForceMeshUpdate(); }

//    // --- 웨이브 범위 API ---
//    public void AddWaveRange(int start, int length)
//    {
//        if (length <= 0) return;
//        waves.Add(new WaveRange { start = start, length = length });
//    }
//    public void UpdateLastWaveLength(int newLength)
//    {
//        if (waves.Count == 0) return;
//        var r = waves[^1]; r.length = Mathf.Max(0, newLength); waves[^1] = r;
//    }

//    // --- 색상 범위 API ---
//    public void AddColorRange(int start, int length, Color32 color)
//    {
//        if (length <= 0) return;
//        colors.Add(new ColorRange { start = start, length = length, color = color });
//    }
//    public void AddShakeRange(int start, int length)
//    {
//        if (length <= 0) return;
//        shakes.Add(new ShakeRange { start = start, length = length });
//    }
//    public void UpdateLastShakeLength(int newLength)
//    {
//        if (shakes.Count == 0) return;
//        var r = shakes[^1]; r.length = Mathf.Max(0, newLength); shakes[^1] = r;
//    }
//    public void UpdateLastColorLength(int newLength)
//    {
//        if (colors.Count == 0) return;
//        var r = colors[^1]; r.length = Mathf.Max(0, newLength); colors[^1] = r;
//    }

//    bool InRange(int idx, int start, int len) => idx >= start && idx < start + len;

//    public void ResetEffects() => ClearEffects();

//    //초기화 부분
//    public void ClearEffects()
//    {
//        waves.Clear();
//        colors.Clear();
//        shakes.Clear(); // 추가
//    }
//    void LateUpdate()
//    {
//        if (!txt) return;

//        txt.ForceMeshUpdate();
//        var ti = txt.textInfo;
//        float t = Time.unscaledTime * speed;

//        for (int mi = 0; mi < ti.meshInfo.Length; mi++)
//        {
//            var meshInfo = ti.meshInfo[mi];
//            var verts = meshInfo.vertices;
//            var cols = meshInfo.colors32;

//            for (int ci = 0; ci < ti.characterCount; ci++)
//            {
//                var ch = ti.characterInfo[ci];
//                if (!ch.isVisible || ch.materialReferenceIndex != mi) continue;

//                int charIndex = ch.index;
//                int v0 = ch.vertexIndex + 0;
//                int v1 = ch.vertexIndex + 1;
//                int v2 = ch.vertexIndex + 2;
//                int v3 = ch.vertexIndex + 3;

//                // 1) 색상 적용 (여러 ColorRange가 겹치면 마지막 등록 색이 우선)
//                for (int i = 0; i < colors.Count; i++)
//                {
//                    var r = colors[i];
//                    if (!InRange(charIndex, r.start, r.length)) continue;
//                    cols[v0] = cols[v1] = cols[v2] = cols[v3] = r.color;
//                }

//                // 2) 웨이브 오프셋
//                for (int i = 0; i < waves.Count; i++)
//                {
//                    var r = waves[i];
//                    if (!InRange(charIndex, r.start, r.length)) continue;
//                    float offY = Mathf.Sin(t + charIndex * (frequency * 0.05f)) * amplitude;
//                    Vector3 oy = new(0, offY, 0);
//                    verts[v0] += oy; verts[v1] += oy; verts[v2] += oy; verts[v3] += oy;
//                    break; // 해당 문자는 한 번만 오프셋
//                }
//                for (int i = 0; i < shakes.Count; i++)
//                {
//                    var r = shakes[i];
//                    if (!InRange(charIndex, r.start, r.length)) continue;

//                    float shakeAmount = 1.5f;
//                    Vector3 offset = new(
//                        Random.Range(-shakeAmount, shakeAmount),
//                        Random.Range(-shakeAmount, shakeAmount),
//                        0f);

//                    verts[v0] += offset;
//                    verts[v1] += offset;
//                    verts[v2] += offset;
//                    verts[v3] += offset;
//                    break; // 1개만 적용
//                }
//            }

//            // 변경 반영
//            meshInfo.mesh.vertices = verts;
//            meshInfo.mesh.colors32 = cols;
//            txt.UpdateGeometry(meshInfo.mesh, mi);
//        }
//    }
//}
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// TMP_Text에 범위 기반으로 웨이브, 떨림, 색상 효과를 적용하는 유틸형 MonoBehaviour
/// 하나의 텍스트 오브젝트뿐 아니라 외부에서도 할당해서 사용 가능
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TMPTextRangeEffects : MonoBehaviour
{
    // 이펙트 정의
    public struct WaveRange { public int start; public int length; }
    public struct ShakeRange { public int start; public int length; }
    public struct ColorRange { public int start; public int length; public Color32 color; }

    [Header("Wave 설정")]
    public float amplitude = 5f;
    public float frequency = 8f;
    public float speed = 4f;

    [Header("대상 TMP_Text")]
    public TMP_Text text;

    public List<WaveRange> waves = new();
    public List<ShakeRange> shakes = new();
    public List<ColorRange> colors = new();

    void Reset()
    {
        text = GetComponent<TMP_Text>();
    }

    void Awake()
    {
        if (!text) text = GetComponent<TMP_Text>();
    }

    void LateUpdate()
    {
        ApplyEffects(); // 컴포넌트로도 자동 작동
    }

    public void ClearEffects()
    {
        waves.Clear();
        shakes.Clear();
        colors.Clear();
    }

    public void AddWaveRange(int start, int length)
    {
        if (length > 0)
            waves.Add(new WaveRange { start = start, length = length });
    }

    public void AddShakeRange(int start, int length)
    {
        if (length > 0)
            shakes.Add(new ShakeRange { start = start, length = length });
    }

    public void AddColorRange(int start, int length, Color32 color)
    {
        if (length > 0)
            colors.Add(new ColorRange { start = start, length = length, color = color });
    }

    public void UpdateLastWaveLength(int newLength)
    {
        if (waves.Count > 0)
        {
            var r = waves[^1];
            r.length = Mathf.Max(0, newLength);
            waves[^1] = r;
        }
    }

    public void UpdateLastShakeLength(int newLength)
    {
        if (shakes.Count > 0)
        {
            var r = shakes[^1];
            r.length = Mathf.Max(0, newLength);
            shakes[^1] = r;
        }
    }

    public void UpdateLastColorLength(int newLength)
    {
        if (colors.Count > 0)
        {
            var r = colors[^1];
            r.length = Mathf.Max(0, newLength);
            colors[^1] = r;
        }
    }

    bool InRange(int idx, int start, int len) => idx >= start && idx < start + len;

    public void ApplyEffects()
    {
        if (!text) return;

        text.ForceMeshUpdate();
        var ti = text.textInfo;
        float t = Time.unscaledTime * speed;

        for (int mi = 0; mi < ti.meshInfo.Length; mi++)
        {
            var meshInfo = ti.meshInfo[mi];
            var verts = meshInfo.vertices;
            var cols = meshInfo.colors32;

            for (int ci = 0; ci < ti.characterCount; ci++)
            {
                var ch = ti.characterInfo[ci];
                if (!ch.isVisible || ch.materialReferenceIndex != mi) continue;

                int charIndex = ch.index;
                int v0 = ch.vertexIndex;

                // 색상
                for (int i = 0; i < colors.Count; i++)
                {
                    var r = colors[i];
                    if (InRange(charIndex, r.start, r.length))
                    {
                        cols[v0 + 0] = r.color;
                        cols[v0 + 1] = r.color;
                        cols[v0 + 2] = r.color;
                        cols[v0 + 3] = r.color;
                    }
                }

                // 웨이브
                for (int i = 0; i < waves.Count; i++)
                {
                    var r = waves[i];
                    if (InRange(charIndex, r.start, r.length))
                    {
                        float offY = Mathf.Sin(t + charIndex * (frequency * 0.05f)) * amplitude;
                        Vector3 oy = new(0, offY, 0);
                        verts[v0 + 0] += oy;
                        verts[v0 + 1] += oy;
                        verts[v0 + 2] += oy;
                        verts[v0 + 3] += oy;
                        break;
                    }
                }

                // 떨림
                for (int i = 0; i < shakes.Count; i++)
                {
                    var r = shakes[i];
                    if (InRange(charIndex, r.start, r.length))
                    {
                        float shakeAmount = 1.5f;
                        Vector3 offset = new(
                            Random.Range(-shakeAmount, shakeAmount),
                            Random.Range(-shakeAmount, shakeAmount),
                            0f);
                        verts[v0 + 0] += offset;
                        verts[v0 + 1] += offset;
                        verts[v0 + 2] += offset;
                        verts[v0 + 3] += offset;
                        break;
                    }
                }
            }

            meshInfo.mesh.vertices = verts;
            meshInfo.mesh.colors32 = cols;
            text.UpdateGeometry(meshInfo.mesh, mi);
        }
    }
}
