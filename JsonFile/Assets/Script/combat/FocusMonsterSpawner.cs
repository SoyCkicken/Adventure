using MyGame;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class FocusMonsterSpawner : MonoBehaviour
{
    [Header("집중 전투 전용")]
    public JsonManager jsonManager;
    public MonsterOptionManager monsterOptionManager;
    public List<GameObject> focusCombatPrefab; //집중 전투 적 오브젝트 <---이거 재활용은 안되고 필요할때마다 뽑아서 사용 해야 함
                                               //그러면 리스트에 넣어서 파일 이름이 같은걸 찾아서 생성 하는 식으로 처리를 해야 할듯
    public GameObject focusCombcanves;   //집중 전투 캔버스
    public GameObject focusCombatImage; //집중 전투 캔버스 이미지 자식으로 생성 될 곳
    public GameObject enemy;
    public GameObject player;
    public BossPartCombatManager bossPartCombatManager; //집중 전투용 매니저
    public TESTBoss TESTBoss; //집중 전투용 테스트 보스
    private GameObject _currentMonster; //<-이거 공용 프리팹으로 사용할듯? 
    // 외부에서 접근할 수 있도록 프로퍼티
    public GameObject CurrentMonster => _currentMonster;
    private void Awake()
    {
        jsonManager = JsonManager.Instance; // 수정

        monsterOptionManager = MonsterOptionManager.Instance; // 몬스터 옵션 매니저 인스턴스 가져오기
        if (player == null) player = GameObject.FindWithTag("Player");
        focusCombcanves.SetActive(false);
    }
    public void SpawnFocusBossByID(string monsterID)
    {
        focusCombcanves.SetActive(true); // 집중 전투 캔버스 활성화

        // 기존 몬스터 제거
        if (_currentMonster != null)
            Destroy(_currentMonster);

        // JSON 데이터 로딩
        var data = jsonManager.GetMonMasters("Mon_Master")
                              .FirstOrDefault(m => m.Mon_ID == monsterID);
        if (data == null)
        {
            Debug.LogError($"[SpawnFocusBossByID] 몬스터 데이터 {monsterID} 없음");
            return;
        }

        // 프리팹 찾기
        var prefab = focusCombatPrefab.FirstOrDefault(go => go.name == data.Mon_Name);
        if (prefab == null)
        {
            Debug.LogError($"[SpawnFocusBossByID] 집중 전투 프리팹 {data.Mon_Name} 없음");
            return;
        }

        // 몬스터 프리팹 생성
        Vector3 spawnPosition = new Vector3(-565, 866, 0);
        _currentMonster = Instantiate(prefab, focusCombatImage.transform.position, Quaternion.identity, focusCombatImage.transform);
        _currentMonster.transform.localPosition = spawnPosition;
        _currentMonster.transform.localScale = new Vector3(100, 100, 1);
        _currentMonster.name = data.Mon_Name;
        enemy = _currentMonster; // enemy 필드에 할당

        // ----------------------- 컴포넌트 추출 및 필드에 할당 -----------------------

        var skeletonAnim = _currentMonster.GetComponent<SkeletonAnimation>();
        var bossScriptFromObj = _currentMonster.GetComponentInChildren<TESTBoss>();
        var partManagerFromObj = this.bossPartCombatManager;

        if (skeletonAnim == null || bossScriptFromObj == null || partManagerFromObj == null)
        {
            if (skeletonAnim == null)
                Debug.LogError("[SpawnFocusBossByID] SkeletonAnimation 컴포넌트가 없음");
            if (bossScriptFromObj == null)
                Debug.LogError("[SpawnFocusBossByID] TESTBoss 컴포넌트가 없음");
            if (partManagerFromObj == null)
                Debug.LogError("[SpawnFocusBossByID] BossPartCombatManager 컴포넌트가 없음");

        }

        // FocusMonsterSpawner 필드에 할당
        TESTBoss = bossScriptFromObj;
        bossPartCombatManager = partManagerFromObj;

        // ----------------------- 데이터 세팅 -----------------------

        TESTBoss.skeletonAnimation = skeletonAnim;
        TESTBoss.BossPartCombatManager = bossPartCombatManager;
        TESTBoss.bossName = data.Mon_Name;
        TESTBoss.MaxTotalHP = data.Mon_HP;
        TESTBoss.CurrentTotalHP = data.Mon_HP;
        TESTBoss.attackPower = data.Mon_ATK;
        //TESTBoss.hitChance = data.Mon_Speed;

        bossPartCombatManager.BossSkeleton = skeletonAnim;
        bossPartCombatManager.TESTBoss = TESTBoss;
        bossPartCombatManager.playerCharacter = player.GetComponent<Character>(); // 필요하면

        // ----------------------- 초기화 -----------------------

        //TESTBoss.InitializePartsFromHitbox(); // 부위 등록
        TESTBoss.Init();                      // 체력 및 OnBreak 세팅
        bossPartCombatManager.Initialize();   // 전투 매니저 초기화

        // ----------------------- 패시브 옵션 적용 -----------------------

        ApplyPassive(data.MonPas_Effect1, data.Effect1_Stat, data.Mon_ID, null); // Character 없음
        ApplyPassive(data.MonPas_Effect2, data.Effect2_Stat, data.Mon_ID, null);

        Debug.Log($"[SpawnFocusBossByID] 집중 전투 몬스터 {data.Mon_Name} 생성 완료");
    }
    private void ApplyPassive(string optionID, int value, string sourceID, Character target)
    {
        if (string.IsNullOrEmpty(optionID) || optionID == "--" || optionID == null)
            return;

        var ctx = new OptionContext
        {
            User = enemy.GetComponent<Character>(),
            Target = player.GetComponent<Character>(),
            option_ID = optionID,
            Value = value,
            // 필요한 추가 컨텍스트 필드 설정
        };
        Debug.Log($"ApplyPassive에서의 {value}");
        Debug.Log($"ApplyPassive = ctx.user의 값 : {ctx.User}\nApplyPassive = ctx.Target = {ctx.Target}");
        monsterOptionManager.ApplyMonsterOption(optionID, ctx);
    }
}
