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
using Lanterna.Terminal;

namespace Lanterna;

/// <summary>
/// This class provides a unified way for the test programs to get their terminal objects.
/// It extends DefaultTerminalFactory with command-line argument parsing capabilities.
/// </summary>
public class TestTerminalFactory : DefaultTerminalFactory
{
    /// <summary>
    /// Creates a new TestTerminalFactory with default settings
    /// </summary>
    public TestTerminalFactory()
    {
    }

    /// <summary>
    /// Creates a new TestTerminalFactory and parses the provided command-line arguments
    /// </summary>
    /// <param name="args">Command-line arguments to parse</param>
    public TestTerminalFactory(string[] args)
    {
        ParseArgs(args);
    }

    /// <summary>
    /// Parses command-line arguments to configure the terminal factory
    /// </summary>
    /// <param name="args">Array of command-line arguments</param>
    public void ParseArgs(string[]? args)
    {
        if (args == null) return;

        foreach (string arg in args)
        {
            if (arg == null) continue;

            string[] tokens = arg.Split('=', 2);
            string command = tokens[0]; // only the part before "="
            string parameter = tokens.Length > 1 ? tokens[1] : "";

            switch (command)
            {
                case "--text-terminal":
                case "--no-swing":
                    // In the C# port, we don't have Swing/AWT, so this just ensures text terminal
                    SetForceTextTerminal(true);
                    break;

                case "--awt":
                case "--swing":
                    // These options are not relevant for the C# port since we don't have GUI terminal emulators
                    // We'll just log that these options are ignored
                    Console.WriteLine($"Warning: {command} option is not supported in the C# port, ignoring.");
                    break;

                case "--mouse-click":
                    SetMouseCaptureMode(MouseCaptureMode.CLICK_RELEASE);
                    break;

                case "--mouse-drag":
                    SetMouseCaptureMode(MouseCaptureMode.CLICK_RELEASE_DRAG);
                    break;

                case "--mouse-move":
                    SetMouseCaptureMode(MouseCaptureMode.CLICK_RELEASE_DRAG_MOVE);
                    break;

                case "--telnet-port":
                    // Telnet functionality is not implemented in the current C# port
                    // We'll parse the value but warn that it's not supported
                    int port = 1024; // default for option w/o param
                    if (!string.IsNullOrEmpty(parameter))
                    {
                        if (!int.TryParse(parameter, out port))
                        {
                            port = 1024;
                        }
                    }
                    Console.WriteLine($"Warning: --telnet-port={port} option is not supported in the C# port, ignoring.");
                    break;

                case "--with-timeout":
                    // Input timeout functionality is not yet implemented in the current C# port
                    // We'll parse the value but warn that it's not supported
                    int inputTimeout = 40; // default for option w/o param
                    if (!string.IsNullOrEmpty(parameter))
                    {
                        if (!int.TryParse(parameter, out inputTimeout))
                        {
                            inputTimeout = 40;
                        }
                    }
                    Console.WriteLine($"Warning: --with-timeout={inputTimeout} option is not supported in the C# port, ignoring.");
                    break;

                default:
                    // Unknown argument - we'll just ignore it silently like the Java version
                    break;
            }
        }
    }
}