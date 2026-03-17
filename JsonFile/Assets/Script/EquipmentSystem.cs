//using UnityEngine;
//using System.Linq;
//using MyGame;
//using UnityEngine.UI;
//using UnityEditor;
//using System;
//using System.Collections.Generic;
//using System.Collections;  // Character, OptionContext 등이 있는 네임스페이스

//public class EquipmentSystem : MonoBehaviour
//{
//    public PlayerState playerState;
//    public JsonManager jsonManager;
//    public InventoryManager inventoryManager;
//    public Character player;

//    private void Start()
//    {
//        jsonManager = JsonManager.Instance; // 수정
//        playerState = PlayerState.Instance;

//        //player.weapon_Name = null;
//        //player.armor_Name = null;
//        jsonManager = jsonManager ?? JsonManager.Instance ?? FindObjectOfType<JsonManager>();

//        //if (jsonManager != null && jsonManager.IsReady)
//        //{
//        //    Init();
//        //}
//        //else if (jsonManager != null)
//        //{
//        //    // ✅ JsonManager 로딩 끝나면 실행
//        //    jsonManager.OnReady += HandleJsonReady;
//        //}
//        //else
//        //{
//        //    Debug.LogWarning("[EquipmentSystem] JsonManager가 아직 씬에 없습니다. 다음 프레임 재시도.");
//        //    StartCoroutine(WaitAndInit());
//        //}
//    }
//    //private void HandleJsonReady()
//    //{
//    //    jsonManager.OnReady -= HandleJsonReady;
//    //    Init();
//    //}
//    //private IEnumerator WaitAndInit()
//    //{
//    //    // JsonManager 생성 대기
//    //    while (JsonManager.Instance == null) yield return null;
//    //    jsonManager = JsonManager.Instance;

//    //    if (jsonManager.IsReady) Init();
//    //    else
//    //    {
//    //        jsonManager.OnReady += HandleJsonReady;
//    //    }
//    //}

//    //public void Init()
//    //{
//    //    jsonManager ??= JsonManager.Instance ?? FindObjectOfType<JsonManager>();
//    //    playerState ??= PlayerState.Instance ?? FindObjectOfType<PlayerState>();
//    //    if (jsonManager == null || player == null || playerState == null)
//    //    {
//    //        Debug.LogWarning("[EquipmentSystem] Init 대기: 참조 부족");
//    //        return;
//    //    }
//    //    if (player != null)
//    //    {
//    //        player.equipmentQuery = new EquipmentQueryImpl(() => player.weapon_Name, () => player.armor_Name);
//    //    }
//    //    ClearInit();
//    //    // 자동 참조

//    //    var weaponId = string.IsNullOrEmpty(player.weapon_Name) ? null : player.weapon_Name;
//    //    var weapon = !string.IsNullOrEmpty(weaponId)
//    //        ? jsonManager.GetWeaponMasters("Weapon_Master").FirstOrDefault(w => w.Weapon_ID == weaponId)
//    //        : null;

