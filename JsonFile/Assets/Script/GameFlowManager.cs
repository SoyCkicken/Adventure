using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }
    public enum FlowState { None, MainStory, RandomEvent, Battle }
    FlowState currentState = FlowState.None;
    FlowState prevState = FlowState.None;

    public StoryDisplayManager mainStoryManager;
    public EventDisplay randomEventManager;
    public BattleManager battleManager;
    public MonsterSpawner monsterSpawner;
    public InventoryManager inventoryManager;

    private string pendingMonsterID;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

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
