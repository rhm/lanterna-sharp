/*
 * This file is part of lanterna (https://github.com/mabe02/lanterna).
 *
 * lanterna is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Copyright (C) 2010-2020 Martin Berglund
 */
using Lanterna.Core;
using Lanterna.Input;
using System.Text.RegularExpressions;

namespace Lanterna.Gui2;

/// <summary>
/// Text input box that can be either single-line or multi-line and provides full text editing functionality.
/// Supports features like password masking, validation patterns, text change listeners, and mouse interaction.
/// </summary>
public class TextBox : AbstractInteractableComponent<TextBox>
{
    public enum Style
    {
        SingleLine,
        MultiLine
    }

    private readonly List<string> _lines;
    private readonly Style _style;
    private readonly int _maxLineLength;
    
    private TerminalPosition _caretPosition;
    private bool _caretWarp;
    private bool _readOnly;
    private bool _horizontalFocusSwitching;
    private bool _verticalFocusSwitching;
    
    private char? _mask;
    private Regex? _validationPattern;
    private ITextChangeListener? _textChangeListener;
    private TerminalSize? _explicitPreferredSize;
    private int _longestRow;

    /// <summary>
    /// Creates a single-line TextBox with default size
    /// </summary>
    public TextBox() : this(new TerminalSize(10, 1))
    {
    }

    /// <summary>
    /// Creates a TextBox with specified size
    /// </summary>
    /// <param name="preferredSize">Preferred size for the text box</param>
    public TextBox(TerminalSize? preferredSize) : this(preferredSize, "")
    {
    }

    /// <summary>
    /// Creates a TextBox with specified size and initial content
    /// </summary>
    /// <param name="preferredSize">Preferred size for the text box</param>
    /// <param name="initialContent">Initial text content</param>
    public TextBox(TerminalSize? preferredSize, string initialContent) 
        : this(preferredSize, initialContent, DetermineStyleFromContent(initialContent))
    {
    }

    /// <summary>
    /// Creates a TextBox with full control over all parameters
    /// </summary>
    /// <param name="preferredSize">Preferred size for the text box</param>
    /// <param name="initialContent">Initial text content</param>
    /// <param name="style">Single-line or multi-line style</param>
    public TextBox(TerminalSize? preferredSize, string initialContent, Style style)
    {
        _lines = new List<string>();
        _style = style;
        _maxLineLength = -1; // Unlimited by default
        _caretPosition = TerminalPosition.TopLeftCorner;
        _caretWarp = false;
        _readOnly = false;
        _horizontalFocusSwitching = true;
        _verticalFocusSwitching = true;
        _explicitPreferredSize = preferredSize;
        _longestRow = 0;

        SetText(initialContent ?? "", false);
    }

    private static Style DetermineStyleFromContent(string content)
    {
        return content.Contains('\n') ? Style.MultiLine : Style.SingleLine;
    }

    /// <summary>
    /// Gets the complete text content with line separators
    /// </summary>
    public string GetText()
    {
        return string.Join("\n", _lines);
    }

    /// <summary>
    /// Sets the complete text content, replacing all existing content
    /// </summary>
    /// <param name="text">New text content</param>
    public void SetText(string text)
    {
        SetText(text, true);
    }

    private void SetText(string text, bool userInteraction)
    {
        text = text ?? "";
        
        _lines.Clear();
        if (string.IsNullOrEmpty(text))
        {
            _lines.Add("");
        }
        else
        {
            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                if (_style == Style.SingleLine)
                {
                    // For single-line, replace newlines with spaces
                    _lines.Add(line.Replace('\n', ' ').Replace('\r', ' '));
                    break; // Only add the first line for single-line style
                }
                else
                {
                    _lines.Add(line.Replace("\r", "")); // Remove carriage returns but keep structure
                }
            }
        }

        // Validate all lines
        foreach (var line in _lines)
        {
            if (!IsValidated(line))
            {
                throw new ArgumentException("Text content does not match validation pattern");
            }
        }

