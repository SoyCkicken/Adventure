using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class PlayerState : MonoBehaviour
{
    // Start is called before the first frame update
    //힘, 민첩, 신성, 매력, 체력, 정신력
    //    스탯 이름   약어 설명  주요 기능
    //힘(Strength)    STR 근접 전투력, 체력 계열   물리 대미지, 무기 착용 조건, 밀기/파괴
    //민첩(Agility)    AGI 속도와 회피력 선공, 회피율, 도주, 은신
    //신성(Divinity)   DIV 신적인 능력치 신계 능력, 신성 장비 사용, 특정 선택지
    //매력(Charisma)   CHA 설득과 대인 영향력  대화/협상 성공률, NPC 반응, 특정 이벤트 해금
    //체력(Health)    플레이어의 전투 체력 및 일반 체력과 연관있음
    //정신력(Mentality) 플레이어의 일반 정신력
    //공격력(Damage)           //전투시 얼마 정도의 데미지를 입히는지
    //공격력 = 무기 공격력 + (스탯 가중치*스탯) 정도? 
    //방어력(Ammor)            //전투시 얼마 정도의 데미지 감소를 시키는지
    //방어력 = 방어구 방어력 +(스탯 가중치 * 스탯)  +if(방패 같이 추가 방어력을 올릴수 있는 수단 + (스탯 가중치 + 스탯))
    //데미지 계산 공식
    //타겟의 체력 =- 공격력 - 방어력 + 장비 옵션 데미지 
    //공격 속도(AttackSpeed) 얼마 속도로 공격을 하는지
    //회피(Dodge)        몇퍼센트 확률로 데미지를 회피하는지
    //회피 장비들은 전부 더하는 식으로 계산할 예정
    //예시) 방어구에서 15%만큼 올려주고 기본으로 15% 무기에서 10%가 추가 된다면 총 40%확률로 회피한다 이런식으로

    [SerializeField]
    public JsonManager jsonManager;
    [SerializeField]
    public int STR = 5;
    [SerializeField]
    public int AGI = 5;
    [SerializeField]
    public int DIV = 5;
    [SerializeField]
    public int INT = 5;
    [SerializeField]
    public int MAG = 5;
    [SerializeField]
    public int CHA = 5;
    [SerializeField]
    public int Health = 5;
    public int CurrentHealth = 0;
    public int HP = 0;
    public int CurrentMental = 0;
    public int MP = 0;
    public int Level = 1;
    [Header("여기부터 능력치 UI관련되어 있는 옵션들입니다")]
    public GameObject PlayerStateObject;
    public int point;
    [SerializeField]
    public TMP_Text StateSTRhtext = null;
    public TMP_Text StateAGItext = null;
    public TMP_Text StateCHAText = null;
    public TMP_Text StateDIVText = null;
    public TMP_Text StateINTText = null;
    public TMP_Text StateMAGtext = null;
    public TMP_Text StateHealthText = null;
    public TMP_Text UISSTRtext = null;
    public TMP_Text UIDEXtext = null;
    public TMP_Text UICHAText = null;
    public TMP_Text UIDIVText = null;
    public TMP_Text UIINTText = null;
    public TMP_Text UIMAGtext = null;
    //public TMP_Text levelTEXT = null;
    public List<GameObject> buttons;
    private int tempp;
    private int temps;
    private int tempd;
    private int tempc;
    private int tempi;
    private int tempm;
    private int temph;
    private int tempDi;
    public GameObject CloseButton;
    public InventoryManager InventoryManager;
    private int Experience_required = 100;//필요 경험치
    public int Experience = 100000;
    private void Awake()
    {
        PlayerStateObject.SetActive(false);

        //시작할때 체력 제한을 두고
        if (Health / 3 >= 3)
        {
            if (Health >= 15)
            {
                HP = 5;
            }
            HP = Health / 3;

        }
        else
        {
            HP = 3;
        }
        if (INT / 3 >= 3)
        {
            if (INT >= 15)
            {
                MP = 5;
            }
            MP = INT / 3;
        }
        else
        {
            MP = 3;
        }
        CurrentHealth = HP;
        CurrentMental = MP;
        updateState();
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentHealth >= HP)
            CurrentHealth = HP;
        if (CurrentMental >= MP)
            CurrentMental = MP;
        //최소 수치 보장
    }
    public void levelUp()
    {
        if (Experience > Experience_required)
        {
            updateState();
            Experience -= Experience_required;
            Experience_required = Convert.ToInt32(Experience_required * 1.2f);
            point += 3;
            tempp = point;
            PlayerStateObject.SetActive(true);
            temps = STR;
            tempd = AGI;
            tempc = CHA;
            tempi = INT;
            tempm = MAG;
            temph = Health;
            tempDi = DIV;
        }

    }
    public void resetPlayerState()
    {
        point = tempp;
        STR = temps;
        AGI = tempd;
        CHA = tempc;
        INT = tempi;
        MAG = tempm;
        Health = temph;
        tempDi = DIV;
        updateState();
    }
    public void closePlayerState()
    {
        PlayerStateObject.SetActive(false);
        tempp = 0;
    }
    public void updateState()
    {
        StateSTRhtext.text = STR.ToString();
        StateAGItext.text = AGI.ToString();
        StateCHAText.text = CHA.ToString();
        StateINTText.text = INT.ToString();
        StateMAGtext.text = MAG.ToString();
        StateHealthText.text = Health.ToString();
        StateDIVText.text = DIV.ToString();

        UISSTRtext.text = STR.ToString();
        UIDEXtext.text = AGI.ToString();
        UICHAText.text = CHA.ToString();
        UIINTText.text = INT.ToString();
        UIMAGtext.text = MAG.ToString() ;
        UIDIVText.text = DIV.ToString() ;
        InventoryManager.updateDPS_MaxHealth();
        InventoryManager.UpdateInventoryByStrength();
    }
    public void AddStrength()
    {
        STR++;
        point--;
        updateState();
    }
    public void AddDEX()
    {
        AGI++;
        point--;
        updateState();
    }
    public void AddCHR()
    {
        CHA++;
        point--;
        updateState();
    }
    public void AddINT()
    {
        INT++;
        point--;
        if (INT / 3 >= 3)
        {
            if (INT >= 15)
            {
                MP = 5;
            }
            MP = INT / 3;
        }
        else
        {
            MP = 3;
        }
        updateState();
    }
    public void AddMAG()
    {
        MAG++;
        point--;
        updateState();
    }
    public void AddHealth()
    {
        Health++;
        point--;
        if (Health / 3 >= 3)
        {
            if (Health >= 15)
            {
                HP = 5;
            }
            HP = Health / 3;

        }
        else
        {
            HP = 3;
        }
        updateState();
    }
    public void AddDivinity()
    {
        DIV++;
        point--;
        updateState();
    }

}