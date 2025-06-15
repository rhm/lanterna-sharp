using Lanterna.Core;
using Xunit;

namespace Lanterna.Tests.Core;

public class TerminalSizeTest
{
    [Fact]
    public void CanCreateTerminalSize()
    {
        var size = new TerminalSize(80, 24);
        
        Assert.Equal(80, size.Columns);
        Assert.Equal(24, size.Rows);
    }

    [Fact]
    public void ZeroSizeIsValid()
    {
        var size = new TerminalSize(0, 0);
        
        Assert.Equal(0, size.Columns);
        Assert.Equal(0, size.Rows);
    }

    [Fact]
    public void EqualSizesAreEqual()
    {
        var size1 = new TerminalSize(100, 50);
        var size2 = new TerminalSize(100, 50);
        
        Assert.Equal(size1, size2);
        Assert.True(size1.Equals(size2));
        Assert.Equal(size1.GetHashCode(), size2.GetHashCode());
    }

    [Fact]
    public void DifferentSizesAreNotEqual()
    {
        var size1 = new TerminalSize(80, 24);
        var size2 = new TerminalSize(100, 30);
        
        Assert.NotEqual(size1, size2);
        Assert.False(size1.Equals(size2));
    }

    [Fact]
    public void ToStringReturnsExpectedFormat()
    {
        var size = new TerminalSize(120, 40);
        var expected = "TerminalSize{columns=120, rows=40}";
        
        Assert.Equal(expected, size.ToString());
    }

    [Fact]
    public void CanCreateOneSize()
    {
        var size = TerminalSize.One;
        
        Assert.Equal(1, size.Columns);
        Assert.Equal(1, size.Rows);
    }

    [Fact]
    public void CanCreateZeroSize()
    {
        var size = TerminalSize.Zero;
        
        Assert.Equal(0, size.Columns);
        Assert.Equal(0, size.Rows);
    }
}