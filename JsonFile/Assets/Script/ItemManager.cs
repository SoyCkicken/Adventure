using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ExcelDataReader.Log;

public class ItemManager : MonoBehaviour
{
    [Header("JSON Manager")]
    public JsonManager jsonManager;

    [Header("Content Parents")]
    [Header("지금 UI 위치가 안 정해져서 일단 텍스트 출력 위치랑 동일한 곳으로 해놨음")]
    //위치인데 일단 하나로 잡음
    public Transform weaponsParent;
    //public Transform armorsParent;
    //public Transform consumablesParent;

    [Header("Item Prefabs")]
    [Header("이건 만약 장비마다 출력 해야 되는 값이 달라지거나 하면 프리팹만 만들어서 추가 해주면 됨")]
    public GameObject weaponItemPrefab;
    public GameObject armorItemPrefab;
    public GameObject consumableItemPrefab;

    void Start()
    {
        //jsonManager가 비워져있는지 확인하고 비워져있으면 파일 찾아서 넣음
        if (jsonManager == null)
            jsonManager = FindObjectOfType<JsonManager>();
        //출력한다
        DisplayWeapons();
        DisplayArmors();
        DisplayConsumables();
    }
    //정리하는데 
    void Clear(Transform parent)
    {
        foreach (Transform t in parent)
            Destroy(t.gameObject);
    }

    void DisplayWeapons()
    {
        //Clear(weaponsParent);
        foreach (var w in jsonManager.GetWeaponMasters("Weapon_Master"))
        {
            var go = Instantiate(weaponItemPrefab, weaponsParent);
            // Header 아이템바의 윗부분이라 헤드
            var header = go.transform.Find("Item Name+Icon");
            header.Find("Name").GetComponent<TMP_Text>().text = w.Weapon_Name;
            // Body: Stats 아이템바의 중간 부분이라 바디
            var body = go.transform.Find("Item_Info");
            var stats = body.Find("Stats").GetComponent<TMP_Text>();
            //텍스트 추가
            stats.text = $"공격력 {w.Weapon_DMG}\n";
            if (w.STR_Scaling != 0) stats.text += $"힘 가중치 : {w.STR_Scaling}\n";
            if (w.DEX_Scaling != 0) stats.text += $"민첩 가중치 : {w.DEX_Scaling}\n";
            if (w.INT_Scaling != 0) stats.text += $"지력 가중치 : {w.INT_Scaling}\n";
            if (w.MAG_Scaling != 0) stats.text += $"마력 가중치 : {w.MAG_Scaling}\n";
            if (w.DIV_Scaling != 0) stats.text += $"신성 가중치 : {w.DIV_Scaling}\n";
            if (w.CHR_Scaling != 0) stats.text += $"카리스마 가중치 : {w.CHR_Scaling}\n";
            // Body: Options
            var opts = body.Find("Options").GetComponent<TMP_Text>();
            var myOpts = new List<string>();
            if (!string.IsNullOrEmpty(w.Option_1_ID))
            {
                var o = jsonManager.GetOptionMasters("Option_Master").FirstOrDefault(x => x.Option_ID == w.Option_1_ID);
                if (o != null) myOpts.Add($"{o.Option_Description} +{w.Option_Value1}");
            }
            if (!string.IsNullOrEmpty(w.Option_2_ID))
            {
                var o = jsonManager.GetOptionMasters("Option_Master").FirstOrDefault(x => x.Option_ID == w.Option_2_ID);
                if (o != null) myOpts.Add($"{o.Option_Description} +{w.Option_Value2}");
            }
            opts.text = myOpts.Count > 0 ? string.Join("\n", myOpts) : "옵션 없음";

            ForceRebuild(body);
        }
    }

    void DisplayArmors()
    {
        //Clear(weaponsParent);
        foreach (var a in jsonManager.GetArmorMasters("Armor_Master"))
        {
            var go = Instantiate(armorItemPrefab, weaponsParent);
            var header = go.transform.Find("Item Name+Icon");
            header.Find("Name").GetComponent<TMP_Text>().text = a.Armor_NAME;

            var body = go.transform.Find("Item_Info");
            var stats = body.Find("Stats").GetComponent<TMP_Text>();
            stats.text = $"방어력 {a.Armor_DEF}\n체력 {a.Armor_HP}";

            var opts = body.Find("Options").GetComponent<TMP_Text>();
            var myOpts = new List<string>();
            if (!string.IsNullOrEmpty(a.Armor_Option1))
            {
                var o = jsonManager.GetOptionMasters("Option_Master").FirstOrDefault(x => x.Option_ID == a.Armor_Option1);
                if (o != null) myOpts.Add($"{o.Option_Description} +{a.Option1_Value}");
            }
            if (!string.IsNullOrEmpty(a.Armor_Option2))
            {
                var o = jsonManager.GetOptionMasters("Option_Master").FirstOrDefault(x => x.Option_ID == a.Armor_Option2);
                if (o != null) myOpts.Add($"{o.Option_Description} +{a.Option2_Value}");
            }
            opts.text = myOpts.Count > 0 ? string.Join("\n", myOpts) : "옵션 없음";

            ForceRebuild(body);
        }
    }

    void DisplayConsumables()
    {
        //Clear(weaponsParent);
        foreach (var it in jsonManager.GetItemMasters("Item_Master"))
        {
            var go = Instantiate(consumableItemPrefab, weaponsParent);
            var header = go.transform.Find("Item Name+Icon");
            header.Find("Name").GetComponent<TMP_Text>().text = it.Item_NAME;

            var body = go.transform.Find("Item_Info");
            var stats = body.Find("Stats").GetComponent<TMP_Text>();
            stats.text = $"{it.Item_Description}\n가격: {it.Item_Price}";

            var opts = body.Find("Options").GetComponent<TMP_Text>();
            var myOpts = new List<string>();
            if (!string.IsNullOrEmpty(it.Item_Option1))
            {
                var o = jsonManager.GetOptionMasters("Option_Master").FirstOrDefault(x => x.Option_ID == it.Item_Option1);
                if (o != null) myOpts.Add($"{o.Option_Description} +{it.Option1_Value}");
            }
            if (!string.IsNullOrEmpty(it.Item_Option2))
            {
                var o = jsonManager.GetOptionMasters("Option_Master").FirstOrDefault(x => x.Option_ID == it.Item_Option2);
                if (o != null) myOpts.Add($"{o.Option_Description} +{it.Option2_Value}");
            }
            opts.text = myOpts.Count > 0 ? string.Join("\n", myOpts) : "옵션 없음";

            ForceRebuild(body);
        }
    }

    void ForceRebuild(Transform body)
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(body.GetComponent<RectTransform>());
    }
}
