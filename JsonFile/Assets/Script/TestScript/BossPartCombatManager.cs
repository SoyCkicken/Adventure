// [1] BossPartCombatManager.cs
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossPartCombatManager : MonoBehaviour
{
    public TMP_Text logText;
    public Slider armSlider;
    public Slider legSlider;
    public Slider headSlider;
    public Slider totalHPSlider;
    public SkeletonAnimation BossSkeleton;
    private Boss testBoss;
    private Player testPlayer;
    private bool isPlayerTurn = true;
    private bool isRightArmBroken = false;


    void Start()
    {
        SkeletonAnimation skeletonAnim = BossSkeleton.GetComponent<SkeletonAnimation>();
        var skeleton = skeletonAnim.Skeleton;

        testBoss = new Boss("테스트보스", 100);

        testBoss.RegisterPart("팔", 50, () =>
        {
            Log("팔이 부서져 공격이 불가능합니다!");
            
            skeleton.FindSlot("R-arm").Attachment = null;
            isRightArmBroken = true;
        });

        testBoss.RegisterPart("다리", 50, () =>
        {
            Log("다리가 부서져 이동이 불가능합니다!");
            skeleton.FindSlot("R-leg").Attachment = null;
        });

        testBoss.RegisterPart("머리", 50, () =>
        {

            Log("머리가 부서져 즉사했습니다!");
            skeleton.FindSlot("head").Attachment = null;
            testBoss.Kill();
            Log("보스를 처치했습니다! (머리 파괴)");
            skeletonAnim.AnimationState.SetEmptyAnimation(0, 0.2f);
        });

        testPlayer = new Player("플레이어", 5000);

        UpdateSliders();
        Log("플레이어의 턴입니다.");
    }

    public void AttackPart(string partName)
    {
        SkeletonAnimation skeletonAnim = BossSkeleton.GetComponent<SkeletonAnimation>();
        if (!isPlayerTurn)
        {
            Log("지금은 플레이어 턴이 아닙니다.");
            return;
        }

        if (!testBoss.CanAttackPart(partName))
        {
            Log($"{partName} 부위는 이미 파괴되어 공격할 수 없습니다.");
            return;
        }

        testBoss.DamagePart(partName, testPlayer.AttackPower);
        Log($"플레이어가 {partName} 부위를 공격했습니다.\n");

        if (testBoss.IsDead)
        {
            Log("보스를 처치했습니다!");
            skeletonAnim.AnimationState.SetEmptyAnimation(0, 0.2f);
            return;
        }

        isPlayerTurn = false;
        UpdateSliders();

        Invoke(nameof(EnemyTurn), 1.5f);
    }

    void EnemyTurn()
    {
        if (testBoss.IsDead) return;
        if (isRightArmBroken)
        {
            Debug.Log("팔이 부러져서 공격이 불가능합니다");
            isPlayerTurn = true;
            Log("플레이어의 턴입니다");
            return;
        }
        testPlayer.TakeDamage(testBoss.attackPower);
        Log($"보스가 플레이어를 공격했습니다. ({testBoss.attackPower} 데미지)");

        if (testPlayer.IsDead)
        {
            Log("플레이어가 쓰러졌습니다...");
            return;
        }

        isPlayerTurn = true;
        Log("플레이어의 턴입니다.");
    }

    void UpdateSliders()
    {
        armSlider.value = testBoss.GetPartHPPercent("팔");
        legSlider.value = testBoss.GetPartHPPercent("다리");
        headSlider.value = testBoss.GetPartHPPercent("머리");
        totalHPSlider.value = testBoss.GetTotalHPPercent();
    }

    void Log(string message)
    {
        logText.text += message + "\n";
    }
}

// [2] Player.cs
public class Player
{
    public string Name;
    public int MaxHP = 500;
    public int CurrentHP;
    public int AttackPower = 30;

    public bool IsDead => CurrentHP <= 0;

    public Player(string name, int hp)
    {
        Name = name;
        MaxHP = hp;
        CurrentHP = hp;
    }

    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;
        CurrentHP = Mathf.Max(CurrentHP, 0);
    }
}

// [3] Boss.cs
public class Boss
{
    public string name;
    public int attackPower = 50;
    public int MaxTotalHP;
    public int CurrentTotalHP;
    private Dictionary<string, MonsterPart> parts = new();

    public bool IsDead => CurrentTotalHP <= 0;

    public Boss(string name, int totalHP)
    {
        this.name = name;
        MaxTotalHP = totalHP;
        CurrentTotalHP = totalHP;
    }

    public void RegisterPart(string name, int hp, System.Action onBreak)
    {
        parts[name] = new MonsterPart(name, hp, onBreak);
    }

    public void DamagePart(string name, int amount)
    {
        if (!parts.ContainsKey(name)) return;

        if (IsDead) return;

        parts[name].Damage(amount);
        CurrentTotalHP -= amount;
        CurrentTotalHP = Mathf.Max(CurrentTotalHP, 0);
    }

    public void Kill()
    {
        CurrentTotalHP = 0;
    }

    public float GetPartHPPercent(string name)
    {
        if (parts.ContainsKey(name))
        {
            return parts[name].CurrentHP / (float)parts[name].MaxHP;
        }
        return 0f;
    }

    public float GetTotalHPPercent()
    {
        return CurrentTotalHP / (float)MaxTotalHP;
    }

    public bool CanAttackPart(string name)
    {
        return parts.ContainsKey(name) && !parts[name].IsBroken;
    }
}

// [4] MonsterPart.cs
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
