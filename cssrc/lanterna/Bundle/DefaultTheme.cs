using Lanterna.Graphics;

namespace Lanterna.Bundle;

/// <summary>
/// Hard-coded fallback theme extending PropertyTheme.
/// Contains complete theme definition as embedded string literal.
/// Used when theme property files can't be loaded from resources.
/// </summary>
public class DefaultTheme : PropertyTheme
{
    private const string DefaultThemeProperties = @"# This is the default properties
# If you want to modify this theme, you must do so both here and in DefaultTheme.cs

foreground = black
background = white
sgr        =
foreground[SELECTED] = white
background[SELECTED] = blue
sgr[SELECTED]        = bold
foreground[PRELIGHT] = white
background[PRELIGHT] = blue
sgr[PRELIGHT]        = bold
foreground[ACTIVE]   = white
background[ACTIVE]   = blue
sgr[ACTIVE]          = bold
foreground[INSENSITIVE] = white
background[INSENSITIVE] = blue
sgr[INSENSITIVE]     =

# By default use the shadow post-renderer
postrenderer = Lanterna.Gui2.WindowShadowRenderer

#Borders
Lanterna.Gui2.AbstractBorder.background[PRELIGHT] = white
Lanterna.Gui2.AbstractBorder.foreground[ACTIVE] = black
Lanterna.Gui2.AbstractBorder.background[ACTIVE] = white
Lanterna.Gui2.AbstractBorder.sgr[ACTIVE] =
Lanterna.Gui2.AbstractBorder.foreground[INSENSITIVE] = black
Lanterna.Gui2.AbstractBorder.background[INSENSITIVE] = white
Lanterna.Gui2.Borders.SingleLine.char[HORIZONTAL_LINE] = \u2500
Lanterna.Gui2.Borders.SingleLine.char[VERTICAL_LINE] = \u2502
Lanterna.Gui2.Borders.SingleLine.char[BOTTOM_LEFT_CORNER] = \u2514
Lanterna.Gui2.Borders.SingleLine.char[TOP_LEFT_CORNER] = \u250c
Lanterna.Gui2.Borders.SingleLine.char[BOTTOM_RIGHT_CORNER] = \u2518
Lanterna.Gui2.Borders.SingleLine.char[TOP_RIGHT_CORNER] = \u2510
Lanterna.Gui2.Borders.SingleLine.char[TITLE_LEFT] = \u2500
Lanterna.Gui2.Borders.SingleLine.char[TITLE_RIGHT] = \u2500
Lanterna.Gui2.Borders.DoubleLine.char[HORIZONTAL_LINE] = \u2550
Lanterna.Gui2.Borders.DoubleLine.char[VERTICAL_LINE] = \u2551
Lanterna.Gui2.Borders.DoubleLine.char[BOTTOM_LEFT_CORNER] = \u255a
Lanterna.Gui2.Borders.DoubleLine.char[TOP_LEFT_CORNER] = \u2554
Lanterna.Gui2.Borders.DoubleLine.char[BOTTOM_RIGHT_CORNER] = \u255d
Lanterna.Gui2.Borders.DoubleLine.char[TOP_RIGHT_CORNER] = \u2557
Lanterna.Gui2.Borders.DoubleLine.char[TITLE_LEFT] = \u2550
Lanterna.Gui2.Borders.DoubleLine.char[TITLE_RIGHT] = \u2550

#Button
Lanterna.Gui2.Button.renderer = Lanterna.Gui2.Button.DefaultButtonRenderer
Lanterna.Gui2.Button.sgr = bold
Lanterna.Gui2.Button.foreground[SELECTED] = yellow
Lanterna.Gui2.Button.foreground[PRELIGHT] = red
Lanterna.Gui2.Button.background[PRELIGHT] = white
Lanterna.Gui2.Button.sgr[PRELIGHT] =
Lanterna.Gui2.Button.foreground[INSENSITIVE] = black
Lanterna.Gui2.Button.background[INSENSITIVE] = white
Lanterna.Gui2.Button.char[LEFT_BORDER] = <
Lanterna.Gui2.Button.char[RIGHT_BORDER] = >

# CheckBox
Lanterna.Gui2.CheckBox.foreground[INSENSITIVE] = black
Lanterna.Gui2.CheckBox.background[INSENSITIVE] = white
Lanterna.Gui2.CheckBox.char[MARKER] = x

# CheckBoxList
Lanterna.Gui2.CheckBoxList.foreground[SELECTED] = black
Lanterna.Gui2.CheckBoxList.background[SELECTED] = white
Lanterna.Gui2.CheckBoxList.sgr[SELECTED] =
Lanterna.Gui2.CheckBoxList.char[LEFT_BRACKET] = [
Lanterna.Gui2.CheckBoxList.char[RIGHT_BRACKET] = ]
Lanterna.Gui2.CheckBoxList.char[MARKER] = x

# ComboBox
Lanterna.Gui2.ComboBox.sgr[PRELIGHT] =
Lanterna.Gui2.ComboBox.foreground[INSENSITIVE] = black
Lanterna.Gui2.ComboBox.background[INSENSITIVE] = white
Lanterna.Gui2.ComboBox.foreground[SELECTED] = black
Lanterna.Gui2.ComboBox.background[SELECTED] = white

# Default color and style for the window decoration renderer
Lanterna.Gui2.DefaultWindowDecorationRenderer.foreground[ACTIVE] = black
Lanterna.Gui2.DefaultWindowDecorationRenderer.background[ACTIVE] = white
Lanterna.Gui2.DefaultWindowDecorationRenderer.sgr[ACTIVE] =
Lanterna.Gui2.DefaultWindowDecorationRenderer.foreground[INSENSITIVE] = black
Lanterna.Gui2.DefaultWindowDecorationRenderer.background[INSENSITIVE] = white
Lanterna.Gui2.DefaultWindowDecorationRenderer.background[PRELIGHT] = white
Lanterna.Gui2.DefaultWindowDecorationRenderer.char[HORIZONTAL_LINE] = \u2500
Lanterna.Gui2.DefaultWindowDecorationRenderer.char[VERTICAL_LINE] = \u2502
Lanterna.Gui2.DefaultWindowDecorationRenderer.char[BOTTOM_LEFT_CORNER] = \u2514
Lanterna.Gui2.DefaultWindowDecorationRenderer.char[TOP_LEFT_CORNER] = \u250c
Lanterna.Gui2.DefaultWindowDecorationRenderer.char[BOTTOM_RIGHT_CORNER] = \u2518
Lanterna.Gui2.DefaultWindowDecorationRenderer.char[TOP_RIGHT_CORNER] = \u2510
Lanterna.Gui2.DefaultWindowDecorationRenderer.char[TITLE_SEPARATOR_LEFT] = \u2500
Lanterna.Gui2.DefaultWindowDecorationRenderer.char[TITLE_SEPARATOR_RIGHT] = \u2500
Lanterna.Gui2.DefaultWindowDecorationRenderer.property[TITLE_PADDING] = false
Lanterna.Gui2.DefaultWindowDecorationRenderer.property[CENTER_TITLE] = false

# GUI Backdrop
Lanterna.Gui2.GUIBackdrop.foreground = cyan
Lanterna.Gui2.GUIBackdrop.background = blue
Lanterna.Gui2.GUIBackdrop.sgr = bold

# List boxes default
Lanterna.Gui2.AbstractListBox.foreground[INSENSITIVE] = black
Lanterna.Gui2.AbstractListBox.background[INSENSITIVE] = white

# Menu
Lanterna.Gui2.Menu.MenuItem.foreground[PRELIGHT] = red
Lanterna.Gui2.Menu.MenuItem.background[PRELIGHT] = white
Lanterna.Gui2.Menu.MenuItem.sgr[PRELIGHT] =
Lanterna.Gui2.Menu.MenuItem.foreground[ACTIVE] = red
Lanterna.Gui2.Menu.MenuItem.background[ACTIVE] = green
Lanterna.Gui2.Menu.MenuItem.sgr[ACTIVE] =
Lanterna.Gui2.Menu.MenuItem.foreground[SELECTED] = black
Lanterna.Gui2.Menu.MenuItem.background[SELECTED] = green
Lanterna.Gui2.Menu.MenuItem.sgr[SELECTED] =

# ProgressBar
Lanterna.Gui2.ProgressBar.foreground = white
Lanterna.Gui2.ProgressBar.background = blue
Lanterna.Gui2.ProgressBar.sgr = bold
Lanterna.Gui2.ProgressBar.background[ACTIVE] = red
Lanterna.Gui2.ProgressBar.foreground[PRELIGHT] = red
Lanterna.Gui2.ProgressBar.sgr[PRELIGHT] =
Lanterna.Gui2.ProgressBar.char[FILLER] = 

# RadioBoxList
Lanterna.Gui2.RadioBoxList.foreground[SELECTED] = black
Lanterna.Gui2.RadioBoxList.background[SELECTED] = white
Lanterna.Gui2.RadioBoxList.sgr[SELECTED] =
Lanterna.Gui2.RadioBoxList.char[LEFT_BRACKET] = <
Lanterna.Gui2.RadioBoxList.char[RIGHT_BRACKET] = >
Lanterna.Gui2.RadioBoxList.char[MARKER] = o

# ScrollBar
Lanterna.Gui2.ScrollBar.char[UP_ARROW]=\u25b2
Lanterna.Gui2.ScrollBar.char[DOWN_ARROW]=\u25bc
Lanterna.Gui2.ScrollBar.char[LEFT_ARROW]=\u25c4
Lanterna.Gui2.ScrollBar.char[RIGHT_ARROW]=\u25ba

Lanterna.Gui2.ScrollBar.char[VERTICAL_BACKGROUND]=\u2592
Lanterna.Gui2.ScrollBar.char[VERTICAL_SMALL_TRACKER]=\u2588
Lanterna.Gui2.ScrollBar.char[VERTICAL_TRACKER_BACKGROUND]=\u2588
Lanterna.Gui2.ScrollBar.char[VERTICAL_TRACKER_TOP]=\u2588
Lanterna.Gui2.ScrollBar.char[VERTICAL_TRACKER_BOTTOM]=\u2588

Lanterna.Gui2.ScrollBar.char[HORIZONTAL_BACKGROUND]=\u2592
Lanterna.Gui2.ScrollBar.char[HORIZONTAL_SMALL_TRACKER]=\u2588
Lanterna.Gui2.ScrollBar.char[HORIZONTAL_TRACKER_BACKGROUND]=\u2588
Lanterna.Gui2.ScrollBar.char[HORIZONTAL_TRACKER_LEFT]=\u2588
Lanterna.Gui2.ScrollBar.char[HORIZONTAL_TRACKER_RIGHT]=\u2588

# Separator
Lanterna.Gui2.Separator.sgr = bold

# Table
Lanterna.Gui2.Table.Table.sgr[HEADER] = underline,bold
Lanterna.Gui2.Table.Table.foreground[SELECTED] = black
Lanterna.Gui2.Table.Table.background[SELECTED] = white
Lanterna.Gui2.Table.Table.sgr[SELECTED] =

# TextBox
Lanterna.Gui2.TextBox.foreground = white
Lanterna.Gui2.TextBox.background = blue

# Window shadow
Lanterna.Gui2.WindowShadowRenderer.background = black
Lanterna.Gui2.WindowShadowRenderer.sgr = bold
Lanterna.Gui2.WindowShadowRenderer.property[DOUBLE_WIDTH] = true
Lanterna.Gui2.WindowShadowRenderer.property[TRANSPARENT] = true";

    /// <summary>
    /// Creates a new DefaultTheme with hard-coded theme properties
    /// </summary>
    public DefaultTheme() : base(DefaultThemeProperties, true)
    {
        // Use ignoreUnknownClasses = true since some class names might not be found
    }
}