//    //    Debug.Log($"무기 = {player.weapon_Name != ""}");
//    //    // 무기 장착 처리
//    //    if (weapon != null)
//    //    {
//    //        int tempDamage = Convert.ToInt32(weapon.Weapon_DMG + (playerState.STR * weapon.STR_Scaling)
//    //            + (playerState.AGI * weapon.DEX_Scaling)
//    //            + (playerState.INT * weapon.INT_Scaling)
//    //            + (playerState.MAG * weapon.MAG_Scaling)
//    //            + (playerState.CHA * weapon.CHR_Scaling)
//    //            + (playerState.DIV * weapon.DIV_Scaling));
//    //        player.damage = tempDamage;
//    //        // 옵션 리스트에 추가
//    //        if (!string.IsNullOrEmpty(weapon.Option_1_ID))
//    //            OptionManager.ApplyOption(weapon.Option_1_ID, new OptionContext
//    //            {
//    //                User = player,
//    //                Value = weapon.Option_Value1,
//    //                item_ID = weapon.Weapon_ID,
//    //                option_ID = weapon.Option_1_ID
//    //            });
//    //        if (!string.IsNullOrEmpty(weapon.Option_2_ID))
//    //            OptionManager.ApplyOption(weapon.Option_2_ID, new OptionContext
//    //            {
//    //                User = player,
//    //                Value = weapon.Option_Value2,
//    //                item_ID = weapon.Weapon_ID,
//    //                option_ID = weapon.Option_2_ID
//    //            });
//    //    }
//    //    var armorId = string.IsNullOrEmpty(player.armor_Name) ? null : player.armor_Name;
//    //    var armor = !string.IsNullOrEmpty(armorId)
//    //        ? jsonManager.GetArmorMasters("Armor_Master").FirstOrDefault(a => a.Armor_ID == armorId)
//    //        : null;
//    //    Debug.Log($"방어구 = {player.armor_Name != ""}");
//    //    // 방어구 장착 처리
//    //    if (armor != null)
//    //    {
//    //        player.armor = armor.Armor_DEF;
//    //        player.MaxHealth = armor.Armor_HP;
//    //        // 옵션 리스트에 추가
//    //        if (!string.IsNullOrEmpty(armor.Armor_Option1))
//    //            OptionManager.ApplyOption(armor.Armor_Option1, new OptionContext
//    //            {
//    //                User = player,
//    //                Value = armor.Option1_Value,
//    //                item_ID = armor.Armor_ID,
//    //                option_ID = armor.Armor_Option1
//    //            });
//    //        if (!string.IsNullOrEmpty(armor.Armor_Option2))
//    //            OptionManager.ApplyOption(armor.Armor_Option2, new OptionContext
//    //            {
//    //                User = player,
//    //                Value = armor.Option2_Value,
//    //                item_ID = armor.Armor_ID,
//    //                option_ID = armor.Armor_Option2
//    //            });
//    //    }
//    //}
//    //public void EquipItem(
//    //ItemData item,
//    //List<ItemData> inventoryItems,
//    //ItemSlotUI weaponSlot,
//    //ItemSlotUI armorSlot,
//    //Action<ItemData> onClick)
//    //{
//    //    if (item.Item_Type == "Weapon")
//    //    {
//    //        if (weaponSlot.CurrentItem != null)
//    //            inventoryItems.Add(weaponSlot.CurrentItem.Clone());

//    //        weaponSlot.Setup(item, onClick);  // ✅ 콜백 전달
//    //        inventoryItems.Remove(item);
//    //        player.weapon_Name = item.Item_ID;
//    //    }
//    //    else if (item.Item_Type == "Armor")
//    //    {
//    //        if (armorSlot.CurrentItem != null)
//    //            inventoryItems.Add(armorSlot.CurrentItem.Clone());

//    //        armorSlot.Setup(item, onClick);  // ✅ 콜백 전달
//    //        inventoryItems.Remove(item);
//    //        player.armor_Name = item.Item_ID;
//    //    }

//    //    Init();
//    //}

//    //public void UnequipItem(ItemSlotUI slot, List<ItemData> inventoryItems)
//    //{
//    //    if (slot.CurrentItem == null) return;

//    //    inventoryItems.Add(slot.CurrentItem.Clone());
//    //    player.RemoveBuffByItem(slot.CurrentItem.Item_ID);
//    //    if (slot.CurrentItem.Item_Type == "Weapon")
//    //        player.weapon_Name = null;
//    //    else if (slot.CurrentItem.Item_Type == "Armor")
//    //        player.armor_Name = null;
//    //    slot.Clear();
//    //    Init();
//    //}
//    public void EquipItem(
//    ItemData item,
//    List<ItemData> inventoryItems,
//    ItemSlotUI weaponSlot,
//    ItemSlotUI armorSlot,
//    Action<ItemData> onClick)
//    {
//        if (item == null) return;

//        // 이미 같은 아이템이면 불필요한 재적용 방지 (더블 스택/더미 방지)
//        //if (item.Item_Type == "Weapon" && player.weapon_Name == item.Item_ID) return;
//        //if (item.Item_Type == "Armor" && player.armor_Name == item.Item_ID) return;

