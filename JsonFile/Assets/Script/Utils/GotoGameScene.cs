//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;

//public class GotoGameScene : MonoBehaviour
//{
//    // Start is called before the first frame update
//    [SerializeField] SaveManager saveManager;
//    public Button Startbutton;
//    public Button Loadbutton;
//    void Start()
//    {
//        Startbutton.onClick.AddListener(() => {
//            SceneManager.LoadSceneAsync("GameScene");
//        });
//        Loadbutton.onClick.AddListener(() => {
//            saveManager.OnClickLoadGame();
//            SceneManager.LoadSceneAsync("GameScene");
            
//        });
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}
