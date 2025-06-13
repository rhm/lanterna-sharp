using Lanterna.Core;
using Lanterna.Graphics;

namespace Lanterna.Gui2;

/// <summary>
/// Classic scrollbar that can be used to display where inside a larger component a view is showing. This implementation
/// is not interactable and needs to be driven externally, meaning you can't focus on the scrollbar itself, you have to
/// update its state as part of another component being modified. ScrollBars are either horizontal or vertical,
/// which affects the way they appear and how they are drawn.
/// </summary>
public class ScrollBar : AbstractComponent<ScrollBar>
{
    private readonly Direction _direction;
    private int _maximum;
    private int _position;
    private int _viewSize;
    private readonly object _lock = new object();

    /// <summary>
    /// Creates a new ScrollBar with a specified direction
    /// </summary>
    /// <param name="direction">Direction of the scrollbar</param>
    public ScrollBar(Direction direction)
    {
        _direction = direction;
        _maximum = 100;
        _position = 0;
        _viewSize = 0;
    }

    /// <summary>
    /// Returns the direction of this ScrollBar
    /// </summary>
    public Direction Direction => _direction;

    /// <summary>
    /// Sets the maximum value the scrollbar's position (minus the view size) can have
    /// </summary>
    /// <param name="maximum">Maximum value</param>
    /// <returns>Itself</returns>
    public ScrollBar SetScrollMaximum(int maximum)
    {
        if (maximum < 0)
        {
            throw new ArgumentException($"Cannot set ScrollBar maximum to {maximum}");
        }
        
        lock (_lock)
        {
            if (_maximum != maximum)
            {
                _maximum = maximum;
                Invalidate();
            }
            return this;
        }
    }

    /// <summary>
    /// Returns the maximum scroll value
    /// </summary>
    public int ScrollMaximum => _maximum;

    /// <summary>
    /// Sets the scrollbar's position, should be a value between 0 and maximum - view size
    /// </summary>
    /// <param name="position">Scrollbar's tracker's position</param>
    /// <returns>Itself</returns>
    public ScrollBar SetScrollPosition(int position)
    {
        lock (_lock)
        {
            int newPosition = Math.Min(position, _maximum);
            if (_position != newPosition)
            {
                _position = newPosition;
                Invalidate();
            }
            return this;
        }
    }

    /// <summary>
    /// Returns the position of the ScrollBar's tracker
    /// </summary>
    public int ScrollPosition => _position;

    /// <summary>
    /// Sets the view size of the scrollbar, determining how big the scrollbar's tracker should be and also affecting the
    /// maximum value of tracker's position
    /// </summary>
    /// <param name="viewSize">View size of the scrollbar</param>
    /// <returns>Itself</returns>
    public ScrollBar SetViewSize(int viewSize)
    {
        lock (_lock)
        {
            _viewSize = viewSize;
            return this;
        }
    }

    /// <summary>
    /// Returns the view size of the scrollbar
    /// </summary>
    public int ViewSize
    {
        get
        {
            if (_viewSize > 0)
            {
                return _viewSize;
            }
            if (_direction == Direction.Horizontal)
            {
                return Size.Columns;
            }
            else
            {
                return Size.Rows;
            }
        }
    }

    protected override IComponentRenderer<ScrollBar> CreateDefaultRenderer()
    {
        return new DefaultScrollBarRenderer();
    }

    /// <summary>
    /// Helper class for making new ScrollBar renderers a little bit cleaner
    /// </summary>
    public abstract class ScrollBarRenderer : IComponentRenderer<ScrollBar>
    {
        public virtual TerminalSize GetPreferredSize(ScrollBar component)
        {
            return TerminalSize.One;
        }

        public abstract void DrawComponent(ITextGUIGraphics graphics, ScrollBar component);
    }

    /// <summary>
    /// Default renderer for ScrollBar which will be used unless overridden. This will draw a scrollbar using
    /// arrows at each extreme end, a background color for spaces between those arrows and the tracker and then the
    /// tracker itself in three different styles depending on the size of the tracker. All characters and colors are
    /// customizable through whatever theme is currently in use.
    /// </summary>
    public class DefaultScrollBarRenderer : ScrollBarRenderer
    {
        private bool _growScrollTracker;

