using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class TestOptionManager
{

    [Test]
    public void test_OptionManager_GetOptionDescription_happy_path()
    {
        // Explicit oracle: "sample"은 optionDescriptions 딕셔너리(Option_001~007, "null")에
        // 없는 키 → TryGetValue 실패 → else 분기 "옵션({optionID})" 포맷 문자열 반환.
        var result = OptionManager.GetOptionDescription("sample");
        Assert.That(result, Is.EqualTo("옵션(sample)"));
    }

    [Test]
    public void test_OptionManager_GetOptionDescription_edge_case_0()
    {
        // Explicit oracle: IsNullOrEmpty("") = true → 가드 분기에서 즉시 null 반환.
        var result = OptionManager.GetOptionDescription("");
        Assert.That(result, Is.Null);
    }

    // Omitted generated case test_OptionManager_GetOptionDescription_exception_0 for OptionManager.GetOptionDescription: generic exception oracle requires manual C# assertion design.

    // Omitted generated case test_OptionManager_GetOption_happy_path for OptionManager.GetOption: requires manual Unity fixture/assertion design.

    // Omitted generated case test_OptionManager_GetOption_edge_case_0 for OptionManager.GetOption: requires manual Unity fixture/assertion design.

    // Omitted generated case test_OptionManager_GetOption_exception_0 for OptionManager.GetOption: generic exception oracle requires manual C# assertion design.

}

