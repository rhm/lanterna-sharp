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
using System.Collections.Generic;

namespace Lanterna.Input
{
    /// <summary>
    /// In order to convert a stream of characters into objects representing keystrokes, we need to apply logic on this
    /// stream to detect special characters. In lanterna, this is done by using a set of character patterns which are matched
    /// against the stream until we've found the best match. This interface represents a set of such patterns, a 'profile' 
    /// with is used when decoding the input. There is a default profile, DefaultKeyDecodingProfile, which will probably
    /// do what you need but you can also extend and define your own patterns.
    /// </summary>
    public interface IKeyDecodingProfile
    {
        /// <summary>
        /// Static constant for the ESC key
        /// </summary>
        const char EscCode = (char) 0x1b;

        /// <summary>
        /// Returns a collection of character patterns that makes up this profile
        /// </summary>
        /// <returns>Collection of patterns in this profile</returns>
        ICollection<ICharacterPattern> GetPatterns();
    }
}