using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class TestChoiceEvaluator
{

    private static void ClearPlayerStateInstance()
    {
        var setter = typeof(PlayerState)
            .GetProperty("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
            .GetSetMethod(true);
        setter.Invoke(null, new object[] { null });
    }

    private static PlayerState CreateChoiceEvaluatorState(out GameObject go)
    {
        ClearPlayerStateInstance();
        go = new GameObject("ChoiceEvaluatorState");
        var state = go.AddComponent<PlayerState>();
        state.STR = 2;
        state.AGI = 3;
        state.INT = 4;
        state.CHA = 5;
        state.DIV = 6;
        state.MAG = 7;
        state.Health = 8;
        return state;
    }

    private static void DestroyChoiceEvaluatorState(GameObject go)
    {
        if (go != null)
            UnityEngine.Object.DestroyImmediate(go);
        ClearPlayerStateInstance();
    }

    [Test]
    public void test_ChoiceEvaluator_EvaluateFormula_manual_oracles()
    {
        GameObject go = null;
        try
        {
            var state = CreateChoiceEvaluatorState(out go);

            Assert.That(ChoiceEvaluator.EvaluateFormula("STR*10", state), Is.EqualTo(0.2f).Within(0.0001f));
            Assert.That(ChoiceEvaluator.EvaluateFormula("DEX*10", state), Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(ChoiceEvaluator.EvaluateFormula("INT*10", state), Is.EqualTo(0.4f).Within(0.0001f));
            Assert.That(ChoiceEvaluator.EvaluateFormula("CHA*10", state), Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(ChoiceEvaluator.EvaluateFormula("DIV*10", state), Is.EqualTo(0.6f).Within(0.0001f));
            Assert.That(ChoiceEvaluator.EvaluateFormula("MAG*10", state), Is.EqualTo(0.7f).Within(0.0001f));
            Assert.That(ChoiceEvaluator.EvaluateFormula("Health*10", state), Is.EqualTo(0.8f).Within(0.0001f));
            Assert.That(ChoiceEvaluator.EvaluateFormula("", state), Is.EqualTo(0f).Within(0.0001f));
            Assert.That(ChoiceEvaluator.EvaluateFormula("UNKNOWN*10", state), Is.EqualTo(0f).Within(0.0001f));
        }
        finally
        {
            DestroyChoiceEvaluatorState(go);
        }
    }

    [Test]
    public void test_ChoiceEvaluator_Resolve_rate_zero_manual_oracle()
    {
        GameObject go = null;
        try
        {
            var state = CreateChoiceEvaluatorState(out go);

            var result = ChoiceEvaluator.Resolve("STR*0", "nextOnSuccess", "nextOnFail", state);

            Assert.That(result.SuccessRate, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.NextCode, Is.EqualTo("nextOnFail"));
        }
        finally
        {
            DestroyChoiceEvaluatorState(go);
        }
    }

    [Test]
    public void test_ChoiceEvaluator_Resolve_rate_one_manual_oracle()
    {
        GameObject go = null;
        try
        {
            var state = CreateChoiceEvaluatorState(out go);

            var result = ChoiceEvaluator.Resolve("STR*50", "nextOnSuccess", "nextOnFail", state);

            Assert.That(result.SuccessRate, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.NextCode, Is.EqualTo("nextOnSuccess"));
        }
        finally
        {
            DestroyChoiceEvaluatorState(go);
        }
    }

    // Omitted generated case test_ChoiceEvaluator_EvaluateFormula_happy_path for ChoiceEvaluator.EvaluateFormula: requires manual Unity fixture/assertion design.

    // Omitted generated case test_ChoiceEvaluator_EvaluateFormula_happy_path_1 for ChoiceEvaluator.EvaluateFormula: requires manual Unity fixture/assertion design.

    // Omitted generated case test_ChoiceEvaluator_EvaluateFormula_happy_path_2 for ChoiceEvaluator.EvaluateFormula: requires manual Unity fixture/assertion design.

    // Omitted generated case test_ChoiceEvaluator_EvaluateFormula_edge_case_0 for ChoiceEvaluator.EvaluateFormula: requires manual Unity fixture/assertion design.

    // Omitted generated case test_ChoiceEvaluator_EvaluateFormula_edge_case_1 for ChoiceEvaluator.EvaluateFormula: requires manual Unity fixture/assertion design.

    // Omitted generated case test_ChoiceEvaluator_EvaluateFormula_exception_0 for ChoiceEvaluator.EvaluateFormula: generic exception oracle requires manual C# assertion design.

    // Omitted generated case test_ChoiceEvaluator_EvaluateFormula_exception_1 for ChoiceEvaluator.EvaluateFormula: generic exception oracle requires manual C# assertion design.

    [Test]
    public void test_ChoiceEvaluator_EvaluateSuccess_happy_path()
    {
        // 정상 경로 테스트: EvaluateSuccess()
        var result = ChoiceEvaluator.EvaluateSuccess(3.14f);
        Assert.That(result, Is.EqualTo(true));
    }

    [Test]
    public void test_ChoiceEvaluator_EvaluateSuccess_happy_path_1()
    {
        // 정상 경로 테스트: EvaluateSuccess()
        var result = ChoiceEvaluator.EvaluateSuccess(3.14f);
        Assert.That(result, Is.EqualTo(true));
    }

    [Test]
    public void test_ChoiceEvaluator_EvaluateSuccess_edge_case_0()
    {
        // 경계값 테스트: EvaluateSuccess() - rate01 파라미터
        var result = ChoiceEvaluator.EvaluateSuccess(0f);
        Assert.That(result, Is.EqualTo(false));
    }

    // Omitted generated case test_ChoiceEvaluator_EvaluateSuccess_exception_0 for ChoiceEvaluator.EvaluateSuccess: generic exception oracle requires manual C# assertion design.

    // Omitted generated case test_ChoiceEvaluator_Resolve_happy_path for ChoiceEvaluator.Resolve: requires manual Unity fixture/assertion design.

    // Omitted generated case test_ChoiceEvaluator_Resolve_edge_case_0 for ChoiceEvaluator.Resolve: requires manual Unity fixture/assertion design.

    // Omitted generated case test_ChoiceEvaluator_Resolve_edge_case_1 for ChoiceEvaluator.Resolve: requires manual Unity fixture/assertion design.

    // Omitted generated case test_ChoiceEvaluator_Resolve_edge_case_2 for ChoiceEvaluator.Resolve: requires manual Unity fixture/assertion design.

    // Omitted generated case test_ChoiceEvaluator_Resolve_edge_case_3 for ChoiceEvaluator.Resolve: requires manual Unity fixture/assertion design.

    // Omitted generated case test_ChoiceEvaluator_Resolve_exception_0 for ChoiceEvaluator.Resolve: generic exception oracle requires manual C# assertion design.

    // Omitted generated case test_ChoiceEvaluator_Resolve_exception_1 for ChoiceEvaluator.Resolve: generic exception oracle requires manual C# assertion design.

}

