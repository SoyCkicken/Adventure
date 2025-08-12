using System.Collections.Generic;
using UnityEngine;

namespace MyGame.TextEffects
{
    public static class TextEffectParser
    {
        public static List<TextFragment> ParseFragments(string input)
        {
            var result = new List<TextFragment>();
            var effectStack = new Stack<TextEffect>();
            int i = 0;

            while (i < input.Length)
            {
                if (input[i] == '<')
                {
                    int tagEnd = input.IndexOf('>', i);
                    if (tagEnd == -1) break;

                    string tagContent = input.Substring(i + 1, tagEnd - i - 1).Trim();

                    // 종료 태그
                    if (tagContent.StartsWith("/"))
                    {
                        if (effectStack.Count > 0)
                            effectStack.Pop();
                        else
                            Debug.LogWarning($"[TextEffectParser] 종료 태그 `{tagContent}`가 잘못되었습니다.");

                        i = tagEnd + 1;
                        continue;
                    }

                    // 시작 태그
                    bool handled = false;

                    if (tagContent == "웨이브")
                    {
                        effectStack.Push(new TextEffect { type = EffectType.Wave });
                        handled = true;
                    }
                    else if (tagContent == "떨림")
                    {
                        effectStack.Push(new TextEffect { type = EffectType.Shake });
                        handled = true;
                    }
                    else if (tagContent.StartsWith("색:"))
                    {
                        string hex = tagContent.Substring(2);
                        if (!hex.StartsWith("#")) hex = "#" + hex;
                        if (ColorUtility.TryParseHtmlString(hex, out var col))
                        {
                            effectStack.Push(new TextEffect
                            {
                                type = EffectType.Color,
                                color = col
                            });
                            handled = true;
                        }
                        else
                        {
                            Debug.LogWarning($"[TextEffectParser] 색상 파싱 실패: {hex}");
                        }
                    }

                    if (!handled)
                        Debug.LogWarning($"[TextEffectParser] 알 수 없는 태그: <{tagContent}>");

                    i = tagEnd + 1;
                }
                else
                {
                    int nextTag = input.IndexOf('<', i);
                    int len = (nextTag == -1 ? input.Length : nextTag) - i;
                    string text = input.Substring(i, len);
                    result.Add(new TextFragment(text, new List<TextEffect>(effectStack)));
                    i += len;
                }
            }

            return result;
        }
    }
}
