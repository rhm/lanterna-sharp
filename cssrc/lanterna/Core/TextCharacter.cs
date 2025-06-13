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

using System.Globalization;

namespace Lanterna.Core;

/// <summary>
/// Represents a single character with additional metadata such as colors and modifiers. This class is immutable and
/// cannot be modified after creation.
/// </summary>
public class TextCharacter : IEquatable<TextCharacter>
{
    public static readonly TextCharacter DefaultCharacter = new(" ", new AnsiTextColor(AnsiColor.Default), new AnsiTextColor(AnsiColor.Default), new HashSet<SGR>());

    private readonly string _character;
    private readonly ITextColor _foregroundColor;
    private readonly ITextColor _backgroundColor;
    private readonly HashSet<SGR> _modifiers;

    /// <summary>
    /// Creates TextCharacter array from a single character
    /// </summary>
    /// <param name="c">Character to convert</param>
    /// <returns>Array containing a single TextCharacter</returns>
    public static TextCharacter[] FromCharacter(char c)
    {
        return FromString(c.ToString());
    }

    /// <summary>
    /// Creates TextCharacter array from a string using default colors
    /// </summary>
    /// <param name="str">String to convert</param>
    /// <returns>Array of TextCharacters</returns>
    public static TextCharacter[] FromString(string str)
    {
        return FromString(str, new AnsiTextColor(AnsiColor.Default), new AnsiTextColor(AnsiColor.Default));
    }

    /// <summary>
    /// Creates a single TextCharacter from a character with specified colors and modifiers
    /// </summary>
    /// <param name="c">Character</param>
    /// <param name="foregroundColor">Foreground color</param>
    /// <param name="backgroundColor">Background color</param>
    /// <param name="modifiers">SGR modifiers</param>
    /// <returns>TextCharacter instance</returns>
    public static TextCharacter FromCharacter(char c, ITextColor foregroundColor, ITextColor backgroundColor, params SGR[] modifiers)
    {
        return FromString(c.ToString(), foregroundColor, backgroundColor, modifiers)[0];
    }

    /// <summary>
    /// Creates TextCharacter array from a string with specified colors and modifiers
    /// </summary>
    /// <param name="str">String to convert</param>
    /// <param name="foregroundColor">Foreground color</param>
    /// <param name="backgroundColor">Background color</param>
    /// <param name="modifiers">SGR modifiers</param>
    /// <returns>Array of TextCharacters</returns>
    public static TextCharacter[] FromString(string str, ITextColor foregroundColor, ITextColor backgroundColor, params SGR[] modifiers)
    {
        return FromString(str, foregroundColor, backgroundColor, new HashSet<SGR>(modifiers));
    }

    /// <summary>
    /// Creates TextCharacter array from a string with specified colors and modifiers
    /// </summary>
    /// <param name="str">String to convert</param>
    /// <param name="foregroundColor">Foreground color</param>
    /// <param name="backgroundColor">Background color</param>
    /// <param name="modifiers">Set of SGR modifiers</param>
    /// <returns>Array of TextCharacters</returns>
    public static TextCharacter[] FromString(string str, ITextColor foregroundColor, ITextColor backgroundColor, HashSet<SGR> modifiers)
    {
        if (string.IsNullOrEmpty(str))
            return Array.Empty<TextCharacter>();

        var result = new List<TextCharacter>();
        var enumerator = StringInfo.GetTextElementEnumerator(str);
        
        while (enumerator.MoveNext())
        {
            string textElement = enumerator.GetTextElement();
            result.Add(new TextCharacter(textElement, foregroundColor, backgroundColor, modifiers));
        }
        
        return result.ToArray();
    }

    /// <summary>
    /// Creates a new TextCharacter based on a physical character, color information and a set of modifiers.
    /// </summary>
    /// <param name="character">Physical character to refer to</param>
    /// <param name="foregroundColor">Foreground color the character has</param>
    /// <param name="backgroundColor">Background color the character has</param>
    /// <param name="modifiers">Set of modifiers to apply when drawing the character</param>
    private TextCharacter(string character, ITextColor foregroundColor, ITextColor backgroundColor, HashSet<SGR> modifiers)
    {
        if (string.IsNullOrEmpty(character))
            throw new ArgumentException("Cannot create TextCharacter from an empty string");

        ValidateSingleCharacter(character);

        // Intern the string to save memory for common characters
        _character = string.Intern(character);
        char firstCharacter = character[0];

        // Don't allow creating a TextCharacter containing a control character
        // For backward-compatibility, do allow tab for now
        if (TerminalTextUtils.IsControlCharacter(firstCharacter) && firstCharacter != '\t')
            throw new ArgumentException($"Cannot create a TextCharacter from a control character (0x{(int)firstCharacter:X})");

        _foregroundColor = foregroundColor ?? new AnsiTextColor(AnsiColor.Default);
        _backgroundColor = backgroundColor ?? new AnsiTextColor(AnsiColor.Default);
        _modifiers = new HashSet<SGR>(modifiers);
    }

