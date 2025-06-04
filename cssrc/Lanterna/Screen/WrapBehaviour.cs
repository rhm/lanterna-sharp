namespace Lanterna.Screen
{
    /// <summary>
    /// How to handle line wrapping when writing text to a Screen.
    /// </summary>
    public enum WrapBehaviour
    {
        SingleLine,
        Clip,
        Char,
        Word
    }

    public static class WrapBehaviourExtensions
    {
        public static bool AllowLineFeed(this WrapBehaviour behaviour)
        {
            return behaviour switch
            {
                WrapBehaviour.SingleLine => false,
                WrapBehaviour.Clip => true,
                WrapBehaviour.Char => true,
                WrapBehaviour.Word => true,
                _ => false
            };
        }

        public static bool AutoWrap(this WrapBehaviour behaviour)
        {
            return behaviour switch
            {
                WrapBehaviour.SingleLine => false,
                WrapBehaviour.Clip => false,
                WrapBehaviour.Char => true,
                WrapBehaviour.Word => true,
                _ => false
            };
        }

        public static bool KeepWords(this WrapBehaviour behaviour)
        {
            return behaviour == WrapBehaviour.Word;
        }
    }
}
