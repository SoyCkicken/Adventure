using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerState : MonoBehaviour
{
    [Header("플레이어 능력치")]
    public int STR = 5, AGI = 5, DIV = 5, MAG = 5, CHA = 5;
    public int Health = 5, INT = 5;
    public int HP { get;  set; }
    public int MP { get;  set; }
    public int CurrentHealth = 0, CurrentMental = 0;

    public int Level = 1;
    public int Experience = 100000;
    public int ExperienceRequired = 100;

    [SerializeField] public PlayerStatsUI statsUI;
    [SerializeField] public InventoryManager inventoryManager;
    [SerializeField] public EquipmentSystem equipmentSystem;

    public static PlayerState Instance { get; private set; }

    public float HealthRatio => HP > 0 ? (float)CurrentHealth / HP : 1f; //계산
    public float MentalRatio => MP > 0 ? (float)CurrentMental / MP : 1f;

    public int CurrentChapterIndex = 0;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        RecalculateHPMP();
    }

    private void Start()
    {
        
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (statsUI == null)
            statsUI = FindObjectOfType<PlayerStatsUI>();
        if (inventoryManager == null)
            inventoryManager = FindObjectOfType<InventoryManager>();
        if (equipmentSystem == null)
            equipmentSystem = FindObjectOfType<EquipmentSystem>();
    }


    public void GenerateRandomStats()
    {
        // 최소 스탯 4를 보장하고, 추가 포인트 18점을 랜덤 분배
        int[] stats = new int[6]; // STR, AGI, DIV, MAG, CHA, Health
        for (int i = 0; i < stats.Length; i++) stats[i] = 4;

        int remaining = 18;

        while (remaining > 0)
        {
            int index = UnityEngine.Random.Range(0, stats.Length);
            stats[index]++;
            remaining--;
        }

        STR = stats[0];
        AGI = stats[1];
        DIV = stats[2];
        MAG = stats[3];
        CHA = stats[4];
        Health = UnityEngine.Random.Range(4, 8);// 체력도 따로 랜덤
        INT = UnityEngine.Random.Range(4, 8); // 지능은 따로 랜덤 (원하면 분배 포함 가능)
        
        RecalculateHPMP();
    }


    public void RecalculateHPMP()
    {
        HP = CalculateHealth(Health);
        MP = CalculateMental(INT);
        CurrentHealth = HP;
        CurrentMental = MP;
    }

    public int CalculateHealth(int value)
    {
        return value >= 15 ? 5 : Mathf.Max(value / 3, 3);
    }
    public int CalculateMental(int value)
    {
        return value >= 15 ? 5 : Mathf.Max(value / 3, 3);
    }

    public void SavePlayer(ref SaveManager.SaveData data)
    {
        data.STR = STR;
        data.INT = INT;
        data.AGI = AGI;
        data.MAG = MAG;
        data.CHA = CHA;
        data.Health = Health;
        data.HP = HP;
        data.MP = MP;
        data.Experience = Experience;
        data.ExperienceRequired = ExperienceRequired;
        data.Level = Level;
    }

    // 불러오기 - 넘겨받은 data에서 값만 꺼내 사용
    public void LoadPlayer(SaveManager.SaveData data)
    {
        STR = data.STR;
        INT = data.INT;
        AGI = data.AGI;
        MAG = data.MAG;
        CHA = data.CHA;
        Health = data.Health;
        HP = data.HP;
        MP = data.MP;
        Level = data.Level;
        Experience = data.Experience;
        ExperienceRequired = data.ExperienceRequired;
        CurrentChapterIndex = data.PlayerCurrentChapterIndex;
        if (statsUI!=null)
        statsUI.UpdateUI();
        if (equipmentSystem != null)
            equipmentSystem.Init();
    }
}