//        if (item.Item_Type == "Weapon")
//        {
//            // 🔴 교체될 기존 무기 버프 먼저 제거
//            if (weaponSlot.CurrentItem != null)
//            {
//                var oldId = weaponSlot.CurrentItem.Item_ID;
//                inventoryItems.Add(weaponSlot.CurrentItem.Clone());
//                //player.RemoveBuffByItem(oldId); // ← 핵심
//            }

//            weaponSlot.Setup(item, onClick);
//            inventoryItems.Remove(item);
//            //player.weapon_Name = item.Item_ID;
//        }
//        else if (item.Item_Type == "Armor")
//        {
//            // 🔴 교체될 기존 방어구 버프 먼저 제거
//            if (armorSlot.CurrentItem != null)
//            {
//                var oldId = armorSlot.CurrentItem.Item_ID;
//                inventoryItems.Add(armorSlot.CurrentItem.Clone());
//                //player.RemoveBuffByItem(oldId); // ← 핵심
//            }

//            armorSlot.Setup(item, onClick);
//            inventoryItems.Remove(item);
//            //player.armor_Name = item.Item_ID;
//        }
//        else
//        {
//            Debug.LogWarning($"[EquipmentSystem] 장착 불가 타입: {item.Item_Type}");
//            return;
//        }

//        // 새 장비 옵션/버프 적용
//        //Init();
//    }

//    public void UnequipItem(ItemSlotUI slot, List<ItemData> inventoryItems)
//    {
//        if (slot?.CurrentItem == null) return;

//        var equipped = slot.CurrentItem;
//        // 1) 인벤토리로 복귀
//        inventoryItems.Add(equipped.Clone());

//        // 2) 장착 해제 (buff 제거는 Init()에서 재계산로 보정 + 아래 Remove로 보조)
//        //player.RemoveBuffByItem(equipped.Item_ID);

//        //if (equipped.Item_Type == "Weapon")
//        //    player.weapon_Name = null;
//        //else if (equipped.Item_Type == "Armor")
//        //    player.armor_Name = null;

//        slot.Clear();

//        // 3) 버프/스탯 재적용
//        //Init();
//    }

//    void ClearInit()
//    {
//        //Debug.LogError("플레이어 능력치 초기화");
//        // 기본치 재설정
//        //player.OnHitOptions.Clear();
//       // Debug.Log($"Player의 장비 장착 여부 무기 : {player.weapon_Name} , 갑옷 : {player.armor_Name} ");

//        //if (player.armor_Name != "" || player.weapon_Name != "")
//        //{
//        //    if (playerState.Health >= 10)
//        //    {
//        //        player.MaxHealth = playerState.Health * 5;
//        //    }
//        //    else
//        //    {
//        //        player.MaxHealth = 50;
//        //    }

//        //    if (playerState.STR >= 10)
//        //    {
//        //        player.damage = playerState.STR;
//        //    }
//        //    else
//        //    {
//        //        player.damage = 10;
//        //    }
//        //    player.armor = 3;

//        //    if (playerState.AGI >= 10)
//        //    {
//        //        player.speed = 0.15f * playerState.AGI;
//        //    }
//        //    else
//        //    {
//        //        player.speed = 1.5f;
//        //    }
//        //    if (playerState.INT >= 10)
//        //    {
//        //        int tempcri = playerState.INT - 10;
//        //        player.CitChance = Convert.ToInt32(10 + (2.5f * tempcri)); // <-- 11이상 부터 크리티컬 확률 증가
//        //    }
//        //    else
//        //    {
//        //        player.CitChance = 10;
//        //    }
//        //}     
//    }
//    public bool MeetsEquipRequirement(string code)
//    {
//        if (string.IsNullOrWhiteSpace(code)) return false;
//        var c = code.Trim();

