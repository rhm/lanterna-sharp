using Lanterna.Core;
using Lanterna.Terminal.Virtual;
using Xunit;

namespace Lanterna.Tests;

/// <summary>
/// Simple test to validate the testing framework is working correctly.
/// </summary>
public class SimpleTest
{
    [Fact]
    public void CanCreateVirtualTerminal()
    {
        var terminal = new DefaultVirtualTerminal();
        
        Assert.NotNull(terminal);
        Assert.Equal(new TerminalSize(80, 24), terminal.GetTerminalSize());
        Assert.Equal(TerminalPosition.TopLeftCorner, terminal.GetCursorPosition());
    }

    [Fact]
    public void CanWriteAndReadCharacter()
    {
        var terminal = new DefaultVirtualTerminal();
        
        terminal.SetCursorPosition(5, 3);
        terminal.PutCharacter('X');
        
        var character = terminal.GetCharacter(5, 3);
        Assert.Equal('X', character.CharacterString[0]);
        Assert.Equal(new TerminalPosition(6, 3), terminal.GetCursorPosition());
    }

    [Fact]
    public void CanClearTerminal()
    {
        var terminal = new DefaultVirtualTerminal();
        
        terminal.SetCursorPosition(10, 10);
        terminal.PutCharacter('T');
        
        terminal.ClearScreen();
        
        var character = terminal.GetCharacter(10, 10);
        Assert.Equal(TextCharacter.DefaultCharacter, character);
        Assert.Equal(TerminalPosition.TopLeftCorner, terminal.GetCursorPosition());
    }
}