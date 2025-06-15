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

using System;
using System.IO;
using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;

namespace Lanterna.Terminal;

/// <summary>
/// This class exposes methods for converting a terminal into an IOSafeTerminal. There are two options available, either
/// one that will convert any IOException to a runtime exception (and re-throw it) or one that will silently swallow any
/// IOException (and return null in those cases the method has a non-void return type).
/// </summary>
public class IOSafeTerminalAdapter : IIOSafeTerminal
{
    public interface IExceptionHandler
    {
        void OnException(IOException e);
    }

    public class ConvertToRuntimeException : IExceptionHandler
    {
        public void OnException(IOException e)
        {
            throw new InvalidOperationException("IO operation failed", e);
        }
    }

    public class DoNothingAndOrReturnNull : IExceptionHandler
    {
        public void OnException(IOException e)
        {
            // Do nothing, silently ignore the exception
        }
    }

    /// <summary>
    /// Creates a wrapper around a Terminal that exposes it as a IOSafeTerminal. If any IOExceptions occur, they will be
    /// wrapped by a runtime exception and re-thrown.
    /// </summary>
    /// <param name="terminal">Terminal to wrap</param>
    /// <returns>IOSafeTerminal wrapping the supplied terminal</returns>
    public static IIOSafeTerminal CreateRuntimeExceptionConvertingAdapter(ITerminal terminal)
    {
        if (terminal is IExtendedTerminal extendedTerminal)
        {
            return CreateRuntimeExceptionConvertingAdapter(extendedTerminal);
        }
        else
        {
            return new IOSafeTerminalAdapter(terminal, new ConvertToRuntimeException());
        }
    }

    /// <summary>
    /// Creates a wrapper around an ExtendedTerminal that exposes it as a IOSafeExtendedTerminal.
    /// If any IOExceptions occur, they will be wrapped by a runtime exception and re-thrown.
    /// </summary>
    /// <param name="terminal">Terminal to wrap</param>
    /// <returns>IOSafeExtendedTerminal wrapping the supplied terminal</returns>
    public static IIOSafeExtendedTerminal CreateRuntimeExceptionConvertingAdapter(IExtendedTerminal terminal)
    {
        return new Extended(terminal, new ConvertToRuntimeException());
    }

    /// <summary>
    /// Creates a wrapper around a Terminal that exposes it as a IOSafeTerminal. If any IOExceptions occur, they will be
    /// silently ignored and for those method with a non-void return type, null will be returned.
    /// </summary>
    /// <param name="terminal">Terminal to wrap</param>
    /// <returns>IOSafeTerminal wrapping the supplied terminal</returns>
    public static IIOSafeTerminal CreateDoNothingOnExceptionAdapter(ITerminal terminal)
    {
        if (terminal is IExtendedTerminal extendedTerminal)
        {
            return CreateDoNothingOnExceptionAdapter(extendedTerminal);
        }
        else
        {
            return new IOSafeTerminalAdapter(terminal, new DoNothingAndOrReturnNull());
        }
    }

    /// <summary>
    /// Creates a wrapper around an ExtendedTerminal that exposes it as a IOSafeExtendedTerminal.
    /// If any IOExceptions occur, they will be silently ignored and for those method with a 
    /// non-void return type, null will be returned.
    /// </summary>
    /// <param name="terminal">Terminal to wrap</param>
    /// <returns>IOSafeExtendedTerminal wrapping the supplied terminal</returns>
    public static IIOSafeExtendedTerminal CreateDoNothingOnExceptionAdapter(IExtendedTerminal terminal)
    {
        return new Extended(terminal, new DoNothingAndOrReturnNull());
    }

    private readonly ITerminal _backend;
    internal readonly IExceptionHandler _exceptionHandler;

    public IOSafeTerminalAdapter(ITerminal backend, IExceptionHandler exceptionHandler)
    {
        _backend = backend;
        _exceptionHandler = exceptionHandler;
    }

