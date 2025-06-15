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
using Lanterna.Core;
using Lanterna.Input;

namespace Lanterna.Terminal;

/// <summary>
/// Class containing graphics code for ANSI compliant text terminals and terminal emulators. All the methods inside of
/// this class uses ANSI escape codes written to the underlying output stream.
/// </summary>
public abstract class ANSITerminal : AbstractTerminal
{
    protected readonly Stream TerminalInput;
    protected readonly Stream TerminalOutput;
    protected readonly Encoding TerminalEncoding;
    private bool _inPrivateMode;
    private readonly Queue<KeyStroke> _inputQueue;
    private readonly object _inputLock = new();

    /// <summary>
    /// Returns true if the terminal is currently in private mode
    /// </summary>
    public bool IsInPrivateMode => _inPrivateMode;

    protected ANSITerminal(Stream terminalInput, Stream terminalOutput, Encoding? terminalEncoding = null)
    {
        TerminalInput = terminalInput ?? throw new ArgumentNullException(nameof(terminalInput));
        TerminalOutput = terminalOutput ?? throw new ArgumentNullException(nameof(terminalOutput));
        TerminalEncoding = terminalEncoding ?? Encoding.UTF8;
        _inPrivateMode = false;
        _inputQueue = new Queue<KeyStroke>();
    }

    protected void WriteCSISequenceToTerminal(params byte[] tail)
    {
        byte[] completeSequence = new byte[tail.Length + 2];
        completeSequence[0] = 0x1b; // ESC
        completeSequence[1] = (byte)'[';
        Array.Copy(tail, 0, completeSequence, 2, tail.Length);
        WriteToTerminal(completeSequence);
    }

    protected void WriteSGRSequenceToTerminal(params byte[] sgrParameters)
    {
        byte[] completeSequence = new byte[sgrParameters.Length + 3];
        completeSequence[0] = 0x1b; // ESC
        completeSequence[1] = (byte)'[';
        completeSequence[completeSequence.Length - 1] = (byte)'m';
        Array.Copy(sgrParameters, 0, completeSequence, 2, sgrParameters.Length);
        WriteToTerminal(completeSequence);
    }

    protected void WriteOSCSequenceToTerminal(params byte[] tail)
    {
        byte[] completeSequence = new byte[tail.Length + 2];
        completeSequence[0] = 0x1b; // ESC
        completeSequence[1] = (byte)']';
        Array.Copy(tail, 0, completeSequence, 2, tail.Length);
        WriteToTerminal(completeSequence);
    }

    protected void WriteToTerminal(byte[] data)
    {
        TerminalOutput.Write(data, 0, data.Length);
    }

    protected void WriteToTerminal(string text)
    {
        byte[] data = TerminalEncoding.GetBytes(text);
        WriteToTerminal(data);
    }

    public override void SetForegroundColor(ITextColor color)
    {
        WriteSGRSequenceToTerminal(color.GetForegroundSGRSequence());
    }

    public override void SetBackgroundColor(ITextColor color)
    {
        WriteSGRSequenceToTerminal(color.GetBackgroundSGRSequence());
    }

    public override void EnableSGR(SGR sgr)
    {
        switch (sgr)
        {
            case SGR.Blink:
                WriteCSISequenceToTerminal((byte)'5', (byte)'m');
                break;
            case SGR.Bold:
                WriteCSISequenceToTerminal((byte)'1', (byte)'m');
                break;
            case SGR.Bordered:
                WriteCSISequenceToTerminal((byte)'5', (byte)'1', (byte)'m');
                break;
            case SGR.Circled:
                WriteCSISequenceToTerminal((byte)'5', (byte)'2', (byte)'m');
                break;
            case SGR.CrossedOut:
                WriteCSISequenceToTerminal((byte)'9', (byte)'m');
                break;
            case SGR.Fraktur:
                WriteCSISequenceToTerminal((byte)'2', (byte)'0', (byte)'m');
                break;
            case SGR.Reverse:
                WriteCSISequenceToTerminal((byte)'7', (byte)'m');
                break;
            case SGR.Underline:
                WriteCSISequenceToTerminal((byte)'4', (byte)'m');
                break;
            case SGR.Italic:
                WriteCSISequenceToTerminal((byte)'3', (byte)'m');
                break;
        }
    }

    public override void DisableSGR(SGR sgr)
    {
        switch (sgr)
        {
            case SGR.Blink:
                WriteCSISequenceToTerminal((byte)'2', (byte)'5', (byte)'m');
                break;
            case SGR.Bold:
                WriteCSISequenceToTerminal((byte)'2', (byte)'2', (byte)'m');
                break;
            case SGR.Bordered:
                WriteCSISequenceToTerminal((byte)'5', (byte)'4', (byte)'m');
                break;
            case SGR.Circled:
                WriteCSISequenceToTerminal((byte)'5', (byte)'4', (byte)'m');
                break;
            case SGR.CrossedOut:
                WriteCSISequenceToTerminal((byte)'2', (byte)'9', (byte)'m');
                break;
            case SGR.Fraktur:
                WriteCSISequenceToTerminal((byte)'2', (byte)'3', (byte)'m');
                break;
            case SGR.Reverse:
                WriteCSISequenceToTerminal((byte)'2', (byte)'7', (byte)'m');
                break;
            case SGR.Underline:
                WriteCSISequenceToTerminal((byte)'2', (byte)'4', (byte)'m');
                break;
            case SGR.Italic:
                WriteCSISequenceToTerminal((byte)'2', (byte)'3', (byte)'m');
                break;
        }
    }

