using System.Collections.Generic;

public static class ChoiceRequirementResolver
{
    public static List<ChoiceRequirement> Resolve(
        JsonManager jsonManager,
        string sceneCode,
        int choiceNo,
        Main_SuccessRate_Master_Main rateRow)
    {
        if (HasInlineRequirements(rateRow?.ChoiceRequirement))
        {
            return rateRow.ChoiceRequirement;
        }

        return ResolveFromManager(jsonManager, sceneCode, choiceNo);
    }

    public static List<ChoiceRequirement> Resolve(
        JsonManager jsonManager,
        string sceneCode,
        int choiceNo,
        Ran_SuccessRate_Master_Events rateRow)
    {
        if (HasInlineRequirements(rateRow?.ChoiceRequirement))
        {
            return rateRow.ChoiceRequirement;
        }

        return ResolveFromManager(jsonManager, sceneCode, choiceNo);
    }

    private static bool HasInlineRequirements(List<ChoiceRequirement> requirements)
    {
        return requirements != null && requirements.Count > 0;
    }

    private static List<ChoiceRequirement> ResolveFromManager(JsonManager jsonManager, string sceneCode, int choiceNo)
    {
        return jsonManager != null
            ? jsonManager.GetChoiceRequirementsByScene(sceneCode, choiceNo)
            : null;
    }
}
