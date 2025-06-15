using Lanterna.Core;
using Lanterna.Graphics;
using Xunit;

namespace Lanterna.Tests.Core;

public class TextCharacterTest
{
    [Fact]
    public void DefaultCharacterHasExpectedValues()
    {
        var defaultChar = TextCharacter.DefaultCharacter;
        
        Assert.Equal(' ', defaultChar.CharacterString[0]);
        Assert.Equal(new AnsiTextColor(AnsiColor.Default), defaultChar.ForegroundColor);
        Assert.Equal(new AnsiTextColor(AnsiColor.Default), defaultChar.BackgroundColor);
        Assert.Empty(defaultChar.Modifiers);
    }

    [Fact]
    public void CanCreateCharacterWithAllProperties()
    {
        var modifiers = new HashSet<SGR> { SGR.Bold, SGR.Italic };
        var textChar = TextCharacter.FromCharacter('A', new AnsiTextColor(AnsiColor.Red), new AnsiTextColor(AnsiColor.Blue), SGR.Bold, SGR.Italic);
        
        Assert.Equal('A', textChar.CharacterString[0]);
        Assert.Equal(new AnsiTextColor(AnsiColor.Red), textChar.ForegroundColor);
        Assert.Equal(new AnsiTextColor(AnsiColor.Blue), textChar.BackgroundColor);
        Assert.Contains(SGR.Bold, textChar.Modifiers);
        Assert.Contains(SGR.Italic, textChar.Modifiers);
    }

    [Fact]
    public void CharactersWithSamePropertiesAreEqual()
    {
        var char1 = TextCharacter.FromCharacter('X', new AnsiTextColor(AnsiColor.Green), new AnsiTextColor(AnsiColor.Yellow), SGR.Bold);
        var char2 = TextCharacter.FromCharacter('X', new AnsiTextColor(AnsiColor.Green), new AnsiTextColor(AnsiColor.Yellow), SGR.Bold);
        
        Assert.Equal(char1, char2);
        Assert.True(char1.Equals(char2));
        Assert.Equal(char1.GetHashCode(), char2.GetHashCode());
    }

    [Fact]
    public void CharactersWithDifferentPropertiesAreNotEqual()
    {
        var char1 = TextCharacter.FromCharacter('A', new AnsiTextColor(AnsiColor.Red), new AnsiTextColor(AnsiColor.Blue));
        var char2 = TextCharacter.FromCharacter('B', new AnsiTextColor(AnsiColor.Red), new AnsiTextColor(AnsiColor.Blue));
        
        Assert.NotEqual(char1, char2);
        Assert.False(char1.Equals(char2));
    }

    [Fact]
    public void CanCreateCharacterWithJustCharAndColors()
    {
        var textChar = TextCharacter.FromCharacter('Z', new AnsiTextColor(AnsiColor.Cyan), new AnsiTextColor(AnsiColor.Magenta));
        
        Assert.Equal('Z', textChar.CharacterString[0]);
        Assert.Equal(new AnsiTextColor(AnsiColor.Cyan), textChar.ForegroundColor);
        Assert.Equal(new AnsiTextColor(AnsiColor.Magenta), textChar.BackgroundColor);
        Assert.Empty(textChar.Modifiers);
    }

    [Fact]
    public void CanCreateCharacterWithJustChar()
    {
        var textChar = TextCharacter.FromCharacter('Q')[0];
        
        Assert.Equal('Q', textChar.CharacterString[0]);
        Assert.Equal(new AnsiTextColor(AnsiColor.Default), textChar.ForegroundColor);
        Assert.Equal(new AnsiTextColor(AnsiColor.Default), textChar.BackgroundColor);
        Assert.Empty(textChar.Modifiers);
    }

    [Fact]
    public void ToStringReturnsCharacterAsString()
    {
        var textChar = TextCharacter.FromCharacter('H')[0];
        Assert.Equal("H", textChar.ToString());
    }
}