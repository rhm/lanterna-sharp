/*
 * This file is part of lanterna (https://github.com/mabe02/lanterna).
 *
 * lanterna is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Copyright (C) 2010-2024 Martin Berglund
 */

using System.Text;

namespace Lanterna.Input;

/// <summary>
/// Represents the user pressing a key on the keyboard. If the user held down ctrl and/or alt before pressing the key, 
/// this may be recorded in this class, depending on the terminal implementation and if such information in available.
/// KeyStroke objects are normally constructed by a KeyDecodingProfile, which works off a character stream that likely
/// coming from the system's standard input. Because of this, the class can only represent what can be read and 
/// interpreted from the input stream; for example, certain key-combinations like ctrl+i is indistinguishable from a tab
/// key press.
/// </summary>
public class KeyStroke : IEquatable<KeyStroke>
{
    /// <summary>
    /// Type of key that was pressed on the keyboard
    /// </summary>
    public KeyType KeyType { get; }
    
    /// <summary>
    /// Character value of the key pressed, or null if it was a special key
    /// </summary>
    public char? Character { get; }
    
    /// <summary>
    /// Returns true if ctrl was held down while the key was typed
    /// </summary>
    public bool IsCtrlDown { get; }
    
    /// <summary>
    /// Returns true if alt was held down while the key was typed
    /// </summary>
    public bool IsAltDown { get; }
    
    /// <summary>
    /// Returns true if shift was held down while the key was typed
    /// </summary>
    public bool IsShiftDown { get; }
    
    /// <summary>
    /// Gets the time when the keystroke was recorded
    /// </summary>
    public DateTime EventTime { get; }

    /// <summary>
    /// Constructs a KeyStroke based on a supplied keyType; character will be null and all modifier keys will be 
    /// considered not pressed. If you try to construct a KeyStroke with type KeyType.Character with this constructor, it
    /// will always throw an exception; use another overload that allows you to specify the character value instead.
    /// </summary>
    /// <param name="keyType">Type of the key pressed by this keystroke</param>
    /// <exception cref="ArgumentException">Thrown when keyType is Character but no character is provided</exception>
    public KeyStroke(KeyType keyType) : this(keyType, null, false, false, false)
    {
    }

    /// <summary>
    /// Constructs a KeyStroke based on a supplied keyType; character will be null.
    /// If you try to construct a KeyStroke with type KeyType.Character with this constructor, it
    /// will always throw an exception; use another overload that allows you to specify the character value instead.
    /// </summary>
    /// <param name="keyType">Type of the key pressed by this keystroke</param>
    /// <param name="ctrlDown">Was ctrl held down when the main key was pressed?</param>
    /// <param name="altDown">Was alt held down when the main key was pressed?</param>
    public KeyStroke(KeyType keyType, bool ctrlDown, bool altDown) : this(keyType, null, ctrlDown, altDown, false)
    {
    }

    /// <summary>
    /// Constructs a KeyStroke based on a supplied keyType; character will be null.
    /// If you try to construct a KeyStroke with type KeyType.Character with this constructor, it
    /// will always throw an exception; use another overload that allows you to specify the character value instead.
    /// </summary>
    /// <param name="keyType">Type of the key pressed by this keystroke</param>
    /// <param name="ctrlDown">Was ctrl held down when the main key was pressed?</param>
    /// <param name="altDown">Was alt held down when the main key was pressed?</param>
    /// <param name="shiftDown">Was shift held down when the main key was pressed?</param>
    public KeyStroke(KeyType keyType, bool ctrlDown, bool altDown, bool shiftDown) : this(keyType, null, ctrlDown, altDown, shiftDown)
    {
    }

    /// <summary>
    /// Constructs a KeyStroke based on a supplied character, keyType is implicitly KeyType.Character.
    /// A character-based KeyStroke does not support the shiftDown flag, as the shift state has
    /// already been accounted for in the character itself, depending on user's keyboard layout.
    /// </summary>
    /// <param name="character">Character that was typed on the keyboard</param>
    /// <param name="ctrlDown">Was ctrl held down when the main key was pressed?</param>
    /// <param name="altDown">Was alt held down when the main key was pressed?</param>
    public KeyStroke(char character, bool ctrlDown, bool altDown) : this(KeyType.Character, character, ctrlDown, altDown, false)
    {
    }

    /// <summary>
    /// Constructs a KeyStroke based on a supplied character, keyType is implicitly KeyType.Character.
    /// A character-based KeyStroke does not support the shiftDown flag, as the shift state has
    /// already been accounted for in the character itself, depending on user's keyboard layout.
    /// </summary>
    /// <param name="character">Character that was typed on the keyboard</param>
    /// <param name="ctrlDown">Was ctrl held down when the main key was pressed?</param>
    /// <param name="altDown">Was alt held down when the main key was pressed?</param>
    /// <param name="shiftDown">Was shift held down when the main key was pressed?</param>
    public KeyStroke(char character, bool ctrlDown, bool altDown, bool shiftDown) : this(KeyType.Character, character, ctrlDown, altDown, shiftDown)
    {
    }

    private KeyStroke(KeyType keyType, char? character, bool ctrlDown, bool altDown, bool shiftDown)
    {
        if (keyType == KeyType.Character && character == null)
            throw new ArgumentException("Cannot construct a KeyStroke with type KeyType.Character but no character information");

        // Enforce character for some key types
        switch (keyType)
        {
            case KeyType.Backspace:
                character = '\b';
                break;
            case KeyType.Enter:
                character = '\n';
                break;
            case KeyType.Tab:
                character = '\t';
                break;
        }

        KeyType = keyType;
        Character = character;
        IsShiftDown = shiftDown;
        IsCtrlDown = ctrlDown;
        IsAltDown = altDown;
        EventTime = DateTime.Now;
    }

