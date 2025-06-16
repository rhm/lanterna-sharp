/*
 * This file demonstrates the basic functionality of the Lanterna C# port.
 * It shows how to use the core data types and color system.
 */

using Lanterna.Core;
using Lanterna.Input;

namespace ExDrawRectangle;

/// <summary>
/// Basic example demonstrating the core Lanterna C# functionality
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Lanterna C# Port - Basic Example ===");
        Console.WriteLine();

        // Check if user wants to run the interactive terminal demo
        if (args.Length > 0 && args[0] == "--terminal")
        {
            TerminalExample.RunTerminalDemo();
            return;
        }
        
        // Check if user wants to run the screen layer demo
        if (args.Length > 0 && args[0] == "--screen")
        {
            ScreenExample.RunScreenDemo();
            return;
        }
        
        // Check if user wants to run the cleanup test
        if (args.Length > 0 && args[0] == "--test-cleanup")
        {
            TestTerminalCleanup.RunCleanupTest();
            return;
        }
        
        // Check if user wants to run the debug terminal demo
        if (args.Length > 0 && args[0] == "--debug-terminal")
        {
            DebugTerminalExample.RunDebugDemo();
            return;
        }

        // Demonstrate TerminalPosition
        Console.WriteLine("1. TerminalPosition Examples:");
        var pos1 = new TerminalPosition(10, 5);
        var pos2 = pos1.WithRelative(3, 2);
        Console.WriteLine($"   Original position: {pos1}");
        Console.WriteLine($"   Relative position (+3,+2): {pos2}");
        Console.WriteLine($"   Top-left corner: {TerminalPosition.TopLeftCorner}");
        Console.WriteLine();

        // Demonstrate TerminalSize
        Console.WriteLine("2. TerminalSize Examples:");
        var size1 = new TerminalSize(80, 24);
        var size2 = size1.WithRelativeColumns(5);
        Console.WriteLine($"   Original size: {size1}");
        Console.WriteLine($"   With 5 more columns: {size2}");
        Console.WriteLine($"   Zero size: {TerminalSize.Zero}");
        Console.WriteLine();

        // Demonstrate TerminalRectangle
        Console.WriteLine("3. TerminalRectangle Examples:");
        var rect = new TerminalRectangle(5, 5, 20, 10);
        Console.WriteLine($"   Rectangle: {rect}");
        Console.WriteLine($"   Contains (10,8): {rect.Contains(10, 8)}");
        Console.WriteLine($"   Contains (30,8): {rect.Contains(30, 8)}");
        Console.WriteLine();

        // Demonstrate Colors
        Console.WriteLine("4. Color System Examples:");
        
        // ANSI Colors
        var redAnsi = new AnsiTextColor(AnsiColor.Red);
        var blueAnsi = new AnsiTextColor(AnsiColor.BlueBright);
        Console.WriteLine($"   ANSI Red: {redAnsi} - RGB({redAnsi.Red},{redAnsi.Green},{redAnsi.Blue})");
        Console.WriteLine($"   ANSI Bright Blue: {blueAnsi} - RGB({blueAnsi.Red},{blueAnsi.Green},{blueAnsi.Blue})");
        
        // Indexed Colors
        var indexed = new IndexedColor(196); // Bright red in 256-color palette
        Console.WriteLine($"   Indexed Color 196: {indexed} - RGB({indexed.Red},{indexed.Green},{indexed.Blue})");
        
        // RGB Colors
        var rgb = new RgbColor(128, 255, 64); // Bright green
        Console.WriteLine($"   RGB Color: {rgb} - RGB({rgb.Red},{rgb.Green},{rgb.Blue})");
        
        // Color parsing
        var parsedColor = TextColorFactory.FromString("#FF0080");
        Console.WriteLine($"   Parsed #FF0080: {parsedColor}");
        Console.WriteLine();

        // Demonstrate TextCharacter
        Console.WriteLine("5. TextCharacter Examples:");
        
        // Basic character
        var char1 = TextCharacter.FromCharacter('A', redAnsi, new AnsiTextColor(AnsiColor.Default));
        Console.WriteLine($"   Character 'A' in red: {char1}");
        
        // Character with modifiers
        var char2 = char1.WithModifier(SGR.Bold).WithModifier(SGR.Underline);
        Console.WriteLine($"   Same character, bold + underline: {char2}");
        Console.WriteLine($"   Is bold: {char2.IsBold}, Is underlined: {char2.IsUnderlined}");
        
        // Unicode/Emoji character
        var emojiChars = TextCharacter.FromString("🚀", rgb, new AnsiTextColor(AnsiColor.Black));
        Console.WriteLine($"   Rocket emoji: {emojiChars[0]}");
        Console.WriteLine($"   Is double width: {emojiChars[0].IsDoubleWidth}");
        Console.WriteLine();

        // Demonstrate KeyStroke
        Console.WriteLine("6. KeyStroke Examples:");
        
        // Regular character
        var key1 = new KeyStroke('a', false, false);
        Console.WriteLine($"   Key 'a': {key1}");
        
        // Modified character
        var key2 = new KeyStroke('A', true, false); // Ctrl+A
        Console.WriteLine($"   Ctrl+A: {key2}");
        
        // Special keys
        var key3 = new KeyStroke(KeyType.F1);
        var key4 = new KeyStroke(KeyType.ArrowUp, false, true); // Alt+Up
        Console.WriteLine($"   F1 key: {key3}");
        Console.WriteLine($"   Alt+Up arrow: {key4}");
        
        // Parse from vim notation
        var parsedKey = KeyStroke.FromString("<c-s>");
        Console.WriteLine($"   Parsed '<c-s>': {parsedKey}");
        Console.WriteLine();

        // Demonstrate utility functions
        Console.WriteLine("7. Utility Functions:");
        var cjkChar = '中';
        var latinChar = 'A';
        Console.WriteLine($"   Is '{cjkChar}' CJK: {TerminalTextUtils.IsCharCJK(cjkChar)}");
        Console.WriteLine($"   Is '{latinChar}' CJK: {TerminalTextUtils.IsCharCJK(latinChar)}");
        Console.WriteLine($"   Is '{cjkChar}' double-width: {TerminalTextUtils.IsCharDoubleWidth(cjkChar)}");
        
        string testString = "Hello 世界";
        Console.WriteLine($"   Column width of '{testString}': {TerminalTextUtils.GetColumnWidth(testString)}");
        Console.WriteLine();

        Console.WriteLine("=== All basic functionality working! ===");
        Console.WriteLine("The Lanterna C# port core is ready for terminal implementation.");
        Console.WriteLine();
        Console.WriteLine("To try the interactive demos, run:");
        Console.WriteLine("  dotnet run --terminal        (basic terminal layer demo)");
        Console.WriteLine("  dotnet run --screen          (advanced screen layer demo)");
        Console.WriteLine("  dotnet run --test-cleanup    (test terminal cleanup)");
        Console.WriteLine("  dotnet run --debug-terminal  (debug terminal echo issues)");
        Console.WriteLine();
        Console.WriteLine("IMPORTANT: Terminal Cleanup");
        Console.WriteLine("---------------------------");
        Console.WriteLine("If your terminal stops echoing characters after running a demo:");
        Console.WriteLine("1. Run: ./reset-terminal.sh");
        Console.WriteLine("2. Or type 'reset' and press Enter (even if you can't see it)");
        Console.WriteLine("3. For diagnostics, run: ./diagnose-terminal.sh");
        Console.WriteLine();
        Console.WriteLine("This is a known issue with VT100 terminals when programs don't");
        Console.WriteLine("properly clear escape sequence responses from the input buffer.");
    }
}