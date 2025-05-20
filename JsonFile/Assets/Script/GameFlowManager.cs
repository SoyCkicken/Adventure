using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public enum FlowState { MainStory, RandomEvent, Battle }
    FlowState currentState;
    FlowState prevState;

    public StoryDisplayManager mainStoryManager;
    public EventDisplay randomEventManager;
    public BattleManager battleManager;
    public MonsterSpawner monsterSpawner;
    private string pendingMonsterID;

    void Start()
    {
        
        mainStoryManager.OnBattleJoin += HandleStoryBattleJoin;
        mainStoryManager.StartMainStory(OnStoryComplete);

        EnterState(FlowState.MainStory);
    }

    void EnterState(FlowState next)
    {
        // 1) 이전 상태 정리
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

        // 2) 새 상태 진입
        currentState = next;
        switch (currentState)
        {
            case FlowState.MainStory:
                mainStoryManager.StartMainStory(OnMainStoryComplete);
                break;
            case FlowState.RandomEvent:
                randomEventManager.StartRandomEvent(OnRandomEventComplete);
                break;
            case FlowState.Battle:
                battleManager.StartBattle(OnBattleComplete);
                break;
        }
    }

    private void HandleStoryBattleJoin(string monsterID)
    {
        Debug.Log("전투에 진입했습니다!");
        // 1) 곧바로 스토리 연출 중지
        mainStoryManager.StopMainStory();
        Debug.Log(monsterID);
        // 2) 전투에 넘길 몬스터 ID 저장
        pendingMonsterID = monsterID;
        monsterSpawner.SpawnMonsterByID(pendingMonsterID);

        // 3) 전투 시작
        battleManager.StartBattle(playerWon =>
        {
            // 전투가 끝나면 다시 스토리로 돌아와서 다음 스크립트 진행
            mainStoryManager.NextScene();
        });
    }

    private void OnStoryComplete()
    {
        // 스토리 흐름 끝났을 때 (필요 시 다른 흐름 진입)
    }

    void OnMainStoryComplete()
    {
        prevState = FlowState.MainStory;
        EnterState(FlowState.Battle);
    }

    void OnRandomEventComplete(bool toBattle)
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

    void OnBattleComplete(bool playerWon)
    {
        // 전투 끝나면 prevState로 복귀
        EnterState(prevState);
    }
}
