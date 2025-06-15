using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;
using Lanterna.Screen;
using Lanterna.Terminal.Virtual;
using Xunit;

namespace Lanterna.Tests.Integration;

/// <summary>
/// Demonstrates comprehensive UI testing by creating and validating a complete dialog interface.
/// This shows how the virtual terminal can be used to test complex UI scenarios end-to-end.
/// </summary>
public class CompleteUITest
{
    [Fact]
    public void CanTestCompleteDialogInterface()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        virtualTerminal.SetTerminalSize(new TerminalSize(60, 20));
        
        var screen = new TerminalScreen(virtualTerminal);
        screen.StartScreen();
        
        RenderDialog(screen, "Confirmation", "Are you sure you want to continue?", new[] { "Yes", "No" });
        
        screen.Refresh();
        
        ValidateDialogRender(virtualTerminal, "Confirmation", "Are you sure you want to continue?", new[] { "Yes", "No" });
        
        SimulateDialogInteraction(virtualTerminal, screen);
    }

    private void RenderDialog(TerminalScreen screen, string title, string message, string[] buttons)
    {
        var graphics = screen.NewTextGraphics();
        var terminalSize = screen.GetTerminalSize();
        
        int dialogWidth = Math.Max(Math.Max(title.Length, message.Length) + 4, 30);
        int dialogHeight = 8;
        int startX = (terminalSize.Columns - dialogWidth) / 2;
        int startY = (terminalSize.Rows - dialogHeight) / 2;
        
        graphics.SetForegroundColor(new AnsiTextColor(AnsiColor.White));
        graphics.SetBackgroundColor(new AnsiTextColor(AnsiColor.Blue));
        graphics.Fill(' ');
        
        graphics.SetForegroundColor(new AnsiTextColor(AnsiColor.Black));
        graphics.SetBackgroundColor(new AnsiTextColor(AnsiColor.White));
        
        for (int row = startY; row < startY + dialogHeight; row++)
        {
            for (int col = startX; col < startX + dialogWidth; col++)
            {
                graphics.SetCharacter(col, row, ' ');
            }
        }
        
        graphics.DrawRectangle(new TerminalPosition(startX, startY), 
                             new TerminalSize(dialogWidth, dialogHeight), '█');
        
        int titleX = startX + (dialogWidth - title.Length) / 2;
        graphics.PutString(titleX, startY + 1, title);
        
        int messageX = startX + (dialogWidth - message.Length) / 2;
        graphics.PutString(messageX, startY + 3, message);
        
        int buttonY = startY + dialogHeight - 2;
        int buttonSpacing = dialogWidth / buttons.Length;
        
        for (int i = 0; i < buttons.Length; i++)
        {
            int buttonX = startX + (buttonSpacing * i) + (buttonSpacing - buttons[i].Length) / 2;
            
            if (i == 0)
            {
                graphics.SetForegroundColor(new AnsiTextColor(AnsiColor.White));
                graphics.SetBackgroundColor(new AnsiTextColor(AnsiColor.Black));
            }
            else
            {
                graphics.SetForegroundColor(new AnsiTextColor(AnsiColor.Black));
                graphics.SetBackgroundColor(new AnsiTextColor(AnsiColor.White));
            }
            
            graphics.PutString(buttonX, buttonY, $"[{buttons[i]}]");
        }
        
        screen.SetCursorPosition(new TerminalPosition(startX + buttonSpacing / 2, buttonY));
    }

    private void ValidateDialogRender(DefaultVirtualTerminal terminal, string title, string message, string[] buttons)
    {
        var terminalSize = terminal.GetTerminalSize();
        int dialogWidth = Math.Max(Math.Max(title.Length, message.Length) + 4, 30);
        int dialogHeight = 8;
        int startX = (terminalSize.Columns - dialogWidth) / 2;
        int startY = (terminalSize.Rows - dialogHeight) / 2;
        
        int titleX = startX + (dialogWidth - title.Length) / 2;
        for (int i = 0; i < title.Length; i++)
        {
            var character = terminal.GetCharacter(titleX + i, startY + 1);
            Assert.Equal(title[i], character.CharacterString[0]);
            Assert.Equal(new AnsiTextColor(AnsiColor.Black), character.ForegroundColor);
            Assert.Equal(new AnsiTextColor(AnsiColor.White), character.BackgroundColor);
        }
        
        int messageX = startX + (dialogWidth - message.Length) / 2;
        for (int i = 0; i < message.Length; i++)
        {
            var character = terminal.GetCharacter(messageX + i, startY + 3);
            Assert.Equal(message[i], character.CharacterString[0]);
        }
        
        int buttonY = startY + dialogHeight - 2;
        int buttonSpacing = dialogWidth / buttons.Length;
        
        for (int i = 0; i < buttons.Length; i++)
        {
            int buttonX = startX + (buttonSpacing * i) + (buttonSpacing - buttons[i].Length) / 2;
            string buttonText = $"[{buttons[i]}]";
            
            for (int j = 0; j < buttonText.Length; j++)
            {
                var character = terminal.GetCharacter(buttonX + j, buttonY);
                Assert.Equal(buttonText[j], character.CharacterString[0]);
                
                if (i == 0)
                {
                    Assert.Equal(new AnsiTextColor(AnsiColor.White), character.ForegroundColor);
                    Assert.Equal(new AnsiTextColor(AnsiColor.Black), character.BackgroundColor);
                }
                else
                {
                    Assert.Equal(new AnsiTextColor(AnsiColor.Black), character.ForegroundColor);
                    Assert.Equal(new AnsiTextColor(AnsiColor.White), character.BackgroundColor);
                }
            }
        }
    }

    private void SimulateDialogInteraction(DefaultVirtualTerminal terminal, TerminalScreen screen)
    {
        terminal.AddInput(new KeyStroke(KeyType.Tab));
        terminal.AddInput(new KeyStroke(KeyType.Enter));
        
        int selectedButton = 0;
        bool dialogActive = true;
        
        while (dialogActive)
        {
            var keyStroke = terminal.PollInput();
            if (keyStroke == null) break;
            
            switch (keyStroke.KeyType)
            {
                case KeyType.Tab:
                    selectedButton = (selectedButton + 1) % 2;
                    UpdateButtonHighlight(screen, selectedButton);
                    screen.Refresh();
                    break;
                    
                case KeyType.Enter:
                    dialogActive = false;
                    break;
            }
        }
        
        Assert.Equal(1, selectedButton);
    }

    private void UpdateButtonHighlight(TerminalScreen screen, int selectedButton)
    {
        var graphics = screen.NewTextGraphics();
        var terminalSize = screen.GetTerminalSize();
        
        int dialogWidth = 30;
        int dialogHeight = 8;
        int startX = (terminalSize.Columns - dialogWidth) / 2;
        int startY = (terminalSize.Rows - dialogHeight) / 2;
        int buttonY = startY + dialogHeight - 2;
        int buttonSpacing = dialogWidth / 2;
        
        string[] buttons = { "Yes", "No" };
        
        for (int i = 0; i < buttons.Length; i++)
        {
            int buttonX = startX + (buttonSpacing * i) + (buttonSpacing - buttons[i].Length) / 2;
            
            if (i == selectedButton)
            {
                graphics.SetForegroundColor(new AnsiTextColor(AnsiColor.White));
                graphics.SetBackgroundColor(new AnsiTextColor(AnsiColor.Black));
            }
            else
            {
                graphics.SetForegroundColor(new AnsiTextColor(AnsiColor.Black));
                graphics.SetBackgroundColor(new AnsiTextColor(AnsiColor.White));
            }
            
            graphics.PutString(buttonX, buttonY, $"[{buttons[i]}]");
        }
    }

    [Fact]
    public void CanTestMenuSystem()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        virtualTerminal.SetTerminalSize(new TerminalSize(50, 15));
        
        var screen = new TerminalScreen(virtualTerminal);
        screen.StartScreen();
        
        string[] menuItems = { "New File", "Open File", "Save File", "Exit" };
        int selectedIndex = 0;
        
        RenderMenu(screen, menuItems, selectedIndex);
        screen.Refresh();
        
        ValidateMenuRender(virtualTerminal, menuItems, selectedIndex);
        
        virtualTerminal.AddInput(new KeyStroke(KeyType.ArrowDown));
        virtualTerminal.AddInput(new KeyStroke(KeyType.ArrowDown));
        virtualTerminal.AddInput(new KeyStroke(KeyType.Enter));
        
        while (true)
        {
            var keyStroke = virtualTerminal.PollInput();
            if (keyStroke == null) break;
            
            switch (keyStroke.KeyType)
            {
                case KeyType.ArrowDown:
                    selectedIndex = Math.Min(selectedIndex + 1, menuItems.Length - 1);
                    RenderMenu(screen, menuItems, selectedIndex);
                    screen.Refresh();
                    break;
                    
                case KeyType.ArrowUp:
                    selectedIndex = Math.Max(selectedIndex - 1, 0);
                    RenderMenu(screen, menuItems, selectedIndex);
                    screen.Refresh();
                    break;
                    
                case KeyType.Enter:
                    goto menuExit;
            }
        }
        
        menuExit:
        Assert.Equal(2, selectedIndex);
        ValidateMenuRender(virtualTerminal, menuItems, selectedIndex);
    }

    private void RenderMenu(TerminalScreen screen, string[] items, int selectedIndex)
    {
        var graphics = screen.NewTextGraphics();
        
        graphics.SetForegroundColor(new AnsiTextColor(AnsiColor.White));
        graphics.SetBackgroundColor(new AnsiTextColor(AnsiColor.Blue));
        graphics.Fill(' ');
        
        graphics.PutString(2, 1, "Main Menu");
        
        for (int i = 0; i < items.Length; i++)
        {
            if (i == selectedIndex)
            {
                graphics.SetForegroundColor(new AnsiTextColor(AnsiColor.Black));
                graphics.SetBackgroundColor(new AnsiTextColor(AnsiColor.White));
            }
            else
            {
                graphics.SetForegroundColor(new AnsiTextColor(AnsiColor.White));
                graphics.SetBackgroundColor(new AnsiTextColor(AnsiColor.Blue));
            }
            
            graphics.PutString(4, 3 + i, $"{i + 1}. {items[i]}");
        }
    }

    private void ValidateMenuRender(DefaultVirtualTerminal terminal, string[] items, int selectedIndex)
    {
        var titleChars = "Main Menu";
        for (int i = 0; i < titleChars.Length; i++)
        {
            var character = terminal.GetCharacter(2 + i, 1);
            Assert.Equal(titleChars[i], character.CharacterString[0]);
            Assert.Equal(new AnsiTextColor(AnsiColor.White), character.ForegroundColor);
            Assert.Equal(new AnsiTextColor(AnsiColor.Blue), character.BackgroundColor);
        }
        
        for (int i = 0; i < items.Length; i++)
        {
            string itemText = $"{i + 1}. {items[i]}";
            
            for (int j = 0; j < itemText.Length; j++)
            {
                var character = terminal.GetCharacter(4 + j, 3 + i);
                Assert.Equal(itemText[j], character.CharacterString[0]);
                
                if (i == selectedIndex)
                {
                    Assert.Equal(new AnsiTextColor(AnsiColor.Black), character.ForegroundColor);
                    Assert.Equal(new AnsiTextColor(AnsiColor.White), character.BackgroundColor);
                }
                else
                {
                    Assert.Equal(new AnsiTextColor(AnsiColor.White), character.ForegroundColor);
                    Assert.Equal(new AnsiTextColor(AnsiColor.Blue), character.BackgroundColor);
                }
            }
        }
    }
}