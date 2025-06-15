using Lanterna.Core;
using Lanterna.Terminal.Virtual;
using Xunit;

namespace Lanterna.Tests.Terminal.Virtual;

public class TextBufferTest
{
    private readonly TextBuffer _buffer;

    public TextBufferTest()
    {
        _buffer = new TextBuffer();
    }

    [Fact]
    public void CanSetAndGetCharacter()
    {
        var testChar = TextCharacter.FromCharacter('A')[0];
        _buffer.SetCharacter(0, 0, testChar);
        var retrieved = _buffer.GetCharacter(0, 0);
        Assert.Equal(testChar, retrieved);
    }

    [Fact]
    public void CanSetMultipleCharactersInSequence()
    {
        var chars = new[] { 'H', 'e', 'l', 'l', 'o' };
        for (int i = 0; i < chars.Length; i++)
        {
            _buffer.SetCharacter(0, i, TextCharacter.FromCharacter(chars[i])[0]);
        }

        for (int i = 0; i < chars.Length; i++)
        {
            var retrieved = _buffer.GetCharacter(0, i);
            Assert.Equal(chars[i].ToString(), retrieved.CharacterString);
        }
    }

    [Fact]
    public void BufferReturnsDefaultCharacterForUnsetPositions()
    {
        var retrieved = _buffer.GetCharacter(2, 3);
        Assert.Equal(TextCharacter.DefaultCharacter, retrieved);
    }

    [Fact]
    public void CanOverwriteCharacters()
    {
        _buffer.SetCharacter(1, 1, TextCharacter.FromCharacter('A')[0]);
        _buffer.SetCharacter(1, 1, TextCharacter.FromCharacter('B')[0]);
        var retrieved = _buffer.GetCharacter(1, 1);
        Assert.Equal("B", retrieved.CharacterString);
    }

    [Fact]
    public void CanClearBuffer()
    {
        _buffer.SetCharacter(0, 0, TextCharacter.FromCharacter('A')[0]);
        _buffer.SetCharacter(1, 1, TextCharacter.FromCharacter('B')[0]);
        
        _buffer.Clear();
        
        Assert.Equal(TextCharacter.DefaultCharacter, _buffer.GetCharacter(0, 0));
        Assert.Equal(TextCharacter.DefaultCharacter, _buffer.GetCharacter(1, 1));
    }

    [Fact]
    public void DebugSingleCharacterWrite()
    {
        var testChar = TextCharacter.FromCharacter('H')[0];
        var result = _buffer.SetCharacter(0, 0, testChar);
        var retrieved = _buffer.GetCharacter(0, 0);
        
        // Debug output
        Console.WriteLine($"Input character: '{testChar.CharacterString}'");
        Console.WriteLine($"Retrieved character: '{retrieved.CharacterString}'");
        Console.WriteLine($"SetCharacter result: {result}");
        Console.WriteLine($"Characters equal: {testChar.Equals(retrieved)}");
        
        Assert.Equal(testChar, retrieved);
    }
}