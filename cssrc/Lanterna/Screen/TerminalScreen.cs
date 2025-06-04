using System;
using System.Collections.Generic;
using Lanterna.Graphics;
using Lanterna.Input;
using Lanterna.Terminal;

namespace Lanterna.Screen
{
    public class TerminalScreen : AbstractScreen
    {
        private readonly Terminal _terminal;
        private bool _isStarted;
        private bool _fullRedrawHint;
        private ScrollHint? _scrollHint;

        public TerminalScreen(Terminal terminal) : this(terminal, ScreenConstants.DEFAULT_CHARACTER)
        {
        }

        public TerminalScreen(Terminal terminal, TextCharacter defaultCharacter) : base(terminal.GetTerminalSize(), defaultCharacter)
        {
            _terminal = terminal;
            _terminal.AddResizeListener(new TerminalScreenResizeListener(this));
            _isStarted = false;
            _fullRedrawHint = true;
        }

        public override void Dispose()
        {
            base.Dispose();
            _terminal.Dispose();
        }

        public override void StartScreen()
        {
            if (_isStarted) return;
            _isStarted = true;
            _terminal.EnterPrivateMode();
            _terminal.GetTerminalSize();
            _terminal.ClearScreen();
            _fullRedrawHint = true;
            var cursor = GetCursorPosition();
            if (cursor != null)
            {
                _terminal.SetCursorVisible(true);
                _terminal.SetCursorPosition(cursor.Value.Column, cursor.Value.Row);
            }
            else
            {
                _terminal.SetCursorVisible(false);
            }
        }

        public override void StopScreen()
        {
            StopScreen(true);
        }

        public void StopScreen(bool flushInput)
        {
            if (!_isStarted) return;
            if (flushInput)
            {
                KeyStroke key;
                do
                {
                    key = PollInput();
                } while (key != null && key.KeyType != KeyType.Eof);
            }
            _terminal.ExitPrivateMode();
            _isStarted = false;
        }

        public override void Refresh(Screen.RefreshType refreshType)
        {
            if (!_isStarted) return;
            if ((refreshType == Screen.RefreshType.Automatic && _fullRedrawHint) || refreshType == Screen.RefreshType.Complete)
            {
                RefreshFull();
                _fullRedrawHint = false;
            }
            else if (refreshType == Screen.RefreshType.Automatic && (_scrollHint == null || _scrollHint == ScrollHint.Invalid))
            {
                double threshold = GetTerminalSize().Rows * GetTerminalSize().Columns * 0.75;
                if (GetBackBuffer().IsVeryDifferent(GetFrontBuffer(), (int)threshold))
                    RefreshFull();
                else
                    RefreshByDelta();
            }
            else
            {
                RefreshByDelta();
            }
            GetBackBuffer().CopyTo(GetFrontBuffer());
            var cursor = GetCursorPosition();
            if (cursor != null)
            {
                _terminal.SetCursorVisible(true);
                if (cursor.Value.Column > 0 && GetFrontBuffer().GetCharacterAt(cursor.Value.WithColumn(cursor.Value.Column - 1)).IsDoubleWidth())
                    _terminal.SetCursorPosition(cursor.Value.Column - 1, cursor.Value.Row);
                else
                    _terminal.SetCursorPosition(cursor.Value.Column, cursor.Value.Row);
            }
            else
            {
                _terminal.SetCursorVisible(false);
            }
            _terminal.Flush();
        }

        private void UseScrollHint()
        {
            if (_scrollHint == null) return;
            try
            {
                if (_scrollHint == ScrollHint.Invalid) return;
                if (_terminal is Scrollable scrollable)
                {
                    _scrollHint.ApplyTo(scrollable);
                    _scrollHint.ApplyTo(GetFrontBuffer());
                }
            }
            catch (NotSupportedException) { }
            finally { _scrollHint = null; }
        }

