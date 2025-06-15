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

using System.Collections.Concurrent;
using System.Text;
using Lanterna.Core;
using Lanterna.Input;

namespace Lanterna.Terminal.Ansi;

/// <summary>
/// An abstract terminal implementing functionality for terminals using Stream/Stream. You can extend from
/// this class if your terminal implementation is using standard input and standard output but not ANSI escape codes (in
/// which case you should extend ANSITerminal). This class also contains some automatic UTF-8 to VT100 character
/// conversion when the terminal is not set to read UTF-8.
/// </summary>
public abstract class StreamBasedTerminal : AbstractTerminal
{
    /// <summary>
    /// Timeout for detecting the terminal size, in milliseconds. Default value is 5000.
    /// </summary>
    private const int TERMINAL_SIZE_TIMEOUT = 5000;

    private static readonly Encoding Utf8Reference = Encoding.UTF8;

    protected readonly Stream TerminalInput;
    protected readonly Stream TerminalOutput;
    protected readonly Encoding TerminalEncoding;

    // Input decoding would be handled by InputDecoder in full implementation
    private readonly ConcurrentQueue<KeyStroke> _keyQueue;
    private readonly object _readLock = new();

    private TerminalPosition? _lastReportedCursorPosition;

    protected StreamBasedTerminal(Stream terminalInput, Stream terminalOutput, Encoding? terminalEncoding = null)
    {
        TerminalInput = terminalInput ?? throw new ArgumentNullException(nameof(terminalInput));
        TerminalOutput = terminalOutput ?? throw new ArgumentNullException(nameof(terminalOutput));
        TerminalEncoding = terminalEncoding ?? Encoding.UTF8;
        
        _keyQueue = new ConcurrentQueue<KeyStroke>();
        _lastReportedCursorPosition = null;
    }

    /// <summary>
    /// The StreamBasedTerminal class will attempt to translate some unicode characters to VT100 if the encoding
    /// attached to this Terminal isn't UTF-8.
    /// </summary>
    public override void PutCharacter(char c)
    {
        if (TerminalTextUtils.IsPrintableCharacter(c))
        {
            WriteToTerminal(TranslateCharacter(c));
        }
    }

    /// <summary>
    /// The StreamBasedTerminal class will attempt to translate some unicode characters to VT100 if the encoding
    /// attached to this Terminal isn't UTF-8.
    /// </summary>
    public override void PutString(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;
            
        for (int i = 0; i < str.Length; i++)
        {
            PutCharacter(str[i]);
        }
    }

    /// <summary>
    /// This method will write a list of bytes directly to the output stream of the terminal.
    /// </summary>
    /// <param name="bytes">Bytes to write to the terminal (synchronized)</param>
    protected void WriteToTerminal(params byte[] bytes)
    {
        lock (TerminalOutput)
        {
            TerminalOutput.Write(bytes, 0, bytes.Length);
        }
    }

    public override byte[] EnquireTerminal(TimeSpan timeout)
    {
        lock (TerminalOutput)
        {
            TerminalOutput.WriteByte(5); // ENQ
            Flush();
        }

        // Wait for input
        DateTime startTime = DateTime.Now;
        while (!DataAvailable())
        {
            if (DateTime.Now - startTime > timeout)
            {
                return Array.Empty<byte>();
            }
            Thread.Sleep(1);
        }

        // We have at least one character, read as far as we can and return
        using var buffer = new MemoryStream();
        while (DataAvailable())
        {
            int data = TerminalInput.ReadByte();
            if (data != -1)
                buffer.WriteByte((byte)data);
        }
        return buffer.ToArray();
    }

    protected virtual bool DataAvailable()
    {
        // Simple implementation - in real implementation this would check for available data
        return TerminalInput.CanRead && TerminalInput.Length > TerminalInput.Position;
    }

    public override void Bell()
    {
        TerminalOutput.WriteByte(7); // BEL
        TerminalOutput.Flush();
    }

    /// <summary>
    /// Used by the cursor reporting methods to reset any previous position memorized, so we're guaranteed to return the
    /// next reported position
    /// </summary>
    protected void ResetMemorizedCursorPosition()
    {
        _lastReportedCursorPosition = null;
    }

    /// <summary>
    /// Waits for up to 5 seconds for a terminal cursor position report to appear in the input stream. If the timeout
    /// expires, it will return null. You should have sent the cursor position query already before
    /// calling this method.
    /// </summary>
    /// <returns>Current position of the cursor, or null if the terminal didn't report it in time.</returns>
    protected TerminalPosition? WaitForCursorPositionReport()
    {
        DateTime startTime = DateTime.Now;
        TerminalPosition? cursorPosition = _lastReportedCursorPosition;
        
        while (cursorPosition == null)
        {
            if (DateTime.Now - startTime > TimeSpan.FromMilliseconds(TERMINAL_SIZE_TIMEOUT))
            {
                return null;
            }
            
            KeyStroke? keyStroke = ReadInput(false, false);
            if (keyStroke != null)
            {
                _keyQueue.Enqueue(keyStroke);
            }
            else
            {
                Thread.Sleep(1);
            }
            cursorPosition = _lastReportedCursorPosition;
        }
        return cursorPosition;
    }

