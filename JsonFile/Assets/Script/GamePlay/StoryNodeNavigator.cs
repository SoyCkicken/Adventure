using System;
using System.Collections.Generic;
using System.Linq;

public static class StoryNodeNavigator
{
    private static readonly HashSet<string> ExecutableDisplayTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "MERCHANT",
        "BATTLE",
        "IMAGE",
        "CLAER",
        "CLEAR"
    };

    public static string NormalizeToSceneCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        string trimmed = code.Trim();
        return trimmed.StartsWith("MainScript", StringComparison.Ordinal)
            ? trimmed.Replace("MainScript", "MainScene")
            : trimmed;
    }

    public static Story_Master_Main FindBySceneCode(IEnumerable<Story_Master_Main> storyList, string sceneCode)
    {
        string normalized = NormalizeToSceneCode(sceneCode);
        if (storyList == null || string.IsNullOrEmpty(normalized))
        {
            return null;
        }

        return storyList.FirstOrDefault(s => string.Equals(s.Scene_Code?.Trim(), normalized, StringComparison.Ordinal));
    }

    public static Story_Master_Main FindNextInSameEvent(IEnumerable<Story_Master_Main> storyList, Story_Master_Main current)
    {
        if (storyList == null || current == null)
        {
            return null;
        }

        return storyList.FirstOrDefault(s =>
            s.Chapter_Index == current.Chapter_Index &&
            s.Event_Index == current.Event_Index &&
            s.Script_Index == current.Script_Index + 1);
    }

    public static Story_Master_Main ResolveChoiceTarget(
        IEnumerable<Story_Master_Main> storyList,
        IEnumerable<Main_Script_Master_Main> scriptEvents,
        string newSceneCode,
        string labelScriptCode,
        out bool shouldAdvanceFromCurrent)
    {
        shouldAdvanceFromCurrent = false;

        Story_Master_Main target = FindBySceneCode(storyList, newSceneCode);
        if (target == null)
        {
            return null;
        }

        Main_Script_Master_Main targetScript = FindScript(scriptEvents, target.Script_Text);
        if (!CanSkipLabelNode(target, targetScript, labelScriptCode))
        {
            return target;
        }

        Story_Master_Main nextInSameEvent = FindNextInSameEvent(storyList, target);
        if (nextInSameEvent != null)
        {
            return nextInSameEvent;
        }

        Story_Master_Main jump = FindBySceneCode(storyList, target.Next_Scene);
        if (jump != null)
        {
            return jump;
        }

        shouldAdvanceFromCurrent = true;
        return null;
    }

    private static Main_Script_Master_Main FindScript(IEnumerable<Main_Script_Master_Main> scriptEvents, string scriptCode)
    {
        if (scriptEvents == null || string.IsNullOrWhiteSpace(scriptCode))
        {
            return null;
        }

        string normalized = scriptCode.Trim();
        return scriptEvents.FirstOrDefault(s => string.Equals(s.Script_Code?.Trim(), normalized, StringComparison.Ordinal));
    }

    private static bool CanSkipLabelNode(Story_Master_Main target, Main_Script_Master_Main targetScript, string labelScriptCode)
    {
        if (target == null ||
            targetScript == null ||
            string.IsNullOrWhiteSpace(labelScriptCode) ||
            string.IsNullOrWhiteSpace(target.Script_Text))
        {
            return false;
        }

        bool isTextNode = string.Equals(targetScript.displayType?.Trim(), "TEXT", StringComparison.OrdinalIgnoreCase);
        bool isExecutableNode = ExecutableDisplayTypes.Contains(targetScript.displayType?.Trim() ?? string.Empty);
        bool hasReward = target.Main_Effect != null && target.Main_Effect.Count > 0;
        bool labelMatches = string.Equals(target.Script_Text.Trim(), labelScriptCode.Trim(), StringComparison.Ordinal);

        return isTextNode && !isExecutableNode && !hasReward && labelMatches;
    }
}
