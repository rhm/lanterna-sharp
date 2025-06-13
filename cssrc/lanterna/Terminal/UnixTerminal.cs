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
using Lanterna.Core;
using Lanterna.Input;

namespace Lanterna.Terminal;

/// <summary>
/// Control behavior for Ctrl+C handling in Unix terminals
/// </summary>
public enum CtrlCBehavior
{
    /// <summary>
    /// Pressing Ctrl+C doesn't kill the application, it will be added to the input queue as any other keystroke
    /// </summary>
    Trap,
    
    /// <summary>
    /// Pressing Ctrl+C will restore the terminal and kill the application as it normally does
    /// </summary>
    CtrlCKillsApplication
}

/// <summary>
/// Unix terminal implementation that provides full terminal control including raw mode,
/// resize detection, and proper cleanup. This implementation uses stty commands to
/// control terminal settings and handles Unix signals for resize detection.
/// </summary>
public class UnixTerminal : ANSITerminal
{
    private readonly CtrlCBehavior _ctrlCBehavior;
    private readonly bool _catchSpecialCharacters;
    private string? _savedTerminalSettings;
    private bool _terminalAcquired;
    private bool _inPrivateMode;
    private readonly object _terminalLock = new();

    /// <summary>
    /// Creates a UnixTerminal with default settings, using Console.In and Console.Out for input/output
    /// and UTF-8 encoding. Ctrl+C signal will be trapped instead of killing the application.
    /// </summary>
    public UnixTerminal() : this(Console.OpenStandardInput(), Console.OpenStandardOutput(), Encoding.UTF8)
    {
    }

    /// <summary>
    /// Creates a UnixTerminal using specified input/output streams and character encoding.
    /// Ctrl+C signal will be trapped instead of killing the application.
    /// </summary>
    /// <param name="terminalInput">Input stream to read terminal input from</param>
    /// <param name="terminalOutput">Output stream to write terminal output to</param>
    /// <param name="terminalEncoding">Character encoding to use when converting characters to bytes</param>
    public UnixTerminal(Stream terminalInput, Stream terminalOutput, Encoding? terminalEncoding = null)
        : this(terminalInput, terminalOutput, terminalEncoding, CtrlCBehavior.Trap)
    {
    }

    /// <summary>
    /// Creates a UnixTerminal using specified input/output streams, character encoding, and Ctrl+C behavior.
    /// </summary>
    /// <param name="terminalInput">Input stream to read terminal input from</param>
    /// <param name="terminalOutput">Output stream to write terminal output to</param>
    /// <param name="terminalEncoding">Character encoding to use when converting characters to bytes</param>
    /// <param name="ctrlCBehavior">How to handle Ctrl+C keystrokes</param>
    public UnixTerminal(Stream terminalInput, Stream terminalOutput, Encoding? terminalEncoding, CtrlCBehavior ctrlCBehavior)
        : base(terminalInput, terminalOutput, terminalEncoding)
    {
        _ctrlCBehavior = ctrlCBehavior;
        
        // Check system property for special character handling
        string catchSpecialProperty = Environment.GetEnvironmentVariable("LANTERNA_CATCH_SPECIAL_CHARS") ?? "true";
        _catchSpecialCharacters = !string.Equals(catchSpecialProperty.Trim(), "false", StringComparison.OrdinalIgnoreCase);
        
        _terminalAcquired = false;
        
        // Set up cleanup handler
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        Console.CancelKeyPress += OnCancelKeyPress;
        
        // Acquire the terminal
        AcquireTerminal();
    }

    /// <summary>
    /// Takes control of the terminal by setting raw mode, disabling echo, and setting up resize handling
    /// </summary>
    private void AcquireTerminal()
    {
        lock (_terminalLock)
        {
            if (_terminalAcquired)
                return;

            try
            {
                // Set initial size
                OnResized(80, 24);

                // Save current terminal settings
                SaveTerminalSettings();
                
                // Configure terminal for raw mode
                SetCanonicalMode(false);
                SetKeyEcho(false);
                
                if (_catchSpecialCharacters)
                {
                    SetKeyStrokeSignalsEnabled(false);
                }

                // Set up signal handlers for resize detection
                RegisterResizeHandler();
                
                _terminalAcquired = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to acquire terminal control", ex);
            }
        }
    }

    /// <summary>
    /// Saves the current terminal settings so they can be restored later
    /// </summary>
    private void SaveTerminalSettings()
    {
        try
        {
            _savedTerminalSettings = RunSttyCommand("-g").Trim();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save terminal settings", ex);
        }
    }

    /// <summary>
    /// Restores the terminal settings that were saved earlier
    /// </summary>
    private void RestoreTerminalSettings()
    {
        if (!string.IsNullOrEmpty(_savedTerminalSettings))
        {
            try
            {
                RunSttyCommand(_savedTerminalSettings);
            }
            catch
            {
                // Ignore errors during restore - we're probably shutting down
            }
        }
    }

