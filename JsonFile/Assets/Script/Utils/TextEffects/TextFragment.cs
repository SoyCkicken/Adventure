using System.Collections.Generic;

namespace MyGame.TextEffects
{
    public class TextFragment
    {
        public string text;
        public List<TextEffect> effects = new();

        public TextFragment(string txt, List<TextEffect> fx)
        {
            text = txt;
            effects = fx;
        }
    }
}
