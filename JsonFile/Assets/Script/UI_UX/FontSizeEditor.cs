using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class FontSizeManager : MonoBehaviour
{
    public int fontSize = 24;
    public int minFontSize = 16;
    public int maxFontSize = 48;
    public int TextLineSize = 0;
    public int TextminLineSize = 0;
    public int TextMaxLineSize = 12;
    public Button upFontSizebutton;
    public Button downFontSizebutton;
    public Button upLineSizebutton;
    public Button downLineSizebutton;
    public Button openOptionButton;
    public Button closeOptionButton;
    public GameObject optionUI;
    public ScrollRect scrollRect;
    public Button resetButton;
    public TMP_Text tMP;
    public TMP_Text tMP2;
    //일부 추가를 해줘야 하는 애들이 있음
    public List<TMP_Text> registeredTexts = new List<TMP_Text>();

    private void Start()
    {
        //여기서 덮어씌우고
       fontSize = Convert.ToInt32(tMP.text);
        TextLineSize = Convert.ToInt32(tMP2.text);

        upFontSizebutton.onClick.AddListener(() =>
        {
            IncreaseFontSize();
        });
        downFontSizebutton.onClick.AddListener(() =>
        {
            DecreaseFontSize();
        });
        upLineSizebutton.onClick.AddListener(() =>
        {
            IncreaseLineSize();
        });
        downLineSizebutton.onClick.AddListener(() =>
        {
            DecreaseLineSize();
        });
        openOptionButton.onClick.AddListener(() =>
        {
            optionUI.SetActive(true);
        });
        closeOptionButton.onClick.AddListener(() =>
        {
            optionUI.SetActive(false);
        });
        resetButton.onClick.AddListener(() =>
        {
            resetTextSetting();
        });
    }

    public void Register(TMP_Text text)
    {
        if (!registeredTexts.Contains(text))
        {
            registeredTexts.Add(text);
            text.fontSize = fontSize;
        }
    }

    public void IncreaseFontSize()
    {
        fontSize = Mathf.Min(fontSize + 2, maxFontSize);
        tMP.text = $"{fontSize}";
        //tMP.fontSize = fontSize;
        ApplyFontSizeToAll();
    }

    public void DecreaseFontSize()
    {
        fontSize = Mathf.Max(fontSize - 2, minFontSize);
        tMP.text = $"{fontSize}";
        //tMP.fontSize = fontSize;
        ApplyFontSizeToAll();
    }

    public void IncreaseLineSize()
    {
        TextLineSize = Mathf.Min(TextLineSize + 2, TextMaxLineSize);
        tMP2.text = $"{TextLineSize}";
        //tMP2.lineSpacing = TextLineSize;
        ApplyFontSizeToAll();
    }

    public void DecreaseLineSize()
    {
        TextLineSize = Mathf.Max(TextLineSize - 2, TextminLineSize);
        tMP2.text = $"{TextLineSize}";
        //tMP2.lineSpacing = TextLineSize;
        ApplyFontSizeToAll();
    }

    public void resetTextSetting()
    {
        TextLineSize = 0;
        fontSize = 24;
        ApplyFontSizeToAll();
    }

    public void ApplyFontSizeToAll()
    {
        foreach (var text in registeredTexts)
        {
            if (text != null)
                text.fontSize = fontSize;
            if (text != null)
                text.lineSpacing = TextLineSize;
        }

        Canvas.ForceUpdateCanvases();
        if (scrollRect != null && scrollRect.content != null)
        {
            var rt = scrollRect.content as RectTransform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            // 스크롤 포지션 유지 or 맨 아래로 보낼 때:
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}