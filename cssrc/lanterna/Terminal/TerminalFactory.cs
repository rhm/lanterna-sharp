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

using System.IO;

namespace Lanterna.Terminal
{
    /// <summary>
    /// This interface is for abstracting the creation of your Terminal object. The bundled implementation is 
    /// DefaultTerminalFactory, which will use a simple auto-detection mechanism for figuring out which terminal 
    /// implementation to create based on characteristics of the system the program is running on.
    /// </summary>
    public interface TerminalFactory
    {
        /// <summary>
        /// Instantiates a Terminal according to the factory implementation.
        /// </summary>
        /// <returns>Terminal implementation</returns>
        /// <exception cref="IOException">If there was an I/O error with the underlying input/output system</exception>
        ITerminal CreateTerminal();
    }
}