using Lanterna.Core;
using Lanterna.Screen;
using Lanterna.Terminal.Virtual;
using Xunit;

namespace Lanterna.Tests.Screen;

public class SimpleScreenTest
{
    [Fact]
    public void SimpleCharacterWriteTest()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        var screen = new TerminalScreen(virtualTerminal);
        screen.StartScreen();
        
        var testChar = TextCharacter.FromCharacter('X')[0];
        
        // Use coordinates (0, 0) to make it simple
        screen.SetCharacter(0, 0, testChar);
        screen.Refresh();
        
        var result = virtualTerminal.GetCharacter(0, 0);
        Assert.Equal(testChar, result);
    }
    
    [Fact]
    public void TestMultiplePositions()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        var screen = new TerminalScreen(virtualTerminal);
        screen.StartScreen();
        
        // Test multiple positions
        screen.SetCharacter(0, 0, TextCharacter.FromCharacter('A')[0]);
        screen.SetCharacter(1, 0, TextCharacter.FromCharacter('B')[0]);
        screen.SetCharacter(0, 1, TextCharacter.FromCharacter('C')[0]);
        
        screen.Refresh();
        
        Assert.Equal("A", virtualTerminal.GetCharacter(0, 0).CharacterString);
        Assert.Equal("B", virtualTerminal.GetCharacter(1, 0).CharacterString);
        Assert.Equal("C", virtualTerminal.GetCharacter(0, 1).CharacterString);
    }
}