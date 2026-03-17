using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    public Character targetCharacter;
    public Slider hpSlider;

    private void Start()
    {
        if (targetCharacter != null)
        {
            targetCharacter.OnHealthChanged += UpdateHPUI;
            UpdateHPUI(targetCharacter.Health, targetCharacter.MaxHealth);
        }
    }

    private void UpdateHPUI(int current, int max)
    {
        if (hpSlider != null)
            hpSlider.value = (float)current / max;
    }

    private void OnDestroy()
    {
        if (targetCharacter != null)
            targetCharacter.OnHealthChanged -= UpdateHPUI;
    }
}