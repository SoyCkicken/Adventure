//using MyGame;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class BuffIconUI : MonoBehaviour
//{
//    public Image iconImage;
//    public Image timerSlider;
//    private BuffData buff;
//    public GameObject BattleImage;
//    //스택 없음
//    //public TMP_Text stackText;
//    //버프 시간 표시 없고 대신 버프 이미지 기준으로 외각으로 값이 들어가는 Silder 추가 할 예정
//    //public TMP_Text durationText;
//    public SpriteBank spriteBank;

//    private void Awake()
//    {
//        //동적으로 생성 되는 애라 할당이 안되는데 그거 고려해서 자동으로 찾게 시킴
//        if (spriteBank == null)
//            spriteBank = FindAnyObjectByType<SpriteBank>();
//        if (BattleImage == null)
//         BattleImage = transform.Find("자동전투화면Canvas(대략적으로 배치를 해 놓은것)").gameObject; // BattleImage 오브젝트 찾기
//    }
//    public void Set(BuffData data)
//    {
//        BattleImage.SetActive(true); // 버프 아이콘이 활성화된 상태로 시작
//        buff = data;
//        iconImage.sprite = spriteBank.Load(buff.OptionID); // 아이콘 등록
//        //timerSlider.fillAmount = data.Duration;
//        //stackText.text = buff.Stack > 1 ? $"x{buff.Stack}" : "";
//        //durationText.text = buff.Duration > 0 ? $"{Mathf.CeilToInt(buff.Duration - buff.Elapsed)}s" : "";
//        timerSlider.fillAmount = 1f; // 처음엔 항상 가득 차 있음
//        BattleImage.SetActive(false); // 버프 아이콘이 활성화된 상태로 시작
//    }
//    private void Update()
//    {
//        if (buff == null || buff.Duration <= 0f) return;

//        buff.Elapsed += Time.deltaTime;
//        float remaining = Mathf.Max(buff.Duration - buff.Elapsed, 0f);
//        timerSlider.fillAmount = remaining / buff.Duration;
//        //버프 지속시간 다되면 자기자신 삭제
//        if (buff.Duration - buff.Elapsed <= 0)
//        {
//            Destroy(this.gameObject);
//        }

//    }
//}

using MyGame;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffIconUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image timerSlider; // UI 게이지 (Image.type = Filled, Fill Amount 사용)
    [SerializeField] private SpriteBank spriteBank;

    public BuffData buffData { get; private set; }

    // 패시브/무한 지속이면 타이머를 돌리지 않는다
    private bool noTimer;

    public void Set(BuffData data)
    {
        buffData = data;

        // 스프라이트 뱅크 지연 바인딩 (씬 전환/비활성 부모 대비)
        if (spriteBank == null) spriteBank = SpriteBank.Instance;

        // 아이콘 세팅
        var spr = (spriteBank != null) ? spriteBank.Load(buffData.OptionID) : null;
        if (spr != null) iconImage.sprite = spr;

        // 패시브 또는 Duration<=0 은 타이머 숨김
        noTimer = buffData.IsPassive || buffData.Duration <= 0f;
        if (timerSlider != null)
        {
            if (noTimer)
            {
                // ① 아예 숨기기
                timerSlider.gameObject.SetActive(false);

                // (선택) 숨기지 않고 비우고 싶다면:
                // timerSlider.gameObject.SetActive(true);
                // timerSlider.fillAmount = 0f;
            }
            else
            {
                timerSlider.gameObject.SetActive(true);
                UpdateFill(); // 최초 반영
            }
        }
    }

    private void Update()
    {
        if (buffData == null) return;

        // 시간 주도권은 Character가 가짐 — UI는 표시만
        if (noTimer) return;

        UpdateFill();
    }

    private void UpdateFill()
    {
        if (buffData.Duration <= 0f) return; // 안전망

        float progress = Mathf.Clamp01(buffData.Elapsed / buffData.Duration);
        timerSlider.fillAmount = progress;

        // 수명 끝나면 UI 제거 (일시적 버프만)
        if (progress >= 1f - 1e-4f)
        {
            Destroy(gameObject);
        }
    }

    // (선택) 외부에서 동일 버프 갱신 시 호출해도 잘 동작하도록
    public void Refresh(BuffData updated)
    {
        buffData.Duration = updated.Duration;
        buffData.Elapsed = updated.Elapsed;
        buffData.Value = updated.Value;
        Set(buffData); // 타이머 on/off 재판단
    }
}