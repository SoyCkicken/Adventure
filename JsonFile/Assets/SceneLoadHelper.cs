///*
// * SceneLoadHelper.cs
// * - 게임 전역 아무 곳에서나 간단히 호출하기 위한 헬퍼
// * - 세이브/로드, 메인→전투 진입 등에 공용으로 쓴다.
// */

//using UnityEngine;
//using UnityEngine.SceneManagement;
//using System.Collections;

//public static class SceneLoadHelper
//{
//    /// <summary>
//    /// 씬을 페이드와 함께 로드한다.
//    /// </summary>
//    public static void Go(string sceneName, float fadeOut = 0.3f, float fadeIn = 0.25f, System.Action after = null)
//    {
//        if (SceneFader.Instance == null)
//        {
//            Debug.LogWarning("[SceneLoadHelper] SceneFader 인스턴스가 없음. 즉시 로드로 대체.");
//            SceneManager.LoadScene(sceneName);
//            after?.Invoke();
//            return;
//        }

//        SceneFader.Instance.LoadSceneWithFade(
//            sceneName,
//            fadeOut,
//            fadeIn,
//            onBeforeUnload: null,
//            onAfterLoad: after
//        );
//    }

//    /// <summary>
//    /// 코루틴 버전 (콜백 대신 yield 기반이 필요할 때)
//    /// </summary>
//    public static IEnumerator GoRoutine(string sceneName, float fadeOut = 0.3f, float fadeIn = 0.25f, System.Action after = null)
//    {
//        if (SceneFader.Instance == null)
//        {
//            Debug.LogWarning("[SceneLoadHelper] SceneFader 인스턴스가 없음. 즉시 로드로 대체.");
//            SceneManager.LoadScene(sceneName);
//            after?.Invoke();
//            yield break;
//        }

//        yield return SceneFader.Instance.LoadSceneWithFadeRoutine(sceneName, fadeOut, fadeIn, null, after);
//    }
//}
