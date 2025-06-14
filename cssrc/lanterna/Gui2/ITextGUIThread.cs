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
/// Interface that represents the thread that is expected to run the event/input/update loop for the ITextGUI. There
/// are mainly two implementations of this interface, one for having lanterna automatically spawn a new thread for doing
/// all the processing and leaving the creator thread free to do other things, and one that assumes the creator thread
/// will hand over control to lanterna for as long as the GUI is running.
/// </summary>
public interface ITextGUIThread
{
    /// <summary>
    /// Invokes custom code on the GUI thread. Even if the current thread IS the GUI thread, the code will be
    /// executed at a later time when the event processing is done.
    /// </summary>
    /// <param name="action">Code to run asynchronously</param>
    /// <exception cref="InvalidOperationException">If the GUI thread is not running</exception>
    void InvokeLater(Action action);

    /// <summary>
    /// Main method to call when you are managing the event/input/update loop yourself. This method will run one round
    /// through the GUI's event/input queue and update the visuals if required. If the operation did nothing (returning
    /// false) you could sleep for a millisecond and then try again. If you use SameTextGUIThread you
    /// must either call this method directly to make the GUI update or use one of the methods on
    /// IWindowBasedTextGUI that blocks until a particular window has closed.
    /// </summary>
    /// <returns>true if there was anything to process or the GUI was updated, otherwise false</returns>
    /// <exception cref="IOException">If there was an I/O error when processing and updating the GUI</exception>
    bool ProcessEventsAndUpdate();

    /// <summary>
    /// Schedules custom code to be executed on the GUI thread and waits until the code has been executed before
    /// returning. If this is run on the GUI thread, it will immediately run the action and then return.
    /// </summary>
    /// <param name="action">Code to be run and waited for completion before this method returns</param>
    /// <exception cref="InvalidOperationException">If the GUI thread is not running</exception>
    void InvokeAndWait(Action action);

    /// <summary>
    /// Updates the exception handler used by this ITextGUIThread. The exception handler will be invoked when an exception
    /// occurs in the main event loop. You can then decide how to log this exception and if you want to terminate the
    /// thread or not.
    /// </summary>
    /// <param name="exceptionHandler">Handler to inspect exceptions</param>
    void SetExceptionHandler(IExceptionHandler? exceptionHandler);

    /// <summary>
    /// Returns the .NET thread which is processing GUI events and updating the screen
    /// </summary>
    /// <returns>Thread which is processing events and updating the screen</returns>
    Thread? Thread { get; }

    /// <summary>
    /// This interface defines an exception handler, that is used for looking at exceptions that occurs during the main
    /// event loop of the ITextGUIThread. You can for example use this for logging, but also decide if you want the
    /// exception to kill the thread.
    /// </summary>
    public interface IExceptionHandler
    {
        /// <summary>
        /// Will be called when an IOException has occurred in the main event thread
        /// </summary>
        /// <param name="e">IOException that occurred</param>
        /// <returns>If you return true, the event thread will be terminated</returns>
        bool OnIOException(IOException e);

        /// <summary>
        /// Will be called when an Exception has occurred in the main event thread
        /// </summary>
        /// <param name="e">Exception that occurred</param>
        /// <returns>If you return true, the event thread will be terminated</returns>
        bool OnException(Exception e);
    }
}