using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StateManager : MonoBehaviour
{
    public Player player;
    public GameObject StateG;
    public TMP_Text TMtext;

    // Start is called before the first frame update

    private void Awake()
    {
        StateG.SetActive(false);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TMtext.text = $"플레이어의 스텟 : " +
            $"\n힘 : {player.Strength}" +
            $"\n민첩 : {player.Agility}" +
            $"\n지능 : {player.Intelligence}" +
            $"\n마력 : {player.Magic}" +
            $"\n신성 : {player.Divinity}" +
            $"\n카리스마(매력) : {player.Charisma}";
    }
    public void StateOn()
    {
        StateG.SetActive(true);
    }
    
}