    /// <summary>
    /// Creates a Key from a string representation in Vim's key notation.
    /// </summary>
    /// <param name="keyStr">the string representation of this key</param>
    /// <returns>the created KeyStroke</returns>
    /// <exception cref="ArgumentException">Thrown when the vim notation is invalid</exception>
    public static KeyStroke FromString(string keyStr)
    {
        string keyStrLC = keyStr.ToLowerInvariant();
        
        if (keyStr.Length == 1)
        {
            return new KeyStroke(keyStr[0], false, false, false);
        }
        
        if (keyStr.StartsWith("<") && keyStr.EndsWith(">"))
        {
            if (keyStrLC.Equals("<s-tab>"))
            {
                return new KeyStroke(KeyType.ReverseTab);
            }
            
            if (keyStr.Contains("-"))
            {
                var segments = new List<string>(keyStr.Substring(1, keyStr.Length - 2).Split('-'));
                if (segments.Count < 2)
                    throw new ArgumentException($"Invalid vim notation: {keyStr}");

                string characterStr = segments[^1];
                segments.RemoveAt(segments.Count - 1);
                
                bool altPressed = false;
                bool ctrlPressed = false;
                
                foreach (string modifier in segments)
                {
                    switch (modifier.ToLowerInvariant())
                    {
                        case "c":
                            ctrlPressed = true;
                            break;
                        case "a":
                            altPressed = true;
                            break;
                        case "s":
                            characterStr = characterStr.ToUpperInvariant();
                            break;
                    }
                }
                
                return new KeyStroke(characterStr[0], ctrlPressed, altPressed);
            }
            
            return keyStrLC switch
            {
                "<esc>" or "<escape>" => new KeyStroke(KeyType.Escape),
                "<cr>" or "<enter>" or "<return>" => new KeyStroke(KeyType.Enter),
                "<bs>" => new KeyStroke(KeyType.Backspace),
                "<tab>" => new KeyStroke(KeyType.Tab),
                "<space>" => new KeyStroke(' ', false, false),
                "<up>" => new KeyStroke(KeyType.ArrowUp),
                "<down>" => new KeyStroke(KeyType.ArrowDown),
                "<left>" => new KeyStroke(KeyType.ArrowLeft),
                "<right>" => new KeyStroke(KeyType.ArrowRight),
                "<insert>" => new KeyStroke(KeyType.Insert),
                "<del>" => new KeyStroke(KeyType.Delete),
                "<home>" => new KeyStroke(KeyType.Home),
                "<end>" => new KeyStroke(KeyType.End),
                "<pageup>" => new KeyStroke(KeyType.PageUp),
                "<pagedown>" => new KeyStroke(KeyType.PageDown),
                "<f1>" => new KeyStroke(KeyType.F1),
                "<f2>" => new KeyStroke(KeyType.F2),
                "<f3>" => new KeyStroke(KeyType.F3),
                "<f4>" => new KeyStroke(KeyType.F4),
                "<f5>" => new KeyStroke(KeyType.F5),
                "<f6>" => new KeyStroke(KeyType.F6),
                "<f7>" => new KeyStroke(KeyType.F7),
                "<f8>" => new KeyStroke(KeyType.F8),
                "<f9>" => new KeyStroke(KeyType.F9),
                "<f10>" => new KeyStroke(KeyType.F10),
                "<f11>" => new KeyStroke(KeyType.F11),
                "<f12>" => new KeyStroke(KeyType.F12),
                _ => throw new ArgumentException($"Invalid vim notation: {keyStr}")
            };
        }
        
        throw new ArgumentException($"Invalid vim notation: {keyStr}");
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("KeyStroke{keytype=").Append(KeyType);
        
        if (Character != null)
        {
            char ch = Character.Value;
            sb.Append(", character='");
            switch (ch)
            {
                case '\0': sb.Append("^@"); break;
                case '\b': sb.Append("\\b"); break;
                case '\t': sb.Append("\\t"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\x1b': sb.Append("^["); break;
                case '\x1c': sb.Append("^\\"); break;
                case '\x1d': sb.Append("^]"); break;
                case '\x1e': sb.Append("^^"); break;
                case '\x1f': sb.Append("^_"); break;
                default:
                    if (ch <= 26)
                        sb.Append('^').Append((char)(ch + 64));
                    else
                        sb.Append(ch);
                    break;
            }
            sb.Append('\'');
        }
        
        if (IsCtrlDown || IsAltDown || IsShiftDown)
        {
            string sep = "";
            sb.Append(", modifiers=[");
            if (IsCtrlDown) { sb.Append(sep).Append("ctrl"); sep = ","; }
            if (IsAltDown) { sb.Append(sep).Append("alt"); sep = ","; }
            if (IsShiftDown) { sb.Append(sep).Append("shift"); }
            sb.Append("]");
        }
        
        return sb.Append('}').ToString();
    }

    public bool Equals(KeyStroke? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return KeyType == other.KeyType &&
               Character == other.Character &&
               IsCtrlDown == other.IsCtrlDown &&
               IsAltDown == other.IsAltDown &&
               IsShiftDown == other.IsShiftDown;
    }

    public override bool Equals(object? obj)
    {
        return obj is KeyStroke other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(KeyType, Character, IsCtrlDown, IsAltDown, IsShiftDown);
    }
}

/// <summary>
/// An F3-KeyStroke that is distinguishable from a CursorLocation report.
/// </summary>
public class RealF3KeyStroke : KeyStroke
{
    public RealF3KeyStroke() : base(KeyType.F3, false, false, false)
    {
    }
}