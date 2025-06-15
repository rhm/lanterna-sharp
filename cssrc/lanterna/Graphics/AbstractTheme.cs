using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lanterna.Core;
using Lanterna.Gui2;

namespace Lanterna.Graphics;

/// <summary>
/// Abstract Theme implementation that manages a hierarchical tree of theme nodes tied to Type objects.
/// Sub-classes will inherit their theme properties from super-class definitions, the System.Object class is
/// considered the root of the tree and as such is the fallback for all other classes.
/// </summary>
public abstract class AbstractTheme : ITheme
{
    private static readonly string STYLE_NORMAL = "";
    private static readonly string STYLE_PRELIGHT = "PRELIGHT";
    private static readonly string STYLE_SELECTED = "SELECTED";
    private static readonly string STYLE_ACTIVE = "ACTIVE";
    private static readonly string STYLE_INSENSITIVE = "INSENSITIVE";
    private static readonly Regex STYLE_FORMAT = new Regex(@"([a-zA-Z]+)(\[([a-zA-Z0-9\-_]+)\])?");

    private readonly ThemeTreeNode rootNode;
    private readonly IWindowPostRenderer? windowPostRenderer;
    private readonly IWindowDecorationRenderer? windowDecorationRenderer;

    protected AbstractTheme(IWindowPostRenderer? postRenderer = null,
                            IWindowDecorationRenderer? decorationRenderer = null)
    {
        this.rootNode = new ThemeTreeNode(typeof(object), null);
        this.windowPostRenderer = postRenderer;
        this.windowDecorationRenderer = decorationRenderer;

        rootNode.foregroundMap[STYLE_NORMAL] = new AnsiTextColor(AnsiColor.White);
        rootNode.backgroundMap[STYLE_NORMAL] = new AnsiTextColor(AnsiColor.Black);
    }

    protected bool AddStyle(string definition, string style, string value)
    {
        ThemeTreeNode? node = GetNode(definition);
        if (node == null)
        {
            return false;
        }
        node.Apply(style, value);
        return true;
    }

