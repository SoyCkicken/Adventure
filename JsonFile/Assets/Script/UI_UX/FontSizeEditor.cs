using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.IO;
using static SaveManager;
using UnityEngine.Playables;

public class FontSizeManager : MonoBehaviour
{
    //싱글톤으로 바꿀 예정
    //이유 게임 로비 화면은 다른 씬인데 그때 수정이 가능하게 
    SaveManager saveManager;
    public int fontSize;
    public int minFontSize;
    public int maxFontSize;
    public int TextLineSize;
    public int TextminLineSize;
    public int TextMaxLineSize;
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
    public TMP_Text SaveDataTimeText;
    public Button SaveButton;
    public Button LoadButton;
    //일부 추가를 해줘야 하는 애들이 있음
    public List<TMP_Text> registeredTexts = new List<TMP_Text>();

    //세이브 로드 용

    private void Awake()
    {
        saveManager = SaveManager.Instance;
    }

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
        SaveButton.onClick.AddListener(() => { saveManager.SaveGame(); });
        LoadButton.onClick.AddListener(() => { saveManager.LoadGame(); });

        LoadSaveTimeOnly();
    }

    public void Register(TMP_Text text)
    {
        if (!registeredTexts.Contains(text))
        {
            registeredTexts.Add(text);
            text.fontSize = fontSize;
            text.lineSpacing = TextLineSize;
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
    public void LoadSaveTimeOnly()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("세이브 파일 없음");
            if (SaveDataTimeText != null) SaveDataTimeText.text = "저장 기록 없음";
            return;
        }

        string json = File.ReadAllText(SavePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (SaveDataTimeText != null)
            SaveDataTimeText.text = $"최근 저장 시간 : {data.saveTime}";
    }
}