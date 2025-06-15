using Lanterna.Core;
using Lanterna.Screen;
using Lanterna.Terminal.Virtual;
using Xunit;

namespace Lanterna.Tests.Screen;

public class ManualRefreshTest
{
    [Fact]
    public void ManualRefreshSimulation()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        var screen = new TerminalScreen(virtualTerminal);
        screen.StartScreen();
        
        // Set a single character
        var testChar = TextCharacter.FromCharacter('X')[0];
        screen.SetCharacter(2, 1, testChar);
        
        // Before refresh - check buffer states
        var backChar = screen.GetBackCharacter(2, 1);
        var frontChar = screen.GetFrontCharacter(2, 1);
        
        Console.WriteLine($"Before refresh:");
        Console.WriteLine($"  Back buffer at (2,1): '{backChar.CharacterString}'");
        Console.WriteLine($"  Front buffer at (2,1): '{frontChar.CharacterString}'");
        Console.WriteLine($"  Are they different? {!backChar.Equals(frontChar)}");
        
        // Manually simulate what DrawCharacterToTerminal should do
        Console.WriteLine($"Manual simulation:");
        virtualTerminal.SetCursorPosition(2, 1);
        Console.WriteLine($"  Set cursor to (2,1)");
        var cursorAfterSet = virtualTerminal.GetCursorPosition();
        Console.WriteLine($"  Cursor position after SetCursorPosition: {cursorAfterSet}");
        
        // Replicate all the steps in DrawCharacterToTerminal
        virtualTerminal.SetForegroundColor(testChar.ForegroundColor);
        virtualTerminal.SetBackgroundColor(testChar.BackgroundColor);
        Console.WriteLine($"  Set colors");
        
        foreach (var modifier in testChar.Modifiers)
        {
            virtualTerminal.EnableSGR(modifier);
        }
        Console.WriteLine($"  Enabled {testChar.Modifiers.Count} modifiers");
        
        virtualTerminal.PutString(testChar.CharacterString);
        Console.WriteLine($"  PutString('{testChar.CharacterString}')");
        
        foreach (var modifier in testChar.Modifiers)
        {
            virtualTerminal.DisableSGR(modifier);
        }
        Console.WriteLine($"  Disabled modifiers");
        
        var cursorAfterPut = virtualTerminal.GetCursorPosition();
        Console.WriteLine($"  Cursor position after PutString: {cursorAfterPut}");
        
        var manualResult = virtualTerminal.GetCharacter(2, 1);
        Console.WriteLine($"  Character at (2,1) after manual operation: '{manualResult.CharacterString}'");
        
        // Now test exact replication of RefreshComplete sequence
        virtualTerminal = new DefaultVirtualTerminal();
        screen = new TerminalScreen(virtualTerminal);
        screen.StartScreen();
        screen.SetCharacter(2, 1, testChar);
        
        Console.WriteLine($"Testing exact RefreshComplete sequence:");
        
        // Replicate the exact RefreshComplete sequence
        virtualTerminal.ClearScreen();
        virtualTerminal.ResetColorAndSGR();
        virtualTerminal.SetCursorPosition(0, 0);
        Console.WriteLine($"  Cleared and reset terminal");
        
        // Get the character from back buffer (same as RefreshComplete)
        var backChar2 = screen.GetBackCharacter(2, 1);
        Console.WriteLine($"  Back buffer character at (2,1): '{backChar2.CharacterString}'");
        
        // Call the actual DrawCharacterToTerminal logic manually
        virtualTerminal.SetCursorPosition(2, 1);
        virtualTerminal.SetForegroundColor(backChar2.ForegroundColor);
        virtualTerminal.SetBackgroundColor(backChar2.BackgroundColor);
        foreach (var modifier in backChar2.Modifiers)
        {
            virtualTerminal.EnableSGR(modifier);
        }
        virtualTerminal.PutString(backChar2.CharacterString);
        foreach (var modifier in backChar2.Modifiers)
        {
            virtualTerminal.DisableSGR(modifier);
        }
        Console.WriteLine($"  Manually called DrawCharacterToTerminal sequence");
        
        var manualResult2 = virtualTerminal.GetCharacter(2, 1);
        Console.WriteLine($"  Character at (2,1) after manual RefreshComplete sequence: '{manualResult2.CharacterString}'");
        
        // Now test actual screen refresh
        virtualTerminal = new DefaultVirtualTerminal();
        screen = new TerminalScreen(virtualTerminal);
        screen.StartScreen();
        screen.SetCharacter(2, 1, testChar);
        
        Console.WriteLine($"Testing actual screen refresh:");
        screen.Refresh();
        
        var refreshResult = virtualTerminal.GetCharacter(2, 1);
        Console.WriteLine($"  Character at (2,1) after screen refresh: '{refreshResult.CharacterString}'");
        var finalCursor = virtualTerminal.GetCursorPosition();
        Console.WriteLine($"  Final cursor position: {finalCursor}");
        
        Assert.Equal(testChar, refreshResult);
    }
}