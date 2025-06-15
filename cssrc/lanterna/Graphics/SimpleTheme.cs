using System;
using System.Collections.Generic;
using Lanterna.Core;
using Lanterna.Gui2;

namespace Lanterna.Graphics;

/// <summary>
/// Very basic implementation of ITheme that allows you to quickly define a theme in code. It is a very simple
/// implementation that doesn't implement any intelligent fallback based on class hierarchy or package names. If a
/// particular class has not been defined with an explicit override, it will get the default theme style definition.
/// </summary>
public class SimpleTheme : ITheme
{
    /// <summary>
    /// Helper method that will quickly setup a new theme with some sensible component overrides.
    /// </summary>
    /// <param name="activeIsBold">Should focused components also use bold SGR style?</param>
    /// <param name="baseForeground">The base foreground color of the theme</param>
    /// <param name="baseBackground">The base background color of the theme</param>
    /// <param name="editableForeground">Foreground color for editable components, or editable areas of components</param>
    /// <param name="editableBackground">Background color for editable components, or editable areas of components</param>
    /// <param name="selectedForeground">Foreground color for the selection marker when a component has multiple selection states</param>
    /// <param name="selectedBackground">Background color for the selection marker when a component has multiple selection states</param>
    /// <param name="guiBackground">Background color of the GUI, if this theme is assigned to the ITextGUI</param>
    /// <returns>Assembled SimpleTheme using the parameters from above</returns>
    public static SimpleTheme MakeTheme(
        bool activeIsBold,
        ITextColor baseForeground,
        ITextColor baseBackground,
        ITextColor editableForeground,
        ITextColor editableBackground,
        ITextColor selectedForeground,
        ITextColor selectedBackground,
        ITextColor guiBackground)
    {
        SGR[] activeStyle = activeIsBold ? new SGR[] { SGR.Bold } : Array.Empty<SGR>();

        SimpleTheme theme = new SimpleTheme(baseForeground, baseBackground);
        ((Definition)theme.GetDefaultDefinition()).SetSelected(baseBackground, baseForeground, activeStyle);
        ((Definition)theme.GetDefaultDefinition()).SetActive(selectedForeground, selectedBackground, activeStyle);

        // Add component-specific overrides
        theme.AddOverride(typeof(AbstractBorder), baseForeground, baseBackground)
            .SetSelected(baseForeground, baseBackground, activeStyle);
        theme.AddOverride(typeof(AbstractListBox<,>), baseForeground, baseBackground)
            .SetSelected(selectedForeground, selectedBackground, activeStyle);
        theme.AddOverride(typeof(Button), baseForeground, baseBackground)
            .SetActive(selectedForeground, selectedBackground, activeStyle)
            .SetSelected(selectedForeground, selectedBackground, activeStyle);
        theme.AddOverride(typeof(CheckBox), baseForeground, baseBackground)
            .SetActive(selectedForeground, selectedBackground, activeStyle)
            .SetPreLight(selectedForeground, selectedBackground, activeStyle)
            .SetSelected(selectedForeground, selectedBackground, activeStyle);
        theme.AddOverride(typeof(CheckBoxList<>), baseForeground, baseBackground)
            .SetActive(selectedForeground, selectedBackground, activeStyle);
        theme.AddOverride(typeof(ComboBox<>), baseForeground, baseBackground)
            .SetActive(editableForeground, editableBackground, activeStyle)
            .SetPreLight(editableForeground, editableBackground);
        theme.AddOverride(typeof(DefaultWindowDecorationRenderer), baseForeground, baseBackground)
            .SetActive(baseForeground, baseBackground, activeStyle);
        theme.AddOverride(typeof(GUIBackdrop), baseForeground, guiBackground);
        theme.AddOverride(typeof(RadioBoxList<>), baseForeground, baseBackground)
            .SetActive(selectedForeground, selectedBackground, activeStyle);
        theme.AddOverride(typeof(TextBox), editableForeground, editableBackground)
            .SetActive(editableForeground, editableBackground, activeStyle)
            .SetSelected(editableForeground, editableBackground, activeStyle);

        theme.SetWindowPostRenderer(new WindowShadowRenderer());

        return theme;
    }

    private readonly Definition defaultDefinition;
    private readonly Dictionary<Type, Definition> overrideDefinitions;
    private IWindowPostRenderer? windowPostRenderer;
    private IWindowDecorationRenderer? windowDecorationRenderer;

