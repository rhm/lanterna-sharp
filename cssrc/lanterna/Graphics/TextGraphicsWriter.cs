using System.Text;
using Lanterna.Core;
using Lanterna.Screen;

namespace Lanterna.Graphics;

/// <summary>
/// Writer pattern for text graphics that provides advanced text rendering with word wrapping,
/// style support, and control character handling.
/// </summary>
public class TextGraphicsWriter : IStyleSet<TextGraphicsWriter>, IScreenTranslator
{
    private readonly ITextGraphics _backend;
    private TerminalPosition _cursorPosition;
    private ITextColor? _foregroundColor;
    private ITextColor? _backgroundColor;
    private readonly HashSet<SGR> _style = new();
    private WrapBehaviour _wrapBehaviour = WrapBehaviour.WORD;
    private bool _styleable = true;

    public TextGraphicsWriter(ITextGraphics backend)
    {
        _backend = backend;
        _foregroundColor = backend.ForegroundColor;
        _backgroundColor = backend.BackgroundColor;
        _style.UnionWith(backend.ActiveModifiers);
        _cursorPosition = new TerminalPosition(0, 0);
    }

    /// <summary>
    /// Writes a string to the graphics context with advanced formatting, wrapping, and style support
    /// </summary>
    /// <param name="text">The text to write</param>
    /// <returns>This TextGraphicsWriter for method chaining</returns>
    public TextGraphicsWriter PutString(string text)
    {
        var wordpart = new StringBuilder();
        var originalStyle = new StyleSet()
            .SetForegroundColor(_backend.ForegroundColor)
            .SetBackgroundColor(_backend.BackgroundColor)
            .SetModifiers(_backend.ActiveModifiers);
        SetBackendStyleFromThis();

        int wordlen = 0; // the whole column-length of the word.
        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];
            switch (ch)
            {
                case '\n':
                    Flush(wordpart, wordlen);
                    wordlen = 0;
                    Linefeed(-1); // -1 means explicit.
                    break;
                case '\t':
                    Flush(wordpart, wordlen);
                    wordlen = 0;
                    if (_backend.TabBehaviour != TabBehaviour.Ignore)
                    {
                        string repl = _backend.TabBehaviour.GetTabReplacement(_cursorPosition.Column);
                        for (int j = 0; j < repl.Length; j++)
                        {
                            _backend.SetCharacter(_cursorPosition.WithRelativeColumn(j), repl[j]);
                        }
                        _cursorPosition = _cursorPosition.WithRelativeColumn(repl.Length);
                    }
                    else
                    {
                        Linefeed(2);
                        PutControlChar(ch);
                    }
                    break;
                case '\x1B': // Escape character
                    if (IsStyleable())
                    {
                        Stash(wordpart, wordlen);
                        string? seq = TerminalTextUtils.GetANSIControlSequenceAt(text, i);
                        if (seq != null)
                        {
                            TerminalTextUtils.UpdateModifiersFromCSICode(seq, this, originalStyle);
                            SetBackendStyleFromThis();
                            i += seq.Length - 1;
                        }
                        else
                        {
                            Flush(wordpart, wordlen);
                            wordlen = 0;
                            Linefeed(2);
                            PutControlChar(ch);
                        }
                    }
                    else
                    {
                        Flush(wordpart, wordlen);
                        wordlen = 0;
                        Linefeed(2);
                        PutControlChar(ch);
                    }
                    break;
                default:
                    if (char.IsControl(ch))
                    {
                        Flush(wordpart, wordlen);
                        wordlen = 0;
                        Linefeed(1);
                        PutControlChar(ch);
                    }
                    else if (char.IsWhiteSpace(ch))
                    {
                        Flush(wordpart, wordlen);
                        wordlen = 0;
                        _backend.SetCharacter(_cursorPosition, ch);
                        _cursorPosition = _cursorPosition.WithRelativeColumn(1);
                    }
                    else if (TerminalTextUtils.IsCharCJK(ch))
                    {
                        Flush(wordpart, wordlen);
                        wordlen = 0;
                        Linefeed(2);
                        _backend.SetCharacter(_cursorPosition, ch);
                        _cursorPosition = _cursorPosition.WithRelativeColumn(2);
                    }
                    else
                    {
                        if (_wrapBehaviour.KeepWords())
                        {
                            // TODO: if at end of line despite starting at col 0, then split word.
                            wordpart.Append(ch);
                            wordlen++;
                        }
                        else
                        {
                            Linefeed(1);
                            _backend.SetCharacter(_cursorPosition, ch);
                            _cursorPosition = _cursorPosition.WithRelativeColumn(1);
                        }
                    }
                    break;
            }
            Linefeed(wordlen);
        }
        Flush(wordpart, wordlen);
        SetBackendStyleFrom(originalStyle);
        return this;
    }

    private void Linefeed(int lenToFit)
    {
        int curCol = _cursorPosition.Column;
        int spaceLeft = _backend.Size.Columns - curCol;
        if (_wrapBehaviour.AllowLineFeed())
        {
            bool wantWrap = curCol > 0 && lenToFit > spaceLeft;
            if (lenToFit < 0 || (wantWrap && _wrapBehaviour.AutoWrap()))
            {
                // TODO: clear to end of current line?
                _cursorPosition = _cursorPosition.WithColumn(0).WithRelativeRow(1);
            }
        }
        else
        {
            if (lenToFit < 0) // encode explicit line feed
            {
                PutControlChar('\n');
            }
        }
    }

    public void PutControlChar(char ch)
    {
        char subst;
        switch (ch)
        {
            case '\x1B': subst = '['; break;
            case '\x1C': subst = '\\'; break;
            case '\x1D': subst = ']'; break;
            case '\x1E': subst = '^'; break;
            case '\x1F': subst = '_'; break;
            case '\x7F': subst = '?'; break;
            default:
                if (ch <= 26)
                {
                    subst = (char)(ch + '@');
                }
                else // normal character - or 0x80-0x9F
                {
                    // just write it out, anyway:
                    _backend.SetCharacter(_cursorPosition, ch);
                    _cursorPosition = _cursorPosition.WithRelativeColumn(1);
                    return;
                }
                break;
        }

        var style = new HashSet<SGR>(ActiveModifiers);
        if (style.Contains(SGR.Reverse))
        {
            style.Remove(SGR.Reverse);
        }
        else
        {
            style.Add(SGR.Reverse);
        }

        var tc = TextCharacter.FromCharacter('^', ForegroundColor, BackgroundColor, style.ToArray());
        _backend.SetCharacter(_cursorPosition, tc);
        _cursorPosition = _cursorPosition.WithRelativeColumn(1);
        tc = tc.WithCharacter(subst);
        _backend.SetCharacter(_cursorPosition, tc);
        _cursorPosition = _cursorPosition.WithRelativeColumn(1);
    }

    // A word (a sequence of characters that is kept together when word-wrapping)
    // may consist of differently styled parts. This class describes one such part.
    private class WordPart : StyleSet
    {
        public string Word { get; }
        public int WordLen { get; }

        public WordPart(string word, int wordlen, TextGraphicsWriter style) : base()
        {
            Word = word;
            WordLen = wordlen;
            SetForegroundColor(style.ForegroundColor);
            SetBackgroundColor(style.BackgroundColor);
            SetModifiers(style.ActiveModifiers);
        }
    }

    private readonly List<WordPart> _chunkQueue = new();

    private void Stash(StringBuilder word, int wordlen)
    {
        if (word.Length > 0)
        {
            var chunk = new WordPart(word.ToString(), wordlen, this);
            _chunkQueue.Add(chunk);
            // for convenience the StringBuilder is reset:
            word.Clear();
        }
    }

    private void Flush(StringBuilder word, int wordlen)
    {
        Stash(word, wordlen);
        if (_chunkQueue.Count == 0)
        {
            return;
        }

        int row = _cursorPosition.Row;
        int col = _cursorPosition.Column;
        int offset = 0;
        foreach (var chunk in _chunkQueue)
        {
            SetBackendStyleFrom(chunk);
            _backend.PutString(col + offset, row, chunk.Word);
            offset += chunk.WordLen;
        }
        _chunkQueue.Clear(); // they're done.
        // set cursor right behind the word:
        _cursorPosition = _cursorPosition.WithColumn(col + offset);
        SetBackendStyleFromThis();
    }

    /// <summary>
    /// Gets or sets the current cursor position
    /// </summary>
    public TerminalPosition CursorPosition
    {
        get => _cursorPosition;
        set => _cursorPosition = value;
    }

    /// <summary>
    /// Gets the current foreground color
    /// </summary>
    public ITextColor ForegroundColor => _foregroundColor ?? new AnsiTextColor(AnsiColor.Default);

    /// <summary>
    /// Sets the foreground color
    /// </summary>
    /// <param name="foreground">The foreground color to set</param>
    /// <returns>This TextGraphicsWriter for method chaining</returns>
    public TextGraphicsWriter SetForegroundColor(ITextColor foreground)
    {
        _foregroundColor = foreground;
        return this;
    }

    /// <summary>
    /// Gets the current background color
    /// </summary>
    public ITextColor BackgroundColor => _backgroundColor ?? new AnsiTextColor(AnsiColor.Default);

    /// <summary>
    /// Sets the background color
    /// </summary>
    /// <param name="background">The background color to set</param>
    /// <returns>This TextGraphicsWriter for method chaining</returns>
    public TextGraphicsWriter SetBackgroundColor(ITextColor background)
    {
        _backgroundColor = background;
        return this;
    }

    /// <summary>
    /// Adds zero or more modifiers to the set of currently active modifiers
    /// </summary>
    public TextGraphicsWriter EnableModifiers(params SGR[] modifiers)
    {
        foreach (var modifier in modifiers)
        {
            _style.Add(modifier);
        }
        return this;
    }

    /// <summary>
    /// Removes zero or more modifiers from the set of currently active modifiers
    /// </summary>
    public TextGraphicsWriter DisableModifiers(params SGR[] modifiers)
    {
        foreach (var modifier in modifiers)
        {
            _style.Remove(modifier);
        }
        return this;
    }

    /// <summary>
    /// Sets the active modifiers to exactly the set passed in to this method
    /// </summary>
    public TextGraphicsWriter SetModifiers(ISet<SGR> modifiers)
    {
        _style.Clear();
        foreach (var modifier in modifiers)
        {
            _style.Add(modifier);
        }
        return this;
    }

    /// <summary>
    /// Removes all active modifiers
    /// </summary>
    public TextGraphicsWriter ClearModifiers()
    {
        _style.Clear();
        return this;
    }

    /// <summary>
    /// Returns all the SGR codes that are currently active
    /// </summary>
    public ISet<SGR> ActiveModifiers => new HashSet<SGR>(_style);

    /// <summary>
    /// Copy colors and set of SGR codes from another style set
    /// </summary>
    public TextGraphicsWriter SetStyleFrom(IStyleSet source)
    {
        SetBackgroundColor(source.BackgroundColor);
        SetForegroundColor(source.ForegroundColor);
        SetModifiers(source.ActiveModifiers);
        return this;
    }

    /// <summary>
    /// Gets or sets the wrap behavior for text rendering
    /// </summary>
    public WrapBehaviour WrapBehaviour
    {
        get => _wrapBehaviour;
        set => _wrapBehaviour = value;
    }

    /// <summary>
    /// Gets or sets whether styles in strings are handled
    /// </summary>
    public bool IsStyleable()
    {
        return _styleable;
    }

    /// <summary>
    /// Sets whether styles in strings should be handled
    /// </summary>
    public void SetStyleable(bool styleable)
    {
        _styleable = styleable;
    }

    /// <summary>
    /// Translates a position into screen coordinates. If null is specified for position,
    /// the cursor location is translated to screen coordinates.
    /// </summary>
    /// <param name="pos">position to translate, pass null for cursor position</param>
    /// <returns>screen (absolute) position</returns>
    public TerminalPosition? ToScreenPosition(TerminalPosition? pos)
    {
        return _backend.ToScreenPosition(pos ?? CursorPosition);
    }

    private void SetBackendStyleFrom(IStyleSet styleSet)
    {
        _backend.SetForegroundColor(styleSet.ForegroundColor);
        _backend.SetBackgroundColor(styleSet.BackgroundColor);
        _backend.SetModifiers(styleSet.ActiveModifiers);
    }

    private void SetBackendStyleFromThis()
    {
        _backend.SetForegroundColor(ForegroundColor);
        _backend.SetBackgroundColor(BackgroundColor);
        _backend.SetModifiers(ActiveModifiers);
    }
}