using TMPro;
using UnityEngine.UI;
using UnityEngine;

public static class ChoiceUIHelper
{
    public static void CreateChanceBadge(
        GameObject buttonGO,
        TMP_Text mainText,
        float rate01,
        Sprite bgSprite = null,
        float yOffset = -8f,
        int labelSize = 22,
        float percentScale = 1.5f)
    {
        if (rate01 <= 0f && rate01 >= 0f == false) return;

        var btnRT = buttonGO.GetComponent<RectTransform>();

        var holder = new GameObject("ChanceBadge", typeof(RectTransform));
        holder.transform.SetParent(buttonGO.transform, false);
        var hRT = (RectTransform)holder.transform;
        hRT.anchorMin = new Vector2(0.5f, 1f);
        hRT.anchorMax = new Vector2(0.5f, 1f);
        hRT.pivot = new Vector2(0.5f, 1f);
        hRT.anchoredPosition = new Vector2(0f, yOffset);
        hRT.sizeDelta = new Vector2(btnRT.rect.width * 0.9f, 0f);

        var le = holder.AddComponent<LayoutElement>();
        le.ignoreLayout = true;

        if (bgSprite != null)
        {
            var bgGO = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bgGO.transform.SetParent(holder.transform, false);
            var bg = bgGO.GetComponent<Image>();
            bg.sprite = bgSprite;
            bg.type = Image.Type.Sliced;
            bg.raycastTarget = false;

            var bgRT = (RectTransform)bgGO.transform;
            bgRT.anchorMin = new Vector2(0f, 0f);
            bgRT.anchorMax = new Vector2(1f, 1f);
            bgRT.pivot = new Vector2(0.5f, 0.5f);
            bgRT.offsetMin = new Vector2(8f, -2f);
            bgRT.offsetMax = new Vector2(-8f, 26f);
        }

        var txtGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(holder.transform, false);
        var t = txtGO.GetComponent<TextMeshProUGUI>();

        if (mainText is TextMeshProUGUI main) t.font = main.font;

        t.richText = true;
        t.fontSize = labelSize;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        t.enableWordWrapping = false;

        t.color = GetChanceColor(rate01);
        var mat = t.fontMaterial;
        mat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.18f);
        mat.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0, 0, 0, 0.75f));

        float pct = Mathf.Clamp01(rate01) * 100f;
        string pctStr = pct.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
        t.text = $"¼º°ø·ü <size={(int)(percentScale * 100)}%>{pctStr}%</size>";

        var trt = (RectTransform)txtGO.transform;
        trt.anchorMin = new Vector2(0f, 0f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(0.5f, 0.5f);
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }

    private static Color GetChanceColor(float r)
    {
        if (r >= 0.8f) return new Color32(65, 200, 90, 255);
        if (r >= 0.6f) return new Color32(200, 190, 60, 255);
        if (r >= 0.4f) return new Color32(220, 120, 60, 255);
        return new Color32(200, 70, 70, 255);
    }
}
