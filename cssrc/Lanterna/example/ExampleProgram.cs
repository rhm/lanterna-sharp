using System;
using Lanterna.Terminal;
using Lanterna.Terminal.Ansi;
using Lanterna.Terminal.Win32;
using Lanterna.Input;

namespace Lanterna.Example
{
    /// <summary>
    /// Simple demonstration program using the Lanterna C# API.
    /// Different displays can be selected by pressing the number keys.
    /// </summary>
    public static class ExampleProgram
    {
        private static Terminal CreateTerminal()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT
                ? new WindowsTerminal()
                : new UnixTerminal();
        }

        private static void WaitForKey(Terminal terminal)
        {
            terminal.PutString("\nPress any key to return...");
            terminal.Flush();
            terminal.ReadInput();
        }

        private static void DisplayBasic(Terminal terminal)
        {
            terminal.ClearScreen();
            terminal.SetCursorPosition(0, 0);
            terminal.PutString("Basic terminal output\n");
            terminal.PutString("This uses the low level Terminal API.");
            terminal.Flush();
            WaitForKey(terminal);
        }

        private static void DisplayColors(Terminal terminal)
        {
            terminal.ClearScreen();
            terminal.SetCursorPosition(0, 0);
            terminal.SetForegroundColor(new TextColor.RGB(255, 0, 0));
            terminal.PutString("Red Text\n");
            terminal.SetForegroundColor(new TextColor.RGB(0, 255, 0));
            terminal.PutString("Green Text\n");
            terminal.SetForegroundColor(new TextColor.RGB(255, 255, 0));
            terminal.EnableSGR(SGR.Bold);
            terminal.PutString("Bold Yellow Text\n");
            terminal.ResetColorAndSGR();
            terminal.Flush();
            WaitForKey(terminal);
        }

        private static void DisplayBox(Terminal terminal)
        {
            terminal.ClearScreen();
            TerminalSize size = terminal.GetTerminalSize();
            int w = size.Columns;
            int h = size.Rows;
            for (int x = 0; x < w; x++)
            {
                terminal.SetCursorPosition(x, 0);
                terminal.PutCharacter('-');
                terminal.SetCursorPosition(x, h - 1);
                terminal.PutCharacter('-');
            }
            for (int y = 1; y < h - 1; y++)
            {
                terminal.SetCursorPosition(0, y);
                terminal.PutCharacter('|');
                terminal.SetCursorPosition(w - 1, y);
                terminal.PutCharacter('|');
            }
            terminal.SetCursorPosition(2, 2);
            terminal.PutString("Simple ASCII box using cursor movement.");
            terminal.Flush();
            WaitForKey(terminal);
        }

        public static void Main()
        {
            using Terminal terminal = CreateTerminal();
            terminal.EnterPrivateMode();
            terminal.ClearScreen();

            bool running = true;
            while (running)
            {
                terminal.SetCursorPosition(0, 0);
                terminal.PutString("1 - Basic output\n2 - Colors\n3 - Box drawing\nq - Quit\n");
                terminal.Flush();
                KeyStroke key = terminal.ReadInput();
                if (key != null && key.KeyType == KeyType.Character)
                {
                    switch (key.Character)
                    {
                        case '1':
                            DisplayBasic(terminal);
                            break;
                        case '2':
                            DisplayColors(terminal);
                            break;
                        case '3':
                            DisplayBox(terminal);
                            break;
                        case 'q':
                        case 'Q':
                            running = false;
                            break;
                    }
                }
            }

            terminal.ExitPrivateMode();
        }
    }
}
