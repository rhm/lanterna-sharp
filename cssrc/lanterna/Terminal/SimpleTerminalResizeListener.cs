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

namespace Lanterna.Terminal
{
    /// <summary>
    /// This class is a simple implementation of ITerminalResizeListener which will keep track of the size of the terminal
    /// and let you know if the terminal has been resized since you last checked. This can be useful to avoid threading 
    /// problems with the resize callback when your application is using a main event loop.
    /// </summary>
    public class SimpleTerminalResizeListener : ITerminalResizeListener
    {
        private bool _wasResized;
        private TerminalSize _lastKnownSize;
        private readonly object _lock = new object();

        /// <summary>
        /// Creates a new SimpleTerminalResizeListener
        /// </summary>
        /// <param name="initialSize">Before any resize event, this listener doesn't know the size of the terminal. By supplying a
        /// value here, you control what GetLastKnownSize() will return if invoked before any resize events has reached us.</param>
        public SimpleTerminalResizeListener(TerminalSize initialSize)
        {
            _wasResized = false;
            _lastKnownSize = initialSize;
        }

        /// <summary>
        /// Checks if the terminal was resized since the last time this method was called. If this is the first time calling
        /// this method, the result is going to be based on if the terminal has been resized since this listener was attached
        /// to the Terminal.
        /// </summary>
        /// <returns>true if the terminal was resized, false otherwise</returns>
        public bool IsTerminalResized()
        {
            lock (_lock)
            {
                if (_wasResized)
                {
                    _wasResized = false;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns the last known size the Terminal is supposed to have.
        /// </summary>
        /// <returns>Size of the terminal, as of the last resize update</returns>
        public TerminalSize GetLastKnownSize()
        {
            return _lastKnownSize;
        }

        public void OnResized(ITerminal terminal, TerminalSize newSize)
        {
            lock (_lock)
            {
                _wasResized = true;
                _lastKnownSize = newSize;
            }
        }
    }
}