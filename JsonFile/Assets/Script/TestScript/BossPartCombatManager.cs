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

        testBoss = new Boss("�׽�Ʈ����", 100);

        testBoss.RegisterPart("��", 50, () =>
        {
            Log("���� �μ��� ������ �Ұ����մϴ�!");

            skeleton.FindSlot("R-arm").Attachment = null;
            isRightArmBroken = true;
        });

        testBoss.RegisterPart("�ٸ�", 50, () =>
        {
            Log("�ٸ��� �μ��� �̵��� �Ұ����մϴ�!");
            skeleton.FindSlot("R-leg").Attachment = null;
        });

        testBoss.RegisterPart("�Ӹ�", 50, () =>
        {

            Log("�Ӹ��� �μ��� ����߽��ϴ�!");
            skeleton.FindSlot("head").Attachment = null;
            testBoss.Kill();
            Log("������ óġ�߽��ϴ�! (�Ӹ� �ı�)");
            skeletonAnim.AnimationState.SetEmptyAnimation(0, 0.2f);
        });

        testPlayer = new Player("�÷��̾�", 5000);

        UpdateSliders();
        Log("�÷��̾��� ���Դϴ�.");
    }

    public void AttackPart(string partName)
    {
        SkeletonAnimation skeletonAnim = BossSkeleton.GetComponent<SkeletonAnimation>();
        if (!isPlayerTurn)
        {
            Log("������ �÷��̾� ���� �ƴմϴ�.");
            return;
        }

        if (!testBoss.CanAttackPart(partName))
        {
            Log($"{partName} ������ �̹� �ı��Ǿ� ������ �� �����ϴ�.");
            return;
        }

        testBoss.DamagePart(partName, testPlayer.AttackPower);
        Log($"�÷��̾ {partName} ������ �����߽��ϴ�.\n");

        if (testBoss.IsDead)
        {
            Log("������ óġ�߽��ϴ�!");
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
            Debug.Log("���� �η����� ������ �Ұ����մϴ�");
            isPlayerTurn = true;
            Log("�÷��̾��� ���Դϴ�");
            return;
        }
        testPlayer.TakeDamage(testBoss.attackPower);
        Log($"������ �÷��̾ �����߽��ϴ�. ({testBoss.attackPower} ������)");

        if (testPlayer.IsDead)
        {
            Log("�÷��̾ ���������ϴ�...");
            return;
        }

        isPlayerTurn = true;
        Log("�÷��̾��� ���Դϴ�.");
    }

    void UpdateSliders()
    {
        armSlider.value = testBoss.GetPartHPPercent("��");
        legSlider.value = testBoss.GetPartHPPercent("�ٸ�");
        headSlider.value = testBoss.GetPartHPPercent("�Ӹ�");
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