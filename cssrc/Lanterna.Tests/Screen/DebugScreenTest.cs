using Lanterna.Core;
using Lanterna.Screen;
using Lanterna.Terminal.Virtual;
using Xunit;

namespace Lanterna.Tests.Screen;

public class DebugScreenTest
{
    [Fact]
    public void DebugScreenRefresh()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        var screen = new TerminalScreen(virtualTerminal);
        screen.StartScreen();
        
        var testChar = TextCharacter.FromCharacter('Y', new AnsiTextColor(AnsiColor.Green), new AnsiTextColor(AnsiColor.Yellow));
        
        // Check initial state
        var initialBackChar = screen.GetBackCharacter(10, 8);
        var initialFrontChar = screen.GetFrontCharacter(10, 8);
        var initialTerminalChar = virtualTerminal.GetCharacter(10, 8);
        
        Console.WriteLine($"Before SetCharacter:");
        Console.WriteLine($"  Back buffer at (10,8): '{initialBackChar.CharacterString}'");
        Console.WriteLine($"  Front buffer at (10,8): '{initialFrontChar.CharacterString}'");
        Console.WriteLine($"  Terminal at (10,8): '{initialTerminalChar.CharacterString}'");
        Console.WriteLine($"  Back == Front: {initialBackChar.Equals(initialFrontChar)}");
        
        screen.SetCharacter(10, 8, testChar);
        
        // Check after SetCharacter
        var afterSetBackChar = screen.GetBackCharacter(10, 8);
        var afterSetFrontChar = screen.GetFrontCharacter(10, 8);
        var afterSetTerminalChar = virtualTerminal.GetCharacter(10, 8);
        
        Console.WriteLine($"After SetCharacter:");
        Console.WriteLine($"  Back buffer at (10,8): '{afterSetBackChar.CharacterString}'");
        Console.WriteLine($"  Front buffer at (10,8): '{afterSetFrontChar.CharacterString}'");
        Console.WriteLine($"  Terminal at (10,8): '{afterSetTerminalChar.CharacterString}'");
        Console.WriteLine($"  Back == Front: {afterSetBackChar.Equals(afterSetFrontChar)}");
        Console.WriteLine($"  Back == TestChar: {afterSetBackChar.Equals(testChar)}");
        
        screen.Refresh();
        
        // Check after Refresh
        var finalBackChar = screen.GetBackCharacter(10, 8);
        var finalFrontChar = screen.GetFrontCharacter(10, 8);
        var finalTerminalChar = virtualTerminal.GetCharacter(10, 8);
        
        Console.WriteLine($"After Refresh:");
        Console.WriteLine($"  Back buffer at (10,8): '{finalBackChar.CharacterString}'");
        Console.WriteLine($"  Front buffer at (10,8): '{finalFrontChar.CharacterString}'");
        Console.WriteLine($"  Terminal at (10,8): '{finalTerminalChar.CharacterString}'");
        Console.WriteLine($"  Back == Front: {finalBackChar.Equals(finalFrontChar)}");
        
        // Also check cursor position to see if it was moved correctly
        var cursorPos = virtualTerminal.GetCursorPosition();
        Console.WriteLine($"  Terminal cursor position: {cursorPos}");
        
        // Test putting a character directly to the terminal to see if it works
        virtualTerminal.SetCursorPosition(10, 8);
        virtualTerminal.PutCharacter('Z');
        var directChar = virtualTerminal.GetCharacter(10, 8);
        Console.WriteLine($"  After direct PutCharacter('Z'): '{directChar.CharacterString}'");
        
        Assert.Equal(testChar, finalTerminalChar);
    }
}