    private static void ValidateSingleCharacter(string character)
    {
        var enumerator = StringInfo.GetTextElementEnumerator(character);
        int count = 0;
        while (enumerator.MoveNext())
        {
            count++;
            if (count > 1)
                throw new ArgumentException("Invalid String for TextCharacter, can only have one logical character");
        }
    }

    /// <summary>
    /// Checks if this TextCharacter represents the specified character
    /// </summary>
    /// <param name="otherCharacter">Character to compare against</param>
    /// <returns>true if this TextCharacter represents the same character</returns>
    public bool Is(char otherCharacter)
    {
        return otherCharacter == _character[0] && _character.Length == 1;
    }

    /// <summary>
    /// The actual character this TextCharacter represents (deprecated - may not work with emoji)
    /// </summary>
    [Obsolete("This won't work with advanced characters like emoji, use CharacterString instead")]
    public char Character => _character[0];

    /// <summary>
    /// Returns the character this TextCharacter represents as a String
    /// </summary>
    public string CharacterString => _character;

    /// <summary>
    /// Foreground color specified for this TextCharacter
    /// </summary>
    public ITextColor ForegroundColor => _foregroundColor;

    /// <summary>
    /// Background color specified for this TextCharacter
    /// </summary>
    public ITextColor BackgroundColor => _backgroundColor;

    /// <summary>
    /// Returns a set of all active modifiers on this TextCharacter
    /// </summary>
    public HashSet<SGR> Modifiers => new(_modifiers);

    /// <summary>
    /// Returns true if this TextCharacter has the bold modifier active
    /// </summary>
    public bool IsBold => _modifiers.Contains(SGR.Bold);

    /// <summary>
    /// Returns true if this TextCharacter has the reverse modifier active
    /// </summary>
    public bool IsReversed => _modifiers.Contains(SGR.Reverse);

    /// <summary>
    /// Returns true if this TextCharacter has the underline modifier active
    /// </summary>
    public bool IsUnderlined => _modifiers.Contains(SGR.Underline);

    /// <summary>
    /// Returns true if this TextCharacter has the blink modifier active
    /// </summary>
    public bool IsBlinking => _modifiers.Contains(SGR.Blink);

    /// <summary>
    /// Returns true if this TextCharacter has the bordered modifier active
    /// </summary>
    public bool IsBordered => _modifiers.Contains(SGR.Bordered);

    /// <summary>
    /// Returns true if this TextCharacter has the crossed-out modifier active
    /// </summary>
    public bool IsCrossedOut => _modifiers.Contains(SGR.CrossedOut);

    /// <summary>
    /// Returns true if this TextCharacter has the italic modifier active
    /// </summary>
    public bool IsItalic => _modifiers.Contains(SGR.Italic);

    /// <summary>
    /// Returns a new TextCharacter with the same colors and modifiers but a different underlying character
    /// </summary>
    /// <param name="character">Character the copy should have</param>
    /// <returns>Copy of this TextCharacter with different underlying character</returns>
    public TextCharacter WithCharacter(char character)
    {
        if (_character.Equals(character.ToString()))
            return this;
        return new TextCharacter(character.ToString(), _foregroundColor, _backgroundColor, _modifiers);
    }

    /// <summary>
    /// Returns a copy of this TextCharacter with a specified foreground color
    /// </summary>
    /// <param name="foregroundColor">Foreground color the copy should have</param>
    /// <returns>Copy of the TextCharacter with a different foreground color</returns>
    public TextCharacter WithForegroundColor(ITextColor foregroundColor)
    {
        if (_foregroundColor == foregroundColor || _foregroundColor.Equals(foregroundColor))
            return this;
        return new TextCharacter(_character, foregroundColor, _backgroundColor, _modifiers);
    }

