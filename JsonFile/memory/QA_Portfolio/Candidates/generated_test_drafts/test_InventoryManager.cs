using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class TestInventoryManager
{
    private InventoryManager _sut;
    private GameObject _go;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _sut = _go.AddComponent<InventoryManager>();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(_go);
    }

    [Test]
    public void test_InventoryManager_CountItem_happy_path()
    {
        // Explicit oracle: Item_001을 2개, Item_002를 1개 추가하면 비스택 카운트 로직(같은
        // 코드의 객체 수를 셈)에 따라 CountItem("Item_001")은 정확히 2를 반환해야 한다.
        _sut.AddItemToInventory(new ItemData { Item_ID = "Item_001" });
        _sut.AddItemToInventory(new ItemData { Item_ID = "Item_001" });
        _sut.AddItemToInventory(new ItemData { Item_ID = "Item_002" });

        var result = _sut.CountItem("Item_001");
        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public void test_InventoryManager_CountItem_happy_path_1()
    {
        // Explicit oracle: 비교가 StringComparison.OrdinalIgnoreCase이므로 대소문자가
        // 달라도 동일 아이템으로 매칭되어 1을 반환해야 한다.
        _sut.AddItemToInventory(new ItemData { Item_ID = "Item_001" });

        var result = _sut.CountItem("item_001");
        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void test_InventoryManager_CountItem_edge_case_0()
    {
        // Explicit oracle: itemCode가 빈 문자열이면 인벤토리에 아이템이 있어도
        // IsNullOrEmpty 가드에서 즉시 0을 반환한다(인벤토리가 비어서 0인 게 아님을 증명).
        _sut.AddItemToInventory(new ItemData { Item_ID = "Item_001" });

        var result = _sut.CountItem("");
        Assert.That(result, Is.EqualTo(0));
    }

    // Omitted generated case test_InventoryManager_CountItem_exception_0 for InventoryManager.CountItem: generic exception oracle requires manual C# assertion design.

    [Test]
    public void test_InventoryManager_CountItemInstances_happy_path()
    {
        // Explicit oracle: 같은 코드의 아이템 3개를 추가하면 비스택 카운트 로직에 따라
        // 정확히 3을 반환해야 한다(소스 주석: "비스택: 같은 코드의 '객체 수'를 센다").
        _sut.AddItemToInventory(new ItemData { Item_ID = "Potion_A" });
        _sut.AddItemToInventory(new ItemData { Item_ID = "Potion_A" });
        _sut.AddItemToInventory(new ItemData { Item_ID = "Potion_A" });

        var result = _sut.CountItemInstances("Potion_A");
        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public void test_InventoryManager_CountItemInstances_happy_path_1()
    {
        // Explicit oracle: 인벤토리에 다른 코드의 아이템만 있을 때 존재하지 않는 코드로
        // 조회하면 0을 반환한다(인벤토리가 비어서가 아니라 매칭이 없어서 0임을 증명).
        _sut.AddItemToInventory(new ItemData { Item_ID = "Potion_A" });
        _sut.AddItemToInventory(new ItemData { Item_ID = "Potion_B" });

        var result = _sut.CountItemInstances("Potion_C");
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void test_InventoryManager_CountItemInstances_edge_case_0()
    {
        // Explicit oracle: itemCode가 빈 문자열이면 인벤토리에 아이템이 있어도
        // IsNullOrEmpty 가드에서 즉시 0을 반환한다.
        _sut.AddItemToInventory(new ItemData { Item_ID = "Potion_A" });

        var result = _sut.CountItemInstances("");
        Assert.That(result, Is.EqualTo(0));
    }

    // Omitted generated case test_InventoryManager_CountItemInstances_exception_0 for InventoryManager.CountItemInstances: generic exception oracle requires manual C# assertion design.

}

