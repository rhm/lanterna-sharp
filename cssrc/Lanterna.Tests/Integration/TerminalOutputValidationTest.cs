using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;
using Lanterna.Terminal.Virtual;
using Xunit;

namespace Lanterna.Tests.Integration;

/// <summary>
/// Demonstrates how to use the virtual terminal for comprehensive output validation testing.
/// This shows how to test complete terminal UI scenarios without requiring actual terminal hardware.
/// </summary>
public class TerminalOutputValidationTest
{
    [Fact]
    public void CanValidateComplexTerminalOutput()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        virtualTerminal.SetTerminalSize(new TerminalSize(40, 10));
        
        virtualTerminal.SetForegroundColor(new AnsiTextColor(AnsiColor.Blue));
        virtualTerminal.SetBackgroundColor(new AnsiTextColor(AnsiColor.White));
        virtualTerminal.EnableSGR(SGR.Bold);
        
        string title = "Test Application";
        virtualTerminal.SetCursorPosition((40 - title.Length) / 2, 1);
        foreach (char c in title)
        {
            virtualTerminal.PutCharacter(c);
        }
        
        virtualTerminal.ResetColorAndSGR();
        
        virtualTerminal.SetCursorPosition(2, 3);
        string menu = "1. Option One";
        foreach (char c in menu)
        {
            virtualTerminal.PutCharacter(c);
        }
        
        virtualTerminal.SetCursorPosition(2, 4);
        string menu2 = "2. Option Two";
        foreach (char c in menu2)
        {
            virtualTerminal.PutCharacter(c);
        }
        
        virtualTerminal.SetForegroundColor(new AnsiTextColor(AnsiColor.Red));
        virtualTerminal.SetCursorPosition(2, 6);
        string prompt = "Enter choice: ";
        foreach (char c in prompt)
        {
            virtualTerminal.PutCharacter(c);
        }
        