    public override void ResetColorAndSGR()
    {
        WriteCSISequenceToTerminal((byte)'0', (byte)'m');
    }

    public override void ClearScreen()
    {
        WriteCSISequenceToTerminal((byte)'2', (byte)'J');
    }

    public override void EnterPrivateMode()
    {
        if (_inPrivateMode)
            throw new InvalidOperationException("Cannot call EnterPrivateMode() when already in private mode");
        
        WriteCSISequenceToTerminal((byte)'?', (byte)'1', (byte)'0', (byte)'4', (byte)'9', (byte)'h');
        Flush();
        _inPrivateMode = true;
    }

    public override void ExitPrivateMode()
    {
        if (!_inPrivateMode)
            throw new InvalidOperationException("Cannot call ExitPrivateMode() when not in private mode");
        
        ResetColorAndSGR();
        SetCursorVisible(true);
        WriteCSISequenceToTerminal((byte)'?', (byte)'1', (byte)'0', (byte)'4', (byte)'9', (byte)'l');
        Flush();
        _inPrivateMode = false;
    }

    public override void SetCursorPosition(int x, int y)
    {
        string sequence = $"{y + 1};{x + 1}H";
        WriteCSISequenceToTerminal(TerminalEncoding.GetBytes(sequence));
    }

    public override void SetCursorPosition(TerminalPosition position)
    {
        SetCursorPosition(position.Column, position.Row);
    }

    public override TerminalPosition GetCursorPosition()
    {
        // Request cursor position report
        WriteCSISequenceToTerminal(TerminalEncoding.GetBytes("6n"));
        Flush();
        
        // For now, return a default position as implementing the response parsing is complex
        // In a full implementation, this would read the response: ESC[row;colR
        return new TerminalPosition(0, 0);
    }

    public override void SetCursorVisible(bool visible)
    {
        string sequence = $"?25{(visible ? "h" : "l")}";
        WriteCSISequenceToTerminal(TerminalEncoding.GetBytes(sequence));
    }

    public override void PutCharacter(char c)
    {
        if (TerminalTextUtils.IsControlCharacter(c) && c != '\t')
            return; // Skip control characters except tab
            
        byte[] data = TerminalEncoding.GetBytes(c.ToString());
        WriteToTerminal(data);
    }

    public override void PutString(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;
            
        // Filter out control characters except tab, newline
        var filteredChars = str.Where(c => !TerminalTextUtils.IsControlCharacter(c) || c == '\t' || c == '\n');
        string filteredString = new string(filteredChars.ToArray());
        
        WriteToTerminal(filteredString);
    }

    public override void Bell()
    {
        WriteToTerminal(new byte[] { 0x07 }); // BEL character
    }

    public override void Flush()
    {
        TerminalOutput.Flush();
    }

    public override byte[] EnquireTerminal(TimeSpan timeout)
    {
        WriteToTerminal(new byte[] { 0x05 }); // ENQ character
        Flush();
        
        // For now, return empty array. Full implementation would read the response.
        return Array.Empty<byte>();
    }

    // Simple input implementation - full version would need proper ANSI sequence parsing
    public override KeyStroke? PollInput()
    {
        lock (_inputLock)
        {
            if (_inputQueue.Count > 0)
                return _inputQueue.Dequeue();
        }

        if (TerminalInput.CanRead && TerminalInput.Length > TerminalInput.Position)
        {
            int data = TerminalInput.ReadByte();
            if (data != -1)
            {
                char c = (char)data;
                
                // Basic key mapping - full implementation would parse escape sequences
                if (c == 27) // ESC
                {
                    return new KeyStroke(KeyType.Escape);
                }
                else if (c == 13) // CR
                {
                    return new KeyStroke(KeyType.Enter);
                }
                else if (c == 9) // TAB
                {
                    return new KeyStroke(KeyType.Tab);
                }
                else if (c == 127) // DEL
                {
                    return new KeyStroke(KeyType.Backspace);
                }
                else if (c >= 32) // Printable characters
                {
                    return new KeyStroke(c, false, false);
                }
            }
        }
        
        return null;
    }

    public override KeyStroke ReadInput()
    {
        KeyStroke? keyStroke;
        do
        {
            keyStroke = PollInput();
            if (keyStroke == null)
            {
                Thread.Sleep(10); // Simple polling - full implementation would use proper blocking I/O
            }
        } while (keyStroke == null);
        
        return keyStroke;
    }

    public override KeyStroke? ReadInput(TimeSpan timeout)
    {
        var endTime = DateTime.Now + timeout;
        while (DateTime.Now < endTime)
        {
            var keyStroke = PollInput();
            if (keyStroke != null)
                return keyStroke;
            Thread.Sleep(10);
        }
        return null;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_inPrivateMode)
            {
                try
                {
                    ExitPrivateMode();
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
        }
        base.Dispose(disposing);
    }
}