        _caretPosition = TerminalPosition.TopLeftCorner;
        UpdateLongestRow();
        Invalidate();
        FireOnTextChanged(userInteraction);
    }

    /// <summary>
    /// Gets a specific line of text (for multi-line TextBox)
    /// </summary>
    /// <param name="index">Line index</param>
    /// <returns>The text of the specified line</returns>
    public string GetLine(int index)
    {
        if (index < 0 || index >= _lines.Count)
            return "";
        return _lines[index];
    }

    /// <summary>
    /// Adds a new line to the text box (multi-line only)
    /// </summary>
    /// <param name="line">Line to add</param>
    public void AddLine(string line)
    {
        if (_style == Style.SingleLine)
            throw new InvalidOperationException("Cannot add lines to single-line TextBox");

        line = line ?? "";
        if (!IsValidated(line))
            throw new ArgumentException("Line does not match validation pattern");

        _lines.Add(line);
        UpdateLongestRow();
        Invalidate();
        FireOnTextChanged(true);
    }

    /// <summary>
    /// Removes a specific line (multi-line only)
    /// </summary>
    /// <param name="lineIndex">Index of line to remove</param>
    public void RemoveLine(int lineIndex)
    {
        if (_style == Style.SingleLine)
            throw new InvalidOperationException("Cannot remove lines from single-line TextBox");

        if (lineIndex >= 0 && lineIndex < _lines.Count)
        {
            _lines.RemoveAt(lineIndex);
            if (_lines.Count == 0)
            {
                _lines.Add("");
            }

            // Adjust caret position if necessary
            if (_caretPosition.Row >= _lines.Count)
            {
                _caretPosition = new TerminalPosition(_caretPosition.Column, _lines.Count - 1);
            }
            if (_caretPosition.Column > _lines[_caretPosition.Row].Length)
            {
                _caretPosition = new TerminalPosition(_lines[_caretPosition.Row].Length, _caretPosition.Row);
            }

            UpdateLongestRow();
            Invalidate();
            FireOnTextChanged(true);
        }
    }

    /// <summary>
    /// Gets the current caret position
    /// </summary>
    public TerminalPosition CaretPosition => _caretPosition;

    /// <summary>
    /// Sets the caret position
    /// </summary>
    /// <param name="column">Column position</param>
    /// <param name="row">Row position</param>
    public void SetCaretPosition(int column, int row)
    {
        row = Math.Max(0, Math.Min(row, _lines.Count - 1));
        column = Math.Max(0, Math.Min(column, _lines[row].Length));
        _caretPosition = new TerminalPosition(column, row);
        Invalidate();
    }

    /// <summary>
    /// Sets password masking character
    /// </summary>
    /// <param name="mask">Character to use for masking</param>
    public void SetMask(char mask)
    {
        _mask = mask;
        Invalidate();
    }

    /// <summary>
    /// Clears password masking
    /// </summary>
    public void ClearMask()
    {
        _mask = null;
        Invalidate();
    }

    /// <summary>
    /// Gets the current mask character
    /// </summary>
    public char? GetMask() => _mask;

    /// <summary>
    /// Sets the validation pattern for input
    /// </summary>
    /// <param name="pattern">Regex pattern to validate against</param>
    public void SetValidationPattern(Regex? pattern)
    {
        if (pattern != null)
        {
            // Validate existing content
            foreach (var line in _lines)
            {
                if (!IsValidated(line))
                {
                    throw new InvalidOperationException("Validation pattern does not match existing content");
                }
            }
        }
        _validationPattern = pattern;
    }

    /// <summary>
    /// Gets the current validation pattern
    /// </summary>
    public Regex? GetValidationPattern() => _validationPattern;

    /// <summary>
    /// Sets the text change listener
    /// </summary>
    /// <param name="listener">Listener to be notified of text changes</param>
    public void SetTextChangeListener(ITextChangeListener? listener)
    {
        _textChangeListener = listener;
    }

    /// <summary>
    /// Gets the current text change listener
    /// </summary>
    public ITextChangeListener? GetTextChangeListener() => _textChangeListener;

    /// <summary>
    /// Sets whether the TextBox is read-only
    /// </summary>
    /// <param name="readOnly">True to make read-only</param>
    public void SetReadOnly(bool readOnly)
    {
        _readOnly = readOnly;
    }

    /// <summary>
    /// Gets whether the TextBox is read-only
    /// </summary>
    public bool IsReadOnly() => _readOnly;

    /// <summary>
    /// Gets the number of lines in the TextBox
    /// </summary>
    public int LineCount => _lines.Count;

    public override TerminalPosition? CursorLocation
    {
        get
        {
            if (IsFocused)
            {
                return new TerminalPosition(Math.Min(_caretPosition.Column, Size.Columns - 1), 
                                          Math.Min(_caretPosition.Row, Size.Rows - 1));
            }
            return null;
        }
    }

    protected override IComponentRenderer<TextBox> CreateDefaultRenderer()
    {
        return new DefaultTextBoxRenderer();
    }

    protected override IInteractable.Result HandleKeyStroke(KeyStroke keyStroke)
    {
        if (_readOnly)
        {
            return HandleReadOnlyInput(keyStroke);
        }

        switch (keyStroke.KeyType)
        {
            case KeyType.Character:
                return HandleCharacterInput(keyStroke);
            case KeyType.Backspace:
                return HandleBackspace();
            case KeyType.Delete:
                return HandleDelete();
            case KeyType.ArrowLeft:
                return HandleArrowLeft();
            case KeyType.ArrowRight:
                return HandleArrowRight();
            case KeyType.ArrowUp:
                return HandleArrowUp();
            case KeyType.ArrowDown:
                return HandleArrowDown();
            case KeyType.Enter:
                return HandleEnter();
            case KeyType.Home:
                return HandleHome();
            case KeyType.End:
                return HandleEnd();
            case KeyType.PageUp:
                return HandlePageUp();
            case KeyType.PageDown:
                return HandlePageDown();
            case KeyType.Tab:
                return HandleTab();
            default:
                if (keyStroke is MouseAction mouseAction)
                {
                    return HandleMouseAction(mouseAction);
                }
                return IInteractable.Result.Unhandled;
        }
    }

    private IInteractable.Result HandleReadOnlyInput(KeyStroke keyStroke)
    {
        // Allow navigation in read-only mode
        switch (keyStroke.KeyType)
        {
            case KeyType.ArrowLeft:
                return HandleArrowLeft();
            case KeyType.ArrowRight:
                return HandleArrowRight();
            case KeyType.ArrowUp:
                return HandleArrowUp();
            case KeyType.ArrowDown:
                return HandleArrowDown();
            case KeyType.Home:
                return HandleHome();
            case KeyType.End:
                return HandleEnd();
            case KeyType.PageUp:
                return HandlePageUp();
            case KeyType.PageDown:
                return HandlePageDown();
            case KeyType.Tab:
                return HandleTab();
            default:
                if (keyStroke is MouseAction mouseAction)
                {
                    return HandleMouseAction(mouseAction);
                }
                return IInteractable.Result.Unhandled;
        }
    }

    private IInteractable.Result HandleCharacterInput(KeyStroke keyStroke)
    {
        if (keyStroke.Character == null)
            return IInteractable.Result.Unhandled;

        char character = keyStroke.Character.Value;
        if (char.IsControl(character))
            return IInteractable.Result.Unhandled;

        string currentLine = _lines[_caretPosition.Row];
        string newLine = currentLine.Substring(0, _caretPosition.Column) + 
                        character + 
                        currentLine.Substring(_caretPosition.Column);

        if (IsValidated(newLine))
        {
            _lines[_caretPosition.Row] = newLine;
            _caretPosition = _caretPosition.WithRelativeColumn(1);
            UpdateLongestRow();
            Invalidate();
            FireOnTextChanged(true);
        }

        return IInteractable.Result.Handled;
    }

    private IInteractable.Result HandleBackspace()
    {
        if (_caretPosition.Column > 0)
        {
            // Delete character before caret
            string currentLine = _lines[_caretPosition.Row];
            string newLine = currentLine.Substring(0, _caretPosition.Column - 1) + 
                            currentLine.Substring(_caretPosition.Column);
            
            if (IsValidated(newLine))
            {
                _lines[_caretPosition.Row] = newLine;
                _caretPosition = _caretPosition.WithRelativeColumn(-1);
                UpdateLongestRow();
                Invalidate();
                FireOnTextChanged(true);
            }
        }
        else if (_style == Style.MultiLine && _caretPosition.Row > 0)
        {
            // Join with previous line
            string currentLine = _lines[_caretPosition.Row];
            string previousLine = _lines[_caretPosition.Row - 1];
            string concatenatedLine = previousLine + currentLine;
            
            if (IsValidated(concatenatedLine))
            {
                _lines.RemoveAt(_caretPosition.Row);
                _caretPosition = new TerminalPosition(previousLine.Length, _caretPosition.Row - 1);
                _lines[_caretPosition.Row] = concatenatedLine;
                UpdateLongestRow();
                Invalidate();
                FireOnTextChanged(true);
            }
        }

        return IInteractable.Result.Handled;
    }

    private IInteractable.Result HandleDelete()
    {
        string currentLine = _lines[_caretPosition.Row];
        
        if (_caretPosition.Column < currentLine.Length)
        {
            // Delete character at caret
            string newLine = currentLine.Substring(0, _caretPosition.Column) + 
                            currentLine.Substring(_caretPosition.Column + 1);
            
            if (IsValidated(newLine))
            {
                _lines[_caretPosition.Row] = newLine;
                UpdateLongestRow();
                Invalidate();
                FireOnTextChanged(true);
            }
        }
        else if (_style == Style.MultiLine && _caretPosition.Row < _lines.Count - 1)
        {
            // Join with next line
            string nextLine = _lines[_caretPosition.Row + 1];
            string concatenatedLine = currentLine + nextLine;
            
            if (IsValidated(concatenatedLine))
            {
                _lines[_caretPosition.Row] = concatenatedLine;
                _lines.RemoveAt(_caretPosition.Row + 1);
                UpdateLongestRow();
                Invalidate();
                FireOnTextChanged(true);
            }
        }

        return IInteractable.Result.Handled;
    }

    private IInteractable.Result HandleArrowLeft()
    {
        if (_caretPosition.Column > 0)
        {
            _caretPosition = _caretPosition.WithRelativeColumn(-1);
            Invalidate();
        }
        else if (_style == Style.MultiLine && _caretWarp && _caretPosition.Row > 0)
        {
            // Wrap to end of previous line
            _caretPosition = new TerminalPosition(_lines[_caretPosition.Row - 1].Length, _caretPosition.Row - 1);
            Invalidate();
        }
        else if (_horizontalFocusSwitching)
        {
            return IInteractable.Result.MoveFocusLeft;
        }

        return IInteractable.Result.Handled;
    }

    private IInteractable.Result HandleArrowRight()
    {
        if (_caretPosition.Column < _lines[_caretPosition.Row].Length)
        {
            _caretPosition = _caretPosition.WithRelativeColumn(1);
            Invalidate();
        }
        else if (_style == Style.MultiLine && _caretWarp && _caretPosition.Row < _lines.Count - 1)
        {
            // Wrap to start of next line
            _caretPosition = new TerminalPosition(0, _caretPosition.Row + 1);
            Invalidate();
        }
        else if (_horizontalFocusSwitching)
        {
            return IInteractable.Result.MoveFocusRight;
        }

        return IInteractable.Result.Handled;
    }

    private IInteractable.Result HandleArrowUp()
    {
        if (_style == Style.MultiLine && _caretPosition.Row > 0)
        {
            int newRow = _caretPosition.Row - 1;
            int newColumn = Math.Min(_caretPosition.Column, _lines[newRow].Length);
            _caretPosition = new TerminalPosition(newColumn, newRow);
            Invalidate();
            return IInteractable.Result.Handled;
        }
        else if (_verticalFocusSwitching)
        {
            return IInteractable.Result.MoveFocusUp;
        }
        
        return IInteractable.Result.Handled;
    }

    private IInteractable.Result HandleArrowDown()
    {
        if (_style == Style.MultiLine && _caretPosition.Row < _lines.Count - 1)
        {
            int newRow = _caretPosition.Row + 1;
            int newColumn = Math.Min(_caretPosition.Column, _lines[newRow].Length);
            _caretPosition = new TerminalPosition(newColumn, newRow);
            Invalidate();
            return IInteractable.Result.Handled;
        }
        else if (_verticalFocusSwitching)
        {
            return IInteractable.Result.MoveFocusDown;
        }
        
        return IInteractable.Result.Handled;
    }

    private IInteractable.Result HandleEnter()
    {
        if (_style == Style.SingleLine)
        {
            return IInteractable.Result.MoveFocusNext;
        }

        // Split current line at caret position
        string currentLine = _lines[_caretPosition.Row];
        string newLine = currentLine.Substring(_caretPosition.Column);
        string oldLine = currentLine.Substring(0, _caretPosition.Column);

        if (IsValidated(newLine) && IsValidated(oldLine))
        {
            _lines[_caretPosition.Row] = oldLine;
            _lines.Insert(_caretPosition.Row + 1, newLine);
            _caretPosition = new TerminalPosition(0, _caretPosition.Row + 1);
            UpdateLongestRow();
            Invalidate();
            FireOnTextChanged(true);
        }

        return IInteractable.Result.Handled;
    }

    private IInteractable.Result HandleHome()
    {
        _caretPosition = new TerminalPosition(0, _caretPosition.Row);
        Invalidate();
        return IInteractable.Result.Handled;
    }

    private IInteractable.Result HandleEnd()
    {
        _caretPosition = new TerminalPosition(_lines[_caretPosition.Row].Length, _caretPosition.Row);
        Invalidate();
        return IInteractable.Result.Handled;
    }

    private IInteractable.Result HandlePageUp()
    {
        if (_style == Style.MultiLine)
        {
            int newRow = Math.Max(0, _caretPosition.Row - Size.Rows);
            int newColumn = Math.Min(_caretPosition.Column, _lines[newRow].Length);
            _caretPosition = new TerminalPosition(newColumn, newRow);
            Invalidate();
        }
        return IInteractable.Result.Handled;
    }

    private IInteractable.Result HandlePageDown()
    {
        if (_style == Style.MultiLine)
        {
            int newRow = Math.Min(_lines.Count - 1, _caretPosition.Row + Size.Rows);
            int newColumn = Math.Min(_caretPosition.Column, _lines[newRow].Length);
            _caretPosition = new TerminalPosition(newColumn, newRow);
            Invalidate();
        }
        return IInteractable.Result.Handled;
    }

    private IInteractable.Result HandleTab()
    {
        return IInteractable.Result.MoveFocusNext;
    }

    private IInteractable.Result HandleMouseAction(MouseAction mouseAction)
    {
        if (mouseAction.IsMouseDown)
        {
            // Position caret at mouse click location
            var clickPosition = mouseAction.Position;
            var globalPosition = ToGlobal(TerminalPosition.TopLeftCorner);
            
            if (globalPosition.HasValue)
            {
                int newColumn = clickPosition.Column - globalPosition.Value.Column;
                int newRow = clickPosition.Row - globalPosition.Value.Row;
                
                if (newRow >= 0 && newRow < _lines.Count && newRow < Size.Rows)
                {
                    newColumn = Math.Max(0, Math.Min(newColumn, _lines[newRow].Length));
                    _caretPosition = new TerminalPosition(newColumn, newRow);
                    Invalidate();
                }
            }
        }
        
        return IInteractable.Result.Handled;
    }

    private bool IsValidated(string text)
    {
        return _validationPattern == null || string.IsNullOrEmpty(text) || _validationPattern.IsMatch(text);
    }

    private void UpdateLongestRow()
    {
        _longestRow = _lines.Max(line => line.Length);
    }

    private void FireOnTextChanged(bool userInteraction)
    {
        _textChangeListener?.OnTextChanged(GetText(), userInteraction);
    }

    /// <summary>
    /// Basic TextBox renderer that handles single and multi-line display
    /// </summary>
    private class DefaultTextBoxRenderer : IInteractableRenderer<TextBox>
    {
        public TerminalSize GetPreferredSize(TextBox component)
        {
            if (component._explicitPreferredSize.HasValue)
            {
                return component._explicitPreferredSize.Value;
            }

            if (component._style == Style.SingleLine)
            {
                int width = Math.Max(10, component._longestRow + 1);
                return new TerminalSize(width, 1);
            }
            else
            {
                int width = Math.Max(10, component._longestRow + 1);
                int height = Math.Max(3, component._lines.Count);
                return new TerminalSize(width, height);
            }
        }

        public void DrawComponent(ITextGUIGraphics graphics, TextBox component)
        {
            graphics.ApplyThemeStyle(component.Theme.GetDefinition(typeof(TextBox)).Normal);
            graphics.Fill(' ');

            int visibleRows = Math.Min(graphics.Size.Rows, component._lines.Count);
            
            for (int row = 0; row < visibleRows; row++)
            {
                string line = component._lines[row];
                string displayText;
                
                if (component._mask.HasValue)
                {
                    displayText = new string(component._mask.Value, line.Length);
                }
                else
                {
                    displayText = line;
                }

                if (displayText.Length > graphics.Size.Columns)
                {
                    displayText = displayText.Substring(0, graphics.Size.Columns);
                }

                if (!string.IsNullOrEmpty(displayText))
                {
                    graphics.PutString(0, row, displayText);
                }
            }
        }

        public TerminalPosition? GetCursorLocation(TextBox component)
        {
            if (!component.IsFocused)
                return null;

            int cursorColumn = Math.Min(component._caretPosition.Column, component.Size.Columns - 1);
            int cursorRow = Math.Min(component._caretPosition.Row, component.Size.Rows - 1);
            
            return new TerminalPosition(cursorColumn, cursorRow);
        }
    }
}