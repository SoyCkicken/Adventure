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
    public Button upbutton;
    public Button downbutton;
    public Button openOptionButton;
    public Button closeOptionButton;
    public GameObject optionUI;
    public TMP_Text tMP;
    //일부 추가를 해줘야 하는 애들이 있음
    public List<TMP_Text> registeredTexts = new List<TMP_Text>();

    private void Start()
    {
        //여기서 덮어씌우고
       fontSize = Convert.ToInt32(tMP.text);

        upbutton.onClick.AddListener(() =>
        {
            IncreaseFontSize();
        });
        downbutton.onClick.AddListener(() =>
        {
            DecreaseFontSize();
        });
        openOptionButton.onClick.AddListener(() =>
        {
            optionUI.SetActive(true);
        });
        closeOptionButton.onClick.AddListener(() =>
        {
            optionUI.SetActive(false);
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
        tMP.fontSize = fontSize;
        ApplyFontSizeToAll();
    }

    public void DecreaseFontSize()
    {
        fontSize = Mathf.Max(fontSize - 2, minFontSize);
        tMP.text = $"{fontSize}";
        tMP.fontSize = fontSize;
        ApplyFontSizeToAll();
    }

    public void ApplyFontSizeToAll()
    {
        foreach (var text in registeredTexts)
        {
            if (text != null)
                text.fontSize = fontSize;
        }
    }
}