        private void RefreshByDelta()
        {
            var updateMap = new SortedDictionary<TerminalPosition, TextCharacter>(new ScreenPointComparator());
            TerminalSize terminalSize = GetTerminalSize();

            UseScrollHint();

            for (int y = 0; y < terminalSize.Rows; y++)
            {
                for (int x = 0; x < terminalSize.Columns; x++)
                {
                    TextCharacter backBufferCharacter = GetBackBuffer().GetCharacterAt(x, y);
                    TextCharacter frontBufferCharacter = GetFrontBuffer().GetCharacterAt(x, y);
                    if (!backBufferCharacter.Equals(frontBufferCharacter))
                        updateMap[new TerminalPosition(x, y)] = backBufferCharacter;
                    if (backBufferCharacter.IsDoubleWidth())
                    {
                        x++;
                    }
                    else if (frontBufferCharacter.IsDoubleWidth())
                    {
                        if (x + 1 < terminalSize.Columns)
                            updateMap[new TerminalPosition(x + 1, y)] = frontBufferCharacter.WithCharacter(' ');
                    }
                }
            }

            if (updateMap.Count == 0) return;
            var enumerator = updateMap.Keys.GetEnumerator();
            enumerator.MoveNext();
            TerminalPosition currentPosition = enumerator.Current;
            _terminal.SetCursorPosition(currentPosition.Column, currentPosition.Row);
            TextCharacter first = updateMap[currentPosition];
            var currentSGR = new HashSet<SGR>(first.Modifiers);
            _terminal.ResetColorAndSGR();
            foreach (var s in currentSGR) _terminal.EnableSGR(s);
            var currentForegroundColor = first.ForegroundColor;
            var currentBackgroundColor = first.BackgroundColor;
            _terminal.SetForegroundColor(currentForegroundColor);
            _terminal.SetBackgroundColor(currentBackgroundColor);
            foreach (var kv in updateMap)
            {
                var position = kv.Key;
                var newCharacter = kv.Value;
                if (!position.Equals(currentPosition))
                {
                    _terminal.SetCursorPosition(position.Column, position.Row);
                    currentPosition = position;
                }
                if (!currentForegroundColor.Equals(newCharacter.ForegroundColor))
                {
                    _terminal.SetForegroundColor(newCharacter.ForegroundColor);
                    currentForegroundColor = newCharacter.ForegroundColor;
                }
                if (!currentBackgroundColor.Equals(newCharacter.BackgroundColor))
                {
                    _terminal.SetBackgroundColor(newCharacter.BackgroundColor);
                    currentBackgroundColor = newCharacter.BackgroundColor;
                }
                foreach (SGR sgr in Enum.GetValues(typeof(SGR)))
                {
                    if (currentSGR.Contains(sgr) && !newCharacter.Modifiers.Contains(sgr))
                    {
                        _terminal.DisableSGR(sgr);
                        currentSGR.Remove(sgr);
                    }
                    else if (!currentSGR.Contains(sgr) && newCharacter.Modifiers.Contains(sgr))
                    {
                        _terminal.EnableSGR(sgr);
                        currentSGR.Add(sgr);
                    }
                }
                _terminal.PutString(newCharacter.CharacterString);
                currentPosition = newCharacter.IsDoubleWidth() ? currentPosition.WithColumn(currentPosition.Column + 2) : currentPosition.WithColumn(currentPosition.Column + 1);
            }
        }

