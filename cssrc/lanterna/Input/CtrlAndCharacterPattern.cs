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
    /// Character pattern that matches characters pressed while CTRL key is held down
    /// </summary>
    public class CtrlAndCharacterPattern : ICharacterPattern
    {
        public Matching? Match(IList<char> seq)
        {
            int size = seq.Count;
            char ch = seq[0];
            if (size != 1)
            {
                return null; // nope
            }
            if (ch < 32)
            {
                // Control-chars: exclude lf,cr,Tab,Esc(^[), but still include ^\, ^], ^^ and ^_
                char ctrlCode;
                switch (ch)
                {
                    case '\n':
                    case '\r':
                    case '\t':
                    case (char)0x08:
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
                var ks = new KeyStroke(ctrlCode, true, false);
                return new Matching(ks); // yep
            }
            else
            {
                return null; // nope
            }
        }
    }
}