    /// <summary>
    /// Creates a new SimpleTheme object that uses the supplied constructor arguments as the default style
    /// </summary>
    /// <param name="foreground">Color to use as the foreground unless overridden</param>
    /// <param name="background">Color to use as the background unless overridden</param>
    /// <param name="styles">Extra SGR styles to apply unless overridden</param>
    public SimpleTheme(ITextColor foreground, ITextColor background, params SGR[] styles)
    {
        this.defaultDefinition = new Definition(new DefaultMutableThemeStyle(foreground, background, styles));
        this.overrideDefinitions = new Dictionary<Type, Definition>();
        this.windowPostRenderer = null;
        this.windowDecorationRenderer = null;
    }

    public IThemeDefinition GetDefaultDefinition()
    {
        return defaultDefinition;
    }

    public IThemeDefinition GetDefinition(Type clazz)
    {
        if (overrideDefinitions.TryGetValue(clazz, out Definition? definition))
        {
            return definition;
        }
        return GetDefaultDefinition();
    }

    /// <summary>
    /// Adds an override for a particular class, or overwrites a previously defined override.
    /// </summary>
    /// <param name="clazz">Class to override the theme for</param>
    /// <param name="foreground">Color to use as the foreground color for this override style</param>
    /// <param name="background">Color to use as the background color for this override style</param>
    /// <param name="styles">SGR styles to apply for this override</param>
    /// <returns>The newly created Definition that corresponds to this override.</returns>
    public Definition AddOverride(Type clazz, ITextColor foreground, ITextColor background, params SGR[] styles)
    {
        Definition definition = new Definition(new DefaultMutableThemeStyle(foreground, background, styles));
        overrideDefinitions[clazz] = definition;
        return definition;
    }

    public IWindowPostRenderer? GetWindowPostRenderer()
    {
        return windowPostRenderer;
    }

    /// <summary>
    /// Changes the IWindowPostRenderer this theme will return. If called with null, the theme returns no
    /// post renderer and the GUI system will use whatever is the default.
    /// </summary>
    /// <param name="windowPostRenderer">Post-renderer to use along with this theme, or null to remove</param>
    /// <returns>Itself</returns>
    public SimpleTheme SetWindowPostRenderer(IWindowPostRenderer? windowPostRenderer)
    {
        this.windowPostRenderer = windowPostRenderer;
        return this;
    }

    public IWindowDecorationRenderer? GetWindowDecorationRenderer()
    {
        return windowDecorationRenderer;
    }

    /// <summary>
    /// Changes the IWindowDecorationRenderer this theme will return. If called with null, the theme
    /// returns no decoration renderer and the GUI system will use whatever is the default.
    /// </summary>
    /// <param name="windowDecorationRenderer">Decoration renderer to use along with this theme, or null to remove</param>
    /// <returns>Itself</returns>
    public SimpleTheme SetWindowDecorationRenderer(IWindowDecorationRenderer? windowDecorationRenderer)
    {
        this.windowDecorationRenderer = windowDecorationRenderer;
        return this;
    }

    /// <summary>
    /// Interface for providing component renderers
    /// </summary>
    public interface IRendererProvider<T> where T : IComponent
    {
        IComponentRenderer<T> GetRenderer(Type type);
    }

    /// <summary>
    /// Internal class inside SimpleTheme used to allow basic editing of the default style and the optional
    /// overrides.
    /// </summary>
    public class Definition : IThemeDefinition
    {
        private readonly IThemeStyle normal;
        private IThemeStyle? preLight;
        private IThemeStyle? selected;
        private IThemeStyle? active;
        private IThemeStyle? insensitive;
        private readonly Dictionary<string, IThemeStyle> customStyles;
        private readonly Dictionary<string, string> properties;
        private readonly Dictionary<string, char> characterMap;
        private readonly Dictionary<Type, IRendererProvider<IComponent>> componentRendererMap;
        private bool cursorVisible;

        internal Definition(IThemeStyle normal)
        {
            this.normal = normal;
            this.preLight = null;
            this.selected = null;
            this.active = null;
            this.insensitive = null;
            this.customStyles = new Dictionary<string, IThemeStyle>();
            this.properties = new Dictionary<string, string>();
            this.characterMap = new Dictionary<string, char>();
            this.componentRendererMap = new Dictionary<Type, IRendererProvider<IComponent>>();
            this.cursorVisible = true;
        }

