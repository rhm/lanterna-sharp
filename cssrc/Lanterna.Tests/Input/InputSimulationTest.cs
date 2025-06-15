using Lanterna.Input;
using Lanterna.Terminal.Virtual;
using Xunit;

namespace Lanterna.Tests.Input;

/// <summary>
/// Demonstrates how to test input handling using the virtual terminal's input queue.
/// This allows testing keyboard input scenarios without actual user interaction.
/// </summary>
public class InputSimulationTest
{
    [Fact]
    public void CanSimulateBasicKeystrokes()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        
        virtualTerminal.AddInput(new KeyStroke('A', false, false));
        virtualTerminal.AddInput(new KeyStroke('B', false, false));
        virtualTerminal.AddInput(new KeyStroke('C', false, false));
        
        var key1 = virtualTerminal.PollInput();
        var key2 = virtualTerminal.PollInput();
        var key3 = virtualTerminal.PollInput();
        var key4 = virtualTerminal.PollInput();
        
        Assert.Equal(new KeyStroke('A', false, false), key1);
        Assert.Equal(new KeyStroke('B', false, false), key2);
        Assert.Equal(new KeyStroke('C', false, false), key3);
        Assert.Null(key4);
    }

    [Fact]
    public void CanSimulateSpecialKeys()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        
        virtualTerminal.AddInput(new KeyStroke(KeyType.Enter));
        virtualTerminal.AddInput(new KeyStroke(KeyType.Tab));
        virtualTerminal.AddInput(new KeyStroke(KeyType.Escape));
        virtualTerminal.AddInput(new KeyStroke(KeyType.ArrowUp));
        virtualTerminal.AddInput(new KeyStroke(KeyType.ArrowDown));
        
        Assert.Equal(KeyType.Enter, virtualTerminal.PollInput()?.KeyType);
        Assert.Equal(KeyType.Tab, virtualTerminal.PollInput()?.KeyType);
        Assert.Equal(KeyType.Escape, virtualTerminal.PollInput()?.KeyType);
        Assert.Equal(KeyType.ArrowUp, virtualTerminal.PollInput()?.KeyType);
        Assert.Equal(KeyType.ArrowDown, virtualTerminal.PollInput()?.KeyType);
    }

    [Fact]
    public void CanSimulateModifierKeys()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        
        virtualTerminal.AddInput(new KeyStroke('A', true, false));
        virtualTerminal.AddInput(new KeyStroke('B', false, false));
        virtualTerminal.AddInput(new KeyStroke('C', false, true));
        
        var ctrlA = virtualTerminal.PollInput();
        var shiftB = virtualTerminal.PollInput();
        var altC = virtualTerminal.PollInput();
        
        Assert.True(ctrlA?.IsCtrlDown);
        Assert.False(ctrlA?.IsShiftDown);
        Assert.False(ctrlA?.IsAltDown);
        
        Assert.False(shiftB?.IsCtrlDown);
        Assert.True(shiftB?.IsShiftDown);
        Assert.False(shiftB?.IsAltDown);
        
        Assert.False(altC?.IsCtrlDown);
        Assert.False(altC?.IsShiftDown);
        Assert.True(altC?.IsAltDown);
    }

    [Fact]
    public void CanTestTextInputHandling()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        string inputText = "Hello World!";
        
        foreach (char c in inputText)
        {
            virtualTerminal.AddInput(new KeyStroke(c, false, false));
        }
        virtualTerminal.AddInput(new KeyStroke(KeyType.Enter));
        
        string receivedText = "";
        KeyStroke? keyStroke;
        
        while ((keyStroke = virtualTerminal.PollInput()) != null)
        {
            if (keyStroke.KeyType == KeyType.Character)
            {
                receivedText += keyStroke.Character;
            }
            else if (keyStroke.KeyType == KeyType.Enter)
            {
                break;
            }
        }
        
        Assert.Equal(inputText, receivedText);
    }

    [Fact]
    public void CanTestMenuNavigation()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        
        virtualTerminal.AddInput(new KeyStroke(KeyType.ArrowDown));
        virtualTerminal.AddInput(new KeyStroke(KeyType.ArrowDown));
        virtualTerminal.AddInput(new KeyStroke(KeyType.ArrowUp));
        virtualTerminal.AddInput(new KeyStroke(KeyType.Enter));
        
        int selectedIndex = 0;
        KeyStroke? keyStroke;
        
        while ((keyStroke = virtualTerminal.PollInput()) != null)
        {
            switch (keyStroke.KeyType)
            {
                case KeyType.ArrowDown:
                    selectedIndex = Math.Min(selectedIndex + 1, 2);
                    break;
                case KeyType.ArrowUp:
                    selectedIndex = Math.Max(selectedIndex - 1, 0);
                    break;
                case KeyType.Enter:
                    goto menuExit;
            }
        }
        
        menuExit:
        Assert.Equal(1, selectedIndex);
    }

    [Fact]
    public void CanTestComplexInputSequence()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        
        virtualTerminal.AddInput(new KeyStroke('t', false, false));
        virtualTerminal.AddInput(new KeyStroke('e', false, false));
        virtualTerminal.AddInput(new KeyStroke('s', false, false));
        virtualTerminal.AddInput(new KeyStroke('t', false, false));
        virtualTerminal.AddInput(new KeyStroke(KeyType.Backspace));
        virtualTerminal.AddInput(new KeyStroke(KeyType.Backspace));
        virtualTerminal.AddInput(new KeyStroke('x', false, false));
        virtualTerminal.AddInput(new KeyStroke('t', false, false));
        virtualTerminal.AddInput(new KeyStroke(KeyType.Enter));
        
        string buffer = "";
        KeyStroke? keyStroke;
        
        while ((keyStroke = virtualTerminal.PollInput()) != null)
        {
            switch (keyStroke.KeyType)
            {
                case KeyType.Character:
                    buffer += keyStroke.Character;
                    break;
                case KeyType.Backspace:
                    if (buffer.Length > 0)
                        buffer = buffer[..^1];
                    break;
                case KeyType.Enter:
                    goto inputComplete;
            }
        }
        
        inputComplete:
        Assert.Equal("text", buffer);
    }

    [Fact]
    public void CanTestFunctionKeys()
    {
        var virtualTerminal = new DefaultVirtualTerminal();
        
        virtualTerminal.AddInput(new KeyStroke(KeyType.F1));
        virtualTerminal.AddInput(new KeyStroke(KeyType.F5));
        virtualTerminal.AddInput(new KeyStroke(KeyType.F12));
        
        var f1 = virtualTerminal.PollInput();
        var f5 = virtualTerminal.PollInput();
        var f12 = virtualTerminal.PollInput();
        
        Assert.Equal(KeyType.F1, f1?.KeyType);
        Assert.Equal(KeyType.F5, f5?.KeyType);
        Assert.Equal(KeyType.F12, f12?.KeyType);
    }
}