//        // 1) 슬롯만 검사 ("Weapon" | "Armor")
//        //if (c.Equals("Weapon", StringComparison.OrdinalIgnoreCase))
//        //    return !string.IsNullOrEmpty(player?.weapon_Name);
//        //if (c.Equals("Armor", StringComparison.OrdinalIgnoreCase))
//        //    return !string.IsNullOrEmpty(player?.armor_Name);

//        // 2) 슬롯:아이템코드 ("Weapon:Weapon_001" | "Armor:Armor_007")
//        var parts = c.Split(':');
//        if (parts.Length == 2)
//        {
//            var slot = parts[0].Trim();
//            var itemCode = parts[1].Trim();

//            //if (slot.Equals("Weapon", StringComparison.OrdinalIgnoreCase))
//            //    return string.Equals(player?.weapon_Name, itemCode, StringComparison.OrdinalIgnoreCase);

//            //if (slot.Equals("Armor", StringComparison.OrdinalIgnoreCase))
//            //    return string.Equals(player?.armor_Name, itemCode, StringComparison.OrdinalIgnoreCase);
//        }

//        // 포맷 불일치
//        return false;
//    }
//    //private class EquipmentQueryImpl : Character.IEquipmentQuery
//    //{
//    //    private readonly Func<string> getWeapon;
//    //    private readonly Func<string> getArmor;
//    //    public EquipmentQueryImpl(Func<string> w, Func<string> a) { getWeapon = w; getArmor = a; }

//    //    public bool IsItemEquipped(string itemID)
//    //    {
//    //        if (string.IsNullOrEmpty(itemID)) return false;
//    //        return string.Equals(getWeapon(), itemID, StringComparison.OrdinalIgnoreCase)
//    //            || string.Equals(getArmor(), itemID, StringComparison.OrdinalIgnoreCase);
//    //    }
//    //}
//}


//using UnityEngine;
//using System;
//using System.Collections.Generic;
//using MyGame;

//public class EquipmentSystem : MonoBehaviour
//{
//    public PlayerState playerState;
//    public InventoryManager inventoryManager;

//    private void Start()
//    {
//        // 인스턴스 할당 확인
//        if (playerState == null) playerState = PlayerState.Instance;
//    }

//    /// <summary>
//    /// 아이템 장착 로직
//    /// </summary>
//    public void EquipItem(ItemData item, List<ItemData> inventoryItems, ItemSlotUI weaponSlot, ItemSlotUI armorSlot, Action<ItemData> onClick)
//    {
//        if (item == null) return;

//        if (item.Item_Type == "Weapon")
//        {
//            // 1. 기존 무기가 있다면 인벤토리에 다시 넣기
//            if (playerState.equippedWeapon != null)
//            {
//                inventoryItems.Add(playerState.equippedWeapon.Clone());
//            }

//            // 2. PlayerState에 새 무기 할당
//            playerState.equippedWeapon = item;

//            // 3. UI 슬롯 업데이트
//            weaponSlot.Setup(item, onClick);
//            inventoryItems.Remove(item);
//        }
//        else if (item.Item_Type == "Armor")
//        {
//            // 1. 기존 방어구가 있다면 인벤토리에 다시 넣기
//            if (playerState.equippedArmor != null)
//            {
//                inventoryItems.Add(playerState.equippedArmor.Clone());
//            }

//            // 2. PlayerState에 새 방어구 할당
//            playerState.equippedArmor = item;

//            // 3. UI 슬롯 업데이트
//            armorSlot.Setup(item, onClick);
//            inventoryItems.Remove(item);
//        }

//        // ★ 핵심: 장비가 바뀌었으므로 최종 능력치 재계산 및 이벤트 발생
//        playerState.RefreshStats();
//    }

//    /// <summary>
//    /// 아이템 해제 로직
//    /// </summary>
//    public void UnequipItem(ItemSlotUI slot, List<ItemData> inventoryItems)
//    {
//        if (slot?.CurrentItem == null) return;

//        ItemData equipped = slot.CurrentItem;

//        // 1. 인벤토리에 추가
//        inventoryItems.Add(equipped.Clone());

