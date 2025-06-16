/*
 * This file demonstrates the Screen layer functionality of the Lanterna C# port.
 * It shows how to use buffered screen operations for efficient terminal applications.
 */

using System.Text;
using Lanterna.Core;
using Lanterna.Input;
using Lanterna.Screen;
using Lanterna.Terminal;

namespace ExDrawRectangle;

/// <summary>
/// Comprehensive screen example demonstrating buffered screen operations
/// </summary>
public static class ScreenExample
{
    public static void RunScreenDemo()
    {
        Console.WriteLine("Starting Lanterna Screen Layer Demo...");
        Console.WriteLine("This demonstrates buffered screen operations.");
        Console.WriteLine("Press any key to continue (or Ctrl+C to cancel)");
        Console.ReadKey();

        UnixTerminal? terminal = null;
        TerminalScreen? screen = null;
        bool cleanupCompleted = false;
        
        // Set up Ctrl+C handler to ensure cleanup
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // Prevent immediate exit
            if (!cleanupCompleted)
            {
                try
                {
                    screen?.StopScreen();
                    screen?.Dispose();
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
            screen = new TerminalScreen(terminal);
            
            screen.StartScreen();
            
            // Run the interactive screen application
            RunScreenApplication(screen);
        }
        catch (Exception ex)
        {
            // Log the error but continue with cleanup
            Console.Error.WriteLine($"Error in screen demo: {ex.Message}");
        }
        finally
        {
            if (!cleanupCompleted)
            {
                // Ensure proper cleanup in the correct order
                try
                {
                    screen?.StopScreen();
                }
                catch
                {
                    // Ignore errors during screen stop
                }
                
                try
                {
                    screen?.Dispose();
                }
                catch
                {
                    // Ignore errors during screen dispose
                }
                
                try
                {
                    terminal?.Dispose();
                }
                catch
                {
                    // Ignore errors during terminal dispose
                }
                
                cleanupCompleted = true;
            }
            
            // If terminal is still not responding properly after cleanup,
            // the user can run the reset script manually
            if (!cleanupCompleted)
            {
                Console.WriteLine("\nIf your terminal is not responding properly, try running: ./reset-terminal.sh");
            }
        }
    }

    private static void RunScreenApplication(IScreen screen)
    {
        var graphics = (ScreenTextGraphics)screen.NewTextGraphics();
        var random = new Random();
        
        bool running = true;
        int frame = 0;
        var startTime = DateTime.Now;
        
        while (running)
        {
            // Handle resize
            var newSize = screen.DoResizeIfNecessary();
            if (newSize != null)
            {
                DrawResizeNotification(graphics, newSize.Value);
            }
            
            // Clear screen and draw current frame
            screen.Clear();
            DrawFrame(graphics, frame, startTime);
            
            // Show cursor at a specific position
            screen.SetCursorPosition(new TerminalPosition(5, screen.GetTerminalSize().Rows - 2));
            
            // Refresh the screen (this actually displays everything)
            screen.Refresh();
            
            // Check for input (non-blocking)
            var input = screen.PollInput();
            if (input != null)
            {
                running = HandleInput(input, screen, graphics, random);
            }
            
            frame++;
            Thread.Sleep(100); // ~10 FPS
        }
        
        // Show exit screen
        ShowExitScreen(graphics);
        screen.Refresh();
        screen.ReadInput(); // Wait for final keypress
    }

    private static void DrawFrame(ScreenTextGraphics graphics, int frame, DateTime startTime)
    {
        var size = graphics.Size;
        var elapsed = DateTime.Now - startTime;
        
        // Draw border
        DrawBorder(graphics, size);
        
        // Draw title
        graphics.ForegroundColor = new AnsiTextColor(AnsiColor.YellowBright);
        graphics.EnableModifiers(SGR.Bold);
        string title = "Lanterna C# Screen Layer Demo";
        graphics.PutString((size.Columns - title.Length) / 2, 1, title);
        graphics.DisableModifiers(SGR.Bold);
        
        // Draw info panel
        DrawInfoPanel(graphics, size, frame, elapsed);
        
        // Draw animated content
        DrawAnimatedContent(graphics, size, frame);
        
        // Draw help text
        DrawHelpText(graphics, size);
    }

    private static void DrawBorder(ScreenTextGraphics graphics, TerminalSize size)
    {
        graphics.ForegroundColor = new AnsiTextColor(AnsiColor.BlueBright);
        
        // Draw corners
        graphics.SetCharacter(0, 0, '╔');
        graphics.SetCharacter(size.Columns - 1, 0, '╗');
        graphics.SetCharacter(0, size.Rows - 1, '╚');
        graphics.SetCharacter(size.Columns - 1, size.Rows - 1, '╝');
        
        // Draw horizontal lines
        for (int x = 1; x < size.Columns - 1; x++)
        {
            graphics.SetCharacter(x, 0, '═');
            graphics.SetCharacter(x, size.Rows - 1, '═');
        }
        
        // Draw vertical lines
        for (int y = 1; y < size.Rows - 1; y++)
        {
            graphics.SetCharacter(0, y, '║');
            graphics.SetCharacter(size.Columns - 1, y, '║');
        }
    }

    private static void DrawInfoPanel(ScreenTextGraphics graphics, TerminalSize size, int frame, TimeSpan elapsed)
    {
        graphics.ForegroundColor = new AnsiTextColor(AnsiColor.White);
        
        var info = new[]
        {
            $"Terminal Size: {size.Columns}x{size.Rows}",
            $"Frame: {frame}",
            $"Elapsed: {elapsed:mm\\:ss\\.ff}",
            $"FPS: {(frame / Math.Max(elapsed.TotalSeconds, 0.1)):F1}",
            ""
        };
        
        for (int i = 0; i < info.Length; i++)
        {
            graphics.PutString(3, 3 + i, info[i]);
        }
    }

    private static void DrawAnimatedContent(ScreenTextGraphics graphics, TerminalSize size, int frame)
    {
        // Draw moving bouncing ball
        int ballX = (int)(Math.Sin(frame * 0.1) * (size.Columns - 10) / 2) + size.Columns / 2;
        int ballY = (int)(Math.Cos(frame * 0.08) * (size.Rows - 10) / 2) + size.Rows / 2;
        
        graphics.ForegroundColor = new AnsiTextColor(AnsiColor.RedBright);
        graphics.SetCharacter(Math.Max(1, Math.Min(ballX, size.Columns - 2)), 
                             Math.Max(1, Math.Min(ballY, size.Rows - 2)), '●');
        
        // Draw rotating pattern
        var centerX = size.Columns / 2;
        var centerY = size.Rows / 2;
        var radius = Math.Min(size.Columns, size.Rows) / 4;
        
        for (int i = 0; i < 8; i++)
        {
            double angle = (frame * 0.05) + (i * Math.PI / 4);
            int x = centerX + (int)(Math.Cos(angle) * radius);
            int y = centerY + (int)(Math.Sin(angle) * radius / 2); // Half radius for Y to account for character aspect ratio
            
            if (x >= 1 && x < size.Columns - 1 && y >= 1 && y < size.Rows - 1)
            {
                graphics.ForegroundColor = new RgbColor(
                    (int)(128 + 127 * Math.Sin(angle)),
                    (int)(128 + 127 * Math.Cos(angle)),
                    128);
                graphics.SetCharacter(x, y, '*');
            }
        }
        
        // Draw progress bar
        int progressBarY = size.Rows - 4;
        int progressBarWidth = size.Columns - 6;
        int progress = (frame / 5) % progressBarWidth;
        
        graphics.ForegroundColor = new AnsiTextColor(AnsiColor.GreenBright);
        graphics.PutString(3, progressBarY, "[");
        graphics.PutString(3 + progressBarWidth + 1, progressBarY, "]");
        
        for (int i = 0; i < progressBarWidth; i++)
        {
            char c = i <= progress ? '█' : '░';
            graphics.SetCharacter(4 + i, progressBarY, c);
        }
    }

    private static void DrawHelpText(ScreenTextGraphics graphics, TerminalSize size)
    {
        graphics.ForegroundColor = new AnsiTextColor(AnsiColor.Cyan);
        
        var helpText = new[]
        {
            "Commands:",
            "R - Refresh screen completely",
            "C - Change colors randomly", 
            "T - Test text rendering",
            "Q/ESC - Quit"
        };
        
        int startY = Math.Max(3, size.Rows - helpText.Length - 3);
        int startX = size.Columns - 25;
        
        for (int i = 0; i < helpText.Length; i++)
        {
            if (startX > 1 && startY + i < size.Rows - 1)
            {
                graphics.PutString(startX, startY + i, helpText[i]);
            }
        }
    }

    private static bool HandleInput(KeyStroke input, IScreen screen, ScreenTextGraphics graphics, Random random)
    {
        if (input.KeyType == KeyType.Character && input.Character != null)
        {
            char key = char.ToLower(input.Character.Value);
            
            switch (key)
            {
                case 'q':
                    return false;
                    
                case 'r':
                    // Force complete refresh
                    screen.Refresh(RefreshType.Complete);
                    break;
                    
                case 'c':
                    // Test color changes
                    TestColorChanges(graphics, random);
                    break;
                    
                case 't':
                    // Test text rendering
                    TestTextRendering(graphics);
                    break;
            }
        }
        else if (input.KeyType == KeyType.Escape)
        {
            return false;
        }
        
        return true;
    }

    private static void TestColorChanges(ScreenTextGraphics graphics, Random random)
    {
        var size = graphics.Size;
        
        // Fill a random rectangle with random colors
        var colors = new ITextColor[]
        {
            new AnsiTextColor(AnsiColor.Red),
            new AnsiTextColor(AnsiColor.Green),
            new AnsiTextColor(AnsiColor.Blue),
            new AnsiTextColor(AnsiColor.Yellow),
            new AnsiTextColor(AnsiColor.Magenta),
            new AnsiTextColor(AnsiColor.Cyan),
            new RgbColor(random.Next(256), random.Next(256), random.Next(256))
        };
        
        var fg = colors[random.Next(colors.Length)];
        var bg = colors[random.Next(colors.Length)];
        
        int x = random.Next(1, size.Columns - 10);
        int y = random.Next(1, size.Rows - 5);
        int width = random.Next(5, Math.Min(20, size.Columns - x - 1));
        int height = random.Next(3, Math.Min(8, size.Rows - y - 1));
        
        graphics.ForegroundColor = fg;
        graphics.BackgroundColor = bg;
        graphics.FillRectangle(new TerminalPosition(x, y), new TerminalSize(width, height), '█');
        
        // Reset colors
        graphics.ForegroundColor = new AnsiTextColor(AnsiColor.Default);
        graphics.BackgroundColor = new AnsiTextColor(AnsiColor.Default);
    }

    private static void TestTextRendering(ScreenTextGraphics graphics)
    {
        var size = graphics.Size;
        
        // Test different text styles
        var testTexts = new[]
        {
            "ASCII: Hello World!",
            "Unicode: Hello 世界! 🌍",
            "Symbols: ♠♣♥♦ ☀☂☃⭐",
            "Box: ┌─┐│ │└─┘",
            "Math: α β γ δ ∑ ∞ ≠ ≤ ≥"
        };
        
        graphics.ForegroundColor = new AnsiTextColor(AnsiColor.YellowBright);
        
        int startY = size.Rows / 2 - testTexts.Length / 2;
        
        for (int i = 0; i < testTexts.Length; i++)
        {
            string text = testTexts[i];
            int x = Math.Max(1, (size.Columns - text.Length) / 2);
            int y = startY + i;
            
            if (y >= 1 && y < size.Rows - 1)
            {
                // Clear the line first
                graphics.ForegroundColor = new AnsiTextColor(AnsiColor.Default);
                graphics.FillRectangle(new TerminalPosition(1, y), new TerminalSize(size.Columns - 2, 1), ' ');
                
                // Draw the text
                graphics.ForegroundColor = new AnsiTextColor(AnsiColor.YellowBright);
                if (i % 2 == 0)
                    graphics.EnableModifiers(SGR.Bold);
                
                graphics.PutString(x, y, text);
                
                if (i % 2 == 0)
                    graphics.DisableModifiers(SGR.Bold);
            }
        }
    }

    private static void DrawResizeNotification(ScreenTextGraphics graphics, TerminalSize newSize)
    {
        graphics.ForegroundColor = new AnsiTextColor(AnsiColor.RedBright);
        graphics.EnableModifiers(SGR.Bold);
        
        string message = $"Terminal resized to {newSize.Columns}x{newSize.Rows}";
        graphics.PutString(2, 2, message);
        
        graphics.DisableModifiers(SGR.Bold);
    }

    private static void ShowExitScreen(ScreenTextGraphics graphics)
    {
        var size = graphics.Size;
        
        graphics.ForegroundColor = new AnsiTextColor(AnsiColor.GreenBright);
        graphics.EnableModifiers(SGR.Bold);
        
        string[] messages = 
        {
            "Thanks for trying the Lanterna C# Screen Layer!",
            "",
            "The screen layer provides:",
            "✓ Buffered drawing operations",
            "✓ Efficient delta updates", 
            "✓ Unicode and emoji support",
            "✓ Automatic resize handling",
            "✓ Rich text graphics API",
            "",
            "Press any key to exit..."
        };
        
        int startY = (size.Rows - messages.Length) / 2;
        
        for (int i = 0; i < messages.Length; i++)
        {
            string message = messages[i];
            int x = Math.Max(1, (size.Columns - message.Length) / 2);
            int y = startY + i;
            
            if (y >= 1 && y < size.Rows - 1)
            {
                graphics.PutString(x, y, message);
            }
        }
        
        graphics.DisableModifiers(SGR.Bold);
    }
}