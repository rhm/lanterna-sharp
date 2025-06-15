/*
 * This file demonstrates how to use TestTerminalFactory for configurable terminal applications.
 * It shows how command-line arguments can be used to control terminal behavior.
 */

using Lanterna.Core;
using Lanterna.Input;
using Lanterna.Terminal;

namespace Lanterna.Examples;

/// <summary>
/// Example demonstrating TestTerminalFactory usage with command-line arguments
/// </summary>
public static class TestTerminalFactoryExample
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== TestTerminalFactory Example ===");
        Console.WriteLine("Available options:");
        Console.WriteLine("  --text-terminal    Force text terminal (default in C# port)");
        Console.WriteLine("  --mouse-click      Enable click/release mouse capture");
        Console.WriteLine("  --mouse-drag       Enable click/release/drag mouse capture");
        Console.WriteLine("  --mouse-move       Enable full mouse capture");
        Console.WriteLine();
        
        if (args.Length == 0)
        {
            Console.WriteLine("No arguments provided. Using default settings.");
            Console.WriteLine("Try running with: --text-terminal --mouse-click");
        }
        else
        {
            Console.WriteLine($"Processing {args.Length} arguments: {string.Join(" ", args)}");
        }
        Console.WriteLine();

        try
        {
            // Create factory with command-line arguments
            var factory = new TestTerminalFactory(args);
            
            // Create terminal
            var terminal = factory.CreateTerminal();
            
            Console.WriteLine("Terminal created successfully!");
            Console.WriteLine($"Terminal type: {terminal.GetType().Name}");
            Console.WriteLine($"Terminal size: {terminal.GetTerminalSize()}");
            
            // Brief terminal demonstration
            RunBriefDemo(terminal);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating terminal: {ex.Message}");
            Console.WriteLine("This might happen if running in a non-terminal environment.");
        }
    }

    private static void RunBriefDemo(ITerminal terminal)
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to run a brief terminal demo (or Ctrl+C to exit)...");
        Console.ReadKey();
        
        try
        {
            // Enter private mode for demonstration
            terminal.EnterPrivateMode();
            terminal.ClearScreen();
            terminal.SetCursorVisible(false);
            
            // Get terminal size
            var size = terminal.GetTerminalSize();
            
            // Draw a simple message
            terminal.SetForegroundColor(new AnsiTextColor(AnsiColor.GreenBright));
            terminal.EnableSGR(SGR.Bold);
            
            string message = "TestTerminalFactory Working!";
            terminal.SetCursorPosition((size.Columns - message.Length) / 2, size.Rows / 2);
            terminal.PutString(message);
            
            terminal.DisableSGR(SGR.Bold);
            terminal.SetForegroundColor(new AnsiTextColor(AnsiColor.White));
            
            string instruction = "Press any key to exit...";
            terminal.SetCursorPosition((size.Columns - instruction.Length) / 2, size.Rows / 2 + 2);
            terminal.PutString(instruction);
            
            terminal.Flush();
            
            // Wait for input
            terminal.ReadInput();
        }
        finally
        {
            try
            {
                terminal?.Dispose();
            }
            catch { }
        }
        
        Console.WriteLine("Demo completed successfully!");
    }
}