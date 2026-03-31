using System;

using UnityEngine;
using UnityEngine.Playables;
using static SaveManager;

public class GameFlowManager : MonoBehaviour
{
    public enum FlowState { None, MainStory, RandomEvent, Battle }
    FlowState currentState = FlowState.None;
    FlowState prevState = FlowState.None;

    [SerializeField] PlayerState playerState;
    public StoryDisplayManager mainStoryManager;
    public EventDisplay randomEventManager;
    public BattleManager battleManager;
    public MonsterSpawner monsterSpawner;
    public InventoryManager inventoryManager;
    [Header("집중 전투 전용")]
    // [SerializeField] public FocusMonsterSpawner focusMonsterSpawner; // 집중 전투용 몬스터 스포너
    [SerializeField] public TESTBoss testBoss; // 집중 전투용 TESTBoss
    [SerializeField] public BossPartCombatManager bossPartCombatManager; // 일반 전투용 CombatTest
    private string pendingMonsterID;
    //public int CurrentChapterIndex = 1;



    void Start()
    {
        playerState = PlayerState.Instance;
        mainStoryManager.OnBattleJoin += HandleStoryBattleJoin;
        randomEventManager.OnBattleJoin += HandleEventBattleJoin;
        if (playerState.CurrentChapterIndex == 0)
        {
            playerState.CurrentChapterIndex++; // 챕터 증가
            EnterState(FlowState.MainStory); // 최초 진입
        }
    }
    //현재 스테이트 확인용 <--
    public FlowState GetCurrentFlowState()
    {
        return currentState;
    }

    public void EnterState(FlowState next)
    {
        if (currentState == next)
        {
            Debug.LogWarning($"[GameFlow] 이미 {next} 상태입니다. 중복 진입 차단.");
            return;
        }

        // 이전 상태 종료
        switch (currentState)
        {
            case FlowState.MainStory:
                mainStoryManager.StopMainStory();
                break;
            case FlowState.RandomEvent:
                randomEventManager.StopRandomEvent();
                break;
            case FlowState.Battle:
                battleManager.StopBattle();
                break;
        }

        currentState = next;

        // 새 상태 시작
        switch (currentState)
        {
            case FlowState.MainStory:
                Debug.Log("▶ 메인 스토리에 진입합니다.");
                mainStoryManager.StartMainStory(OnMainStoryComplete);
                
                break;
            case FlowState.RandomEvent:
                Debug.Log("▶ 랜덤 이벤트에 진입합니다.");
                randomEventManager.StartEventSequence(OnRandomEventComplete);
                playerState.CurrentChapterIndex++; // 챕터 증가
                break;
            case FlowState.Battle:
                Debug.Log("▶ 전투에 진입합니다.");
                battleManager.StartBattle(OnBattleComplete);
                break;
        }
    }

    private void OnMainStoryComplete()
    {
        prevState = FlowState.MainStory;
        EnterState(FlowState.RandomEvent);
    }

    private void OnRandomEventComplete(bool toBattle)
    {
        if (toBattle)
        {
            prevState = FlowState.RandomEvent;
            EnterState(FlowState.Battle);
        }
        else
        {
            EnterState(FlowState.MainStory);
        }
    }

    private void OnBattleComplete(bool playerWon)
    {
        EnterState(prevState);
    }

    private void HandleStoryBattleJoin(string monsterID)
    {
        Debug.Log("▶ 메인스토리 중 전투 진입 요청");
        mainStoryManager.StopMainStory();

        pendingMonsterID = monsterID;
        monsterSpawner.SpawnMonsterByID(pendingMonsterID);

        currentState = FlowState.Battle;

        battleManager.StartBattle(playerWon =>
        {
            mainStoryManager.WinBattle(playerWon);
            currentState = FlowState.None;
        });
    }

    private void HandleEventBattleJoin(string monsterID)
    {
        Debug.Log("▶ 랜덤 이벤트 중 전투 진입 요청");
        randomEventManager.StopRandomEvent();

        pendingMonsterID = monsterID;
        monsterSpawner.SpawnMonsterByID(pendingMonsterID);

        currentState = FlowState.Battle;

        battleManager.StartBattle(playerWon =>
        {
            randomEventManager.WinBattle(playerWon);
            currentState = FlowState.None;
        });
    }

    public bool CanEnterFlow()
    {
        return currentState == FlowState.None;
    }
    public void SetState(FlowState state)
    {
        currentState = state;
    }

