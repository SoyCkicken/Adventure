using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class TestSaveManager
{

    private static string CreateIsolatedSavePath()
    {
        var dir = System.IO.Path.Combine(
            Application.temporaryCachePath,
            "CodexSaveManagerTests",
            Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(dir);
        return System.IO.Path.Combine(dir, "save.json");
    }

    private static void ClearSaveManagerInstance()
    {
        var setter = typeof(SaveManager)
            .GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
            .GetSetMethod(true);
        setter.Invoke(null, new object[] { null });
        SaveManager.pendingLoadData = null;
    }

    private static SaveManager CreateSaveManagerFixture(string savePath, out GameObject go)
    {
        SaveManager.SetSavePathForTesting(savePath);
        ClearSaveManagerInstance();
        go = new GameObject("SaveManagerTest");
        var sut = go.AddComponent<SaveManager>();
        if (System.IO.File.Exists(savePath))
            System.IO.File.Delete(savePath);
        return sut;
    }

    private static void CleanupSaveManagerFixture(GameObject go, string savePath)
    {
        if (go != null)
            UnityEngine.Object.DestroyImmediate(go);
        if (!string.IsNullOrEmpty(savePath))
        {
            if (System.IO.File.Exists(savePath))
                System.IO.File.Delete(savePath);
            var dir = System.IO.Path.GetDirectoryName(savePath);
            if (!string.IsNullOrEmpty(dir) && System.IO.Directory.Exists(dir))
                System.IO.Directory.Delete(dir, true);
        }
        SaveManager.ClearSavePathForTesting();
        ClearSaveManagerInstance();
    }

    [Test]
    public void test_SaveManager_CreateDefaultSave_manual_oracle()
    {
        var savePath = CreateIsolatedSavePath();
        GameObject go = null;
        try
        {
            var sut = CreateSaveManagerFixture(savePath, out go);

            var data = sut.CreateDefaultSave();

            Assert.That(data, Is.Not.Null);
            Assert.That(data.lastSeenVersion, Is.EqualTo(""));
            Assert.That(data.showPatchNoteToggle, Is.True);
        }
        finally
        {
            CleanupSaveManagerFixture(go, savePath);
        }
    }

    [Test]
    public void test_SaveManager_WriteReadRoundTrip_manual_oracle()
    {
        var savePath = CreateIsolatedSavePath();
        GameObject go = null;
        try
        {
            var sut = CreateSaveManagerFixture(savePath, out go);
            var data = sut.CreateDefaultSave();
            data.playerName = "Codex";
            data.STR = 11;
            data.AGI = 12;
            data.INT = 13;
            data.Experience = 99;
            data.Level = 3;
            data.lastSeenVersion = "1.2.3";
            data.showPatchNoteToggle = false;

            sut.WriteSaveFile(data);
            var loaded = sut.ReadSaveFile();

            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded.playerName, Is.EqualTo("Codex"));
            Assert.That(loaded.STR, Is.EqualTo(11));
            Assert.That(loaded.AGI, Is.EqualTo(12));
            Assert.That(loaded.INT, Is.EqualTo(13));
            Assert.That(loaded.Experience, Is.EqualTo(99));
            Assert.That(loaded.Level, Is.EqualTo(3));
            Assert.That(loaded.lastSeenVersion, Is.EqualTo("1.2.3"));
            Assert.That(loaded.showPatchNoteToggle, Is.False);
        }
        finally
        {
            CleanupSaveManagerFixture(go, savePath);
        }
    }

    [Test]
    public void test_SaveManager_HasSave_manual_oracle()
    {
        var savePath = CreateIsolatedSavePath();
        GameObject go = null;
        try
        {
            var sut = CreateSaveManagerFixture(savePath, out go);

            Assert.That(SaveManager.HasSave(), Is.False);
            sut.WriteSaveFile(sut.CreateDefaultSave());

            Assert.That(SaveManager.HasSave(), Is.True);
        }
        finally
        {
            CleanupSaveManagerFixture(go, savePath);
        }
    }

    [Test]
    public void test_SaveManager_GetOrCreateSave_manual_oracle()
    {
        var savePath = CreateIsolatedSavePath();
        GameObject go = null;
        try
        {
            var sut = CreateSaveManagerFixture(savePath, out go);

            Assert.That(SaveManager.HasSave(), Is.False);
            var data = sut.GetOrCreateSave();

            Assert.That(data, Is.Not.Null);
            Assert.That(data.lastSeenVersion, Is.EqualTo(""));
            Assert.That(data.showPatchNoteToggle, Is.True);
            Assert.That(SaveManager.HasSave(), Is.True);
            Assert.That(sut.ReadSaveFile(), Is.Not.Null);
        }
        finally
        {
            CleanupSaveManagerFixture(go, savePath);
        }
    }

    // Omitted generated case test_SaveManager_HasSave_happy_path for SaveManager.HasSave: requires manual Unity fixture/assertion design.

    // Omitted generated case test_SaveManager_ReadSaveFile_happy_path for SaveManager.ReadSaveFile: requires manual Unity fixture/assertion design.

    // Omitted generated case test_SaveManager_WriteLoadFile_happy_path for SaveManager.WriteLoadFile: requires manual Unity fixture/assertion design.

    // Omitted generated case test_SaveManager_CreateDefaultSave_happy_path for SaveManager.CreateDefaultSave: requires manual Unity fixture/assertion design.

    // Omitted generated case test_SaveManager_GetOrCreateSave_happy_path for SaveManager.GetOrCreateSave: requires manual Unity fixture/assertion design.

}

