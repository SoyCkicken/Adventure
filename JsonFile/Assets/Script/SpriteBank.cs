using System.Collections.Generic;
using UnityEngine;

public class SpriteBank : MonoBehaviour
{
    // → Awake 시점에 “Images” 폴더(및 하위폴더) 안의 모든 스프라이트를 미리 로드
    Dictionary<string, Sprite> dict;

    void Awake()
    {
        // “Images” 안에 있고, 그 하위 폴더(Chapter1, UI 등)에 있는 모든 Sprite를 로드
        Sprite[] all = Resources.LoadAll<Sprite>("Images");

        dict = new Dictionary<string, Sprite>(all.Length);
        foreach (var sp in all)
        {
            // st.Name 은 파일명(확장자 제거) 이다
            if (!dict.ContainsKey(sp.name))
                dict.Add(sp.name, sp);
            else
                Debug.LogWarning($"같은 이름의 스프라이트가 이미 있습니다: {sp.name}");
        }
    }

    // 이름만 주면 내부 딕셔너리에서 찾아서 리턴
    public Sprite Load(string spriteName)
    {
        if (dict.TryGetValue(spriteName, out var result))
            return result;
        Debug.LogError($"SpriteBank: '{spriteName}' 스프라이트를 찾을 수 없습니다.");
        return null;
    }
}