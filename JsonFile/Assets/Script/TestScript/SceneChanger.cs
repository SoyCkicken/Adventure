using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

        void Update()
    {
        // 예시: 키보드에서 F1 누르면 "BattleScene"으로 전환
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SceneManager.LoadScene("TestScene");
        }

        // 예시: F2 키로 "MainScene"으로 돌아가기
        if (Input.GetKeyDown(KeyCode.F2))
        {
            SceneManager.LoadScene("SampleScene");
        }
    }
}