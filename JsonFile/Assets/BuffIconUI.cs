using MyGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffIconUI : MonoBehaviour
{
    public Image iconImage;
    public Image timerSlider;
    private BuffData buff;
    public GameObject BattleImage;
    //스택 없음
    //public TMP_Text stackText;
    //버프 시간 표시 없고 대신 버프 이미지 기준으로 외각으로 값이 들어가는 Silder 추가 할 예정
    //public TMP_Text durationText;
    public SpriteBank spriteBank;

    private void Awake()
    {
        //동적으로 생성 되는 애라 할당이 안되는데 그거 고려해서 자동으로 찾게 시킴
        if (spriteBank == null)
            spriteBank = FindAnyObjectByType<SpriteBank>();
        if (BattleImage == null)
         BattleImage = transform.Find("자동전투화면Canvas(대략적으로 배치를 해 놓은것)").gameObject; // BattleImage 오브젝트 찾기
    }
    public void Set(BuffData data)
    {
        BattleImage.SetActive(true); // 버프 아이콘이 활성화된 상태로 시작
        buff = data;
        iconImage.sprite = spriteBank.Load(buff.OptionID); // 아이콘 등록
        //timerSlider.fillAmount = data.Duration;
        //stackText.text = buff.Stack > 1 ? $"x{buff.Stack}" : "";
        //durationText.text = buff.Duration > 0 ? $"{Mathf.CeilToInt(buff.Duration - buff.Elapsed)}s" : "";
        timerSlider.fillAmount = 1f; // 처음엔 항상 가득 차 있음
        BattleImage.SetActive(false); // 버프 아이콘이 활성화된 상태로 시작
    }
    private void Update()
    {
        if (buff == null || buff.Duration <= 0f) return;

        buff.Elapsed += Time.deltaTime;
        float remaining = Mathf.Max(buff.Duration - buff.Elapsed, 0f);
        timerSlider.fillAmount = remaining / buff.Duration;
        //버프 지속시간 다되면 자기자신 삭제
        if (buff.Duration - buff.Elapsed <= 0)
        {
            Destroy(this.gameObject);
        }

    }
}