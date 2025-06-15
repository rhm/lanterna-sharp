using Lanterna.Core;
using Lanterna.Graphics;
using Lanterna.Input;

namespace Lanterna.Gui2;

/// <summary>
/// A resizable split panel that displays two components separated by a draggable splitter.
/// The panel can be oriented horizontally (left/right) or vertically (top/bottom).
/// </summary>
public class SplitPanel : Panel
{
    private readonly IComponent _compA;
    private readonly ImageComponent _thumb;
    private readonly IComponent _compB;

    private bool _isHorizontal;
    private double _ratio = 0.5;

    /// <summary>
    /// Creates a horizontal split panel with the specified left and right components
    /// </summary>
    /// <param name="left">Component for the left side</param>
    /// <param name="right">Component for the right side</param>
    /// <returns>A new horizontal SplitPanel</returns>
    public static SplitPanel OfHorizontal(IComponent left, IComponent right)
    {
        return new SplitPanel(left, right, true);
    }

    /// <summary>
    /// Creates a vertical split panel with the specified top and bottom components
    /// </summary>
    /// <param name="top">Component for the top</param>
    /// <param name="bottom">Component for the bottom</param>
    /// <returns>A new vertical SplitPanel</returns>
    public static SplitPanel OfVertical(IComponent top, IComponent bottom)
    {
        return new SplitPanel(top, bottom, false);
    }

    /// <summary>
    /// Creates a new SplitPanel with the specified components and orientation
    /// </summary>
    /// <param name="a">First component (left for horizontal, top for vertical)</param>
    /// <param name="b">Second component (right for horizontal, bottom for vertical)</param>
    /// <param name="isHorizontal">true for horizontal split, false for vertical split</param>
    protected SplitPanel(IComponent a, IComponent b, bool isHorizontal)
    {
        _compA = a ?? throw new ArgumentNullException(nameof(a));
        _compB = b ?? throw new ArgumentNullException(nameof(b));
        _isHorizontal = isHorizontal;
        _thumb = MakeThumb();
        SetLayoutManager(new SplitPanelLayoutManager(this));
        SetRatio(10, 10);

        AddComponent(a);
        AddComponent(_thumb);
        AddComponent(b);
    }

    private ImageComponent MakeThumb()
    {
        return new ThumbImageComponent(this);
    }

    /// <summary>
    /// Sets the ratio between the two components
    /// </summary>
    /// <param name="left">Size of the first component (left/top)</param>
    /// <param name="right">Size of the second component (right/bottom)</param>
    public void SetRatio(int left, int right)
    {
        if (left == 0 || right == 0)
        {
            _ratio = 0.5;
        }
        else
        {
            int total = Math.Abs(left) + Math.Abs(right);
            _ratio = (double)left / total;
        }
    }

    /// <summary>
    /// Controls whether the splitter thumb is visible
    /// </summary>
    /// <param name="visible">true to show the thumb, false to hide it</param>
    public void SetThumbVisible(bool visible)
    {
        _thumb.Visible = visible;

        if (visible)
        {
            SetPreferredSize(null);
        }
        else
        {
            _thumb.SetPreferredSize(new TerminalSize(1, 1));
        }
    }

    /// <summary>
    /// Custom ImageComponent that handles mouse input for resizing the split panel
    /// </summary>
    private class ThumbImageComponent : ImageComponent
    {
        private readonly SplitPanel _splitPanel;
        private TerminalSize _aSize;
        private TerminalSize _bSize;
        private TerminalSize _tSize;
        private TerminalPosition? _down = null;
        private TerminalPosition? _drag = null;

        public ThumbImageComponent(SplitPanel splitPanel)
        {
            _splitPanel = splitPanel;
        }

        protected override IInteractable.Result HandleKeyStroke(KeyStroke keyStroke)
        {
            if (keyStroke is MouseAction mouseAction)
            {
                return HandleMouseAction(mouseAction);
            }
            // TODO: Implement keyboard based resizing
            else
            {
                return base.HandleKeyStroke(keyStroke);
            }
        }

        private IInteractable.Result HandleMouseAction(MouseAction mouseAction)
        {
            if (mouseAction.IsMouseDown)
            {
                _aSize = _splitPanel._compA.Size;
                _bSize = _splitPanel._compB.Size;
                _tSize = _splitPanel._thumb.Size;
                _down = mouseAction.Position;
            }
            if (mouseAction.IsMouseDrag)
            {
                _drag = mouseAction.Position;

                // This is a hack, should not be needed if the pane drag
                // only on mouse down'd comp stuff was completely working
                if (_down == null)
                {
                    _down = _drag;
                }

                int delta = _splitPanel._isHorizontal ? 
                    (_drag.Value - _down.Value).Column : 
                    (_drag.Value - _down.Value).Row;
                
                if (_splitPanel._isHorizontal)
                {
                    int a = Math.Max(1, _tSize.Columns + _aSize.Columns + delta);
                    int b = Math.Max(1, _bSize.Columns - delta);
                    _splitPanel.SetRatio(a, b);
                }
                else
                {
                    int a = Math.Max(1, _tSize.Rows + _aSize.Rows + delta);
                    int b = Math.Max(1, _bSize.Rows - delta);
                    _splitPanel.SetRatio(a, b);
                }
            }
            if (mouseAction.IsMouseUp)
            {
                _down = null;
                _drag = null;
            }
            return IInteractable.Result.Handled;
        }
    }

    /// <summary>
    /// Custom layout manager for the split panel that arranges the two components and splitter
    /// </summary>
    private class SplitPanelLayoutManager : ILayoutManager
    {
        private readonly SplitPanel _splitPanel;
        private bool _hasChanged;

