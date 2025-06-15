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
using System.Text;
using Lanterna.Graphics;

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

    /// <summary>
    /// Given a string that may or may not contain CJK characters, returns the substring which will fit inside
    /// availableColumnSpace columns. This method does not handle special cases like tab or new-line.
    /// Calling this method is the same as calling FitString(str, 0, availableColumnSpace).
    /// </summary>
    /// <param name="str">The string to fit inside the availableColumnSpace</param>
    /// <param name="availableColumnSpace">Number of columns to fit the string inside</param>
    /// <returns>The whole or part of the input string which will fit inside the supplied availableColumnSpace</returns>
    public static string FitString(string str, int availableColumnSpace)
    {
        return FitString(str, 0, availableColumnSpace);
    }

    /// <summary>
    /// Given a string that may or may not contain CJK characters, returns the substring which will fit inside
    /// availableColumnSpace columns. This method does not handle special cases like tab or new-line.
    /// <para>
    /// This overload has a fromColumn parameter that specified where inside the string to start fitting. Please
    /// notice that fromColumn is not a character index inside the string, but a column index as if the string
    /// has been printed from the left-most side of the terminal. So if the string is "日本語", fromColumn set to 1 will
    /// not starting counting from the second character ("本") in the string but from the CJK filler character belonging
    /// to "日". If you want to count from a particular character index inside the string, please pass in a substring
    /// and use fromColumn set to 0.
    /// </para>
    /// </summary>
    /// <param name="str">The string to fit inside the availableColumnSpace</param>
    /// <param name="fromColumn">From what column of the input string to start fitting (see description above!)</param>
    /// <param name="availableColumnSpace">Number of columns to fit the string inside</param>
    /// <returns>The whole or part of the input string which will fit inside the supplied availableColumnSpace</returns>
    public static string FitString(string str, int fromColumn, int availableColumnSpace)
    {
        if (availableColumnSpace <= 0)
        {
            return "";
        }

        var result = new StringBuilder();
        int column = 0;
        int index = 0;
        while (index < str.Length && column < fromColumn)
        {
            char c = str[index++];
            column += IsCharCJK(c) ? 2 : 1;
        }
        if (column > fromColumn)
        {
            result.Append(" ");
            availableColumnSpace--;
        }

        while (availableColumnSpace > 0 && index < str.Length)
        {
            char c = str[index++];
            availableColumnSpace -= IsCharCJK(c) ? 2 : 1;
            if (availableColumnSpace < 0)
            {
                result.Append(' ');
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Given a string and an index in that string, returns the complete ANSI control sequence starting at that position.
    /// If there is no control sequence starting there, the method will return null.
    /// </summary>
    /// <param name="text">String to scan for control sequences</param>
    /// <param name="index">Index in the string where the control sequence begins</param>
    /// <returns>null if there was no control sequence starting at the specified index, otherwise the entire control sequence</returns>
    public static string? GetANSIControlSequenceAt(string text, int index)
    {
        int len = GetANSIControlSequenceLength(text, index);
        return len == 0 ? null : text.Substring(index, len);
    }

    /// <summary>
    /// Given a string and an index in that string, returns the number of characters starting at index that make up
    /// a complete ANSI control sequence. If there is no control sequence starting there, the method will return 0.
    /// </summary>
    /// <param name="text">String to scan for control sequences</param>
    /// <param name="index">Index in the string where the control sequence begins</param>
    /// <returns>0 if there was no control sequence starting at the specified index, otherwise the length of the entire control sequence</returns>
    public static int GetANSIControlSequenceLength(string text, int index)
    {
        int len = 0;
        int restlen = text.Length - index;
        
        if (restlen >= 3) // Control sequences require a minimum of three characters
        {
            char esc = text[index];
            char bracket = text[index + 1];
            
            if (esc == 0x1B && bracket == '[') // escape & open bracket
            {
                len = 3; // esc, bracket and (later) terminator
                
                // digits or semicolons can still precede the terminator:
                for (int i = 2; i < restlen; i++)
                {
                    char ch = text[i + index];
                    // only ascii-digits or semicolons allowed here:
                    if ((ch >= '0' && ch <= '9') || ch == ';')
                    {
                        len++;
                    }
                    else
                    {
                        break;
                    }
                }
                
                // if string ends in digits/semicolons, then it's not a sequence.
                if (len > restlen)
                {
                    len = 0;
                }
            }
        }
        
        return len;
    }

    /// <summary>
    /// Updates style modifiers from a CSI (Control Sequence Introducer) code
    /// </summary>
    /// <param name="controlSequence">The ANSI control sequence to parse</param>
    /// <param name="target">The target style set to update</param>
    /// <param name="original">The original style set to restore from when reset codes are encountered</param>
    public static void UpdateModifiersFromCSICode<T>(string controlSequence, IStyleSet<T> target, IStyleSet original) where T : IStyleSet<T>
    {
        char controlCodeType = controlSequence[controlSequence.Length - 1];
        string codeString = controlSequence.Substring(2, controlSequence.Length - 3);
        var codes = MapCodesToIntArray(codeString.Split(';'));

        var palette = new ITextColor[]
        {
            new AnsiTextColor(AnsiColor.Black),
            new AnsiTextColor(AnsiColor.Red),
            new AnsiTextColor(AnsiColor.Green),
            new AnsiTextColor(AnsiColor.Yellow),
            new AnsiTextColor(AnsiColor.Blue),
            new AnsiTextColor(AnsiColor.Magenta),
            new AnsiTextColor(AnsiColor.Cyan),
            new AnsiTextColor(AnsiColor.White)
        };

        if (controlCodeType == 'm') // SGRs
        {
            for (int i = 0; i < codes.Length; i++)
            {
                int code = codes[i];
                switch (code)
                {
                    case 0:
                        target.SetStyleFrom(original);
                        break;
                    case 1:
                        target.EnableModifiers(SGR.Bold);
                        break;
                    case 3:
                        target.EnableModifiers(SGR.Italic);
                        break;
                    case 4:
                        target.EnableModifiers(SGR.Underline);
                        break;
                    case 5:
                        target.EnableModifiers(SGR.Blink);
                        break;
                    case 7:
                        target.EnableModifiers(SGR.Reverse);
                        break;
                    case 21: // both do. 21 seems more straightforward.
                    case 22:
                        target.DisableModifiers(SGR.Bold);
                        break;
                    case 23:
                        target.DisableModifiers(SGR.Italic);
                        break;
                    case 24:
                        target.DisableModifiers(SGR.Underline);
                        break;
                    case 25:
                        target.DisableModifiers(SGR.Blink);
                        break;
                    case 27:
                        target.DisableModifiers(SGR.Reverse);
                        break;
                    case 38:
                        if (i + 2 < codes.Length && codes[i + 1] == 5)
                        {
                            target.SetForegroundColor(new IndexedColor(codes[i + 2]));
                            i += 2;
                        }
                        else if (i + 4 < codes.Length && codes[i + 1] == 2)
                        {
                            target.SetForegroundColor(new RgbColor(codes[i + 2], codes[i + 3], codes[i + 4]));
                            i += 4;
                        }
                        break;
                    case 39:
                        target.SetForegroundColor(original.ForegroundColor);
                        break;
                    case 48:
                        if (i + 2 < codes.Length && codes[i + 1] == 5)
                        {
                            target.SetBackgroundColor(new IndexedColor(codes[i + 2]));
                            i += 2;
                        }
                        else if (i + 4 < codes.Length && codes[i + 1] == 2)
                        {
                            target.SetBackgroundColor(new RgbColor(codes[i + 2], codes[i + 3], codes[i + 4]));
                            i += 4;
                        }
                        break;
                    case 49:
                        target.SetBackgroundColor(original.BackgroundColor);
                        break;
                    default:
                        if (code >= 30 && code <= 37)
                        {
                            target.SetForegroundColor(palette[code - 30]);
                        }
                        else if (code >= 40 && code <= 47)
                        {
                            target.SetBackgroundColor(palette[code - 40]);
                        }
                        break;
                }
            }
        }
    }

    private static int[] MapCodesToIntArray(string[] codes)
    {
        var result = new int[codes.Length];
        for (int i = 0; i < result.Length; i++)
        {
            if (string.IsNullOrEmpty(codes[i]))
            {
                result[i] = 0;
            }
            else
            {
                if (!int.TryParse(codes[i], out result[i]))
                {
                    throw new ArgumentException($"Unknown CSI code {codes[i]}");
                }
            }
        }
        return result;
    }
}