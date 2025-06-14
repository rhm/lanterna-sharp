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
/// Default implementation of ITextGUIThread, this class runs the GUI event processing on a dedicated thread. The GUI
/// needs to be explicitly started in order for the event processing loop to begin, so you must call Start()
/// for this. The GUI thread will stop if Stop() is called, the input stream returns EOF or an exception is
/// thrown from inside the event handling loop.
/// <para>
/// Here is an example of how to use this ITextGUIThread:
/// <code>
/// MultiWindowTextGUI textGUI = new MultiWindowTextGUI(new SeparateTextGUIThread.Factory(), screen);
/// // ... add components ...
/// ((IAsynchronousTextGUIThread)textGUI.GUIThread).Start();
/// // ... this thread will continue while the GUI runs on a separate thread ...
/// </code>
/// </para>
/// </summary>
/// <seealso cref="ITextGUIThread"/>
/// <seealso cref="SameTextGUIThread"/>
public class SeparateTextGUIThread : AbstractTextGUIThread, IAsynchronousTextGUIThread
{
    private volatile IAsynchronousTextGUIThread.State _state;
    private readonly System.Threading.Thread _textGUIThread;
    private readonly ManualResetEventSlim _waitEvent;

    private SeparateTextGUIThread(ITextGUI textGUI) : base(textGUI)
    {
        _waitEvent = new ManualResetEventSlim(false);
        _textGUIThread = new System.Threading.Thread(MainGUILoop)
        {
            Name = "LanternaGUI",
            IsBackground = true
        };
        _state = IAsynchronousTextGUIThread.State.Created;
    }

    public void Start()
    {
        _textGUIThread.Start();
        _state = IAsynchronousTextGUIThread.State.Started;
    }

    public void Stop()
    {
        if (_state != IAsynchronousTextGUIThread.State.Started)
        {
            return;
        }

        _state = IAsynchronousTextGUIThread.State.Stopping;
    }

    public void WaitForStop()
    {
        _waitEvent.Wait();
    }

    public void WaitForStop(TimeSpan timeout)
    {
        _waitEvent.Wait(timeout);
    }

    public IAsynchronousTextGUIThread.State CurrentState => _state;

    public override System.Threading.Thread? Thread => _textGUIThread;

    public override void InvokeLater(Action action)
    {
        if (_state != IAsynchronousTextGUIThread.State.Started)
        {
            throw new InvalidOperationException($"Cannot schedule {action} for execution on the TextGUIThread " +
                    $"because the thread is in {_state} state");
        }
        base.InvokeLater(action);
    }

    private void MainGUILoop()
    {
        try
        {
            // Draw initial screen, after this only draw when the GUI is marked as invalid
            try
            {
                TextGUI.UpdateScreen();
            }
            catch (IOException e)
            {
                ExceptionHandler?.OnIOException(e);
            }
            catch (Exception e)
            {
                ExceptionHandler?.OnException(e);
            }

            while (_state == IAsynchronousTextGUIThread.State.Started)
            {
                try
                {
                    if (!ProcessEventsAndUpdate())
                    {
                        try
                        {
                            System.Threading.Thread.Sleep(1);
                        }
                        catch (ThreadInterruptedException)
                        {
                            // Ignored
                        }
                    }
                }
                catch (EndOfStreamException)
                {
                    Stop();
                    if (TextGUI is IWindowBasedTextGUI windowBasedTextGUI)
                    {
                        // Close all windows on EOF
                        foreach (var window in windowBasedTextGUI.Windows.ToList())
                        {
                            window.Close();
                        }
                    }
                    break; // Break out quickly from the main loop
                }
                catch (IOException e)
                {
                    if (ExceptionHandler?.OnIOException(e) == true)
                    {
                        Stop();
                        break;
                    }
                }
                catch (Exception e)
                {
                    if (ExceptionHandler?.OnException(e) == true)
                    {
                        Stop();
                        break;
                    }
                }
            }
        }
        finally
        {
            _state = IAsynchronousTextGUIThread.State.Stopped;
            _waitEvent.Set();
        }
    }

    /// <summary>
    /// Factory class for creating SeparateTextGUIThread objects
    /// </summary>
    public class Factory : ITextGUIThreadFactory
    {
        public ITextGUIThread CreateTextGUIThread(ITextGUI textGUI)
        {
            return new SeparateTextGUIThread(textGUI);
        }
    }
}