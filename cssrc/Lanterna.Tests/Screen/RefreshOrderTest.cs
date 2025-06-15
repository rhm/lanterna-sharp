using Lanterna.Core;
using Lanterna.Screen;
using Lanterna.Terminal.Virtual;
using Xunit;

namespace Lanterna.Tests.Screen;

public class RefreshOrderTest
{
    [Fact]
    public void TestDrawingManyCharactersInSequence()
    {
        var terminal = new DefaultVirtualTerminal();
        
        // Simulate what RefreshComplete does - clear and draw many characters
        terminal.ClearScreen();
        terminal.ResetColorAndSGR();
        terminal.SetCursorPosition(0, 0);
        
        // Draw characters across the first few rows (like RefreshComplete would)
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                char charToWrite = (col == 2 && row == 1) ? 'X' : ' ';
                
                terminal.SetCursorPosition(col, row);
                terminal.PutCharacter(charToWrite);
            }
        }
        
        // Check if our target character is still there
        var result = terminal.GetCharacter(2, 1);
        Console.WriteLine($"Character at (2,1): '{result.CharacterString}'");
        Assert.Equal("X", result.CharacterString);
    }
    
    [Fact]
    public void TestMinimalRefreshSequence()
    {
        var terminal = new DefaultVirtualTerminal();
        
        // Very minimal version of what RefreshComplete does
        terminal.ClearScreen();
        
        // Set target character
        terminal.SetCursorPosition(2, 1);
        terminal.PutCharacter('X');
        
        // Write one space character after it (simulating other characters being drawn)
        terminal.SetCursorPosition(3, 1);
        terminal.PutCharacter(' ');
        
        // Check if our character is still there
        var result = terminal.GetCharacter(2, 1);
        Console.WriteLine($"Character at (2,1) after writing space at (3,1): '{result.CharacterString}'");
        Assert.Equal("X", result.CharacterString);
    }
    
    [Fact]
    public void TestWithColorAndSGROperations()
    {
        var terminal = new DefaultVirtualTerminal();
        
        terminal.ClearScreen();
        terminal.ResetColorAndSGR();
        
        // Write a character with color operations (like DrawCharacterToTerminal does)
        terminal.SetCursorPosition(2, 1);
        terminal.SetForegroundColor(new AnsiTextColor(AnsiColor.Red));
        terminal.SetBackgroundColor(new AnsiTextColor(AnsiColor.Blue));
        terminal.PutCharacter('X');
        
        // Reset colors and write another character
        terminal.ResetColorAndSGR();
        terminal.SetCursorPosition(3, 1);
        terminal.PutCharacter('Y');
        
        // Check both characters
        var char1 = terminal.GetCharacter(2, 1);
        var char2 = terminal.GetCharacter(3, 1);
        
        Console.WriteLine($"Character at (2,1): '{char1.CharacterString}'");
        Console.WriteLine($"Character at (3,1): '{char2.CharacterString}'");
        
        Assert.Equal("X", char1.CharacterString);
        Assert.Equal("Y", char2.CharacterString);
    }
}