    /// <summary>
    /// Sets whether key echo is enabled (characters appear on screen when typed)
    /// </summary>
    private void SetKeyEcho(bool enabled)
    {
        RunSttyCommand(enabled ? "echo" : "-echo");
    }

    /// <summary>
    /// Sets whether canonical mode is enabled (line buffering vs character-by-character input)
    /// </summary>
    private void SetCanonicalMode(bool enabled)
    {
        RunSttyCommand(enabled ? "icanon" : "-icanon");
        if (!enabled)
        {
            RunSttyCommand("min", "1");
        }
    }

    /// <summary>
    /// Sets whether keystroke signals (like Ctrl+C) are enabled
    /// </summary>
    private void SetKeyStrokeSignalsEnabled(bool enabled)
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

    /// <summary>
    /// Runs an stty command with the specified parameters
    /// </summary>
    private string RunSttyCommand(params string[] parameters)
    {
        var commandLine = new List<string> { "/usr/bin/env", "stty" };
        commandLine.AddRange(parameters);
        
        return ExecuteCommand(commandLine.ToArray());
    }

    /// <summary>
    /// Executes a system command and returns its output
    /// </summary>
    private string ExecuteCommand(string[] command)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command[0],
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            for (int i = 1; i < command.Length; i++)
            {
                startInfo.ArgumentList.Add(command[i]);
            }

            using var process = Process.Start(startInfo);
            if (process == null)
                throw new InvalidOperationException($"Failed to start process: {string.Join(" ", command)}");