        ValidateTitle(virtualTerminal, title);
        ValidateMenu(virtualTerminal);
        ValidatePrompt(virtualTerminal, prompt);
    }

    private void ValidateTitle(DefaultVirtualTerminal terminal, string expectedTitle)
    {
        int startColumn = (40 - expectedTitle.Length) / 2;
        
        for (int i = 0; i < expectedTitle.Length; i++)
        {
            var character = terminal.GetCharacter(startColumn + i, 1);
            Assert.Equal(expectedTitle[i], character.CharacterString[0]);
            Assert.Equal(new AnsiTextColor(AnsiColor.Blue), character.ForegroundColor);
            Assert.Equal(new AnsiTextColor(AnsiColor.White), character.BackgroundColor);
            Assert.Contains(SGR.Bold, character.Modifiers);
        }
    }

    private void ValidateMenu(DefaultVirtualTerminal terminal)
    {
        var option1 = terminal.GetCharacter(2, 3);
        Assert.Equal('1', option1.CharacterString[0]);
        Assert.Equal(new AnsiTextColor(AnsiColor.Default), option1.ForegroundColor);
        Assert.Equal(new AnsiTextColor(AnsiColor.Default), option1.BackgroundColor);
        Assert.Empty(option1.Modifiers);
        
        string option1Text = "1. Option One";
        for (int i = 0; i < option1Text.Length; i++)
        {
            var character = terminal.GetCharacter(2 + i, 3);
            Assert.Equal(option1Text[i], character.CharacterString[0]);
        }
        
        string option2Text = "2. Option Two";
        for (int i = 0; i < option2Text.Length; i++)
        {
            var character = terminal.GetCharacter(2 + i, 4);
            Assert.Equal(option2Text[i], character.CharacterString[0]);
        }
    }

    private void ValidatePrompt(DefaultVirtualTerminal terminal, string expectedPrompt)
    {
        for (int i = 0; i < expectedPrompt.Length; i++)
        {
            var character = terminal.GetCharacter(2 + i, 6);
            Assert.Equal(expectedPrompt[i], character.CharacterString[0]);
            Assert.Equal(new AnsiTextColor(AnsiColor.Red), character.ForegroundColor);
        }
    }

    [Fact]
    public void CanTestProgressBarRender()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        virtualTerminal.SetTerminalSize(new TerminalSize(50, 10));
        
        RenderProgressBar(virtualTerminal, 5, 5, 30, 60);
        
        ValidateProgressBar(virtualTerminal, 5, 5, 30, 60);
    }

    private void RenderProgressBar(DefaultVirtualTerminal terminal, int x, int y, int width, int percentage)
    {
        int filledWidth = (width * percentage) / 100;
        
        terminal.SetCursorPosition(x, y);
        terminal.PutCharacter('[');
        
        for (int i = 0; i < width; i++)
        {
            if (i < filledWidth)
            {
                terminal.SetForegroundColor(new AnsiTextColor(AnsiColor.Green));
                terminal.SetBackgroundColor(new AnsiTextColor(AnsiColor.Green));
                terminal.PutCharacter(' ');
            }
            else
            {
                terminal.SetForegroundColor(new AnsiTextColor(AnsiColor.Default));
                terminal.SetBackgroundColor(new AnsiTextColor(AnsiColor.Default));
                terminal.PutCharacter(' ');
            }
        }
        
        terminal.ResetColorAndSGR();
        terminal.PutCharacter(']');
        
        terminal.SetCursorPosition(x + width + 3, y);
        string percentText = $"{percentage}%";
        foreach (char c in percentText)
        {
            terminal.PutCharacter(c);
        }
    }

    private void ValidateProgressBar(DefaultVirtualTerminal terminal, int x, int y, int width, int percentage)
    {
        Assert.Equal('[', terminal.GetCharacter(x, y).CharacterString[0]);
        Assert.Equal(']', terminal.GetCharacter(x + width + 1, y).CharacterString[0]);
        
        int filledWidth = (width * percentage) / 100;
        
        for (int i = 0; i < width; i++)
        {
            var character = terminal.GetCharacter(x + 1 + i, y);
            if (i < filledWidth)
            {
                Assert.Equal(new AnsiTextColor(AnsiColor.Green), character.BackgroundColor);
            }
            else
            {
                Assert.Equal(new AnsiTextColor(AnsiColor.Default), character.BackgroundColor);
            }
        }
        
        string expectedPercent = $"{percentage}%";
        for (int i = 0; i < expectedPercent.Length; i++)
        {
            var character = terminal.GetCharacter(x + width + 3 + i, y);
            Assert.Equal(expectedPercent[i], character.CharacterString[0]);
        }
    }

    [Fact]
    public void CanTestBoxDrawing()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        virtualTerminal.SetTerminalSize(new TerminalSize(20, 10));
        
        DrawBox(virtualTerminal, 2, 2, 15, 5);
        
        ValidateBox(virtualTerminal, 2, 2, 15, 5);
    }

    private void DrawBox(DefaultVirtualTerminal terminal, int x, int y, int width, int height)
    {
        terminal.SetCursorPosition(x, y);
        terminal.PutCharacter('┌');
        
        for (int i = 1; i < width - 1; i++)
        {
            terminal.PutCharacter('─');
        }
        terminal.PutCharacter('┐');
        
        for (int row = 1; row < height - 1; row++)
        {
            terminal.SetCursorPosition(x, y + row);
            terminal.PutCharacter('│');
            
            for (int col = 1; col < width - 1; col++)
            {
                terminal.PutCharacter(' ');
            }
            
            terminal.PutCharacter('│');
        }
        
        terminal.SetCursorPosition(x, y + height - 1);
        terminal.PutCharacter('└');
        
        for (int i = 1; i < width - 1; i++)
        {
            terminal.PutCharacter('─');
        }
        terminal.PutCharacter('┘');
    }

    private void ValidateBox(DefaultVirtualTerminal terminal, int x, int y, int width, int height)
    {
        Assert.Equal('┌', terminal.GetCharacter(x, y).CharacterString[0]);
        Assert.Equal('┐', terminal.GetCharacter(x + width - 1, y).CharacterString[0]);
        Assert.Equal('└', terminal.GetCharacter(x, y + height - 1).CharacterString[0]);
        Assert.Equal('┘', terminal.GetCharacter(x + width - 1, y + height - 1).CharacterString[0]);
        
        for (int i = 1; i < width - 1; i++)
        {
            Assert.Equal('─', terminal.GetCharacter(x + i, y).CharacterString[0]);
            Assert.Equal('─', terminal.GetCharacter(x + i, y + height - 1).CharacterString[0]);
        }
        
        for (int i = 1; i < height - 1; i++)
        {
            Assert.Equal('│', terminal.GetCharacter(x, y + i).CharacterString[0]);
            Assert.Equal('│', terminal.GetCharacter(x + width - 1, y + i).CharacterString[0]);
        }
        
        for (int row = 1; row < height - 1; row++)
        {
            for (int col = 1; col < width - 1; col++)
            {
                Assert.Equal(' ', terminal.GetCharacter(x + col, y + row).CharacterString[0]);
            }
        }
    }
}