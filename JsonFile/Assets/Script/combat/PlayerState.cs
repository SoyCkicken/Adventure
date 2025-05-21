using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public int Strength = 5;
    [SerializeField]
    public int DEX = 5;
    [SerializeField]
    public int Divinity = 5;
    [SerializeField]
    public int Int = 5;
    [SerializeField]
    public int MAG = 5;
    [SerializeField]
    public int Charisma = 5;
    [SerializeField]
    public int Health = 5;
    public int HP = 5;
    public int MP = 5;
    public int Level = 1;
    [Header("여기부터 능력치 UI관련되어 있는 옵션들입니다")]
    public GameObject PlayerStateObject;
    public int point;
    [SerializeField]
    public TMP_Text Strengthtext = null;
    public TMP_Text DEXtext = null;
    public TMP_Text CharismaText = null;
    public TMP_Text DivinityText = null;
    public TMP_Text IntText = null;
    public TMP_Text MAGtext = null;
    public TMP_Text HealthText = null;
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
    private int E_State = 0;
    private int Experience_required = 100;//필요 경험치
    private int Experience = 100000;
    private void Awake()
    {
        PlayerStateObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //최소 수치 보장
        if (Health / 3 >= 3)
        {
            HP = Health / 3;
        }
        else
        {
            HP = 3;
        }
        if (Int / 3 >= 3)
        {
            MP = Int / 3;
        }
        else
        {
            MP = 3;
        }

        if (point > 0)
        {
            CloseButton.SetActive(false);
            foreach (GameObject b in buttons)
            {
                b.SetActive(true);
            }
        }
        else
        {
            CloseButton.SetActive(true);
            foreach (GameObject b in buttons)
            {
                b.SetActive(false);
            }
        }

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
            temps = Strength;
            tempd = DEX;
            tempc = Charisma;
            tempi = Int;
            tempm = MAG;
            temph = Health;
            tempDi = Divinity;
        }
       
    }
    public void resetPlayerState()
    {
        point = tempp;
        Strength = temps;
        DEX = tempd;
        Charisma = tempc;
        Int = tempi;
        MAG = tempm;
        Health = temph;
        tempDi = Divinity;
        updateState();
    }
    public void closePlayerState()
    {
        PlayerStateObject.SetActive(false);
        tempp = 0;
    }
    public void updateState()
    {
        Strengthtext.text = Strength.ToString();
        DEXtext.text = DEX.ToString();
        CharismaText.text = Charisma.ToString();
        IntText.text = Int.ToString();
        MAGtext.text = MAG.ToString();
        HealthText.text = Health.ToString();
        DivinityText.text = Divinity.ToString();
    }
    public void AddStrength()
    {
        Strength++;
        point--;
        updateState();
    }
    public void AddDEX()
    {
        DEX++;
        point--;
        updateState();
    }
    public void AddCHR()
    {
        Charisma++;
        point--;
        updateState();
    }
    public void AddINT()
    {
        Int++;
        point--;
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
        updateState();
    }
    public void AddDivinity()
    {
        Divinity++;
        point--;
        updateState();
    }

}
