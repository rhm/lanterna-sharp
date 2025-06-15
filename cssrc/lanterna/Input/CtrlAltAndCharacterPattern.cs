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
    /// Character pattern that matches characters pressed while ALT and CTRL keys are held down
    /// </summary>
    public class CtrlAltAndCharacterPattern : ICharacterPattern
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
            char ch = seq[1];
            if (ch < 32 && ch != 0x08)
            {
                // Control-chars: exclude Esc(^[), but still include ^\, ^], ^^ and ^_
                char ctrlCode;
                switch (ch)
                {
                    case IKeyDecodingProfile.EscCode:
                        return null; // nope
                    case (char)0:  /* ^@ */ 
                        ctrlCode = ' '; 
                        break;
                    case (char)28: /* ^\ */ 
                        ctrlCode = '\\'; 
                        break;
                    case (char)29: /* ^] */ 
                        ctrlCode = ']'; 
                        break;
                    case (char)30: /* ^^ */ 
                        ctrlCode = '^'; 
                        break;
                    case (char)31: /* ^_ */ 
                        ctrlCode = '_'; 
                        break;
                    default: 
                        ctrlCode = (char)('a' - 1 + ch);
                        break;
                }
                var ks = new KeyStroke(ctrlCode, true, true);
                return new Matching(ks); // yep
            }
            else if (ch == 0x7f || ch == 0x08)
            {
                var ks = new KeyStroke(KeyType.Backspace, false, true);
                return new Matching(ks); // yep
            }
            else
            {
                return null; // nope
            }
        }
    }
}