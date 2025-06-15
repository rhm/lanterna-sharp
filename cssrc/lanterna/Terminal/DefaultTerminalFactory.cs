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

using System.Runtime.InteropServices;
using System.Text;
using Lanterna.Core;
using Lanterna.Screen;
using Lanterna.Terminal.Ansi;

namespace Lanterna.Terminal;

/// <summary>
/// This TerminalFactory implementation uses a simple auto-detection mechanism for figuring out which terminal
/// implementation to create based on characteristics of the system the program is running on.
/// 
/// Note that for all systems with a graphical environment present, a graphical terminal emulator would be chosen
/// in the Java version. However, the C# port focuses on console/terminal applications, so this factory will
/// primarily create text-based terminals suitable for console applications.
/// </summary>
public class DefaultTerminalFactory : ITerminalFactory
{
    private static readonly Stream DefaultOutputStream = Console.OpenStandardOutput();
    private static readonly Stream DefaultInputStream = Console.OpenStandardInput();
    private static readonly Encoding DefaultCharset = Encoding.UTF8;

    private readonly Stream _outputStream;
    private readonly Stream _inputStream;
    private readonly Encoding _charset;

    private TerminalSize? _initialTerminalSize;
    private bool _forceTextTerminal;
    private MouseCaptureMode? _mouseCaptureMode;
    private CtrlCBehavior _unixTerminalCtrlCBehaviour;

    /// <summary>
    /// Creates a new DefaultTerminalFactory with all properties set to their defaults
    /// </summary>
    public DefaultTerminalFactory() : this(DefaultOutputStream, DefaultInputStream, DefaultCharset)
    {
    }

    /// <summary>
    /// Creates a new DefaultTerminalFactory with I/O and character set options customizable.
    /// </summary>
    /// <param name="outputStream">Output stream to use for text-based Terminal implementations</param>
    /// <param name="inputStream">Input stream to use for text-based Terminal implementations</param>
    /// <param name="charset">Character set to assume the client is using</param>
    public DefaultTerminalFactory(Stream outputStream, Stream inputStream, Encoding charset)
    {
        _outputStream = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
        _inputStream = inputStream ?? throw new ArgumentNullException(nameof(inputStream));
        _charset = charset ?? throw new ArgumentNullException(nameof(charset));

        _forceTextTerminal = true; // C# port focuses on text terminals
        _mouseCaptureMode = null;
        _unixTerminalCtrlCBehaviour = CtrlCBehavior.CtrlCKillsApplication;
    }

    public ITerminal CreateTerminal()
    {
        // For the C# port, we focus on creating headless (text-based) terminals
        // since GUI terminal emulators are not commonly used in .NET environments
        return CreateHeadlessTerminal();
    }

    /// <summary>
    /// Instantiates a Terminal that is guaranteed to be text-based, ignoring any GUI preferences.
    /// This method is useful when using tools that rely on AOT compilation to ensure GUI code paths are not hit.
    /// </summary>
    /// <returns>Terminal implementation</returns>
    /// <exception cref="IOException">If there was an I/O error with the underlying input/output system</exception>
    public ITerminal CreateHeadlessTerminal()
    {
        if (IsOperatingSystemWindows())
        {
            return CreateWindowsTerminal();
        }

        return CreateUnixTerminal(_outputStream, _inputStream, _charset);
    }

    /// <summary>
    /// Create a TerminalScreen and immediately wrap it up in a TerminalScreen
    /// </summary>
    /// <returns>New TerminalScreen created with a terminal from CreateTerminal()</returns>
    /// <exception cref="IOException">In case there was an I/O error</exception>
    public TerminalScreen CreateScreen()
    {
        return new TerminalScreen(CreateTerminal());
    }

