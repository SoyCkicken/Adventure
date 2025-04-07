using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //    스탯 이름   약어 설명  주요 기능
    //힘(Strength)    STR 근접 전투력, 체력 계열   물리 대미지, 무기 착용 조건, 밀기/파괴
    //민첩(Agility)    AGI 속도와 회피력 선공, 회피율, 도주, 은신
    //지능(Intelligence)   INT 지식, 전략적 판단 퍼즐 해석, 마법 숙련도, 정보 습득
    //마력(Magic)  MAG 마법적 에너지 마법 데미지, 마나량, 마법 무기 사용
    //신성(Divinity)   DIV 신적인 능력치 신계 능력, 신성 장비 사용, 특정 선택지
    //매력(Charisma)   CHA 설득과 대인 영향력  대화/협상 성공률, NPC 반응, 특정 이벤트 해금

    public int Strength = 5;
    public int Agility = 5;
    public int Intelligence = 5;
    public int Magic = 5;
    public int Divinity = 5;
    public int Charisma = 5;
    public int HP = 5;
    public int MP = 5;
    private int Level = 1;
    private int Experience_required = 100;//필요 경험치
    private int Experience = 100000;
    public void Awake()
    {
        Experience_required = (int)(100 * (Level * 1.12f));
        Debug.Log($"플레이어의 레벨 : {Level} 다음 레벨에 필요한 경험치 : {Experience_required} 남은 경험치 : {Experience}");
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Experience_required);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LevelUp()
    {
        
        if (Experience >= Experience_required)
        {
            Experience -= Experience_required;
            Level++;
            Experience_required = (int)(100 * (Level * 1.12f));
            Debug.Log($"플레이어의 레벨 : {Level} 다음 레벨에 필요한 경험치 : {Experience_required} 남은 경험치 : {Experience}");
        }
        else 
        {
            Debug.LogError("경험치가 부족합니다");
        }
        



    }
}
