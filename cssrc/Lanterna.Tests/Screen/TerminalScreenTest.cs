using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Screen;
using Lanterna.Terminal.Virtual;
using Xunit;

namespace Lanterna.Tests.Screen;

/// <summary>
/// Demonstrates how to test the Screen layer using virtual terminals.
/// This shows how to validate buffered screen operations and refresh cycles.
/// </summary>
public class TerminalScreenTest
{
    [Fact]
    public void CanCreateTerminalScreen()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        var screen = new TerminalScreen(virtualTerminal);
        
        Assert.Equal(virtualTerminal.GetTerminalSize(), screen.GetTerminalSize());
        Assert.Equal(virtualTerminal, screen.GetTerminal());
    }

    [Fact]
    public void CanWriteToScreenBuffer()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        var screen = new TerminalScreen(virtualTerminal);
        screen.StartScreen();
        
        var textChar = TextCharacter.FromCharacter('X', new AnsiTextColor(AnsiColor.Red), new AnsiTextColor(AnsiColor.Blue));
        screen.SetCharacter(5, 3, textChar);
        
        Assert.Equal(textChar, screen.GetBackCharacter(5, 3));
        
        Assert.Equal(TextCharacter.DefaultCharacter, virtualTerminal.GetCharacter(5, 3));
    }

    [Fact]
    public void CanRefreshScreenToTerminal()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        var screen = new TerminalScreen(virtualTerminal);
        screen.StartScreen();
        
        var textChar = TextCharacter.FromCharacter('Y', new AnsiTextColor(AnsiColor.Green), new AnsiTextColor(AnsiColor.Yellow));
        screen.SetCharacter(10, 8, textChar);
        
        screen.Refresh();
        
        Assert.Equal(textChar, virtualTerminal.GetCharacter(10, 8));
    }

    [Fact]
    public void CanTestPartialRefresh()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        var screen = new TerminalScreen(virtualTerminal);
        screen.StartScreen();
        
        screen.SetCharacter(5, 5, TextCharacter.FromCharacter('A')[0]);
        screen.SetCharacter(10, 10, TextCharacter.FromCharacter('B')[0]);
        screen.SetCharacter(15, 15, TextCharacter.FromCharacter('C')[0]);
        
        var dirtyCells = virtualTerminal.GetDirtyCells();
        Assert.Contains(new TerminalPosition(5, 5), dirtyCells);
        Assert.Contains(new TerminalPosition(10, 10), dirtyCells);
        Assert.Contains(new TerminalPosition(15, 15), dirtyCells);
        
        screen.Refresh();
        
        Assert.Equal('A', virtualTerminal.GetCharacter(5, 5).CharacterString[0]);
        Assert.Equal('B', virtualTerminal.GetCharacter(10, 10).CharacterString[0]);
        Assert.Equal('C', virtualTerminal.GetCharacter(15, 15).CharacterString[0]);
        
        var postRefreshDirtyCells = virtualTerminal.GetDirtyCells();
        Assert.Empty(postRefreshDirtyCells);
    }

    [Fact]
    public void CanTestScreenGraphics()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        var screen = new TerminalScreen(virtualTerminal);
        screen.StartScreen();
        
        var graphics = screen.NewTextGraphics();
        graphics.SetForegroundColor(new AnsiTextColor(AnsiColor.Cyan));
        graphics.SetBackgroundColor(new AnsiTextColor(AnsiColor.Magenta));
        
        graphics.PutString(3, 2, "Hello Graphics!");
        
        string expectedText = "Hello Graphics!";
        for (int i = 0; i < expectedText.Length; i++)
        {
            var character = screen.GetBackCharacter(3 + i, 2);
            Assert.Equal(expectedText[i], character.CharacterString[0]);
            Assert.Equal(new AnsiTextColor(AnsiColor.Cyan), character.ForegroundColor);
            Assert.Equal(new AnsiTextColor(AnsiColor.Magenta), character.BackgroundColor);
        }
        
        screen.Refresh();
        
        for (int i = 0; i < expectedText.Length; i++)
        {
            var character = virtualTerminal.GetCharacter(3 + i, 2);
            Assert.Equal(expectedText[i], character.CharacterString[0]);
            Assert.Equal(new AnsiTextColor(AnsiColor.Cyan), character.ForegroundColor);
            Assert.Equal(new AnsiTextColor(AnsiColor.Magenta), character.BackgroundColor);
        }
    }

    // TODO: Fill method is not implemented in TerminalScreen
    // [Fact]
    // public void CanTestScreenFillAndClear()
    // {
    //     var virtualTerminal = new DefaultVirtualTerminal();
    //     var screen = new TerminalScreen(virtualTerminal);
    //     screen.StartScreen();
    //     
    //     var fillChar = TextCharacter.FromCharacter('*', new AnsiTextColor(AnsiColor.White), new AnsiTextColor(AnsiColor.Black));
    //     screen.Fill(fillChar);
    //     
    //     var terminalSize = screen.GetTerminalSize();
    //     for (int row = 0; row < terminalSize.Rows; row++)
    //     {
    //         for (int col = 0; col < terminalSize.Columns; col++)
    //         {
    //             Assert.Equal(fillChar, screen.GetBackCharacter(col, row));
    //         }
    //     }
    //     
    //     screen.Clear();
    //     
    //     for (int row = 0; row < terminalSize.Rows; row++)
    //     {
    //         for (int col = 0; col < terminalSize.Columns; col++)
    //         {
    //             Assert.Equal(TextCharacter.DefaultCharacter, screen.GetBackCharacter(col, row));
    //         }
    //     }
    // }

    [Fact]
    public void CanTestScreenCursor()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        var screen = new TerminalScreen(virtualTerminal);
        screen.StartScreen();
        
        var cursorPosition = new TerminalPosition(12, 8);
        screen.SetCursorPosition(cursorPosition);
        
        Assert.Equal(cursorPosition, screen.GetCursorPosition());
        
        screen.Refresh();
        
        Assert.Equal(cursorPosition, virtualTerminal.GetCursorPosition());
    }

    [Fact]
    public void CanTestScreenResize()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        var screen = new TerminalScreen(virtualTerminal);
        screen.StartScreen();
        
        var newSize = new TerminalSize(60, 20);
        virtualTerminal.SetTerminalSize(newSize);
        
        screen.DoResizeIfNecessary();
        
        Assert.Equal(newSize, screen.GetTerminalSize());
        
        for (int row = 0; row < newSize.Rows; row++)
        {
            for (int col = 0; col < newSize.Columns; col++)
            {
                Assert.Equal(TextCharacter.DefaultCharacter, screen.GetBackCharacter(col, row));
            }
        }
    }

    [Fact]
    public void CanTestMultipleScreenOperations()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        var screen = new TerminalScreen(virtualTerminal);
        screen.StartScreen();
        
        var graphics = screen.NewTextGraphics();
        
        graphics.SetForegroundColor(new AnsiTextColor(AnsiColor.Yellow));
        graphics.DrawLine(5, 5, 15, 5, '*');
        
        graphics.SetForegroundColor(new AnsiTextColor(AnsiColor.Blue));
        graphics.DrawLine(10, 3, 10, 7, '|');
        
        graphics.SetForegroundColor(new AnsiTextColor(AnsiColor.Red));
        graphics.PutString(12, 8, "Test");
        
        screen.Refresh();
        
        for (int col = 5; col <= 15; col++)
        {
            var character = virtualTerminal.GetCharacter(col, 5);
            if (col == 10)
            {
                Assert.Equal('|', character.CharacterString[0]);
                Assert.Equal(new AnsiTextColor(AnsiColor.Blue), character.ForegroundColor);
            }
            else
            {
                Assert.Equal('*', character.CharacterString[0]);
                Assert.Equal(new AnsiTextColor(AnsiColor.Yellow), character.ForegroundColor);
            }
        }
        
        for (int row = 3; row <= 7; row++)
        {
            var character = virtualTerminal.GetCharacter(10, row);
            Assert.Equal('|', character.CharacterString[0]);
            Assert.Equal(new AnsiTextColor(AnsiColor.Blue), character.ForegroundColor);
        }
        
        string testText = "Test";
        for (int i = 0; i < testText.Length; i++)
        {
            var character = virtualTerminal.GetCharacter(12 + i, 8);
            Assert.Equal(testText[i], character.CharacterString[0]);
            Assert.Equal(new AnsiTextColor(AnsiColor.Red), character.ForegroundColor);
        }
    }
}