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

namespace Lanterna.Gui2;

/// <summary>
/// Extended interface of ITextGUIThread for implementations that uses a separate thread for all GUI event processing and
/// updating.
/// </summary>
public interface IAsynchronousTextGUIThread : ITextGUIThread
{
    /// <summary>
    /// Starts the IAsynchronousTextGUIThread, typically meaning that the event processing loop will start.
    /// </summary>
    void Start();

    /// <summary>
    /// Requests that the IAsynchronousTextGUIThread stops, typically meaning that the event processing loop will exit
    /// </summary>
    void Stop();

    /// <summary>
    /// Blocks until the GUI loop has stopped
    /// </summary>
    /// <exception cref="ThreadInterruptedException">In case this thread was interrupted while waiting for the GUI thread to exit</exception>
    void WaitForStop();

    /// <summary>
    /// Blocks until the GUI loop has stopped or the timeout expires
    /// </summary>
    /// <param name="timeout">Maximum time to wait</param>
    /// <exception cref="ThreadInterruptedException">In case this thread was interrupted while waiting for the GUI thread to exit</exception>
    void WaitForStop(TimeSpan timeout);

    /// <summary>
    /// Returns the current status of this GUI thread
    /// </summary>
    /// <returns>Current status of the GUI thread</returns>
    State CurrentState { get; }

    /// <summary>
    /// Enum representing the states of the GUI thread life-cycle
    /// </summary>
    public enum State
    {
        /// <summary>
        /// The instance has been created but not yet started
        /// </summary>
        Created,
        /// <summary>
        /// The thread has started and is running
        /// </summary>
        Started,
        /// <summary>
        /// The thread is trying to stop but is still running
        /// </summary>
        Stopping,
        /// <summary>
        /// The thread has stopped
        /// </summary>
        Stopped
    }
}