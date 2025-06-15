using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;
using Lanterna.Terminal.Virtual;
using Xunit;

namespace Lanterna.Tests.Terminal.Virtual;

public class DefaultVirtualTerminalTest
{
    private static readonly TextCharacter DefaultCharacter = TextCharacter.DefaultCharacter;
    private readonly DefaultVirtualTerminal _virtualTerminal;

    public DefaultVirtualTerminalTest()
    {
        _virtualTerminal = new DefaultVirtualTerminal();
    }

    [Fact]
    public void InitialTerminalStateIsAsExpected()
    {
        Assert.Equal(TerminalPosition.TopLeftCorner, _virtualTerminal.GetCursorPosition());
        var terminalSize = _virtualTerminal.GetTerminalSize();
        Assert.Equal(new TerminalSize(80, 24), terminalSize);

        for (int row = 0; row < terminalSize.Rows; row++)
        {
            for (int column = 0; column < terminalSize.Columns; column++)
            {
                Assert.Equal(DefaultCharacter, _virtualTerminal.GetCharacter(column, row));
            }
        }
    }

    [Fact]
    public void CanWriteCharacterAtPosition()
    {
        _virtualTerminal.SetCursorPosition(5, 3);
        _virtualTerminal.PutCharacter('A');
        
        var expectedChar = TextCharacter.FromCharacter('A')[0];
        Assert.Equal(expectedChar, _virtualTerminal.GetCharacter(5, 3));
        Assert.Equal(new TerminalPosition(6, 3), _virtualTerminal.GetCursorPosition());
    }

    [Fact]
    public void CanWriteString()
    {
        _virtualTerminal.SetCursorPosition(2, 1);
        string testString = "Hello World";
        
        foreach (char c in testString)
        {
            _virtualTerminal.PutCharacter(c);
        }

        for (int i = 0; i < testString.Length; i++)
        {
            var expectedChar = TextCharacter.FromCharacter(testString[i])[0];
            Assert.Equal(expectedChar, _virtualTerminal.GetCharacter(2 + i, 1));
        }
        
        Assert.Equal(new TerminalPosition(2 + testString.Length, 1), _virtualTerminal.GetCursorPosition());
    }

    [Fact]
    public void CursorWrapsToNextLineAtRightEdge()
    {
        var terminalSize = _virtualTerminal.GetTerminalSize();
        _virtualTerminal.SetCursorPosition(terminalSize.Columns - 1, 5);
        
        _virtualTerminal.PutCharacter('X');
        Assert.Equal(new TerminalPosition(0, 6), _virtualTerminal.GetCursorPosition());
        
        var expectedChar = TextCharacter.FromCharacter('X')[0];
        Assert.Equal(expectedChar, _virtualTerminal.GetCharacter(terminalSize.Columns - 1, 5));
    }

    [Fact]
    public void CanSetAndGetColors()
    {
        _virtualTerminal.SetForegroundColor(new AnsiTextColor(AnsiColor.Red));
        _virtualTerminal.SetBackgroundColor(new AnsiTextColor(AnsiColor.Blue));
        
        _virtualTerminal.SetCursorPosition(10, 10);
        _virtualTerminal.PutCharacter('C');
        
        var character = _virtualTerminal.GetCharacter(10, 10);
        Assert.Equal('C', character.CharacterString[0]);
        Assert.Equal(new AnsiTextColor(AnsiColor.Red), character.ForegroundColor);
        Assert.Equal(new AnsiTextColor(AnsiColor.Blue), character.BackgroundColor);
    }

    [Fact]
    public void CanSetAndGetSGRModifiers()
    {
        _virtualTerminal.EnableSGR(SGR.Bold);
        _virtualTerminal.EnableSGR(SGR.Italic);
        
        _virtualTerminal.SetCursorPosition(5, 5);
        _virtualTerminal.PutCharacter('B');
        
        var character = _virtualTerminal.GetCharacter(5, 5);
        Assert.Equal('B', character.CharacterString[0]);
        Assert.Contains(SGR.Bold, character.Modifiers);
        Assert.Contains(SGR.Italic, character.Modifiers);
    }

