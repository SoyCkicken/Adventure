using UnityEngine;

namespace MyGame.TextEffects
{
    public enum EffectType
    {
        Wave,
        Shake,
        Color
        // 나중에 Size, Bold 등 확장 가능
    }

    public class TextEffect
    {
        public EffectType type;
        public Color? color; // Color 타입일 때만 사용
    }
}

