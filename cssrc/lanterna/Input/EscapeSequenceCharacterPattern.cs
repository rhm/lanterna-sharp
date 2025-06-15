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
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Lanterna.Input
{
    /// <summary>
    /// This implementation of ICharacterPattern matches two similar patterns
    /// of Escape sequences, that many terminals produce for special keys.
    /// 
    /// These sequences all start with Escape, followed by either an open bracket
    /// or a capital letter O (these two are treated as equivalent).
    /// 
    /// Then follows a list of zero or up to two decimals separated by a 
    /// semicolon, and a non-digit last character.
    /// 
    /// If the last character is a tilde (~) then the first number defines
    /// the key (through stdMap), otherwise the last character itself defines
    /// the key (through finMap).
    /// 
    /// The second number, if provided by the terminal, specifies the modifier
    /// state (shift,alt,ctrl). The value is 1 + sum(modifiers), where shift is 1,
    /// alt is 2 and ctrl is 4.
    /// 
    /// The two maps stdMap and finMap can be customized in subclasses to add,
    /// remove or replace keys - to support non-standard Terminals.
    /// 
    /// Examples: (on a gnome terminal)
    /// ArrowUp is "Esc [ A"; Alt-ArrowUp is "Esc [ 1 ; 3 A"
    /// both are handled by finMap mapping 'A' to ArrowUp 
    /// 
    /// F6 is "Esc [ 1 7 ~"; Ctrl-Shift-F6 is "Esc [ 1 7 ; 6 R"
    /// both are handled by stdMap mapping 17 to F6 
    /// </summary>
    public class EscapeSequenceCharacterPattern : ICharacterPattern
    {
        // State machine used to match key sequence:
        private enum State
        {
            Start, Intro, Num1, Num2, Done
        }

        // Bit-values for modifier keys: only used internally
        public const int Shift = 1, Alt = 2, Ctrl = 4;

        /// <summary>
        /// Map of recognized "standard pattern" sequences:
        /// e.g.: 24 -> F12 : "Esc [ 24 ~"
        /// </summary>
        protected readonly Dictionary<int, KeyType> StdMap = new Dictionary<int, KeyType>();

        /// <summary>
        /// Map of recognized "finish pattern" sequences:
        /// e.g.: 'A' -> ArrowUp : "Esc [ A"
        /// </summary>
        protected readonly Dictionary<char, KeyType> FinMap = new Dictionary<char, KeyType>();

        /// <summary>
        /// A flag to control, whether an Esc-prefix for an Esc-sequence is to be treated
        /// as Alt-pressed. Some Terminals (e.g. putty) report the Alt-modifier like that.
        /// If the application is e.g. more interested in seeing separate Escape and plain
        /// Arrow keys, then it should replace this class by a subclass that sets this flag
        /// to false. (It might then also want to remove the CtrlAltAndCharacterPattern.)
        /// </summary>
        protected bool UseEscEsc = true;

        /// <summary>
        /// Create an instance with a standard set of mappings.
        /// </summary>
        public EscapeSequenceCharacterPattern()
        {
            FinMap['A'] = KeyType.ArrowUp;
            FinMap['B'] = KeyType.ArrowDown;
            FinMap['C'] = KeyType.ArrowRight;
            FinMap['D'] = KeyType.ArrowLeft;
            FinMap['E'] = KeyType.KeyType; // gnome-terminal center key on numpad
            FinMap['G'] = KeyType.KeyType; // putty center key on numpad
            FinMap['H'] = KeyType.Home;
            FinMap['F'] = KeyType.End;
            FinMap['P'] = KeyType.F1;
            FinMap['Q'] = KeyType.F2;
            FinMap['R'] = KeyType.F3;
            FinMap['S'] = KeyType.F4;
            FinMap['Z'] = KeyType.ReverseTab;

            StdMap[1] = KeyType.Home;
            StdMap[2] = KeyType.Insert;
            StdMap[3] = KeyType.Delete;
            StdMap[4] = KeyType.End;
            StdMap[5] = KeyType.PageUp;
            StdMap[6] = KeyType.PageDown;
            StdMap[11] = KeyType.F1;
            StdMap[12] = KeyType.F2;
            StdMap[13] = KeyType.F3;
            StdMap[14] = KeyType.F4;
            StdMap[15] = KeyType.F5;
            StdMap[16] = KeyType.F5;
            StdMap[17] = KeyType.F6;
            StdMap[18] = KeyType.F7;
            StdMap[19] = KeyType.F8;
            StdMap[20] = KeyType.F9;
            StdMap[21] = KeyType.F10;
            StdMap[23] = KeyType.F11;
            StdMap[24] = KeyType.F12;
            StdMap[25] = KeyType.F13;
            StdMap[26] = KeyType.F14;
            StdMap[28] = KeyType.F15;
            StdMap[29] = KeyType.F16;
            StdMap[31] = KeyType.F17;
            StdMap[32] = KeyType.F18;
            StdMap[33] = KeyType.F19;
        }

        /// <summary>
        /// Combines a KeyType and modifiers into a KeyStroke.
        /// Subclasses can override this for customization purposes.
        /// </summary>
        /// <param name="key">The KeyType as determined by parsing the sequence.
        /// It will be null, if the pattern looked like a key sequence but wasn't identified.</param>
        /// <param name="mods">The bitmask of the modifier keys pressed along with the key.</param>
        /// <returns>Either null (to report mis-match), or a valid KeyStroke.</returns>
        protected virtual KeyStroke? GetKeyStroke(KeyType? key, int mods)
        {
            bool bShift = false, bCtrl = false, bAlt = false;
            if (key == null) { return null; } // alternative: key = KeyType.Unknown;
            if (mods >= 0) // only use when non-negative!
            {
                bShift = (mods & Shift) != 0;
                bAlt = (mods & Alt) != 0;
                bCtrl = (mods & Ctrl) != 0;
            }
            else if (mods == -1 && key == KeyType.F3)
            {
                return new RealF3KeyStroke();
            }
            return new KeyStroke(key.Value, bCtrl, bAlt, bShift);
        }

        /// <summary>
        /// Combines the raw parts of the sequence into a KeyStroke.
        /// This method does not check the first char, but overrides may do so.
        /// </summary>
        /// <param name="first">The char following after Esc in the sequence (either [ or O)</param>
        /// <param name="num1">The first decimal, or 0 if not in the sequence</param>
        /// <param name="num2">The second decimal, or 0 if not in the sequence</param>
        /// <param name="last">The terminating char.</param>
        /// <param name="bEsc">Whether an extra Escape-prefix was found.</param>
        /// <returns>Either null (to report mis-match), or a valid KeyStroke.</returns>
        protected virtual KeyStroke? GetKeyStrokeRaw(char first, int num1, int num2, char last, bool bEsc)
        {
            KeyType? kt;
            bool bPuttyCtrl = false, bRealF3 = false;
            if (last == '~' && StdMap.ContainsKey(num1))
            {
                kt = StdMap[num1];
            }
            else if (FinMap.ContainsKey(last))
            {
                kt = FinMap[last];
                if (first == 'O')
                {
                    // Putty sends ^[OA for ctrl arrow-up, ^[[A for plain arrow-up:
                    // but only for A-D -- other ^[O... sequences are just plain keys
                    // if we ever stumble into "keypad-mode", then it will end up inverted.
                    if (last >= 'A' && last <= 'D') { bPuttyCtrl = true; }
                    // ^[OR is a "real" F3 Key, ^[[1;1R may be F3 or a CursorLocation report!
                    if (last == 'R') { bRealF3 = true; }
                }
            }
            else
            {
                kt = null; // unknown key.
            }
            int mods = num2 - 1;
            if (bEsc)
            {
                if (mods >= 0) { mods |= Alt; }
                else { mods = Alt; }
            }
            if (bPuttyCtrl)
            {
                if (mods >= 0) { mods |= Ctrl; }
                else { mods = Ctrl; }
            }
            if (bRealF3)
            {
                mods = -1;
            }
            return GetKeyStroke(kt, mods);
        }

        public Matching? Match(IList<char> cur)
        {
            State state = State.Start;
            int num1 = 0, num2 = 0;
            char first = '\0', last = '\0';
            bool bEsc = false;

            foreach (char ch in cur)
            {
                switch (state)
                {
                    case State.Start:
                        if (ch != IKeyDecodingProfile.EscCode)
                        {
                            return null; // nope
                        }
                        state = State.Intro;
                        continue;
                    case State.Intro:
                        // Recognize a second Escape to mean "Alt is pressed".
                        // (at least putty sends it that way)
                        if (UseEscEsc && ch == IKeyDecodingProfile.EscCode && !bEsc)
                        {
                            bEsc = true; 
                            continue;
                        }

                        // Key sequences supported by this class must
                        // start either with Esc-[ or Esc-O
                        if (ch != '[' && ch != 'O')
                        {
                            return null; // nope
                        }
                        first = ch; 
                        state = State.Num1;
                        continue;
                    case State.Num1:
                        if (ch == ';')
                        {
                            state = State.Num2;
                        }
                        else if (char.IsDigit(ch))
                        {
                            num1 = num1 * 10 + (int)char.GetNumericValue(ch);
                        }
                        else
                        {
                            last = ch; 
                            state = State.Done;
                        }
                        continue;
                    case State.Num2:
                        if (char.IsDigit(ch))
                        {
                            num2 = num2 * 10 + (int)char.GetNumericValue(ch);
                        }
                        else
                        {
                            last = ch; 
                            state = State.Done;
                        }
                        continue;
                    case State.Done: // once done, extra characters spoil it
                        return null; // nope
                }
            }
            if (state == State.Done)
            {
                KeyStroke? ks = GetKeyStrokeRaw(first, num1, num2, last, bEsc);
                return ks != null ? new Matching(ks) : null; // depends
            }
            else
            {
                return Matching.NotYet; // maybe later
            }
        }
    }
}