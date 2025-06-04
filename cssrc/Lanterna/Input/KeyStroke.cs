using System;

namespace Lanterna.Input
{
    /// <summary>
    /// Basic representation of a key event. This is a minimal
    /// subset of the Java KeyStroke class used by Lanterna.
    /// </summary>
    public class KeyStroke
    {
        public KeyType KeyType { get; }
        public char? Character { get; }
        public bool CtrlDown { get; }
        public bool AltDown { get; }
        public bool ShiftDown { get; }
        public long EventTime { get; }

        public KeyStroke(KeyType keyType)
            : this(keyType, null, false, false, false)
        {
        }

        public KeyStroke(KeyType keyType, bool ctrlDown, bool altDown, bool shiftDown = false)
            : this(keyType, null, ctrlDown, altDown, shiftDown)
        {
        }

        public KeyStroke(char character, bool ctrlDown = false, bool altDown = false)
            : this(KeyType.Character, character, ctrlDown, altDown, false)
        {
        }

        public KeyStroke(KeyType keyType, char? character, bool ctrlDown, bool altDown, bool shiftDown)
        {
            if (keyType == KeyType.Character && character == null)
                throw new ArgumentException("Character must be specified when key type is Character");

            KeyType = keyType;
            Character = character;
            CtrlDown = ctrlDown;
            AltDown = altDown;
            ShiftDown = shiftDown;
            EventTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
