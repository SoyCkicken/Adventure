using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class P0PlayModeSmokeTests
{
    private static readonly string[] RequiredScenes =
    {
        "LobbyScenes",
        "GameScene",
        "GameEndingScene"
    };

    [Test]
    public void PlayModeRunnerStartsAndRequiredScenesResolve()
    {
        Assert.That(Application.isPlaying, Is.True);

        foreach (string sceneName in RequiredScenes)
        {
            string scenePath = $"Assets/Scenes/{sceneName}.unity";
            Assert.That(SceneUtility.GetBuildIndexByScenePath(scenePath), Is.GreaterThanOrEqualTo(0), scenePath);
        }
    }

    [UnityTest]
    public IEnumerator RequiredScenesLoadInPlayMode()
    {
        foreach (string sceneName in RequiredScenes)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null, $"{sceneName} load operation should start.");

            float timeoutAt = Time.realtimeSinceStartup + 10f;
            while (!load.isDone)
            {
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(timeoutAt), $"{sceneName} load timed out.");
                yield return null;
            }

            yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(sceneName));
        }
    }

    [UnityTest]
    public IEnumerator LobbyStartButtonLoadsGameScene()
    {
        AsyncOperation load = SceneManager.LoadSceneAsync("LobbyScenes", LoadSceneMode.Single);
        Assert.That(load, Is.Not.Null, "LobbyScenes load operation should start.");

        float timeoutAt = Time.realtimeSinceStartup + 10f;
        while (!load.isDone)
        {
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(timeoutAt), "LobbyScenes load timed out.");
            yield return null;
        }

        yield return null;

        Type saveManagerType = FindRuntimeType("SaveManager");
        Assert.That(saveManagerType, Is.Not.Null, "SaveManager should be available in PlayMode.");

        UnityEngine.Object saveManager = UnityEngine.Object.FindObjectOfType(saveManagerType, true);
        Assert.That(saveManager, Is.Not.Null, "LobbyScenes should contain or retain a SaveManager.");

        Button startButton = saveManagerType.GetField("_startButton")?.GetValue(saveManager) as Button;
        Assert.That(startButton, Is.Not.Null, "SaveManager should bind the lobby start button during initial scene startup.");

        startButton.onClick.Invoke();

        timeoutAt = Time.realtimeSinceStartup + 10f;
        while (SceneManager.GetActiveScene().name != "GameScene")
        {
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(timeoutAt), "Start button did not load GameScene.");
            yield return null;
        }

        Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("GameScene"));
    }

    [Test]
    public void CoreRuntimeTypesAreAvailableToPlayMode()
    {
        AssertRuntimeTypeExists("PlayerState");
        AssertRuntimeTypeExists("EquipmentSystem");
        AssertRuntimeTypeExists("InventoryManager");
        AssertRuntimeTypeExists("CombatTest");
    }

    private static void AssertRuntimeTypeExists(string typeName)
    {
        Type type = FindRuntimeType(typeName);
        Assert.That(type, Is.Not.Null, $"{typeName} should be available in PlayMode.");
    }

    private static Type FindRuntimeType(string typeName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(typeName))
            .FirstOrDefault(foundType => foundType != null);
    }
}