    private ThemeTreeNode? GetNode(string definition)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(definition))
            {
                return GetNode(typeof(object));
            }
            else
            {
                Type? type = Type.GetType(definition);
                return type != null ? GetNode(type) : null;
            }
        }
        catch
        {
            return null;
        }
    }

    private ThemeTreeNode GetNode(Type definition)
    {
        if (definition == typeof(object))
        {
            return rootNode;
        }
        
        ThemeTreeNode parent = GetNode(definition.BaseType ?? typeof(object));
        if (parent.childMap.ContainsKey(definition))
        {
            return parent.childMap[definition];
        }

        ThemeTreeNode node = new ThemeTreeNode(definition, parent);
        parent.childMap[definition] = node;
        return node;
    }

    public virtual IThemeDefinition GetDefaultDefinition()
    {
        return new DefinitionImpl(rootNode);
    }

    public virtual IThemeDefinition GetDefinition(Type clazz)
    {
        List<Type> hierarchy = new List<Type>();
        Type? currentType = clazz;
        while (currentType != null && currentType != typeof(object))
        {
            hierarchy.Insert(0, currentType);
            currentType = currentType.BaseType;
        }

        ThemeTreeNode node = rootNode;
        foreach (Type type in hierarchy)
        {
            if (node.childMap.ContainsKey(type))
            {
                node = node.childMap[type];
            }
            else
            {
                break;
            }
        }
        return new DefinitionImpl(node);
    }

    public virtual IWindowPostRenderer? GetWindowPostRenderer()
    {
        return windowPostRenderer;
    }

    public virtual IWindowDecorationRenderer? GetWindowDecorationRenderer()
    {
        return windowDecorationRenderer;
    }

    protected static object? InstanceByClassName(string className)
    {
        if (string.IsNullOrWhiteSpace(className))
        {
            return null;
        }
        try
        {
            Type? type = Type.GetType(className);
            return type != null ? Activator.CreateInstance(type) : null;
        }
        catch
        {
            throw new InvalidOperationException($"Cannot create instance of class: {className}");
        }
    }

    /// <summary>
    /// Returns a list of redundant theme entries in this theme
    /// </summary>
    public List<string> FindRedundantDeclarations()
    {
        List<string> result = new List<string>();
        foreach (ThemeTreeNode node in rootNode.childMap.Values)
        {
            FindRedundantDeclarations(result, node);
        }
        result.Sort();
        return result;
    }

    private void FindRedundantDeclarations(List<string> result, ThemeTreeNode node)
    {
        foreach (string style in node.foregroundMap.Keys)
        {
            string formattedStyle = $"[{style}]";
            if (formattedStyle.Length == 2)
            {
                formattedStyle = "";
            }
            ITextColor color = node.foregroundMap[style];
            ITextColor colorFromParent = new StyleImpl(node.parent!, style).ForegroundColor;
            if (color.Equals(colorFromParent))
            {
                result.Add($"{node.clazz.Name}.foreground{formattedStyle}");
            }
        }
        
        foreach (string style in node.backgroundMap.Keys)
        {
            string formattedStyle = $"[{style}]";
            if (formattedStyle.Length == 2)
            {
                formattedStyle = "";
            }
            ITextColor color = node.backgroundMap[style];
            ITextColor colorFromParent = new StyleImpl(node.parent!, style).BackgroundColor;
            if (color.Equals(colorFromParent))
            {
                result.Add($"{node.clazz.Name}.background{formattedStyle}");
            }
        }
        
        foreach (string style in node.sgrMap.Keys)
        {
            string formattedStyle = $"[{style}]";
            if (formattedStyle.Length == 2)
            {
                formattedStyle = "";
            }
            HashSet<SGR> sgrs = node.sgrMap[style];
            ISet<SGR> sgrsFromParent = new StyleImpl(node.parent!, style).SGRs;
            if (sgrs.SetEquals(sgrsFromParent))
            {
                result.Add($"{node.clazz.Name}.sgr{formattedStyle}");
            }
        }

        foreach (ThemeTreeNode childNode in node.childMap.Values)
        {
            FindRedundantDeclarations(result, childNode);
        }
    }

    private class DefinitionImpl : IThemeDefinition
    {
        internal readonly ThemeTreeNode node;

        public DefinitionImpl(ThemeTreeNode node)
        {
            this.node = node;
        }

        public IThemeStyle Normal => new StyleImpl(node, STYLE_NORMAL);
        public IThemeStyle PreLight => new StyleImpl(node, STYLE_PRELIGHT);
        public IThemeStyle Selected => new StyleImpl(node, STYLE_SELECTED);
        public IThemeStyle Active => new StyleImpl(node, STYLE_ACTIVE);
        public IThemeStyle Insensitive => new StyleImpl(node, STYLE_INSENSITIVE);

        public IThemeStyle? GetCustom(string name)
        {
            return new StyleImpl(node, name);
        }

        public IThemeStyle GetCustom(string name, IThemeStyle defaultValue)
        {
            IThemeStyle? customStyle = GetCustom(name);
            return customStyle ?? defaultValue;
        }

        public char GetCharacter(string name, char fallback)
        {
            if (node.characterMap.TryGetValue(name, out char character))
            {
                return character;
            }
            if (node == node.GetRootNode())
            {
                return fallback;
            }
            else
            {
                return new DefinitionImpl(node.parent!).GetCharacter(name, fallback);
            }
        }

        public bool IsCursorVisible()
        {
            if (node.cursorVisible.HasValue)
            {
                return node.cursorVisible.Value;
            }
            if (node == node.GetRootNode())
            {
                return true;
            }
            else
            {
                return new DefinitionImpl(node.parent!).IsCursorVisible();
            }
        }

        public bool GetBooleanProperty(string name, bool defaultValue)
        {
            if (node.propertyMap.TryGetValue(name, out string propertyValue))
            {
                return bool.Parse(propertyValue);
            }
            if (node == node.GetRootNode())
            {
                return defaultValue;
            }
            else
            {
                return new DefinitionImpl(node.parent!).GetBooleanProperty(name, defaultValue);
            }
        }

        public T? GetRenderer<T>() where T : class
        {
            string? rendererClass = node.renderer;
            if (rendererClass == null)
            {
                if (node == node.GetRootNode())
                {
                    return null;
                }
                else
                {
                    return new DefinitionImpl(node.parent!).GetRenderer<T>();
                }
            }
            return (T?)InstanceByClassName(rendererClass);
        }
    }

    private class StyleImpl : IThemeStyle
    {
        private readonly ThemeTreeNode styleNode;
        private readonly string name;

        internal StyleImpl(ThemeTreeNode node, string name)
        {
            this.styleNode = node;
            this.name = name;
        }

        public ITextColor ForegroundColor
        {
            get
            {
                ThemeTreeNode? node = styleNode;
                while (node != null)
                {
                    if (node.foregroundMap.ContainsKey(name))
                    {
                        return node.foregroundMap[name];
                    }
                    node = node.parent;
                }
                ITextColor? fallback = styleNode.GetRootNode().foregroundMap.GetValueOrDefault(STYLE_NORMAL);
                return fallback ?? new AnsiTextColor(AnsiColor.White);
            }
        }

        public ITextColor BackgroundColor
        {
            get
            {
                ThemeTreeNode? node = styleNode;
                while (node != null)
                {
                    if (node.backgroundMap.ContainsKey(name))
                    {
                        return node.backgroundMap[name];
                    }
                    node = node.parent;
                }
                ITextColor? fallback = styleNode.GetRootNode().backgroundMap.GetValueOrDefault(STYLE_NORMAL);
                return fallback ?? new AnsiTextColor(AnsiColor.Black);
            }
        }

        public ISet<SGR> SGRs
        {
            get
            {
                ThemeTreeNode? node = styleNode;
                while (node != null)
                {
                    if (node.sgrMap.ContainsKey(name))
                    {
                        return new HashSet<SGR>(node.sgrMap[name]);
                    }
                    node = node.parent;
                }
                HashSet<SGR>? fallback = styleNode.GetRootNode().sgrMap.GetValueOrDefault(STYLE_NORMAL);
                return fallback != null ? new HashSet<SGR>(fallback) : new HashSet<SGR>();
            }
        }
    }

    private class ThemeTreeNode
    {
        internal readonly Type clazz;
        internal readonly ThemeTreeNode? parent;
        internal readonly Dictionary<Type, ThemeTreeNode> childMap;
        internal readonly Dictionary<string, ITextColor> foregroundMap;
        internal readonly Dictionary<string, ITextColor> backgroundMap;
        internal readonly Dictionary<string, HashSet<SGR>> sgrMap;
        internal readonly Dictionary<string, char> characterMap;
        internal readonly Dictionary<string, string> propertyMap;
        internal bool? cursorVisible;
        internal string? renderer;

        internal ThemeTreeNode(Type clazz, ThemeTreeNode? parent)
        {
            this.clazz = clazz;
            this.parent = parent;
            this.childMap = new Dictionary<Type, ThemeTreeNode>();
            this.foregroundMap = new Dictionary<string, ITextColor>();
            this.backgroundMap = new Dictionary<string, ITextColor>();
            this.sgrMap = new Dictionary<string, HashSet<SGR>>();
            this.characterMap = new Dictionary<string, char>();
            this.propertyMap = new Dictionary<string, string>();
            this.cursorVisible = true;
            this.renderer = null;
        }

        internal ThemeTreeNode GetRootNode()
        {
            ThemeTreeNode node = this;
            while (node.parent != null)
            {
                node = node.parent;
            }
            return node;
        }

        internal void Apply(string style, string value)
        {
            value = value.Trim();
            Match matcher = STYLE_FORMAT.Match(style);
            if (!matcher.Success)
            {
                throw new ArgumentException($"Unknown style declaration: {style}");
            }
            
            string styleComponent = matcher.Groups[1].Value;
            string? group = matcher.Groups.Count > 3 ? matcher.Groups[3].Value : null;
            
            switch (styleComponent.ToLower().Trim())
            {
                case "foreground":
                    foregroundMap[GetCategory(group)] = ParseTextColor(value);
                    break;
                case "background":
                    backgroundMap[GetCategory(group)] = ParseTextColor(value);
                    break;
                case "sgr":
                    sgrMap[GetCategory(group)] = ParseSGR(value);
                    break;
                case "char":
                    characterMap[GetCategory(group)] = string.IsNullOrEmpty(value) ? ' ' : value[0];
                    break;
                case "cursor":
                    cursorVisible = bool.Parse(value);
                    break;
                case "property":
                    propertyMap[GetCategory(group)] = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
                    break;
                case "renderer":
                    renderer = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
                    break;
                case "postrenderer":
                case "windowdecoration":
                    // Don't do anything with this now, we might use it later
                    break;
                default:
                    throw new ArgumentException($"Unknown style component \"{styleComponent}\" in style \"{style}\"");
            }
        }

        private ITextColor ParseTextColor(string value)
        {
            // Simple implementation - this would need TextColor.Factory.FromString equivalent
            if (Enum.TryParse<AnsiColor>(value, true, out AnsiColor ansiColor))
            {
                return new AnsiTextColor(ansiColor);
            }
            return new AnsiTextColor(AnsiColor.Default);
        }

        private HashSet<SGR> ParseSGR(string value)
        {
            value = value.Trim();
            string[] sgrEntries = value.Split(',');
            HashSet<SGR> sgrSet = new HashSet<SGR>();
            
            foreach (string entry in sgrEntries)
            {
                string trimmedEntry = entry.Trim().ToUpper();
                if (!string.IsNullOrEmpty(trimmedEntry))
                {
                    if (Enum.TryParse<SGR>(trimmedEntry, out SGR sgr))
                    {
                        sgrSet.Add(sgr);
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown SGR code \"{trimmedEntry}\"");
                    }
                }
            }
            return sgrSet;
        }

        private string GetCategory(string? group)
        {
            if (group == null)
            {
                return STYLE_NORMAL;
            }
            
            string[] styles = { STYLE_ACTIVE, STYLE_INSENSITIVE, STYLE_PRELIGHT, STYLE_NORMAL, STYLE_SELECTED };
            foreach (string style in styles)
            {
                if (group.ToUpper().Equals(style))
                {
                    return style;
                }
            }
            return group;
        }
    }
}