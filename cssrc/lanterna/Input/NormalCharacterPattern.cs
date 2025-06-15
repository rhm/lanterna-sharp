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
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Lanterna.Input
{
    /// <summary>
    /// Character pattern that matches one character as one KeyStroke with the character that was read
    /// </summary>
    public class NormalCharacterPattern : ICharacterPattern
    {
        public Matching? Match(IList<char> seq)
        {
            if (seq.Count != 1)
            {
                return null; // nope
            }
            char ch = seq[0];
            if (IsPrintableChar(ch))
            {
                var ks = new KeyStroke(ch, false, false);
                return new Matching(ks);
            }
            else
            {
                return null; // nope
            }
        }

        /// <summary>
        /// From http://stackoverflow.com/questions/220547/printable-char-in-java
        /// </summary>
        /// <param name="c">character to test</param>
        /// <returns>True if this is a 'normal', printable character, false otherwise</returns>
        private static bool IsPrintableChar(char c)
        {
            if (char.IsControl(c)) { return false; }
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            return category != UnicodeCategory.OtherNotAssigned &&
                   category != UnicodeCategory.Format &&
                   category != UnicodeCategory.Control;
        }
    }
}