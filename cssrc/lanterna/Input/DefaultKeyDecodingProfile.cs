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

namespace Lanterna.Input
{
    /// <summary>
    /// This profile attempts to collect as many code combinations as possible without causing any collisions between 
    /// patterns. The patterns in here are tested with Linux terminal, XTerm, Gnome terminal, XFCE terminal, Cygwin and 
    /// Mac OS X terminal.
    /// </summary>
    public class DefaultKeyDecodingProfile : IKeyDecodingProfile
    {
        private static readonly List<ICharacterPattern> CommonPatterns = new List<ICharacterPattern>
        {
            new BasicCharacterPattern(new KeyStroke(KeyType.Escape), IKeyDecodingProfile.EscCode),
            new BasicCharacterPattern(new KeyStroke(KeyType.Tab), '\t'),
            new BasicCharacterPattern(new KeyStroke(KeyType.Enter), '\n'),
            new BasicCharacterPattern(new KeyStroke(KeyType.Enter), '\r', '\u0000'), //OS X
            new BasicCharacterPattern(new KeyStroke(KeyType.Backspace), (char) 0x7f),
            new BasicCharacterPattern(new KeyStroke(KeyType.Backspace), (char) 0x08),
            new BasicCharacterPattern(new KeyStroke(KeyType.F1), IKeyDecodingProfile.EscCode, '[', '[', 'A'), //Linux
            new BasicCharacterPattern(new KeyStroke(KeyType.F2), IKeyDecodingProfile.EscCode, '[', '[', 'B'), //Linux
            new BasicCharacterPattern(new KeyStroke(KeyType.F3), IKeyDecodingProfile.EscCode, '[', '[', 'C'), //Linux
            new BasicCharacterPattern(new KeyStroke(KeyType.F4), IKeyDecodingProfile.EscCode, '[', '[', 'D'), //Linux
            new BasicCharacterPattern(new KeyStroke(KeyType.F5), IKeyDecodingProfile.EscCode, '[', '[', 'E'), //Linux

            // Note: These patterns are referenced in the Java version but not included in this initial port
            // They would need to be implemented as separate classes:
            // new EscapeSequenceCharacterPattern(),
            new NormalCharacterPattern(),
            // new AltAndCharacterPattern(),
            // new CtrlAndCharacterPattern(),
            // new CtrlAltAndCharacterPattern(),
            // new ScreenInfoCharacterPattern(),
            // new MouseCharacterPattern()
        };

        public ICollection<ICharacterPattern> GetPatterns()
        {
            return new List<ICharacterPattern>(CommonPatterns);
        }
    }
}