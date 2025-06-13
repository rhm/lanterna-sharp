/*
 * This file demonstrates the UnixTerminal functionality of the Lanterna C# port.
 * It shows how to use the terminal for interactive applications.
 */

using System.Text;
using Lanterna.Core;
using Lanterna.Input;
using Lanterna.Terminal;

namespace ExDrawRectangle;

/// <summary>
/// Interactive terminal example demonstrating UnixTerminal capabilities
/// </summary>
public static class TerminalExample
{
    public static void RunTerminalDemo()
    {
        Console.WriteLine("Starting Lanterna Terminal Demo...");
        Console.WriteLine("This will take control of your terminal.");
        Console.WriteLine("Press any key to continue (or Ctrl+C to cancel)");
        Console.ReadKey();

        UnixTerminal? terminal = null;
        bool cleanupCompleted = false;
        
        // Set up Ctrl+C handler to ensure cleanup
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // Prevent immediate exit
            if (!cleanupCompleted)
            {
                try
                {
                    terminal?.Dispose();
                }
                catch { }
                cleanupCompleted = true;
            }
            Environment.Exit(0);
        };
        
        try
        {
            terminal = new UnixTerminal();
            
            // Enter private mode (alternate screen buffer)
            terminal.EnterPrivateMode();
            terminal.ClearScreen();
            terminal.SetCursorVisible(false);

            // Get terminal size
            var terminalSize = terminal.GetTerminalSize();
            
            // Draw a welcome message
            DrawWelcomeScreen(terminal, terminalSize);
            
            // Interactive drawing loop
            InteractiveDrawingLoop(terminal, terminalSize);
        }
        finally
        {
            if (!cleanupCompleted)
            {
                try
                {
                    terminal?.Dispose();
                }
                catch { }
                cleanupCompleted = true;
            }
        }
    }

    private static void DrawWelcomeScreen(ITerminal terminal, TerminalSize size)
    {
        // Set colors and draw border
        terminal.SetForegroundColor(new AnsiTextColor(AnsiColor.BlueBright));
        terminal.SetBackgroundColor(new AnsiTextColor(AnsiColor.Default));
        
        // Draw border
        for (int x = 0; x < size.Columns; x++)
        {
            terminal.SetCursorPosition(x, 0);
            terminal.PutCharacter('═');
            terminal.SetCursorPosition(x, size.Rows - 1);
            terminal.PutCharacter('═');
        }
        
        for (int y = 1; y < size.Rows - 1; y++)
        {
            terminal.SetCursorPosition(0, y);
            terminal.PutCharacter('║');
            terminal.SetCursorPosition(size.Columns - 1, y);
            terminal.PutCharacter('║');
        }
        
        // Draw corners
        terminal.SetCursorPosition(0, 0);
        terminal.PutCharacter('╔');
        terminal.SetCursorPosition(size.Columns - 1, 0);
        terminal.PutCharacter('╗');
        terminal.SetCursorPosition(0, size.Rows - 1);
        terminal.PutCharacter('╚');
        terminal.SetCursorPosition(size.Columns - 1, size.Rows - 1);
        terminal.PutCharacter('╝');

        // Title and instructions
        terminal.SetForegroundColor(new AnsiTextColor(AnsiColor.YellowBright));
        terminal.EnableSGR(SGR.Bold);
        
        string title = "Lanterna C# Terminal Demo";
        terminal.SetCursorPosition((size.Columns - title.Length) / 2, 2);
        terminal.PutString(title);
        
        terminal.DisableSGR(SGR.Bold);
        terminal.SetForegroundColor(new AnsiTextColor(AnsiColor.White));
        
        string[] instructions = 
        {
            "Use WASD keys to move the cursor around",
            "Press SPACE to draw a star",
            "Press C to change colors",
            "Press R to clear the screen",
            "Press Q or ESC to quit",
            "",
            "Current terminal size: " + size.Columns + "x" + size.Rows
        };
        
        for (int i = 0; i < instructions.Length; i++)
        {
            string instruction = instructions[i];
            terminal.SetCursorPosition((size.Columns - instruction.Length) / 2, 4 + i);
            terminal.PutString(instruction);
        }
        
        terminal.Flush();
    }

    private static void InteractiveDrawingLoop(ITerminal terminal, TerminalSize size)
    {
        var currentPos = new TerminalPosition(size.Columns / 2, size.Rows / 2);
        var colors = new ITextColor[]
        {
            new AnsiTextColor(AnsiColor.Red),
            new AnsiTextColor(AnsiColor.Green),
            new AnsiTextColor(AnsiColor.Blue),
            new AnsiTextColor(AnsiColor.Yellow),
            new AnsiTextColor(AnsiColor.Magenta),
            new AnsiTextColor(AnsiColor.Cyan),
            new RgbColor(255, 128, 0), // Orange
            new RgbColor(128, 255, 128) // Light green
        };
        int currentColorIndex = 0;
        
        // Set initial position
        terminal.SetCursorPosition(currentPos);
        terminal.SetCursorVisible(true);
        terminal.Flush();
        
        bool running = true;
        while (running)
        {
            var keyStroke = terminal.ReadInput();
            
            if (keyStroke.KeyType == KeyType.Character && keyStroke.Character != null)
            {
                char key = char.ToLower(keyStroke.Character.Value);
                
                switch (key)
                {
                    case 'w':
                        if (currentPos.Row > 1)
                            currentPos = currentPos.WithRow(currentPos.Row - 1);
                        break;
                        
                    case 's':
                        if (currentPos.Row < size.Rows - 2)
                            currentPos = currentPos.WithRow(currentPos.Row + 1);
                        break;
                        
                    case 'a':
                        if (currentPos.Column > 1)
                            currentPos = currentPos.WithColumn(currentPos.Column - 1);
                        break;
                        
                    case 'd':
                        if (currentPos.Column < size.Columns - 2)
                            currentPos = currentPos.WithColumn(currentPos.Column + 1);
                        break;
                        
                    case ' ':
                        // Draw a star at current position
                        terminal.SetForegroundColor(colors[currentColorIndex]);
                        terminal.PutCharacter('★');
                        terminal.SetCursorPosition(currentPos);
                        break;
                        
                    case 'c':
                        // Change color
                        currentColorIndex = (currentColorIndex + 1) % colors.Length;
                        terminal.SetForegroundColor(colors[currentColorIndex]);
                        break;
                        
                    case 'r':
                        // Clear and redraw
                        DrawWelcomeScreen(terminal, size);
                        terminal.SetCursorPosition(currentPos);
                        terminal.SetCursorVisible(true);
                        break;
                        
                    case 'q':
                        running = false;
                        break;
                }
            }
            else if (keyStroke.KeyType == KeyType.Escape)
            {
                running = false;
            }
            else if (keyStroke.KeyType == KeyType.ArrowUp && currentPos.Row > 1)
            {
                currentPos = currentPos.WithRow(currentPos.Row - 1);
            }
            else if (keyStroke.KeyType == KeyType.ArrowDown && currentPos.Row < size.Rows - 2)
            {
                currentPos = currentPos.WithRow(currentPos.Row + 1);
            }
            else if (keyStroke.KeyType == KeyType.ArrowLeft && currentPos.Column > 1)
            {
                currentPos = currentPos.WithColumn(currentPos.Column - 1);
            }
            else if (keyStroke.KeyType == KeyType.ArrowRight && currentPos.Column < size.Columns - 2)
            {
                currentPos = currentPos.WithColumn(currentPos.Column + 1);
            }
            
            // Update cursor position
            terminal.SetCursorPosition(currentPos);
            terminal.Flush();
        }
        
        // Show goodbye message
        terminal.ClearScreen();
        terminal.SetForegroundColor(new AnsiTextColor(AnsiColor.GreenBright));
        terminal.EnableSGR(SGR.Bold);
        
        string goodbye = "Thanks for trying Lanterna C#!";
        terminal.SetCursorPosition((size.Columns - goodbye.Length) / 2, size.Rows / 2);
        terminal.PutString(goodbye);
        
        terminal.DisableSGR(SGR.Bold);
        terminal.SetForegroundColor(new AnsiTextColor(AnsiColor.White));
        
        string pressKey = "Press any key to exit...";
        terminal.SetCursorPosition((size.Columns - pressKey.Length) / 2, size.Rows / 2 + 2);
        terminal.PutString(pressKey);
        
        terminal.Flush();
        terminal.ReadInput();
    }
}