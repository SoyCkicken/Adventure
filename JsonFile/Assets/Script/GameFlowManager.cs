using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public enum FlowState { MainStory, RandomEvent, Battle }
    FlowState currentState;
    FlowState prevState;

    public StoryDisplayManager mainStoryManager;
    public EventDisplay randomEventManager;
    public BattleManager battleManager;

    void Start() => EnterState(FlowState.RandomEvent);

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
