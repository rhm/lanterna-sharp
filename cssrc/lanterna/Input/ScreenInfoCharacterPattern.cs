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
using Lanterna.Core;

namespace Lanterna.Input
{
    /// <summary>
    /// This class recognizes character combinations which are actually a cursor position report. See
    /// <a href="http://en.wikipedia.org/wiki/ANSI_escape_code">Wikipedia</a>'s article on ANSI escape codes for more
    /// information about how cursor position reporting works ("DSR – Device Status Report").
    /// </summary>
    public class ScreenInfoCharacterPattern : EscapeSequenceCharacterPattern
    {
        public ScreenInfoCharacterPattern()
        {
            UseEscEsc = false; // stdMap and finMap don't matter here.
        }

        protected override KeyStroke? GetKeyStrokeRaw(char first, int num1, int num2, char last, bool bEsc)
        {
            if (first != '[' || last != 'R' || num1 == 0 || num2 == 0 || bEsc)
            {
                return null; // nope
            }
            if (num1 == 1 && num2 <= 8)
            {
                return null; // nope: much more likely it's an F3 with modifiers
            }
            var pos = new TerminalPosition(num2, num1);
            return new ScreenInfoAction(pos); // yep
        }

        public static ScreenInfoAction? TryToAdopt(KeyStroke? ks)
        {
            if (ks == null)
            {
                return null;
            }
            switch (ks.KeyType)
            {
                case KeyType.CursorLocation:
                    return (ScreenInfoAction)ks;
                case KeyType.F3: // reconstruct position from F3's modifiers.
                    if (ks is RealF3KeyStroke) { return null; }
                    int col = 1 + (ks.IsAltDown ? Alt : 0)
                              + (ks.IsCtrlDown ? Ctrl : 0)
                              + (ks.IsShiftDown ? Shift : 0);
                    var pos = new TerminalPosition(col, 1);
                    return new ScreenInfoAction(pos);
                default:
                    return null;
            }
        }
    }
}