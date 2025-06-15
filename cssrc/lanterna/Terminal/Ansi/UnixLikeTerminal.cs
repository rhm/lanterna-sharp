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

using System.Text;
using Lanterna.Input;

namespace Lanterna.Terminal.Ansi;

/// <summary>
/// Base class for all terminals that generally behave like Unix terminals. This class defined a number of abstract
/// methods that needs to be implemented which are all used to setup the terminal environment (turning off echo,
/// canonical mode, etc) and also a control variable for how to react to CTRL+c keystroke.
/// </summary>
public abstract class UnixLikeTerminal : ANSITerminal
{
    /// <summary>
    /// This enum lets you control how Lanterna will handle a ctrl+c keystroke from the user.
    /// </summary>
    public enum CtrlCBehaviour
    {
        /// <summary>
        /// Pressing ctrl+c doesn't kill the application, it will be added to the input queue as any other key stroke
        /// </summary>
        Trap,
        
        /// <summary>
        /// Pressing ctrl+c will restore the terminal and kill the application as it normally does with terminal
        /// applications. Lanterna will restore the terminal and then call Environment.Exit(1) for this.
        /// </summary>
        CtrlCKillsApplication,
    }

    private readonly CtrlCBehaviour _terminalCtrlCBehaviour;
    private readonly bool _catchSpecialCharacters;
    private bool _acquired;

    protected UnixLikeTerminal(Stream terminalInput,
                            Stream terminalOutput,
                            Encoding? terminalCharset,
                            CtrlCBehaviour terminalCtrlCBehaviour)
        : base(terminalInput, terminalOutput, terminalCharset)
    {
        _acquired = false;

        string catchSpecialCharactersPropValue = Environment.GetEnvironmentVariable(
            "LANTERNA_CATCH_SPECIAL_CHARACTERS") ?? "";
        _catchSpecialCharacters = !"false".Equals(catchSpecialCharactersPropValue.Trim(), StringComparison.OrdinalIgnoreCase);
        _terminalCtrlCBehaviour = terminalCtrlCBehaviour;
        
        // Set up shutdown hook equivalent
        AppDomain.CurrentDomain.ProcessExit += (_, _) => ExitPrivateModeAndRestoreState();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // Prevent immediate termination
            ExitPrivateModeAndRestoreState();
            if (_terminalCtrlCBehaviour == CtrlCBehaviour.CtrlCKillsApplication)
            {
                Environment.Exit(1);
            }
        };
        
        Acquire();
    }

    /// <summary>
    /// Effectively taking over the terminal and enabling it for Lanterna to use, by turning off echo and canonical mode,
    /// adding resize listeners and optionally trap unix signals. This should be called automatically by the constructor
    /// of any end-user class extending from UnixLikeTerminal
    /// </summary>
    protected virtual void Acquire()
    {
        // Make sure to set an initial size
        OnResized(80, 24);

        SaveTerminalSettings();
        CanonicalMode(false);
        KeyEchoEnabled(false);
        if (_catchSpecialCharacters)
        {
            KeyStrokeSignalsEnabled(false);
        }
        RegisterTerminalResizeListener(() =>
        {
            // This will trigger a resize notification as the size will be different than before
            try
            {
                GetTerminalSize();
            }
            catch
            {
                // Not much to do here, we can't re-throw it
            }
        });
        _acquired = true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ExitPrivateModeAndRestoreState();
            _acquired = false;
        }
        base.Dispose(disposing);
    }

    public override KeyStroke? PollInput()
    {
        // Check if we have ctrl+c coming
        KeyStroke? key = base.PollInput();
        CheckCtrlC(key);
        return key;
    }

    public override KeyStroke ReadInput()
    {
        // Check if we have ctrl+c coming
        KeyStroke key = base.ReadInput();
        CheckCtrlC(key);
        return key;
    }

    protected CtrlCBehaviour GetTerminalCtrlCBehaviour()
    {
        return _terminalCtrlCBehaviour;
    }

    protected abstract void RegisterTerminalResizeListener(Action onResize);

    /// <summary>
    /// Stores the current terminal device settings (the ones that are modified through this interface) so that they can
    /// be restored later using RestoreTerminalSettings()
    /// </summary>
    protected abstract void SaveTerminalSettings();

    /// <summary>
    /// Restores the terminal settings from last time SaveTerminalSettings() was called
    /// </summary>
    protected abstract void RestoreTerminalSettings();

    private void RestoreTerminalSettingsAndKeyStrokeSignals()
    {
        RestoreTerminalSettings();
        if (_catchSpecialCharacters)
        {
            KeyStrokeSignalsEnabled(true);
        }
    }

    /// <summary>
    /// Enables or disable key echo mode, which means when the user press a key, the terminal will immediately print that
    /// key to the terminal. Normally for Lanterna, this should be turned off so the software can take the key as an
    /// input event, put it on the input queue and then depending on the code decide what to do with it.
    /// </summary>
    /// <param name="enabled">true if key echo should be enabled, false otherwise</param>
    protected abstract void KeyEchoEnabled(bool enabled);

    /// <summary>
    /// In canonical mode, data are accumulated in a line editing buffer, and do not become "available for reading" until
    /// line editing has been terminated by the user sending a line delimiter character. This is usually the default mode
    /// for a terminal. Lanterna wants to read each character as they are typed, without waiting for the final newline,
    /// so it will attempt to turn canonical mode off on initialization.
    /// </summary>
    /// <param name="enabled">true if canonical input mode should be enabled, false otherwise</param>
    protected abstract void CanonicalMode(bool enabled);

    /// <summary>
    /// This method causes certain keystrokes (at the moment only ctrl+c) to be passed in to the program as a regular
    /// KeyStroke instead of as a signal to the process. For example,
    /// ctrl+c will normally send an interrupt that causes the process to shut down, but this method will make it pass
    /// in ctrl+c as a regular KeyStroke instead. You can of course still
    /// make ctrl+c kill the application through your own input handling if you like.
    /// </summary>
    /// <param name="enabled">Pass in true if you want keystrokes to generate system signals (like process interrupt),
    /// false if you want lanterna to catch and interpret these keystrokes are regular keystrokes</param>
    protected abstract void KeyStrokeSignalsEnabled(bool enabled);

    private void CheckCtrlC(KeyStroke? key)
    {
        if (key != null
            && _terminalCtrlCBehaviour == CtrlCBehaviour.CtrlCKillsApplication
            && key.Character != null
            && key.Character == 'c'
            && !key.IsAltDown
            && key.IsCtrlDown)
        {
            if (IsInPrivateMode)
            {
                ExitPrivateMode();
            }
            Environment.Exit(1);
        }
    }

    private void ExitPrivateModeAndRestoreState()
    {
        if (!_acquired)
        {
            return;
        }
        try
        {
            if (IsInPrivateMode)
            {
                ExitPrivateMode();
            }
        }
        catch
        {
            // Ignore exceptions during cleanup
        }

        try
        {
            RestoreTerminalSettingsAndKeyStrokeSignals();
        }
        catch
        {
            // Ignore exceptions during cleanup
        }
    }
}