[System.Serializable]
public class EffectTrigger
{
    public string ID;             // 예: SoulGain, HpLoss, ItemGain
    public string Code;           // 예: Weapon_002 (없으면 null)
    public int Value;             // 예: 100, -10, 1 등
}
