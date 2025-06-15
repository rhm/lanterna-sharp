using Lanterna.Core;
using Lanterna.Terminal.Virtual;
using Xunit;

namespace Lanterna.Tests.Terminal.Virtual;

public class CursorPositionTest
{
    [Fact]
    public void CanSetAndGetCursorPosition()
    {
        var terminal = new DefaultVirtualTerminal();
        
        terminal.SetCursorPosition(5, 3);
        var position = terminal.GetCursorPosition();
        
        Assert.Equal(new TerminalPosition(5, 3), position);
    }
    
    [Fact]
    public void CursorPositionAffectsCharacterWriting()
    {
        var terminal = new DefaultVirtualTerminal();
        
        // Write a character at (3, 2)
        terminal.SetCursorPosition(3, 2);
        terminal.PutCharacter('X');
        
        // Check that character is at the right position
        var character = terminal.GetCharacter(3, 2);
        Assert.Equal("X", character.CharacterString);
        
        // Check that cursor moved to the next position
        var cursorPos = terminal.GetCursorPosition();
        Assert.Equal(new TerminalPosition(4, 2), cursorPos);
    }
    
    [Fact]
    public void CanWriteAtMultiplePositions()
    {
        var terminal = new DefaultVirtualTerminal();
        
        // Write characters at different positions
        terminal.SetCursorPosition(1, 1);
        terminal.PutCharacter('A');
        
        terminal.SetCursorPosition(5, 3);
        terminal.PutCharacter('B');
        
        terminal.SetCursorPosition(0, 0);
        terminal.PutCharacter('C');
        
        // Verify each character is in the right place
        Assert.Equal("A", terminal.GetCharacter(1, 1).CharacterString);
        Assert.Equal("B", terminal.GetCharacter(5, 3).CharacterString);
        Assert.Equal("C", terminal.GetCharacter(0, 0).CharacterString);
    }
}