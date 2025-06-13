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

namespace Lanterna.Input;

/// <summary>
/// Objects implementing this interface can read character streams and transform them into KeyStroke objects which can
/// be read in a FIFO manner.
/// </summary>
public interface IInputProvider
{
    /// <summary>
    /// Returns the next KeyStroke off the input queue or null if there is no more input events available. Note, this
    /// method call is not blocking, it returns null immediately if there is nothing on the input stream.
    /// </summary>
    /// <returns>KeyStroke object which represents a keystroke coming in through the input stream</returns>
    /// <exception cref="IOException">Propagated error if the underlying stream gave errors</exception>
    KeyStroke? PollInput();

    /// <summary>
    /// Returns the next KeyStroke off the input queue or blocks until one is available. 
    /// NOTE: In previous versions of Lanterna, this method was not blocking. From lanterna 3, it is blocking and you can call
    /// PollInput() for the non-blocking version.
    /// </summary>
    /// <returns>KeyStroke object which represents a keystroke coming in through the input stream</returns>
    /// <exception cref="IOException">Propagated error if the underlying stream gave errors</exception>
    KeyStroke ReadInput();

    /// <summary>
    /// Returns the next KeyStroke off the input queue, waiting up to the specified timeout. If no input is available
    /// within the timeout period, returns null.
    /// </summary>
    /// <param name="timeout">Maximum time to wait for input</param>
    /// <returns>KeyStroke object or null if timeout expired</returns>
    /// <exception cref="IOException">Propagated error if the underlying stream gave errors</exception>
    KeyStroke? ReadInput(TimeSpan timeout);
}