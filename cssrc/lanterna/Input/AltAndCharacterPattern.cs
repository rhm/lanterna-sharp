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

namespace Lanterna.Input
{
    /// <summary>
    /// Character pattern that matches characters pressed while ALT key is held down
    /// </summary>
    public class AltAndCharacterPattern : ICharacterPattern
    {
        public Matching? Match(IList<char> seq)
        {
            int size = seq.Count;
            if (size > 2 || seq[0] != IKeyDecodingProfile.EscCode)
            {
                return null; // nope
            }
            if (size == 1)
            {
                return Matching.NotYet; // maybe later
            }
            if (char.IsControl(seq[1]))
            {
                return null; // nope
            }
            var ks = new KeyStroke(seq[1], false, true);
            return new Matching(ks); // yep
        }
    }
}