            process.WaitForExit();
            string output = process.StandardOutput.ReadToEnd();
            
            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                throw new InvalidOperationException($"Command failed with exit code {process.ExitCode}: {error}");
            }

            return output;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to execute command: {string.Join(" ", command)}", ex);
        }
    }

    /// <summary>
    /// Sets up signal handlers for terminal resize detection
    /// </summary>
    private void RegisterResizeHandler()
    {
        // On Unix systems, we would typically register for SIGWINCH signals
        // In .NET, this is challenging without P/Invoke or unsafe code
        // For now, we'll implement a simple polling mechanism
        // A full implementation could use Platform.Unix package or P/Invoke
        
        // This is a simplified approach - a production implementation would use proper signal handling
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Start a background thread to periodically check terminal size
            Task.Run(async () =>
            {
                var lastSize = new TerminalSize(80, 24);
                while (_terminalAcquired && !_disposed)
                {
                    try
                    {
                        var currentSize = QueryTerminalSize();
                        if (!currentSize.Equals(lastSize))
                        {
                            lastSize = currentSize;
                            OnResized(currentSize);
                        }
                    }
                    catch
                    {
                        // Ignore errors in background size checking
                    }
                    
                    await Task.Delay(500); // Check every 500ms
                }
            });
        }
    }

    /// <summary>
    /// Queries the terminal size using cursor positioning technique
    /// </summary>
    private TerminalSize QueryTerminalSize()
    {
        // If we're disposed or not acquired, don't query - just return default
        if (_disposed || !_terminalAcquired)
        {
            return new TerminalSize(80, 24);
        }
        
        try
        {
            // Try to get size from environment variables first (safer)
            string? cols = Environment.GetEnvironmentVariable("COLUMNS");
            string? rows = Environment.GetEnvironmentVariable("LINES");
            
            if (int.TryParse(cols, out int envCols) && int.TryParse(rows, out int envRows) &&
                envCols > 0 && envRows > 0)
            {
                return new TerminalSize(envCols, envRows);
            }
            
            // Fallback to querying via cursor position (this generates the problematic responses)
            // But don't do this during cleanup
            if (!_disposed)
            {
                // Save cursor position
                WriteCSISequenceToTerminal(TerminalEncoding.GetBytes("s"));
                
                // Move cursor to a very large position
                SetCursorPosition(5000, 5000);
                
                // Request cursor position report
                WriteCSISequenceToTerminal(TerminalEncoding.GetBytes("6n"));
                Flush();
                
                // Restore cursor position
                WriteCSISequenceToTerminal(TerminalEncoding.GetBytes("u"));
                
                // For now, return a default size
                // A full implementation would parse the cursor position report response: ESC[row;colR
                // This requires implementing proper ANSI sequence parsing in the input handling
            }
            
            return new TerminalSize(80, 24);
        }
        catch
        {
            return new TerminalSize(80, 24);
        }
    }

    public override TerminalSize GetTerminalSize()
    {
        return QueryTerminalSize();
    }

    public override void EnterPrivateMode()
    {
        base.EnterPrivateMode();
        _inPrivateMode = true;
    }

    public override void ExitPrivateMode()
    {
        base.ExitPrivateMode();
        _inPrivateMode = false;
    }

    public override KeyStroke? PollInput()
    {
        var keyStroke = base.PollInput();
        if (keyStroke != null)
        {
            HandleCtrlC(keyStroke);
        }
        return keyStroke;
    }

    public override KeyStroke ReadInput()
    {
        var keyStroke = base.ReadInput();
        HandleCtrlC(keyStroke);
        return keyStroke;
    }

    /// <summary>
    /// Handles Ctrl+C behavior based on configuration
    /// </summary>
    private void HandleCtrlC(KeyStroke keyStroke)
    {
        if (keyStroke.Character == 3 || // Ctrl+C ASCII value
            (keyStroke.Character == 'c' && keyStroke.IsCtrlDown))
        {
            if (_ctrlCBehavior == CtrlCBehavior.CtrlCKillsApplication)
            {
                RestoreTerminalAndExit();
            }
            // If Trap behavior, just let the keystroke pass through normally
        }
    }

    /// <summary>
    /// Event handler for process exit
    /// </summary>
    private void OnProcessExit(object? sender, EventArgs e)
    {
        RestoreTerminal();
    }

    /// <summary>
    /// Event handler for Ctrl+C console interrupt
    /// </summary>
    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        if (_ctrlCBehavior == CtrlCBehavior.Trap)
        {
            // Cancel the default behavior and let our input handling deal with it
            e.Cancel = true;
        }
        else
        {
            // Let the default behavior proceed but clean up first
            RestoreTerminal();
        }
    }

    /// <summary>
    /// Restores terminal settings and exits the application
    /// </summary>
    private void RestoreTerminalAndExit()
    {
        RestoreTerminal();
        Environment.Exit(1);
    }

    /// <summary>
    /// Restores the terminal to its original state
    /// </summary>
    private void RestoreTerminal()
    {
        lock (_terminalLock)
        {
            if (!_terminalAcquired)
                return;

            try
            {
                // First, drain any pending input to prevent it from being interpreted by the shell
                DrainPendingInput();
                
                // Reset colors and SGR first
                ResetColorAndSGR();
                
                // Exit private mode if we're in it
                if (_inPrivateMode)
                {
                    ExitPrivateMode();
                }

                // Make sure cursor is visible
                SetCursorVisible(true);
                
                // Clear any remaining input after mode changes
                DrainPendingInput();
                
                // Restore terminal settings (this should restore echo and canonical mode)
                RestoreTerminalSettings();
                
                // Re-enable key stroke signals
                if (_catchSpecialCharacters)
                {
                    SetKeyStrokeSignalsEnabled(true);
                }

                // Final drain after restoring settings
                DrainPendingInput();

                // Force a flush to make sure all commands are sent
                Flush();

                _terminalAcquired = false;
            }
            catch
            {
                // If restore fails, try a more aggressive approach
                try
                {
                    // Emergency restoration - force reset terminal to sane defaults
                    ForceTerminalReset();
                }
                catch
                {
                    // Last resort - ignore all errors
                }
            }
        }
    }

    /// <summary>
    /// Drains any pending input from the terminal to prevent it from being interpreted by the shell
    /// </summary>
    private void DrainPendingInput()
    {
        try
        {
            // Use stty to flush input buffer - this is more reliable than trying to read it ourselves
            RunSttyCommand("-F", "/dev/stdin", "flush");
        }
        catch
        {
            try
            {
                // Fallback: try without the -F flag
                RunSttyCommand("flush");
            }
            catch
            {
                // If stty flush doesn't work, try a different approach
                try
                {
                    // Use tcflush via a shell command
                    ExecuteCommand(new[] { "/bin/bash", "-c", "exec < /dev/tty && read -t 0.01 -n 1000 || true" });
                }
                catch
                {
                    // Final fallback: ignore - we tried our best
                }
            }
        }
    }

    /// <summary>
    /// Emergency terminal reset when normal restoration fails
    /// </summary>
    private void ForceTerminalReset()
    {
        try
        {
            // First try to clear input buffer using stty
            RunSttyCommand("flush");
        }
        catch
        {
            // Ignore if flush not supported
        }
        
        try
        {
            // Try to run reset command to restore terminal to sane state
            ExecuteCommand(new[] { "reset" });
        }
        catch
        {
            try
            {
                // Fallback: try stty sane
                RunSttyCommand("sane");
            }
            catch
            {
                // Final fallback: manually restore common settings
                try
                {
                    RunSttyCommand("echo", "icanon", "intr", "^C");
                }
                catch
                {
                    // Give up - can't restore terminal
                }
            }
        }
    }

    private bool _disposed;

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Unregister event handlers
                AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
                Console.CancelKeyPress -= OnCancelKeyPress;
                
                // Restore terminal
                RestoreTerminal();
            }
            _disposed = true;
        }
        base.Dispose(disposing);
    }
}