public static class ChoiceBranchResolver
{
    public static ChoiceResult Resolve(Main_SuccessRate_Master_Main rateRow, PlayerState state)
    {
        if (rateRow == null)
        {
            return null;
        }

        return ChoiceEvaluator.Resolve(
            formula: rateRow.Success_Formula,
            nextOnSuccess: rateRow.Success_Next_Script,
            nextOnFail: rateRow.Fail_Next_Script,
            state: state);
    }

    public static ChoiceResult Resolve(Ran_SuccessRate_Master_Events rateRow, PlayerState state)
    {
        if (rateRow == null)
        {
            return null;
        }

        return ChoiceEvaluator.Resolve(
            formula: rateRow.Success_Formula,
            nextOnSuccess: rateRow.Success_Next_Script,
            nextOnFail: rateRow.Fail_Next_Script,
            state: state);
    }

    public static string ResolveNextCode(Main_SuccessRate_Master_Main rateRow, ChoiceResult choiceResult, string fallbackCode)
    {
        if (rateRow == null || choiceResult == null)
        {
            return fallbackCode;
        }

        bool success = ChoiceEvaluator.EvaluateSuccess(choiceResult.SuccessRate);
        string nextCode = success
            ? rateRow.Success_Next_Script?.Trim()
            : rateRow.Fail_Next_Script?.Trim();

        return string.IsNullOrEmpty(nextCode) ? fallbackCode : nextCode;
    }

    public static string ResolveNextCode(Ran_SuccessRate_Master_Events rateRow, ChoiceResult choiceResult, string fallbackCode)
    {
        if (rateRow == null || choiceResult == null)
        {
            return fallbackCode;
        }

        bool success = ChoiceEvaluator.EvaluateSuccess(choiceResult.SuccessRate);
        string nextCode = success
            ? rateRow.Success_Next_Script?.Trim()
            : rateRow.Fail_Next_Script?.Trim();

        return string.IsNullOrEmpty(nextCode) ? fallbackCode : nextCode;
    }
}