    public void EnterPrivateMode()
    {
        try
        {
            _backend.EnterPrivateMode();
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public void ExitPrivateMode()
    {
        try
        {
            _backend.ExitPrivateMode();
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public void ClearScreen()
    {
        try
        {
            _backend.ClearScreen();
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public void SetCursorPosition(int x, int y)
    {
        try
        {
            _backend.SetCursorPosition(x, y);
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public void SetCursorPosition(TerminalPosition position)
    {
        try
        {
            _backend.SetCursorPosition(position);
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public TerminalPosition GetCursorPosition()
    {
        try
        {
            return _backend.GetCursorPosition();
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
        return new TerminalPosition(0, 0); // Default fallback position
    }

    public void SetCursorVisible(bool visible)
    {
        try
        {
            _backend.SetCursorVisible(visible);
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public void PutCharacter(char c)
    {
        try
        {
            _backend.PutCharacter(c);
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public void PutString(string str)
    {
        try
        {
            _backend.PutString(str);
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public ITextGraphics NewTextGraphics()
    {
        try
        {
            return _backend.NewTextGraphics();
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
        return new NullTextGraphics(); // Return a null object pattern implementation
    }

    public void EnableSGR(SGR sgr)
    {
        try
        {
            _backend.EnableSGR(sgr);
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public void DisableSGR(SGR sgr)
    {
        try
        {
            _backend.DisableSGR(sgr);
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public void ResetColorAndSGR()
    {
        try
        {
            _backend.ResetColorAndSGR();
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public void SetForegroundColor(ITextColor color)
    {
        try
        {
            _backend.SetForegroundColor(color);
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public void SetBackgroundColor(ITextColor color)
    {
        try
        {
            _backend.SetBackgroundColor(color);
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public void AddResizeListener(ITerminalResizeListener listener)
    {
        _backend.AddResizeListener(listener);
    }

    public void RemoveResizeListener(ITerminalResizeListener listener)
    {
        _backend.RemoveResizeListener(listener);
    }

    public TerminalSize GetTerminalSize()
    {
        try
        {
            return _backend.GetTerminalSize();
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
        return new TerminalSize(80, 24); // Default fallback size
    }

    public byte[] EnquireTerminal(TimeSpan timeout)
    {
        try
        {
            return _backend.EnquireTerminal(timeout);
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
        return new byte[0]; // Return empty array
    }

    public void Bell()
    {
        try
        {
            _backend.Bell();
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public void Flush()
    {
        try
        {
            _backend.Flush();
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public void Close()
    {
        try
        {
            _backend.Dispose();
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
    }

    public KeyStroke? PollInput()
    {
        try
        {
            return _backend.PollInput();
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
        return null;
    }

    public KeyStroke ReadInput()
    {
        try
        {
            return _backend.ReadInput();
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
        return new KeyStroke(KeyType.EOF); // Return EOF keystroke as fallback
    }

    public KeyStroke? ReadInput(TimeSpan timeout)
    {
        try
        {
            return _backend.ReadInput(timeout);
        }
        catch (IOException e)
        {
            _exceptionHandler.OnException(e);
        }
        return null;
    }

    public void Dispose()
    {
        Close();
    }

    /// <summary>
    /// This class exposes methods for converting an extended terminal into an IOSafeExtendedTerminal.
    /// </summary>
    public class Extended : IOSafeTerminalAdapter, IIOSafeExtendedTerminal
    {
        private new readonly IExtendedTerminal _backend;

        public Extended(IExtendedTerminal backend, IExceptionHandler exceptionHandler)
            : base(backend, exceptionHandler)
        {
            _backend = backend;
        }

        public void SetTerminalSize(int columns, int rows)
        {
            try
            {
                _backend.SetTerminalSize(columns, rows);
            }
            catch (IOException e)
            {
                _exceptionHandler.OnException(e);
            }
        }

        public void SetTitle(string title)
        {
            try
            {
                _backend.SetTitle(title);
            }
            catch (IOException e)
            {
                _exceptionHandler.OnException(e);
            }
        }

        public void PushTitle()
        {
            try
            {
                _backend.PushTitle();
            }
            catch (IOException e)
            {
                _exceptionHandler.OnException(e);
            }
        }

        public void PopTitle()
        {
            try
            {
                _backend.PopTitle();
            }
            catch (IOException e)
            {
                _exceptionHandler.OnException(e);
            }
        }

        public void Iconify()
        {
            try
            {
                _backend.Iconify();
            }
            catch (IOException e)
            {
                _exceptionHandler.OnException(e);
            }
        }

        public void Deiconify()
        {
            try
            {
                _backend.Deiconify();
            }
            catch (IOException e)
            {
                _exceptionHandler.OnException(e);
            }
        }

        public void Maximize()
        {
            try
            {
                _backend.Maximize();
            }
            catch (IOException e)
            {
                _exceptionHandler.OnException(e);
            }
        }

        public void Unmaximize()
        {
            try
            {
                _backend.Unmaximize();
            }
            catch (IOException e)
            {
                _exceptionHandler.OnException(e);
            }
        }

        public void SetMouseCaptureMode(MouseCaptureMode? mouseCaptureMode)
        {
            try
            {
                _backend.SetMouseCaptureMode(mouseCaptureMode);
            }
            catch (IOException e)
            {
                _exceptionHandler.OnException(e);
            }
        }

        public void ScrollLines(int firstLine, int lastLine, int distance)
        {
            try
            {
                _backend.ScrollLines(firstLine, lastLine, distance);
            }
            catch (IOException e)
            {
                _exceptionHandler.OnException(e);
            }
        }
    }
}