        public SplitPanelLayoutManager(SplitPanel splitPanel)
        {
            _splitPanel = splitPanel;
            _hasChanged = true;
        }

        public TerminalSize GetPreferredSize(IReadOnlyList<IComponent> components)
        {
            TerminalSize sizeA = _splitPanel._compA.PreferredSize;
            int aWidth = sizeA.Columns;
            int aHeight = sizeA.Rows;
            TerminalSize sizeB = _splitPanel._compB.PreferredSize;
            int bWidth = sizeB.Columns;
            int bHeight = sizeB.Rows;

            int tWidth = _splitPanel._thumb.PreferredSize.Columns;
            int tHeight = _splitPanel._thumb.PreferredSize.Rows;

            if (_splitPanel._isHorizontal)
            {
                return new TerminalSize(aWidth + tWidth + bWidth, Math.Max(aHeight, Math.Max(tHeight, bHeight)));
            }
            else
            {
                return new TerminalSize(Math.Max(aWidth, Math.Max(tWidth, bWidth)), aHeight + tHeight + bHeight);
            }
        }

        public void DoLayout(TerminalSize area, IReadOnlyList<IComponent> components)
        {
            TerminalSize size = _splitPanel.Size;

            // TODO: themed
            int length = _splitPanel._isHorizontal ? size.Rows : size.Columns;
            TerminalSize tsize = new TerminalSize(
                _splitPanel._isHorizontal ? 1 : length, 
                !_splitPanel._isHorizontal ? 1 : length);
            
            var textImage = new BasicTextImage(tsize);
            var theme = _splitPanel.Theme;
            var themeDefinition = theme.GetDefinition(_splitPanel.GetType());
            var themeStyle = themeDefinition.Normal;

            var thumbRenderer = TextCharacter.FromCharacter(
                _splitPanel._isHorizontal ? Symbols.SingleLineVertical : Symbols.SingleLineHorizontal,
                themeStyle.ForegroundColor,
                themeStyle.BackgroundColor);
            
            if (_splitPanel._thumb.IsFocused)
            {
                thumbRenderer = thumbRenderer.WithModifier(SGR.Bold);
            }

            textImage.SetAll(thumbRenderer);
            _splitPanel._thumb.SetTextImage(textImage);

            int tWidth = _splitPanel._thumb.PreferredSize.Columns;
            int tHeight = _splitPanel._thumb.PreferredSize.Rows;

            int w = size.Columns;
            int h = size.Rows;

            if (_splitPanel._isHorizontal)
            {
                w -= tWidth;
            }
            else
            {
                h -= tHeight;
            }

            TerminalSize compAPrevSize = _splitPanel._compA.Size;
            TerminalSize compBPrevSize = _splitPanel._compB.Size;
            TerminalSize thumbPrevSize = _splitPanel._thumb.Size;
            TerminalPosition compAPrevPos = _splitPanel._compA.Position;
            TerminalPosition compBPrevPos = _splitPanel._compB.Position;
            TerminalPosition thumbPrevPos = _splitPanel._thumb.Position;

            if (_splitPanel._isHorizontal)
            {
                int leftWidth = Math.Max(0, (int)(w * _splitPanel._ratio));
                int leftHeight = Math.Max(0, Math.Min(_splitPanel._compA.PreferredSize.Rows, h));

                int rightWidth = Math.Max(0, w - leftWidth);
                int rightHeight = Math.Max(0, Math.Min(_splitPanel._compB.PreferredSize.Rows, h));

                _splitPanel._compA.Size = new TerminalSize(leftWidth, leftHeight);
                _splitPanel._thumb.Size = _splitPanel._thumb.PreferredSize;
                _splitPanel._compB.Size = new TerminalSize(rightWidth, rightHeight);

                _splitPanel._compA.Position = new TerminalPosition(0, 0);
                _splitPanel._thumb.Position = new TerminalPosition(leftWidth, h / 2 - tHeight / 2);
                _splitPanel._compB.Position = new TerminalPosition(leftWidth + tWidth, 0);
            }
            else
            {
                int leftWidth = Math.Max(0, Math.Min(_splitPanel._compA.PreferredSize.Columns, w));
                int leftHeight = Math.Max(0, (int)(h * _splitPanel._ratio));

                int rightWidth = Math.Max(0, Math.Min(_splitPanel._compB.PreferredSize.Columns, w));
                int rightHeight = Math.Max(0, h - leftHeight);

                _splitPanel._compA.Size = new TerminalSize(leftWidth, leftHeight);
                _splitPanel._thumb.Size = _splitPanel._thumb.PreferredSize;
                _splitPanel._compB.Size = new TerminalSize(rightWidth, rightHeight);

                _splitPanel._compA.Position = new TerminalPosition(0, 0);
                _splitPanel._thumb.Position = new TerminalPosition(w / 2 - tWidth / 2, leftHeight);
                _splitPanel._compB.Position = new TerminalPosition(0, leftHeight + tHeight);
            }

            _hasChanged = !compAPrevPos.Equals(_splitPanel._compA.Position) ||
                         !compAPrevSize.Equals(_splitPanel._compA.Size) ||
                         !compBPrevPos.Equals(_splitPanel._compB.Position) ||
                         !compBPrevSize.Equals(_splitPanel._compB.Size) ||
                         !thumbPrevPos.Equals(_splitPanel._thumb.Position) ||
                         !thumbPrevSize.Equals(_splitPanel._thumb.Size);
        }

        public bool HasChanged => _hasChanged;
    }
}