    /// <summary>
    /// Returns a copy of this TextCharacter with a specified background color
    /// </summary>
    /// <param name="backgroundColor">Background color the copy should have</param>
    /// <returns>Copy of the TextCharacter with a different background color</returns>
    public TextCharacter WithBackgroundColor(ITextColor backgroundColor)
    {
        if (_backgroundColor == backgroundColor || _backgroundColor.Equals(backgroundColor))
            return this;
        return new TextCharacter(_character, _foregroundColor, backgroundColor, _modifiers);
    }

    /// <summary>
    /// Returns a copy of this TextCharacter with specified list of SGR modifiers. None of the currently active SGR codes
    /// will be carried over to the copy, only those in the passed in value.
    /// </summary>
    /// <param name="modifiers">SGR modifiers the copy should have</param>
    /// <returns>Copy of the TextCharacter with a different set of SGR modifiers</returns>
    public TextCharacter WithModifiers(IEnumerable<SGR> modifiers)
    {
        var newSet = new HashSet<SGR>(modifiers);
        if (_modifiers.SetEquals(newSet))
            return this;
        return new TextCharacter(_character, _foregroundColor, _backgroundColor, newSet);
    }

    /// <summary>
    /// Returns a copy of this TextCharacter with an additional SGR modifier. All of the currently active SGR codes
    /// will be carried over to the copy, in addition to the one specified.
    /// </summary>
    /// <param name="modifier">SGR modifier the copy should have in addition to all currently present</param>
    /// <returns>Copy of the TextCharacter with a new SGR modifier</returns>
    public TextCharacter WithModifier(SGR modifier)
    {
        if (_modifiers.Contains(modifier))
            return this;
        var newSet = new HashSet<SGR>(_modifiers) { modifier };
        return new TextCharacter(_character, _foregroundColor, _backgroundColor, newSet);
    }

    /// <summary>
    /// Returns a copy of this TextCharacter with an SGR modifier removed. All of the currently active SGR codes
    /// will be carried over to the copy, except for the one specified. If the current TextCharacter doesn't have the
    /// SGR specified, it will return itself.
    /// </summary>
    /// <param name="modifier">SGR modifier the copy should not have</param>
    /// <returns>Copy of the TextCharacter without the SGR modifier</returns>
    public TextCharacter WithoutModifier(SGR modifier)
    {
        if (!_modifiers.Contains(modifier))
            return this;
        var newSet = new HashSet<SGR>(_modifiers);
        newSet.Remove(modifier);
        return new TextCharacter(_character, _foregroundColor, _backgroundColor, newSet);
    }

    /// <summary>
    /// Checks if this character is expected to take up two columns when rendered
    /// </summary>
    public bool IsDoubleWidth
    {
        get
        {
            // TODO: make this better to work properly with emoji and other complicated "characters"
            return TerminalTextUtils.IsCharDoubleWidth(_character[0]) ||
                   IsEmoji(_character) ||
                   // If the character takes up more than one char, assume it's double width (unless Thai)
                   (_character.Length > 1 && !TerminalTextUtils.IsCharThai(_character[0]));
        }
    }

    private static bool IsEmoji(string s)
    {
        // This is really hard to do properly and would require an emoji library as a dependency, so here's a hack that
        // basically assumes anything NOT a regular latin1/CJK/thai character is an emoji
        char firstCharacter = s[0];
        return s.Length > 1 ||
               !(TerminalTextUtils.IsCharCJK(firstCharacter) ||
                 TerminalTextUtils.IsPrintableCharacter(firstCharacter) ||
                 TerminalTextUtils.IsCharThai(firstCharacter) ||
                 TerminalTextUtils.IsControlCharacter(firstCharacter));
    }

    public bool Equals(TextCharacter? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return _character == other._character &&
               Equals(_foregroundColor, other._foregroundColor) &&
               Equals(_backgroundColor, other._backgroundColor) &&
               _modifiers.SetEquals(other._modifiers);
    }

    public override bool Equals(object? obj)
    {
        return obj is TextCharacter other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            _character,
            _foregroundColor,
            _backgroundColor,
            _modifiers.Count > 0 ? string.Join(",", _modifiers.OrderBy(x => x).Select(x => x.ToString())) : string.Empty);
    }

    public override string ToString()
    {
        return $"TextCharacter{{character={_character}, foregroundColor={_foregroundColor}, backgroundColor={_backgroundColor}, modifiers=[{string.Join(", ", _modifiers)}]}}";
    }
}