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

using Lanterna.Core;

namespace Lanterna.Terminal.Ansi
{
    /// <summary>
    /// Using this terminal size provider, your terminal will be set to a fixed size and will never receive any resize
    /// events. Of course if the physical terminal is resized, in reality it will have a different size, but the application
    /// won't know about it. The size reported to the user is always the size attached to this object.
    /// </summary>
    public class FixedTerminalSizeProvider : IUnixTerminalSizeQuerier
    {
        private readonly TerminalSize _size;

        /// <summary>
        /// Creating a FixedTerminalSizeProvider set to a particular size that it will always report whenever the
        /// associated Terminal interface queries.
        /// </summary>
        /// <param name="size">Size the terminal should be statically initialized to</param>
        public FixedTerminalSizeProvider(TerminalSize size)
        {
            _size = size;
        }

        public TerminalSize QueryTerminalSize()
        {
            return _size;
        }
    }
}