//using UnityEngine;

//public class GameFlowManager : MonoBehaviour
//{
//    public enum FlowState { MainStory, RandomEvent, Battle }
//    FlowState currentState;
//    FlowState prevState;

//    public StoryDisplayManager mainStoryManager;
//    public EventDisplay randomEventManager;
//    public BattleManager battleManager;
//    public MonsterSpawner monsterSpawner;
//    private string pendingMonsterID;
//    public InventoryManager inventoryManager;
//    void Start()
//    {

//        mainStoryManager.OnBattleJoin += HandleStoryBattleJoin;
//        //mainStoryManager.StartMainStory(OnStoryComplete);

//        randomEventManager.OnBattleJoin += HandleEventBattleJoin;
//        //randomEventManager.StartRandomEvent(OnRandomEventComplete);

//        EnterState(FlowState.MainStory);
//        //EnterState(FlowState.RandomEvent);
//    }

//    void EnterState(FlowState next)
//    {
//        // 1) 이전 상태 정리
//        switch (currentState)
//        {
//            case FlowState.MainStory:
//                mainStoryManager.StopMainStory();
//                break;
//            case FlowState.RandomEvent:
//                randomEventManager.StopRandomEvent();
//                break;
//            case FlowState.Battle:
//                battleManager.StopBattle();
//                break;
//        }

//        // 2) 새 상태 진입
//        currentState = next;
//        switch (currentState)
//        {
//            case FlowState.MainStory:
//                Debug.Log("메인스토리에 진입했습니다");
//                mainStoryManager.StartMainStory(OnMainStoryComplete);
//                break;
//            case FlowState.RandomEvent:
//                Debug.Log("이벤트스토리에 진입했습니다");
//                randomEventManager.StartEventSequence(OnRandomEventComplete);
//                break;
//            case FlowState.Battle:
//                Debug.Log("전투스토리에 진입했습니다");
//                battleManager.StartBattle(OnBattleComplete);
//                break;
//        }
//    }

//    private void HandleStoryBattleJoin(string monsterID)
//    {
//        Debug.Log("메인 스토리에서 전투에 진입했습니다!");
//        // 1) 곧바로 스토리 연출 중지
//        mainStoryManager.StopMainStory();
//        Debug.Log(monsterID);
//        // 2) 전투에 넘길 몬스터 ID 저장
//        pendingMonsterID = monsterID;
//        monsterSpawner.SpawnMonsterByID(pendingMonsterID);

//        // 3) 전투 시작
//        battleManager.StartBattle(playerWon =>
//        {
//            // 전투가 끝나면 다시 스토리로 돌아와서 다음 스크립트 진행
//                //승리해서 스토리 이어서 출력
//                //mainStoryManager.NextScene();
//                mainStoryManager.WinBattle(playerWon);
//        });
//    }

//    private void HandleEventBattleJoin(string monsterID)
//    {
//        Debug.Log("이벤트에서 전투에 진입했습니다!");
//        // 1) 곧바로 스토리 연출 중지
//        randomEventManager.StopRandomEvent();
//        Debug.Log(monsterID);
//        // 2) 전투에 넘길 몬스터 ID 저장
//        pendingMonsterID = monsterID;
//        monsterSpawner.SpawnMonsterByID(pendingMonsterID);

//        // 3) 전투 시작
//        battleManager.StartBattle(playerWon =>
//        {
//            // 전투가 끝나면 다시 스토리로 돌아와서 다음 스크립트 진행
//            //randomEventManager.AdvanceEvent();
//            Debug.Log($"전투 결과 : {playerWon}");
//            randomEventManager.WinBattle(playerWon);
//        });
//    }

//    private void OnStoryComplete()
//    {
//        // 스토리 흐름 끝났을 때 (필요 시 다른 흐름 진입)
//    }

//    void OnMainStoryComplete()
//    {

//        prevState = FlowState.MainStory;
//        EnterState(FlowState.RandomEvent);
//    }

//    void OnRandomEventComplete(bool toBattle)
//    {
//        if (toBattle)
//        {
//            prevState = FlowState.RandomEvent;
//            EnterState(FlowState.Battle);
//        }
//        else
//        {
//            EnterState(FlowState.MainStory);
//        }
//    }

//    void OnBattleComplete(bool playerWon)
//    {
//        // 전투 끝나면 prevState로 복귀
//        EnterState(prevState);
//    }

//    public void ForceBattleWithMonster(string monsterID)
//    {
//        Debug.Log($"[리모컨] 수동 전투 실행: {monsterID}");

//        // 전투 외 상태라면 스토리 종료나 UI 닫기 처리 필요
//        mainStoryManager?.StopMainStory();

//        // 몬스터 스폰
//        monsterSpawner.SpawnMonsterByID(monsterID);

//        // 전투 시작
//        battleManager.StartBattle(playerWon =>
//        {
//            Debug.Log($"[리모컨] 전투 종료 - 결과: {(playerWon ? "승리" : "패배")}");

//            // 테스트 환경에서는 스토리 연결 없이 결과만 확인
//            // 원하면 여기서 테스트용 결과 UI 띄우거나 리셋해도 됨
//        });
//    }

//    private void Update()
//    {
//        //이건 이제 안쓰는 거임
//        if (Input.GetKeyDown(KeyCode.I))
//        {
//            var potion = new ItemData
//            {
//                Item_ID = "Potion_Heal",
//                Item_Type = "Consumable",
//                Item_Name = "빨간 포션",
//                Heal_Value = 25,
//                Mental_Heal_Value = 0,
//                Description = "체력을 회복하는 포션입니다.",
//                Icon = "potion_red"
//            };

//            inventoryManager.AddItemToInventory(potion);
//        }
//    }
//}
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public enum FlowState { None, MainStory, RandomEvent, Battle }
    FlowState currentState = FlowState.None;
    FlowState prevState = FlowState.None;

    public StoryDisplayManager mainStoryManager;
    public EventDisplay randomEventManager;
    public BattleManager battleManager;
    public MonsterSpawner monsterSpawner;
    public InventoryManager inventoryManager;

    private string pendingMonsterID;

    void Start()
    {
        mainStoryManager.OnBattleJoin += HandleStoryBattleJoin;
        randomEventManager.OnBattleJoin += HandleEventBattleJoin;

        EnterState(FlowState.MainStory); // 최초 진입
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

    private void Update()
    {
        // 디버그 테스트용 아이템 추가
        if (Input.GetKeyDown(KeyCode.I))
        {
            var potion = new ItemData
            {
                Item_ID = "Potion_Heal",
                Item_Type = "Consumable",
                Item_Name = "빨간 포션",
                Heal_Value = 25,
                Mental_Heal_Value = 0,
                Description = "체력을 회복하는 포션입니다.",
                Icon = "potion_red"
            };

            inventoryManager.AddItemToInventory(potion);
        }
    }
}
