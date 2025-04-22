using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
   public JsonManager jsonManager;
    public List<Weapon_Master> weapons;

    public void Awake()
    {
        //무기 정보 받아오기
        weapons = jsonManager.Weapon_Masters;
        if (weapons == null || weapons.Count == 0)
        {
            //값이 비워져있는지 확인
            Debug.Log("값이 비워져있습니다");
        }
        else
        {
            Debug.Log(weapons.Count);
        }
    }
}