        private void RefreshFull()
        {
            _terminal.SetForegroundColor(TextColor.ANSI.Default);
            _terminal.SetBackgroundColor(TextColor.ANSI.Default);
            _terminal.ClearScreen();
            _terminal.ResetColorAndSGR();
            _scrollHint = null;
            var currentSGR = new HashSet<SGR>();
            var currentForegroundColor = TextColor.ANSI.Default;
            var currentBackgroundColor = TextColor.ANSI.Default;
            for (int y = 0; y < GetTerminalSize().Rows; y++)
            {
                _terminal.SetCursorPosition(0, y);
                int currentColumn = 0;
                for (int x = 0; x < GetTerminalSize().Columns; x++)
                {
                    TextCharacter newCharacter = GetBackBuffer().GetCharacterAt(x, y);
                    if (newCharacter.Equals(ScreenConstants.DEFAULT_CHARACTER))
                        continue;
                    if (!currentForegroundColor.Equals(newCharacter.ForegroundColor))
                    {
                        _terminal.SetForegroundColor(newCharacter.ForegroundColor);
                        currentForegroundColor = newCharacter.ForegroundColor;
                    }
                    if (!currentBackgroundColor.Equals(newCharacter.BackgroundColor))
                    {
                        _terminal.SetBackgroundColor(newCharacter.BackgroundColor);
                        currentBackgroundColor = newCharacter.BackgroundColor;
                    }
                    foreach (SGR sgr in Enum.GetValues(typeof(SGR)))
                    {
                        if (currentSGR.Contains(sgr) && !newCharacter.Modifiers.Contains(sgr))
                        {
                            _terminal.DisableSGR(sgr);
                            currentSGR.Remove(sgr);
                        }
                        else if (!currentSGR.Contains(sgr) && newCharacter.Modifiers.Contains(sgr))
                        {
                            _terminal.EnableSGR(sgr);
                            currentSGR.Add(sgr);
                        }
                    }
                    if (currentColumn != x)
                    {
                        _terminal.SetCursorPosition(x, y);
                        currentColumn = x;
                    }
                    _terminal.PutString(newCharacter.CharacterString);
                    if (newCharacter.IsDoubleWidth())
                    {
                        currentColumn += 2;
                        x++;
                    }
                    else
                    {
                        currentColumn += 1;
                    }
                }
            }
        }

        public Terminal GetTerminal() => _terminal;

        public override KeyStroke ReadInput() => _terminal.ReadInput();
        public override KeyStroke PollInput() => _terminal.PollInput();

        public override void Clear()
        {
            base.Clear();
            _fullRedrawHint = true;
            _scrollHint = ScrollHint.Invalid;
        }

        public override TerminalSize? DoResizeIfNecessary()
        {
            var newSize = base.DoResizeIfNecessary();
            if (newSize != null)
                _fullRedrawHint = true;
            return newSize;
        }

        public override void ScrollLines(int firstLine, int lastLine, int distance)
        {
            if (distance == 0 || firstLine > lastLine) return;
            base.ScrollLines(firstLine, lastLine, distance);
            var newHint = new ScrollHint(firstLine, lastLine, distance);
            if (_scrollHint == null)
                _scrollHint = newHint;
            else if (_scrollHint == ScrollHint.Invalid)
            {
            }
            else if (_scrollHint.Matches(newHint))
            {
                _scrollHint.Distance += newHint.Distance;
            }
            else
            {
                _scrollHint = ScrollHint.Invalid;
            }
        }

        private class TerminalScreenResizeListener : TerminalResizeListener
        {
            private readonly TerminalScreen _parent;
            public TerminalScreenResizeListener(TerminalScreen parent) => _parent = parent;
            public void OnResized(Terminal terminal, TerminalSize newSize)
            {
                _parent.AddResizeRequest(newSize);
            }
        }

        private class ScreenPointComparator : IComparer<TerminalPosition>
        {
            public int Compare(TerminalPosition o1, TerminalPosition o2)
            {
                if (o1.Row == o2.Row)
                {
                    if (o1.Column == o2.Column)
                        return 0;
                    return o1.Column.CompareTo(o2.Column);
                }
                return o1.Row.CompareTo(o2.Row);
            }
        }

        private class ScrollHint
        {
            public static readonly ScrollHint Invalid = new ScrollHint(-1, -1, 0);
            public readonly int FirstLine;
            public readonly int LastLine;
            public int Distance;
            public ScrollHint(int firstLine, int lastLine, int distance)
            {
                FirstLine = firstLine;
                LastLine = lastLine;
                Distance = distance;
            }
            public bool Matches(ScrollHint other) => FirstLine == other.FirstLine && LastLine == other.LastLine;
            public void ApplyTo(Scrollable scr) => scr.ScrollLines(FirstLine, LastLine, Distance);
        }
    }
}
