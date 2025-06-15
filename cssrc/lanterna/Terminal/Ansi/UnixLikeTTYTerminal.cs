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

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Lanterna.Terminal.Ansi;

/// <summary>
/// UnixLikeTerminal extends from ANSITerminal and defines functionality that is common to
/// Unix terminals and similar systems, like setting tty modes; echo, cbreak
/// and minimum characters for reading as well as a shutdown hook to set the tty back to
/// original state at the end.
/// 
/// If requested, it handles Control-C input to terminate the program, and hooks
/// into Unix WINCH signal to detect when the user has resized the terminal,
/// if supported by the runtime.
/// </summary>
public abstract class UnixLikeTTYTerminal : UnixLikeTerminal
{
    private readonly string? _ttyDevice;
    private string? _sttyStatusToRestore;

    /// <summary>
    /// Creates a UnixTerminal using a specified input stream, output stream and character set, with a custom size
    /// querier instead of using the default one. This way you can override size detection (if you want to force the
    /// terminal to a fixed size, for example). You also choose how you want ctrl+c key strokes to be handled.
    /// </summary>
    /// <param name="ttyDevice">TTY device file that is representing this terminal session, will be used when calling stty to make
    /// it operate on this session</param>
    /// <param name="terminalInput">Input stream to read terminal input from</param>
    /// <param name="terminalOutput">Output stream to write terminal output to</param>
    /// <param name="terminalCharset">Character set to use when converting characters to bytes</param>
    /// <param name="terminalCtrlCBehaviour">Special settings on how the terminal will behave, see CtrlCBehaviour for
    /// more details</param>
    protected UnixLikeTTYTerminal(
        string? ttyDevice,
        Stream terminalInput,
        Stream terminalOutput,
        Encoding? terminalCharset,
        CtrlCBehaviour terminalCtrlCBehaviour)
        : base(terminalInput, terminalOutput, terminalCharset, terminalCtrlCBehaviour)
    {
        _ttyDevice = ttyDevice;
        
        // Take ownership of the terminal
        RealAcquire();
    }

    protected override void Acquire()
    {
        // Hack! Override to prevent double acquisition
    }

    private void RealAcquire()
    {
        base.Acquire();
    }

    protected override void RegisterTerminalResizeListener(Action onResize)
    {
        // On .NET, we can use Console.CancelKeyPress for Ctrl+C but terminal resize detection
        // is more complex. For now, we'll implement a basic version that checks periodically.
        // In a full implementation, this would use platform-specific APIs.
        
        // For Unix systems, we could use P/Invoke to handle SIGWINCH
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Note: This is a simplified implementation
            // A full implementation would use signal handling
            Task.Run(async () =>
            {
                var lastSize = GetTerminalSize();
                while (!disposed)
                {
                    await Task.Delay(1000); // Check every second
                    try
                    {
                        var currentSize = GetTerminalSize();
                        if (!currentSize.Equals(lastSize))
                        {
                            lastSize = currentSize;
                            onResize();
                        }
                    }
                    catch
                    {
                        // Ignore errors during size checking
                    }
                }
            });
        }
    }

    private volatile bool disposed = false;

    protected override void SaveTerminalSettings()
    {
        _sttyStatusToRestore = RunSttyCommand("-g")?.Trim();
    }

    protected override void RestoreTerminalSettings()
    {
        if (_sttyStatusToRestore != null)
        {
            RunSttyCommand(_sttyStatusToRestore);
        }
    }

    protected override void KeyEchoEnabled(bool enabled)
    {
        RunSttyCommand(enabled ? "echo" : "-echo");
    }

    protected override void CanonicalMode(bool enabled)
    {
        RunSttyCommand(enabled ? "icanon" : "-icanon");
        if (!enabled)
        {
            RunSttyCommand("min", "1");
        }
    }

    protected override void KeyStrokeSignalsEnabled(bool enabled)
    {
        if (enabled)
        {
            RunSttyCommand("intr", "^C");
        }
        else
        {
            RunSttyCommand("intr", "undef");
        }
    }

    protected string? RunSttyCommand(params string[] parameters)
    {
        var commandLine = new List<string>(GetSttyCommand());
        commandLine.AddRange(parameters);
        return ExecuteCommand(commandLine.ToArray());
    }

    protected string? ExecuteCommand(params string[] cmd)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = cmd[0],
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Add arguments
            for (int i = 1; i < cmd.Length; i++)
            {
                processStartInfo.ArgumentList.Add(cmd[i]);
            }

            // If we have a TTY device, redirect input to it
            if (!string.IsNullOrEmpty(_ttyDevice))
            {
                processStartInfo.RedirectStandardInput = true;
            }

            using var process = Process.Start(processStartInfo);
            if (process == null)
                return null;

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            
            return output;
        }
        catch
        {
            return null;
        }
    }

    protected string[] GetSttyCommand()
    {
        string? sttyOverride = Environment.GetEnvironmentVariable("LANTERNA_STTY_COMMAND");
        if (sttyOverride != null)
        {
            return new[] { sttyOverride };
        }
        else
        {
            // Issue #519: this will hopefully be more portable across linux distributions
            // Previously we hard-coded "/bin/stty" here
            return new[]
            {
                "/usr/bin/env",
                "stty"
            };
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            disposed = true;
        }
        base.Dispose(disposing);
    }
}