    // 리모컨용 전투 실행
    public void ForceBattleWithMonster(string monsterID)
    {
        if (currentState != FlowState.None)
        {
            Debug.LogWarning($"[리모컨] 현재 상태({currentState})에서 전투 진입 차단됨.");
            //return;
        }

        Debug.Log($"[리모컨] 수동 전투 실행: {monsterID}");
        Debug.LogError("여기서 잠시 멈춥니다 정보 확인 하세요");

        prevState = FlowState.None;
        currentState = FlowState.Battle;

        monsterSpawner.SpawnMonsterByID(monsterID);

        battleManager.StartBattle(playerWon =>
        {
            Debug.Log($"[리모컨] 전투 종료 - 결과: {(playerWon ? "승리" : "패배")}");
            currentState = FlowState.None;
        });
    }

    // 리모컨용 상태 리셋 (테스트 용도)
    public void ForceResetState()
    {
        switch (currentState)
        {
            case FlowState.MainStory:
                mainStoryManager.StopMainStory();
                break;
            case FlowState.RandomEvent:
                randomEventManager.StopRandomEvent();
                break;
            case FlowState.Battle:
                battleManager.StopBattle();
                break;
        }

        currentState = FlowState.None;
        Debug.Log("▶ 상태 강제 초기화 완료");
    }

    public FlowState GetCurrentState()
    {
        return currentState; // 또는 현재 상태를 반환하는 로직
    }

    public void SaveFlow(ref SaveData data)
    {
        data.flowState = currentState.ToString();
    }

    public void LoadFlow(SaveData data)
    {
        if (string.IsNullOrEmpty(data.flowState))
        {
            Debug.LogWarning("[GameFlowManager] 저장된 흐름 상태가 없습니다. 기본값으로 설정됩니다.");
            currentState = FlowState.MainStory;
            //mainStoryManager.StartMainStory(); // 직접 실행
            return;
        }

        if (Enum.TryParse<FlowState>(data.flowState, out var parsedState))
        {
            currentState = parsedState;
            Debug.Log($"[GameFlowManager] 상태 복원 완료: {parsedState}");

            switch (currentState)
            {
                case FlowState.MainStory:
                    //진행중에 넘어가면 자기꺼만 초기화를 때려서 안되는 문제가 있었음
                    //그래서 각자 자기꺼 남아있는거 전부 삭제 때리고 넘어가게 했음
                    mainStoryManager.StopMainStory();
                    randomEventManager.StopRandomEvent();
                    mainStoryManager.LoadMainStory(data);
                    randomEventManager.LoadEventData(data);
                    mainStoryManager.SetOnCompleteCallback(OnMainStoryComplete);
                    mainStoryManager.DisplayCurrentStory(); // ✅ 요걸 직접 추가
                    Debug.Log("메인스토리 불러옴");
                    break;

                case FlowState.RandomEvent:
                    mainStoryManager.StopMainStory();
                    randomEventManager.StopRandomEvent();
                    randomEventManager.LoadEventData(data);
                    mainStoryManager.LoadMainStory(data);
                    randomEventManager.SetOnCompleteCallback(OnRandomEventComplete);
                    randomEventManager.DisplayCurrentEvent();   // ✅ 요걸 직접 추가
                    Debug.Log("이벤트 불러옴");
                    break;

                case FlowState.Battle:
                    // (선택) 전투 상태 복구가 필요하면 여기에 추가
                    break;
            }
        }
        else
        {
            Debug.LogWarning($"[GameFlowManager] 알 수 없는 흐름 상태: {data.flowState}. 기본값으로 설정됩니다.");
            currentState = FlowState.MainStory;
            mainStoryManager.StartMainStory(() => { });
        }
    }
    private void Update()
    {
        //// 디버그 테스트용 아이템 추가
        //if (Input.GetKeyDown(KeyCode.I))
        //{
        //    var potion = new ItemData
        //    {
        //        Item_ID = "Potion_Heal",
        //        Item_Type = "Consumable",
        //        Item_Name = "빨간 포션",
        //        Heal_Value = 25,
        //        Mental_Heal_Value = 0,
        //        Description = "체력을 회복하는 포션입니다.",
        //        Icon = "potion_red"
        //    };

        //    inventoryManager.AddItemToInventory(potion);
        //}
    }
}
