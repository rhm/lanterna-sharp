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

using System.Collections.Concurrent;

namespace Lanterna.Gui2;

/// <summary>
/// Abstract implementation of ITextGUIThread with common logic for both available concrete implementations.
/// </summary>
public abstract class AbstractTextGUIThread : ITextGUIThread
{
    protected readonly ITextGUI TextGUI;
    protected readonly ConcurrentQueue<Action> CustomTasks;
    protected ITextGUIThread.IExceptionHandler? ExceptionHandler;

    /// <summary>
    /// Sets up this AbstractTextGUIThread for operations on the supplied ITextGUI
    /// </summary>
    /// <param name="textGUI">Text GUI this ITextGUIThread implementation will be operating on</param>
    protected AbstractTextGUIThread(ITextGUI textGUI)
    {
        ExceptionHandler = new DefaultExceptionHandler();
        TextGUI = textGUI;
        CustomTasks = new ConcurrentQueue<Action>();
    }

    public virtual void InvokeLater(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }
        CustomTasks.Enqueue(action);
    }

    public virtual void SetExceptionHandler(ITextGUIThread.IExceptionHandler? exceptionHandler)
    {
        if (exceptionHandler == null)
        {
            throw new ArgumentException("Cannot call SetExceptionHandler(null)");
        }
        ExceptionHandler = exceptionHandler;
    }

    public virtual bool ProcessEventsAndUpdate()
    {
        if (Thread != System.Threading.Thread.CurrentThread)
        {
            throw new InvalidOperationException("Calling ProcessEventsAndUpdate outside of GUI thread");
        }

        try
        {
            TextGUI.ProcessInput();
            while (CustomTasks.TryDequeue(out var action))
            {
                action();
            }

            if (TextGUI.IsPendingUpdate)
            {
                TextGUI.UpdateScreen();
                return true;
            }
            return false;
        }
        catch (EndOfStreamException)
        {
            // Always re-throw EndOfStreamExceptions so the UI system knows we've closed the terminal
            throw;
        }
        catch (IOException e)
        {
            if (ExceptionHandler != null)
            {
                ExceptionHandler.OnIOException(e);
            }
            else
            {
                throw;
            }
        }
        catch (Exception e)
        {
            if (ExceptionHandler != null)
            {
                ExceptionHandler.OnException(e);
            }
            else
            {
                throw;
            }
        }
        return true;
    }

    public virtual void InvokeAndWait(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var guiThread = Thread;
        if (guiThread == null || System.Threading.Thread.CurrentThread == guiThread)
        {
            action();
        }
        else
        {
            using var resetEvent = new ManualResetEventSlim(false);
            InvokeLater(() =>
            {
                try
                {
                    action();
                }
                finally
                {
                    resetEvent.Set();
                }
            });
            resetEvent.Wait();
        }
    }

    public abstract System.Threading.Thread? Thread { get; }

    /// <summary>
    /// Default exception handler that prints exceptions to the console
    /// </summary>
    private class DefaultExceptionHandler : ITextGUIThread.IExceptionHandler
    {
        public bool OnIOException(IOException e)
        {
            Console.WriteLine(e);
            return true;
        }

        public bool OnException(Exception e)
        {
            Console.WriteLine(e);
            return true;
        }
    }
}