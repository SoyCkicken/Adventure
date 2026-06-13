using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

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
        Type type = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(typeName))
            .FirstOrDefault(foundType => foundType != null);

        Assert.That(type, Is.Not.Null, $"{typeName} should be available in PlayMode.");
    }
}
