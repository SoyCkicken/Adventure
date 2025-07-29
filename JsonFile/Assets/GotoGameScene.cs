using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GotoGameScene : MonoBehaviour
{
    // Start is called before the first frame update
    public Button Startbutton;
    public Button Loadbutton;
    void Start()
    {
        Startbutton.onClick.AddListener(() => {
            SceneManager.LoadSceneAsync("TestScene");
        });
        Loadbutton.onClick.AddListener(() => {
            SceneManager.LoadSceneAsync("TestScene");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