    /// <summary>
    /// Sets a hint to the TerminalFactory of what size to use when creating the terminal.
    /// </summary>
    /// <param name="initialTerminalSize">Size (in rows and columns) of the newly created terminal</param>
    /// <returns>Reference to itself, so multiple .set-calls can be chained</returns>
    public DefaultTerminalFactory SetInitialTerminalSize(TerminalSize initialTerminalSize)
    {
        _initialTerminalSize = initialTerminalSize;
        return this;
    }

    /// <summary>
    /// Controls whether a text-based Terminal shall be created even if the system supports a graphical environment.
    /// In the C# port, this is always effectively true since we focus on console applications.
    /// </summary>
    /// <param name="forceTextTerminal">If true, will always create a text-based Terminal</param>
    /// <returns>Reference to itself, so multiple .set-calls can be chained</returns>
    public DefaultTerminalFactory SetForceTextTerminal(bool forceTextTerminal)
    {
        _forceTextTerminal = forceTextTerminal;
        return this;
    }

    /// <summary>
    /// Sets the default CTRL-C behavior to use for all UnixTerminal objects created by this factory. You can
    /// use this to tell Lanterna to trap CTRL-C instead of exiting the application. Non-UNIX terminals are not affected
    /// by this.
    /// </summary>
    /// <param name="unixTerminalCtrlCBehaviour">CTRL-C behavior to use for UnixTerminals</param>
    /// <returns>Reference to itself, so multiple .set-calls can be chained</returns>
    public DefaultTerminalFactory SetUnixTerminalCtrlCBehaviour(CtrlCBehavior unixTerminalCtrlCBehaviour)
    {
        _unixTerminalCtrlCBehaviour = unixTerminalCtrlCBehaviour;
        return this;
    }

    /// <summary>
    /// Sets the mouse capture mode the terminal should use. Please note that this is an extension which isn't widely
    /// supported!
    /// </summary>
    /// <param name="mouseCaptureMode">Capture mode for mouse interactions</param>
    /// <returns>Itself</returns>
    public DefaultTerminalFactory SetMouseCaptureMode(MouseCaptureMode mouseCaptureMode)
    {
        _mouseCaptureMode = mouseCaptureMode;
        return this;
    }

    private ITerminal CreateWindowsTerminal()
    {
        // For Windows, we'll use the UnixTerminal which should work in most modern Windows environments
        // with ANSI support (Windows 10+ and Windows Terminal)
        
        // Create a Unix-like terminal that should work on Windows with ANSI support
        var terminal = new UnixTerminal(_inputStream, _outputStream, _charset, _unixTerminalCtrlCBehaviour);
        
        // Mouse capture mode support would be added here when implemented
        // if (_mouseCaptureMode != null)
        // {
        //     terminal.SetMouseCaptureMode(_mouseCaptureMode.Value);
        // }
        
        return terminal;
    }

    private ITerminal CreateUnixTerminal(Stream outputStream, Stream inputStream, Encoding charset)
    {
        UnixTerminal unixTerminal = new UnixTerminal(inputStream, outputStream, charset, _unixTerminalCtrlCBehaviour);
        
        // Mouse capture mode support would be added here when implemented
        // if (_mouseCaptureMode != null)
        // {
        //     unixTerminal.SetMouseCaptureMode(_mouseCaptureMode.Value);
        // }
        
        return unixTerminal;
    }

    /// <summary>
    /// Detects whether the running platform is Windows by looking at the operating system name system property
    /// </summary>
    /// <returns>True if running on Windows, false otherwise</returns>
    private static bool IsOperatingSystemWindows()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }

    /// <summary>
    /// Checks if we have access to a terminal/console. This is equivalent to System.console() != null in Java.
    /// </summary>
    /// <returns>True if we have terminal access, false otherwise</returns>
    private static bool HasTerminal()
    {
        // In .NET, we can check if we're running in a console environment
        try
        {
            // Try to get console window handle or check if stdin/stdout are redirected
            return !Console.IsInputRedirected && !Console.IsOutputRedirected;
        }
        catch
        {
            return false;
        }
    }
}