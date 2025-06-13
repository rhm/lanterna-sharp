using Lanterna.Core;

namespace Lanterna.Gui2;

public static class TerminalTextUtils
{
    public static string FitString(string text, int maxWidth)
    {
        if (string.IsNullOrEmpty(text) || maxWidth <= 0)
            return string.Empty;
            
        if (text.Length <= maxWidth)
            return text;
            
        return text.Substring(0, maxWidth);
    }
    
    public static string[] GetWordWrappedText(int width, string text)
    {
        if (string.IsNullOrEmpty(text) || width <= 0)
            return Array.Empty<string>();
            
        var lines = new List<string>();
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var currentLine = string.Empty;
        
        foreach (var word in words)
        {
            var currentWord = word;
            var testLine = string.IsNullOrEmpty(currentLine) ? currentWord : currentLine + " " + currentWord;
            
            if (testLine.Length <= width)
            {
                currentLine = testLine;
            }
            else
            {
                if (!string.IsNullOrEmpty(currentLine))
                {
                    lines.Add(currentLine);
                    currentLine = currentWord;
                }
                else
                {
                    // Word is longer than width, break it
                    while (currentWord.Length > width)
                    {
                        lines.Add(currentWord.Substring(0, width));
                        currentWord = currentWord.Substring(width);
                    }
                    currentLine = currentWord;
                }
            }
        }
        
        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }
        
        return lines.ToArray();
    }
    
    public static int GetColumnWidth(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;
            
        // Simple implementation - just return string length
        // In more advanced scenarios, this would handle Unicode width calculations
        return text.Length;
    }
    
    public static int GetStringCharacterIndex(string text, int columnIndex)
    {
        if (string.IsNullOrEmpty(text) || columnIndex < 0)
            return 0;
            
        // Simple implementation - in more advanced scenarios this would handle 
        // double-width characters and combining characters
        return Math.Min(columnIndex, text.Length - 1);
    }
    
    public static bool IsCharDoubleWidth(char character)
    {
        // Simple implementation - returns false for now
        // In more advanced scenarios, this would detect CJK and other double-width characters
        return false;
    }
}