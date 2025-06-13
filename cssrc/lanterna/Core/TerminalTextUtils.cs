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
 * Copyright (C) 2010-2020 Martin Berglund
 */

using System.Globalization;

namespace Lanterna.Core;

/// <summary>
/// This class contains a number of utility methods for analyzing characters and strings in a terminal context. The main
/// purpose is to make it easier to work with text that may or may not contain double-width text characters, such as CJK
/// (Chinese, Japanese, Korean) and other special symbols.
/// </summary>
public static class TerminalTextUtils
{
    /// <summary>
    /// Given a character, is this character considered to be a CJK character?
    /// </summary>
    /// <param name="c">Character to test</param>
    /// <returns>true if the character is a CJK character</returns>
    public static bool IsCharCJK(char c)
    {
        var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
        var unicodeBlock = char.GetUnicodeCategory(c);
        
        // Check for common CJK Unicode blocks
        return c >= 0x1100 && c <= 0x11FF ||   // Hangul Jamo
               c >= 0x3040 && c <= 0x309F ||   // Hiragana
               c >= 0x30A0 && c <= 0x30FF ||   // Katakana
               c >= 0x3130 && c <= 0x318F ||   // Hangul Compatibility Jamo
               c >= 0x3200 && c <= 0x32FF ||   // Enclosed CJK Letters and Months
               c >= 0x3400 && c <= 0x4DBF ||   // CJK Unified Ideographs Extension A
               c >= 0x4E00 && c <= 0x9FFF ||   // CJK Unified Ideographs
               c >= 0xAC00 && c <= 0xD7AF ||   // Hangul Syllables
               c >= 0xF900 && c <= 0xFAFF ||   // CJK Compatibility Ideographs
               c >= 0xFE30 && c <= 0xFE4F ||   // CJK Compatibility Forms
               (c >= 0xFF00 && c <= 0xFF60);   // Fullwidth Forms (excluding halfwidth)
    }

    /// <summary>
    /// Given a character, is this character considered to be a Thai character?
    /// </summary>
    /// <param name="c">Character to test</param>
    /// <returns>true if the character is a Thai character</returns>
    public static bool IsCharThai(char c)
    {
        return c >= 0x0E00 && c <= 0x0E7F; // Thai Unicode block
    }

    /// <summary>
    /// Checks if a character is expected to be taking up two columns if printed to a terminal. This will generally be
    /// true for CJK (Chinese, Japanese and Korean) characters.
    /// </summary>
    /// <param name="c">Character to test if it's double-width when printed to a terminal</param>
    /// <returns>true if this character is expected to be taking up two columns when printed to the terminal</returns>
    public static bool IsCharDoubleWidth(char c)
    {
        return IsCharCJK(c);
    }

    /// <summary>
    /// Checks if a particular character is a control character, in Lanterna this currently means it's 0-31 or 127 in the
    /// ascii table.
    /// </summary>
    /// <param name="c">character to test</param>
    /// <returns>true if the character is a control character, false otherwise</returns>
    public static bool IsControlCharacter(char c)
    {
        return c < 32 || c == 127;
    }

    /// <summary>
    /// Checks if a particular character is printable. This generally means that the code is not a control character that
    /// isn't able to be printed to the terminal properly. However, certain control characters have a particular meaning to
    /// the terminal and are as such considered printable. In Lanterna, we consider these control characters printable:
    /// Backspace, Horizontal Tab, Line feed
    /// </summary>
    /// <param name="c">character to test</param>
    /// <returns>true if the character is considered printable, false otherwise</returns>
    public static bool IsPrintableCharacter(char c)
    {
        return !IsControlCharacter(c) || c == '\t' || c == '\n' || c == '\b';
    }

    /// <summary>
    /// Given a string, returns how many columns this string would need to occupy in a terminal, taking into account that
    /// CJK characters takes up two columns.
    /// </summary>
    /// <param name="s">String to check length</param>
    /// <returns>Number of actual terminal columns the string would occupy</returns>
    public static int GetColumnWidth(string s)
    {
        if (string.IsNullOrEmpty(s))
            return 0;

        int width = 0;
        foreach (char c in s)
        {
            if (c == '\t')
                width += 4; // Tab counts as 4 spaces
            else if (IsCharDoubleWidth(c))
                width += 2;
            else if (IsPrintableCharacter(c))
                width += 1;
            // Control characters don't add width
        }
        return width;
    }
}