        /// <summary>
        /// Default constructor
        /// </summary>
        public DefaultScrollBarRenderer()
        {
            _growScrollTracker = true;
        }

        /// <summary>
        /// Should tracker automatically grow in size along with the ScrollBar (default: true)
        /// </summary>
        /// <param name="growScrollTracker">Automatically grow tracker</param>
        public void SetGrowScrollTracker(bool growScrollTracker)
        {
            _growScrollTracker = growScrollTracker;
        }

        public override void DrawComponent(ITextGUIGraphics graphics, ScrollBar component)
        {
            var size = graphics.Size;
            var direction = component.Direction;
            int position = component.ScrollPosition;
            int maximum = component.ScrollMaximum;
            int viewSize = component.ViewSize;

            if (size.Rows == 0 || size.Columns == 0)
            {
                return;
            }

            // Adjust position if necessary
            if (position + viewSize >= maximum)
            {
                position = Math.Max(0, maximum - viewSize);
                component.SetScrollPosition(position);
            }

            var themeDefinition = component.ThemeDefinition;
            graphics.ApplyThemeStyle(themeDefinition.Normal);

            if (direction == Direction.Vertical)
            {
                if (size.Rows == 1)
                {
                    graphics.SetCharacter(0, 0, themeDefinition.GetCharacter("VERTICAL_BACKGROUND", Symbols.Block_Middle));
                }
                else if (size.Rows == 2)
                {
                    graphics.SetCharacter(0, 0, themeDefinition.GetCharacter("UP_ARROW", Symbols.Triangle_Up_Pointing_Black));
                    graphics.SetCharacter(0, 1, themeDefinition.GetCharacter("DOWN_ARROW", Symbols.Triangle_Down_Pointing_Black));
                }
                else
                {
                    int scrollableArea = size.Rows - 2;
                    int scrollTrackerSize = 1;
                    if (_growScrollTracker)
                    {
                        float ratio = ClampRatio((float)viewSize / (float)maximum);
                        scrollTrackerSize = Math.Max(1, (int)(ratio * (float)scrollableArea));
                    }

                    float positionRatio = ClampRatio((float)position / (float)(maximum - viewSize));
                    int scrollTrackerPosition = (int)(positionRatio * (float)(scrollableArea - scrollTrackerSize)) + 1;

                    graphics.SetCharacter(0, 0, themeDefinition.GetCharacter("UP_ARROW", Symbols.Triangle_Up_Pointing_Black));
                    graphics.DrawLine(0, 1, 0, size.Rows - 2, themeDefinition.GetCharacter("VERTICAL_BACKGROUND", Symbols.Block_Middle));
                    graphics.SetCharacter(0, size.Rows - 1, themeDefinition.GetCharacter("DOWN_ARROW", Symbols.Triangle_Down_Pointing_Black));
                    
                    if (scrollTrackerSize == 1)
                    {
                        graphics.SetCharacter(0, scrollTrackerPosition, themeDefinition.GetCharacter("VERTICAL_SMALL_TRACKER", Symbols.Block_Solid));
                    }
                    else if (scrollTrackerSize == 2)
                    {
                        graphics.SetCharacter(0, scrollTrackerPosition, themeDefinition.GetCharacter("VERTICAL_TRACKER_TOP", Symbols.Block_Solid));
                        graphics.SetCharacter(0, scrollTrackerPosition + 1, themeDefinition.GetCharacter("VERTICAL_TRACKER_BOTTOM", Symbols.Block_Solid));
                    }
                    else
                    {
                        graphics.SetCharacter(0, scrollTrackerPosition, themeDefinition.GetCharacter("VERTICAL_TRACKER_TOP", Symbols.Block_Solid));
                        graphics.DrawLine(0, scrollTrackerPosition + 1, 0, scrollTrackerPosition + scrollTrackerSize - 2, themeDefinition.GetCharacter("VERTICAL_TRACKER_BACKGROUND", Symbols.Block_Solid));
                        graphics.SetCharacter(0, scrollTrackerPosition + (scrollTrackerSize / 2), themeDefinition.GetCharacter("VERTICAL_SMALL_TRACKER", Symbols.Block_Solid));
                        graphics.SetCharacter(0, scrollTrackerPosition + scrollTrackerSize - 1, themeDefinition.GetCharacter("VERTICAL_TRACKER_BOTTOM", Symbols.Block_Solid));
                    }
                }
            }
            else
            {
                if (size.Columns == 1)
                {
                    graphics.SetCharacter(0, 0, themeDefinition.GetCharacter("HORIZONTAL_BACKGROUND", Symbols.Block_Middle));
                }
                else if (size.Columns == 2)
                {
                    graphics.SetCharacter(0, 0, Symbols.Triangle_Left_Pointing_Black);
                    graphics.SetCharacter(1, 0, Symbols.Triangle_Right_Pointing_Black);
                }
                else
                {
                    int scrollableArea = size.Columns - 2;
                    int scrollTrackerSize = 1;
                    if (_growScrollTracker)
                    {
                        float ratio = ClampRatio((float)viewSize / (float)maximum);
                        scrollTrackerSize = Math.Max(1, (int)(ratio * (float)scrollableArea));
                    }

                    float positionRatio = ClampRatio((float)position / (float)(maximum - viewSize));
                    int scrollTrackerPosition = (int)(positionRatio * (float)(scrollableArea - scrollTrackerSize)) + 1;

                    graphics.SetCharacter(0, 0, themeDefinition.GetCharacter("LEFT_ARROW", Symbols.Triangle_Left_Pointing_Black));
                    graphics.DrawLine(1, 0, size.Columns - 2, 0, themeDefinition.GetCharacter("HORIZONTAL_BACKGROUND", Symbols.Block_Middle));
                    graphics.SetCharacter(size.Columns - 1, 0, themeDefinition.GetCharacter("RIGHT_ARROW", Symbols.Triangle_Right_Pointing_Black));
                    
                    if (scrollTrackerSize == 1)
                    {
                        graphics.SetCharacter(scrollTrackerPosition, 0, themeDefinition.GetCharacter("HORIZONTAL_SMALL_TRACKER", Symbols.Block_Solid));
                    }
                    else if (scrollTrackerSize == 2)
                    {
                        graphics.SetCharacter(scrollTrackerPosition, 0, themeDefinition.GetCharacter("HORIZONTAL_TRACKER_LEFT", Symbols.Block_Solid));
                        graphics.SetCharacter(scrollTrackerPosition + 1, 0, themeDefinition.GetCharacter("HORIZONTAL_TRACKER_RIGHT", Symbols.Block_Solid));
                    }
                    else
                    {
                        graphics.SetCharacter(scrollTrackerPosition, 0, themeDefinition.GetCharacter("HORIZONTAL_TRACKER_LEFT", Symbols.Block_Solid));
                        graphics.DrawLine(scrollTrackerPosition + 1, 0, scrollTrackerPosition + scrollTrackerSize - 2, 0, themeDefinition.GetCharacter("HORIZONTAL_TRACKER_BACKGROUND", Symbols.Block_Solid));
                        graphics.SetCharacter(scrollTrackerPosition + (scrollTrackerSize / 2), 0, themeDefinition.GetCharacter("HORIZONTAL_SMALL_TRACKER", Symbols.Block_Solid));
                        graphics.SetCharacter(scrollTrackerPosition + scrollTrackerSize - 1, 0, themeDefinition.GetCharacter("HORIZONTAL_TRACKER_RIGHT", Symbols.Block_Solid));
                    }
                }
            }
        }

        private static float ClampRatio(float value)
        {
            if (value < 0.0f)
            {
                return 0.0f;
            }
            else if (value > 1.0f)
            {
                return 1.0f;
            }
            else
            {
                return value;
            }
        }
    }
}