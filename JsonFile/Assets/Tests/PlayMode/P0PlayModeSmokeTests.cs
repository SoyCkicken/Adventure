using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

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