//        // 2. PlayerState에서 제거
//        if (equipped.Item_Type == "Weapon")
//            playerState.equippedWeapon = null;
//        else if (equipped.Item_Type == "Armor")
//            playerState.equippedArmor = null;

//        // 3. UI 슬롯 비우기
//        slot.Clear();

//        // ★ 핵심: 능력치 재계산 및 이벤트 발생
//        playerState.RefreshStats();
//    }

//    /// <summary>
//    /// 특정 아이템이 장착되어 있는지 확인 (퀘스트나 조건 확인용)
//    /// </summary>
//    public bool IsItemEquipped(string itemID)
//    {
//        if (string.IsNullOrEmpty(itemID)) return false;

//        bool isWeapon = playerState.equippedWeapon != null && playerState.equippedWeapon.Item_ID == itemID;
//        bool isArmor = playerState.equippedArmor != null && playerState.equippedArmor.Item_ID == itemID;

//        return isWeapon || isArmor;
//    }
//}

using UnityEngine;
using System;
using System.Collections.Generic;
using MyGame;

public class EquipmentSystem : MonoBehaviour
{
    public PlayerState playerState;
    public InventoryManager inventoryManager;

    private void Start()
    {
        // 인스턴스 할당 확인
        if (playerState == null) playerState = PlayerState.Instance;
    }

    /// <summary>
    /// 아이템 장착 로직
    /// </summary>
    public void EquipItem(ItemData item, List<ItemData> inventoryItems, ItemSlotUI weaponSlot, ItemSlotUI armorSlot, Action<ItemData> onClick)
    {
        if (item == null) return;

        if (item.Item_Type == "Weapon")
        {
            // 1. 기존 무기가 있다면 인벤토리에 다시 넣기
            if (playerState.equippedWeapon != null)
            {
                inventoryItems.Add(playerState.equippedWeapon.Clone());
            }

            // 2. PlayerState에 새 무기 할당
            playerState.equippedWeapon = item;

            // 3. UI 슬롯 업데이트
            weaponSlot.Setup(item, onClick);
            inventoryItems.Remove(item);
        }
        else if (item.Item_Type == "Armor")
        {
            // 1. 기존 방어구가 있다면 인벤토리에 다시 넣기
            if (playerState.equippedArmor != null)
            {
                inventoryItems.Add(playerState.equippedArmor.Clone());
            }

            // 2. PlayerState에 새 방어구 할당
            playerState.equippedArmor = item;

            // 3. UI 슬롯 업데이트
            armorSlot.Setup(item, onClick);
            inventoryItems.Remove(item);
        }

        // ★ 핵심: 장비가 바뀌었으므로 최종 능력치 재계산 및 이벤트 발생
        playerState.RefreshStats();
    }

    /// <summary>
    /// 아이템 해제 로직
    /// </summary>
    public void UnequipItem(ItemSlotUI slot, List<ItemData> inventoryItems)
    {
        if (slot?.CurrentItem == null) return;

        ItemData equipped = slot.CurrentItem;

        // 1. 인벤토리에 추가
        inventoryItems.Add(equipped.Clone());

        // 2. PlayerState에서 제거
        if (equipped.Item_Type == "Weapon")
            playerState.equippedWeapon = null;
        else if (equipped.Item_Type == "Armor")
            playerState.equippedArmor = null;

        // 3. UI 슬롯 비우기
        slot.Clear();

        // ★ 핵심: 능력치 재계산 및 이벤트 발생
        playerState.RefreshStats();
    }

    /// <summary>
    /// 특정 아이템이 장착되어 있는지 확인 (퀘스트나 조건 확인용)
    /// </summary>
    public bool IsItemEquipped(string itemID)
    {
        if (string.IsNullOrEmpty(itemID)) return false;

        bool isWeapon = playerState.equippedWeapon != null && playerState.equippedWeapon.Item_ID == itemID;
        bool isArmor = playerState.equippedArmor != null && playerState.equippedArmor.Item_ID == itemID;

        return isWeapon || isArmor;
    }
}