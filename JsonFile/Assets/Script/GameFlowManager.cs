using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    // 1) 현재 흐름 상태를 정의
    public enum FlowState
    {
        MainStory,
        RandomEvent,
        Battle
    }

    public FlowState currentState { get; private set; }

    // 2) 하위 매니저 참조
    [Header("Managers")]
    public StoryDisplayManager mainStoryManager;
    //public EventDisplay randomEventManager;
    public CombatTest battleManager;

    void Start()
    {
        // 초기 진입: 메인 스토리
        EnterState(FlowState.MainStory);
    }

    // 3) 상태 전환 메서드
    public void EnterState(FlowState nextState)
    {
        ExitState(currentState);
        currentState = nextState;

        switch (currentState)
        {
            case FlowState.MainStory:
                mainStoryManager.StartMainStory(OnMainStoryComplete);
                break;
            case FlowState.RandomEvent:
                //randomEventManager.StartRandomEvent(OnRandomEventComplete);
                break;
            case FlowState.Battle:
                //battleManager.StartBattle(OnBattleComplete);
                break;
        }
    }

    void ExitState(FlowState state)
    {
        // 필요 시, 이전 매니저 정리 호출
        switch (state)
        {
            case FlowState.MainStory:
                mainStoryManager.StopMainStory();
                break;
            case FlowState.RandomEvent:
                //randomEventManager.StopRandomEvent();
                break;
            case FlowState.Battle:
                //battleManager.StopBattle();
                break;
        }
    }

    // 4) 콜백에서 다음 흐름 결정
    void OnMainStoryComplete()
    {
        // 메인 스토리 → 전투
        EnterState(FlowState.Battle);
    }

    void OnRandomEventComplete(bool goToBattle)
    {
        if (goToBattle)
            EnterState(FlowState.Battle);
        else
            EnterState(FlowState.MainStory);
    }

    //void OnBattleComplete(bool playerWon)
    //{
    //    // 전투 결과에 따라 돌아갈 곳 결정
    //    if (mainStoryManager.IsCurrentlyPlaying)
    //        EnterState(FlowState.MainStory);
    //    else
    //        EnterState(FlowState.RandomEvent);
    //}
}