    [Fact]
    public void CanClearScreen()
    {
        _virtualTerminal.SetCursorPosition(10, 10);
        _virtualTerminal.PutCharacter('X');
        
        _virtualTerminal.ClearScreen();
        
        Assert.Equal(DefaultCharacter, _virtualTerminal.GetCharacter(10, 10));
        Assert.Equal(TerminalPosition.TopLeftCorner, _virtualTerminal.GetCursorPosition());
    }

    [Fact]
    public void CanResizeTerminal()
    {
        var newSize = new TerminalSize(100, 30);
        _virtualTerminal.SetTerminalSize(newSize);
        
        Assert.Equal(newSize, _virtualTerminal.GetTerminalSize());
        
        for (int row = 0; row < newSize.Rows; row++)
        {
            for (int column = 0; column < newSize.Columns; column++)
            {
                Assert.Equal(DefaultCharacter, _virtualTerminal.GetCharacter(column, row));
            }
        }
    }

    [Fact]
    public void CanQueueAndPollInput()
    {
        var keyStroke = new KeyStroke('A', false, false);
        _virtualTerminal.AddInput(keyStroke);
        
        var polledKeyStroke = _virtualTerminal.PollInput();
        Assert.Equal(keyStroke, polledKeyStroke);
        
        var emptyPoll = _virtualTerminal.PollInput();
        Assert.Null(emptyPoll);
    }

    [Fact]
    public void CanDetectDirtyCells()
    {
        var initialDirtyCells = _virtualTerminal.GetAndResetDirtyCells();
        Assert.Empty(initialDirtyCells);
        
        _virtualTerminal.SetCursorPosition(15, 8);
        _virtualTerminal.PutCharacter('D');
        
        var dirtyCells = _virtualTerminal.GetAndResetDirtyCells();
        Assert.Contains(new TerminalPosition(15, 8), dirtyCells);
        
        var subsequentDirtyCells = _virtualTerminal.GetAndResetDirtyCells();
        Assert.Empty(subsequentDirtyCells);
    }

    // TODO: ScrollLines method is not implemented in DefaultVirtualTerminal
    // [Fact]
    // public void CanScrollBuffer()
    // {
    //     _virtualTerminal.SetCursorPosition(0, 0);
    //     _virtualTerminal.PutCharacter('T');
    //     
    //     _virtualTerminal.ScrollLines(0, _virtualTerminal.GetTerminalSize().Rows - 1, 1);
    //     
    //     Assert.Equal(DefaultCharacter, _virtualTerminal.GetCharacter(0, 0));
    //     Assert.Equal(TextCharacter.FromCharacter('T'), _virtualTerminal.GetCharacter(0, 1));
    // }

    [Fact]
    public void CursorVisibilityWorks()
    {
        Assert.True(_virtualTerminal.IsCursorVisible);
        
        _virtualTerminal.SetCursorVisible(false);
        Assert.False(_virtualTerminal.IsCursorVisible);
        
        _virtualTerminal.SetCursorVisible(true);
        Assert.True(_virtualTerminal.IsCursorVisible);
    }

    [Fact]
    public void CanEnterAndExitPrivateMode()
    {
        _virtualTerminal.SetCursorPosition(5, 5);
        _virtualTerminal.PutCharacter('M');
        
        _virtualTerminal.EnterPrivateMode();
        
        Assert.Equal(DefaultCharacter, _virtualTerminal.GetCharacter(5, 5));
        Assert.Equal(TerminalPosition.TopLeftCorner, _virtualTerminal.GetCursorPosition());
        
        _virtualTerminal.SetCursorPosition(10, 10);
        _virtualTerminal.PutCharacter('P');
        
        _virtualTerminal.ExitPrivateMode();
        
        Assert.Equal(TextCharacter.FromCharacter('M')[0], _virtualTerminal.GetCharacter(5, 5));
        Assert.Equal(new TerminalPosition(5, 5), _virtualTerminal.GetCursorPosition());
        Assert.Equal(DefaultCharacter, _virtualTerminal.GetCharacter(10, 10));
    }
}