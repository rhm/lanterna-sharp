using Lanterna.Core;
using Xunit;

namespace Lanterna.Tests.Core;

public class TerminalPositionTest
{
    [Fact]
    public void CanCreateTerminalPosition()
    {
        var position = new TerminalPosition(10, 5);
        
        Assert.Equal(10, position.Column);
        Assert.Equal(5, position.Row);
    }

    [Fact]
    public void TopLeftCornerIsZeroZero()
    {
        var topLeft = TerminalPosition.TopLeftCorner;
        
        Assert.Equal(0, topLeft.Column);
        Assert.Equal(0, topLeft.Row);
    }

    [Fact]
    public void EqualPositionsAreEqual()
    {
        var pos1 = new TerminalPosition(15, 20);
        var pos2 = new TerminalPosition(15, 20);
        
        Assert.Equal(pos1, pos2);
        Assert.True(pos1.Equals(pos2));
        Assert.Equal(pos1.GetHashCode(), pos2.GetHashCode());
    }

    [Fact]
    public void DifferentPositionsAreNotEqual()
    {
        var pos1 = new TerminalPosition(10, 5);
        var pos2 = new TerminalPosition(10, 6);
        
        Assert.NotEqual(pos1, pos2);
        Assert.False(pos1.Equals(pos2));
    }

    [Fact]
    public void CanMovePosition()
    {
        var position = new TerminalPosition(5, 10);
        
        var moved = position.WithRelative(3, -2);
        
        Assert.Equal(8, moved.Column);
        Assert.Equal(8, moved.Row);
        
        Assert.Equal(5, position.Column);
        Assert.Equal(10, position.Row);
    }

    [Fact]
    public void CanMoveByOffset()
    {
        var position = new TerminalPosition(10, 10);
        
        var moved = position.WithRelative(5, 3);
        
        Assert.Equal(15, moved.Column);
        Assert.Equal(13, moved.Row);
    }

    [Fact]
    public void ToStringReturnsExpectedFormat()
    {
        var position = new TerminalPosition(25, 30);
        var expected = "TerminalPosition{column=25, row=30}";
        
        Assert.Equal(expected, position.ToString());
    }

    [Fact]
    public void CompareToWorksCorrectly()
    {
        var pos1 = new TerminalPosition(5, 10);
        var pos2 = new TerminalPosition(5, 15);
        var pos3 = new TerminalPosition(10, 10);
        var pos4 = new TerminalPosition(5, 10);
        
        Assert.True(pos1.CompareTo(pos2) < 0);
        Assert.True(pos1.CompareTo(pos3) < 0);
        Assert.Equal(0, pos1.CompareTo(pos4));
        Assert.True(pos2.CompareTo(pos1) > 0);
    }
}