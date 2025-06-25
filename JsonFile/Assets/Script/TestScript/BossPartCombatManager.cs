using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossPartCombatManager : MonoBehaviour
{
    public Text logText;
    public Slider armSlider;
    public Slider legSlider;
    public Slider headSlider;

    private Boss testBoss;

    void Start()
    {
        testBoss = new Boss("테스트보스");

        testBoss.RegisterPart("팔", 100, () =>
        {
            testBoss.attackPower -= 20;
            Log("팔이 부서져 공격력이 감소합니다!");
        });

        testBoss.RegisterPart("다리", 100, () =>
        {
            Log("다리가 부서져 이동력이 감소합니다!");
        });

        testBoss.RegisterPart("머리", 80, () =>
        {
            Log("머리가 부서져 정확도가 떨어집니다!");
        });

        UpdateSliders();
    }

    public void AttackPart(string partName)
    {
        testBoss.DamagePart(partName, 30);
        Log($"{partName} 부위를 공격했습니다.");
        UpdateSliders();
    }

    void UpdateSliders()
    {
        armSlider.value = testBoss.GetPartHPPercent("팔");
        legSlider.value = testBoss.GetPartHPPercent("다리");
        headSlider.value = testBoss.GetPartHPPercent("머리");
    }

    void Log(string message)
    {
        logText.text += message + "\n";
    }
    // [2] Boss.cs
    public class Boss
    {
        public string name;
        public int attackPower = 50;
        private Dictionary<string, MonsterPart> parts = new();

        public Boss(string name)
        {
            this.name = name;
        }

        public void RegisterPart(string name, int hp, System.Action onBreak)
        {
            parts[name] = new MonsterPart(name, hp, onBreak);
        }

        public void DamagePart(string name, int amount)
        {
            if (parts.ContainsKey(name))
            {
                parts[name].Damage(amount);
            }
        }

        public float GetPartHPPercent(string name)
        {
            if (parts.ContainsKey(name))
            {
                return parts[name].CurrentHP / (float)parts[name].MaxHP;
            }
            return 0f;
        }
    }

    // [3] MonsterPart.cs
    public class MonsterPart
    {
        public string Name;
        public int MaxHP;
        public int CurrentHP;
        public System.Action OnBreak;

        public bool IsBroken => CurrentHP <= 0;

        public MonsterPart(string name, int hp, System.Action onBreak)
        {
            Name = name;
            MaxHP = hp;
            CurrentHP = hp;
            OnBreak = onBreak;
        }

        public void Damage(int amount)
        {
            if (IsBroken) return;

            CurrentHP -= amount;
            CurrentHP = Mathf.Max(CurrentHP, 0);

            if (IsBroken)
            {
                OnBreak?.Invoke();
            }
        }
    }
}