/*
 * Debug version of terminal example to help diagnose echo issues
 */

using System.Diagnostics;
using System.Text;
using Lanterna.Core;
using Lanterna.Input;
using Lanterna.Terminal;

namespace ExDrawRectangle;

public static class DebugTerminalExample
{
    public static void RunDebugDemo()
    {
        Console.WriteLine("=== Debug Terminal Demo ===");
        Console.WriteLine("This will help diagnose terminal echo issues.");
        
        // Check initial terminal state
        Console.WriteLine("\n1. Checking initial terminal state...");
        RunCommand("stty -a | grep -E '(echo|icanon)'", "Initial state");
        
        Console.WriteLine("\nPress any key to start the terminal demo...");
        Console.ReadKey();
        
        UnixTerminal? terminal = null;
        
        try
        {
            Console.WriteLine("\n2. Creating UnixTerminal...");
            terminal = new UnixTerminal();
            
            Console.WriteLine("3. Terminal created. Checking state after creation...");
            RunCommand("stty -a | grep -E '(echo|icanon)'", "After terminal creation");
            
            // Simple test - just enter and exit private mode
            Console.WriteLine("\n4. Entering private mode...");
            terminal.EnterPrivateMode();
            terminal.ClearScreen();
            terminal.SetCursorPosition(10, 5);
            terminal.PutString("Terminal Test - Press Q to exit");
            terminal.Flush();
            
            // Wait for Q
            while (true)
            {
                var key = terminal.ReadInput();
                if (key.Character == 'q' || key.Character == 'Q')
                    break;
            }
            
            Console.WriteLine("\n5. Exiting private mode...");
            terminal.ExitPrivateMode();
        }
        finally
        {
            Console.WriteLine("\n6. Disposing terminal...");
            terminal?.Dispose();
            
            // Check terminal state after disposal
            Console.WriteLine("\n7. Checking terminal state after disposal...");
            RunCommand("stty -a | grep -E '(echo|icanon)'", "After disposal");
        }
        
        Console.WriteLine("\n8. Testing echo functionality...");
        Console.Write("Type something and press Enter: ");
        string? input = Console.ReadLine();
        
        if (string.IsNullOrEmpty(input))
        {
            Console.WriteLine("WARNING: No input received! Echo might be disabled.");
            
            // Try to fix it
            Console.WriteLine("\n9. Attempting to restore echo...");
            RunCommand("stty echo", "Restore echo");
            
            Console.Write("Try typing again: ");
            input = Console.ReadLine();
            
            if (!string.IsNullOrEmpty(input))
            {
                Console.WriteLine($"Success! You typed: {input}");
            }
            else
            {
                Console.WriteLine("Echo is still broken. Run ./reset-terminal.sh");
            }
        }
        else
        {
            Console.WriteLine($"Success! Echo is working. You typed: {input}");
        }
    }
    
    private static void RunCommand(string command, string description)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            if (process != null)
            {
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                
                Console.WriteLine($"{description}: {output.Trim()}");
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"  Error: {error.Trim()}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{description}: Failed - {ex.Message}");
        }
    }
}