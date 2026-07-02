using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class TestEquipmentSystem
{
    private EquipmentSystem _sut;
    private GameObject _go;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _sut = _go.AddComponent<EquipmentSystem>();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(_go);
    }

    [Test]
    public void test_EquipmentSystem_MeetsEquipRequirement_happy_path()
    {
        // Explicit oracle: "sample"은 슬롯명("Weapon"/"Armor")도, "슬롯:코드" 형식도 아니므로
        // 모든 분기를 통과해 최종 fallback(return false)에 도달한다.
        var result = _sut.MeetsEquipRequirement("sample");
        Assert.That(result, Is.EqualTo(false));
    }

    [Test]
    public void test_EquipmentSystem_MeetsEquipRequirement_happy_path_1()
    {
        // Explicit oracle: "Weapon" 슬롯 검사 분기. player를 설정하지 않아 null이므로
        // player?.weapon_Name → null → IsNullOrEmpty(null)=true → !true = false.
        var result = _sut.MeetsEquipRequirement("Weapon");
        Assert.That(result, Is.EqualTo(false));
    }

    [Test]
    public void test_EquipmentSystem_MeetsEquipRequirement_happy_path_2()
    {
        // Explicit oracle: "슬롯:아이템코드" 형식 분기. player가 null이므로
        // string.Equals(null, "Armor_001", OrdinalIgnoreCase) = false.
        var result = _sut.MeetsEquipRequirement("Armor:Armor_001");
        Assert.That(result, Is.EqualTo(false));
    }

    [Test]
    public void test_EquipmentSystem_MeetsEquipRequirement_edge_case_0()
    {
        // Explicit oracle: 공백/빈 문자열 → IsNullOrWhiteSpace 가드에서 즉시 false 반환.
        var result = _sut.MeetsEquipRequirement("");
        Assert.That(result, Is.EqualTo(false));
    }

    // Omitted generated case test_EquipmentSystem_MeetsEquipRequirement_exception_0 for EquipmentSystem.MeetsEquipRequirement: generic exception oracle requires manual C# assertion design.

}