        public IThemeStyle Normal => normal;

        public IThemeStyle PreLight => preLight ?? normal;

        /// <summary>
        /// Sets the theme definition style "prelight"
        /// </summary>
        public Definition SetPreLight(ITextColor foreground, ITextColor background, params SGR[] styles)
        {
            this.preLight = new DefaultMutableThemeStyle(foreground, background, styles);
            return this;
        }

        public IThemeStyle Selected => selected ?? normal;

        /// <summary>
        /// Sets the theme definition style "selected"
        /// </summary>
        public Definition SetSelected(ITextColor foreground, ITextColor background, params SGR[] styles)
        {
            this.selected = new DefaultMutableThemeStyle(foreground, background, styles);
            return this;
        }

        public IThemeStyle Active => active ?? normal;

        /// <summary>
        /// Sets the theme definition style "active"
        /// </summary>
        public Definition SetActive(ITextColor foreground, ITextColor background, params SGR[] styles)
        {
            this.active = new DefaultMutableThemeStyle(foreground, background, styles);
            return this;
        }

        public IThemeStyle Insensitive => insensitive ?? normal;

        /// <summary>
        /// Sets the theme definition style "insensitive"
        /// </summary>
        public Definition SetInsensitive(ITextColor foreground, ITextColor background, params SGR[] styles)
        {
            this.insensitive = new DefaultMutableThemeStyle(foreground, background, styles);
            return this;
        }

        public IThemeStyle? GetCustom(string name)
        {
            return customStyles.GetValueOrDefault(name);
        }

        public IThemeStyle GetCustom(string name, IThemeStyle defaultValue)
        {
            return GetCustom(name) ?? defaultValue;
        }

        /// <summary>
        /// Adds a custom definition style to the theme using the supplied name. This will be returned using the matching
        /// call to GetCustom(string).
        /// </summary>
        public Definition SetCustom(string name, ITextColor foreground, ITextColor background, params SGR[] styles)
        {
            customStyles[name] = new DefaultMutableThemeStyle(foreground, background, styles);
            return this;
        }

        public bool GetBooleanProperty(string name, bool defaultValue)
        {
            if (properties.TryGetValue(name, out string? value))
            {
                return bool.Parse(value);
            }
            return defaultValue;
        }

        /// <summary>
        /// Attaches a boolean value property to this SimpleTheme that will be returned if calling
        /// GetBooleanProperty(string, bool) with the same name.
        /// </summary>
        public Definition SetBooleanProperty(string name, bool value)
        {
            properties[name] = value.ToString();
            return this;
        }

        public bool IsCursorVisible()
        {
            return cursorVisible;
        }

        /// <summary>
        /// Sets the value that suggests if the cursor should be visible or not (it's still up to the component renderer
        /// if it's going to honour this or not).
        /// </summary>
        public Definition SetCursorVisible(bool cursorVisible)
        {
            this.cursorVisible = cursorVisible;
            return this;
        }

        public char GetCharacter(string name, char fallback)
        {
            return characterMap.GetValueOrDefault(name, fallback);
        }

        /// <summary>
        /// Stores a character value in this definition under a specific name. This is used to customize the appearance
        /// of certain components. It is returned with call to GetCharacter(string, char) with the
        /// same name.
        /// </summary>
        public Definition SetCharacter(string name, char character)
        {
            characterMap[name] = character;
            return this;
        }

        public T? GetRenderer<T>() where T : class
        {
            if (componentRendererMap.TryGetValue(typeof(T), out IRendererProvider<IComponent>? rendererProvider))
            {
                return rendererProvider.GetRenderer(typeof(T)) as T;
            }
            return null;
        }

        /// <summary>
        /// Registered a callback to get a custom IComponentRenderer for a particular class. Use this to make a
        /// certain component (built-in or external) to use a custom renderer.
        /// </summary>
        public Definition SetRenderer<T>(Type type, IRendererProvider<T>? rendererProvider) where T : IComponent
        {
            if (rendererProvider == null)
            {
                componentRendererMap.Remove(type);
            }
            else
            {
                componentRendererMap[type] = (IRendererProvider<IComponent>)rendererProvider;
            }
            return this;
        }
    }
}