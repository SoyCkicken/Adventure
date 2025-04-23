using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
   public JsonManager jsonManager;
    public List<Weapon_Master> weapons;
    public Option_Master option_Master;
    public Weapon_Master currentweaponManager;
    //public TMP_Text text;
    public Transform contentParent;
    public GameObject weaponItemPrefab;

    void Start()
    {
        if (jsonManager == null)
            jsonManager = FindObjectOfType<JsonManager>();

        DisplayWeapons();
    }
    void DisplayWeapons()
    {
        // 기존 UI 지우기
        foreach (Transform t in contentParent) Destroy(t.gameObject);

        foreach (var w in jsonManager.Weapon_Masters)
        {
            var go = Instantiate(weaponItemPrefab, contentParent);
            var header = go.transform.Find("Item Name+Icon");
            //var iconImg = header.Find("Icon").GetComponent<Image>();
            var nameTxt = header.Find("Name").GetComponent<TMP_Text>();
            nameTxt.text = w.Weapon_Name;
            // 아이콘 로드
            var body = go.transform.Find("Item_Info");
            var statsTxt = body.Find("Stats").GetComponent<TMP_Text>();
            var optionsTxt = body.Find("Options").GetComponent<TMP_Text>();
            statsTxt.text = $"공격력  {w.Weapon_DMG}\n";
            string STR, DEX, INT, MAG, DIV, CHR;
            if (w.STR_Scaling != 0)
            {
                statsTxt.text += "힘 가중치 : "+w.STR_Scaling.ToString() +"\n";
            }
            if (w.DEX_Scaling != 0)
            {
                statsTxt.text += "민첩 가중치 : " + w.DEX_Scaling.ToString() + "\n";
            }
            if (w.INT_Scaling != 0)
            {
                statsTxt.text += "지력 가중치 : " + w.INT_Scaling.ToString() + "\n";
            }
            if (w.MAG_Scaling != 0)
            {
                statsTxt.text += "마력 가중치 : " + w.MAG_Scaling.ToString() + "\n";
            }
            if (w.DIV_Scaling != 0)
            {
                statsTxt.text += "신성 가중치 : " + w.DIV_Scaling.ToString() + "\n";
            }
            if (w.CHR_Scaling != 0)
            {
                statsTxt.text += "카리스마 가중치 : " + w.CHR_Scaling.ToString() + "\n";
            }
            // 옵션 표시
            var myOpts = new List<string>();
            foreach (var opt in jsonManager.Item_Options)
            {
                //첫번째 옵션
                if (opt.Option_ID == w.Option_1_ID)
                    myOpts.Add($"{opt.Option_Description}");
                //두번째 옵션
                if(opt.Option_ID == w.Option_2_ID)
                    myOpts.Add($"{opt.Option_Description}");
            }

            // 4) Options 텍스트에 개행(‘\n’)으로 붙여버리면 끝!
            optionsTxt.text = myOpts.Count > 0
                ? string.Join("\n", myOpts)
                : "옵션 없음";
            var bodyRect = go.transform.Find("Item_Info").GetComponent<RectTransform>();

            // 1) 캔버스 레이아웃 업데이트 예약
            Canvas.ForceUpdateCanvases();

            // 2) 강제 재계산
            LayoutRebuilder.ForceRebuildLayoutImmediate(bodyRect);
        }
    }
}


