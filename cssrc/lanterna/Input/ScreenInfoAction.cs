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
    /// ScreenInfoAction, a KeyStroke in disguise, this class contains the reported position of the screen cursor.
    /// </summary>
    public class ScreenInfoAction : KeyStroke
    {
        /// <summary>
        /// The location of the cursor when this event was generated.
        /// </summary>
        public TerminalPosition Position { get; }

        /// <summary>
        /// Constructs a ScreenInfoAction based on a location on the screen
        /// </summary>
        /// <param name="position">The TerminalPosition reported from terminal</param>
        public ScreenInfoAction(TerminalPosition position) : base(KeyType.CursorLocation)
        {
            Position = position;
        }

        public override string ToString()
        {
            return $"ScreenInfoAction{{position={Position}}}";
        }
    }
}