    public override KeyStroke? PollInput()
    {
        return ReadInput(false, true);
    }

    public override KeyStroke ReadInput()
    {
        var result = ReadInput(true, true);
        return result ?? throw new InvalidOperationException("Unexpected null result from blocking read");
    }

    public override KeyStroke? ReadInput(TimeSpan timeout)
    {
        DateTime endTime = DateTime.Now + timeout;
        while (DateTime.Now < endTime)
        {
            var keyStroke = ReadInput(false, true);
            if (keyStroke != null)
                return keyStroke;
            Thread.Sleep(10);
        }
        return null;
    }

    private KeyStroke? ReadInput(bool blocking, bool useKeyQueue)
    {
        while (true)
        {
            if (useKeyQueue && _keyQueue.TryDequeue(out KeyStroke? previouslyReadKey))
            {
                return previouslyReadKey;
            }

            bool lockAcquired;
            if (blocking)
            {
                Monitor.Enter(_readLock);
                lockAcquired = true;
            }
            else
            {
                lockAcquired = Monitor.TryEnter(_readLock);
                if (!lockAcquired)
                    return null;
            }

            try
            {
                // Simple input decoding - full implementation would use InputDecoder
                KeyStroke? key = GetNextCharacter(blocking);
                
                // Handle screen info reports here if needed
                // For now, just return the key
                return key;
            }
            finally
            {
                if (lockAcquired)
                    Monitor.Exit(_readLock);
            }
        }
    }

    /// <summary>
    /// Simple character reading implementation. Full implementation would use InputDecoder.
    /// </summary>
    protected virtual KeyStroke? GetNextCharacter(bool blocking)
    {
        if (!DataAvailable() && !blocking)
            return null;

        int data = TerminalInput.ReadByte();
        if (data == -1)
            return null;

        char c = (char)data;
        
        // Basic key mapping - full implementation would parse escape sequences
        return c switch
        {
            (char)27 => new KeyStroke(KeyType.Escape), // ESC
            (char)13 => new KeyStroke(KeyType.Enter),  // CR
            (char)9 => new KeyStroke(KeyType.Tab),     // TAB
            (char)127 => new KeyStroke(KeyType.Backspace), // DEL
            _ when c >= 32 => new KeyStroke(c, false, false), // Printable characters
            _ => null
        };
    }

    public override void Flush()
    {
        lock (TerminalOutput)
        {
            TerminalOutput.Flush();
        }
    }

    protected Encoding GetCharset()
    {
        return TerminalEncoding;
    }

    protected byte[] TranslateCharacter(char input)
    {
        if (Utf8Reference.Equals(TerminalEncoding))
        {
            return ConvertToCharset(input);
        }

        // Convert ACS to ordinary terminal codes
        return input switch
        {
            // Note: Symbols class not yet fully ported, using literal characters for now
            '↓' => ConvertToVt100('v'),
            '←' => ConvertToVt100('<'),
            '→' => ConvertToVt100('>'),
            '↑' => ConvertToVt100('^'),
            '█' or '▓' or '▒' or '░' => ConvertToVt100((char)97),
            '♥' or '♣' or '♠' => ConvertToVt100('?'),
            '☺' or '☻' or '♦' => ConvertToVt100((char)96),
            '•' => ConvertToVt100((char)102),
            '┼' or '┿' => ConvertToVt100((char)110),
            '─' or '═' => ConvertToVt100((char)113),
            '└' or '╚' => ConvertToVt100((char)109),
            '┘' or '╝' => ConvertToVt100((char)106),
            '┬' or '╦' => ConvertToVt100((char)119),
            '┤' or '╣' => ConvertToVt100((char)117),
            '├' or '╠' => ConvertToVt100((char)116),
            '┴' or '╩' => ConvertToVt100((char)118),
            '┌' or '╔' => ConvertToVt100((char)108),
            '┐' or '╗' => ConvertToVt100((char)107),
            '│' or '║' => ConvertToVt100((char)120),
            _ => ConvertToCharset(input)
        };
    }

    private byte[] ConvertToVt100(char code)
    {
        // Warning! This might be terminal type specific!
        // So far it's worked everywhere I've tried it (xterm, gnome-terminal, putty)
        return new byte[] { 27, 40, 48, (byte)code, 27, 40, 66 };
    }

    private byte[] ConvertToCharset(char input)
    {
        return TerminalEncoding.GetBytes(input.ToString());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Don't close the streams here as they might be used elsewhere
            // If someone uses lanterna just temporarily and want to switch back to using Console streams manually,
            // they won't be too happy if we closed the streams
        }
        base.Dispose(disposing);
    }
}