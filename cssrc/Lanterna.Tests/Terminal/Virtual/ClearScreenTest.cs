using Lanterna.Core;
using Lanterna.Terminal.Virtual;
using Xunit;

namespace Lanterna.Tests.Terminal.Virtual;

public class ClearScreenTest
{
    [Fact]
    public void ClearScreenBasicTest()
    {
        var terminal = new DefaultVirtualTerminal();
        
        // Write a character
        terminal.SetCursorPosition(5, 3);
        terminal.PutCharacter('A');
        
        // Verify it's there
        var beforeClear = terminal.GetCharacter(5, 3);
        Assert.Equal("A", beforeClear.CharacterString);
        
        // Clear screen
        terminal.ClearScreen();
        
        // Verify it's gone
        var afterClear = terminal.GetCharacter(5, 3);
        Assert.Equal(TextCharacter.DefaultCharacter, afterClear);
        
        // Verify cursor position is reset
        var cursorPos = terminal.GetCursorPosition();
        Assert.Equal(TerminalPosition.TopLeftCorner, cursorPos);
    }
    
    [Fact]
    public void CanWriteAfterClearScreen()
    {
        var terminal = new DefaultVirtualTerminal();
        
        // Write, clear, then write again
        terminal.SetCursorPosition(2, 1);
        terminal.PutCharacter('A');
        terminal.ClearScreen();
        
        terminal.SetCursorPosition(3, 2);
        terminal.PutCharacter('B');
        
        // Should have 'B' at (3,2) and default at old position
        Assert.Equal("B", terminal.GetCharacter(3, 2).CharacterString);
        Assert.Equal(TextCharacter.DefaultCharacter, terminal.GetCharacter(2, 1));
    }
    
    [Fact]
    public void ClearScreenThenSequentialWrites()
    {
        var terminal = new DefaultVirtualTerminal();
        
        // Clear screen first
        terminal.ClearScreen();
        
        // Write multiple characters
        terminal.SetCursorPosition(1, 1);
        terminal.PutCharacter('X');
        
        terminal.SetCursorPosition(2, 1);
        terminal.PutCharacter('Y');
        
        terminal.SetCursorPosition(3, 1);
        terminal.PutCharacter('Z');
        
        // Verify all characters are present
        Assert.Equal("X", terminal.GetCharacter(1, 1).CharacterString);
        Assert.Equal("Y", terminal.GetCharacter(2, 1).CharacterString);
        Assert.Equal("Z", terminal.GetCharacter(3, 1).CharacterString);
    }
}