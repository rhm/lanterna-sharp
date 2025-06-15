using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Lanterna.Graphics;

namespace Lanterna.Bundle;

/// <summary>
/// Theme registry and catalog for managing multiple themes.
/// Provides static methods for theme registration, retrieval, and enumeration.
/// Includes fallback mechanism for loading bundled themes.
/// </summary>
public static class LanternaThemes
{
    // Thread-safe registry for themes
    private static readonly ConcurrentDictionary<string, ITheme> ThemeRegistry = new();
    
    // Built-in theme names
    private static readonly string[] BuiltInThemes = {
        "bigsnake",
        "businessmachine", 
        "conqueror",
        "defrost", 
        "blaster"
    };

    static LanternaThemes()
    {
        // Initialize with default theme
        RegisterBuiltInThemes();
    }

    /// <summary>
    /// Registers a theme with the specified name
    /// </summary>
    /// <param name="name">Theme name</param>
    /// <param name="theme">Theme instance</param>
    public static void RegisterTheme(string name, ITheme theme)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Theme name cannot be null or empty", nameof(name));
        }
        
        if (theme == null)
        {
            throw new ArgumentNullException(nameof(theme));
        }

        ThemeRegistry[name] = theme;
    }

    /// <summary>
    /// Gets a theme by name. Returns DefaultTheme if not found.
    /// </summary>
    /// <param name="name">Theme name</param>
    /// <returns>The requested theme or DefaultTheme as fallback</returns>
    public static ITheme GetRegisteredTheme(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return GetDefaultTheme();
        }

        if (ThemeRegistry.TryGetValue(name, out ITheme? theme))
        {
            return theme;
        }

        // Try to load built-in theme if not already registered
        theme = TryLoadBuiltInTheme(name);
        if (theme != null)
        {
            RegisterTheme(name, theme);
            return theme;
        }

        // Fallback to default theme
        return GetDefaultTheme();
    }

    /// <summary>
    /// Gets the default theme (fallback theme used when others can't be loaded)
    /// </summary>
    /// <returns>DefaultTheme instance</returns>
    public static ITheme GetDefaultTheme()
    {
        const string defaultThemeName = "default";
        
        if (ThemeRegistry.TryGetValue(defaultThemeName, out ITheme? theme))
        {
            return theme;
        }

        // Create and register default theme
        theme = new DefaultTheme();
        RegisterTheme(defaultThemeName, theme);
        return theme;
    }

    /// <summary>
    /// Gets all registered theme names
    /// </summary>
    /// <returns>Collection of theme names</returns>
    public static IEnumerable<string> GetRegisteredThemes()
    {
        return ThemeRegistry.Keys;
    }

    /// <summary>
    /// Checks if a theme is registered
    /// </summary>
    /// <param name="name">Theme name</param>    
    /// <returns>True if theme is registered</returns>
    public static bool IsThemeRegistered(string name)
    {
        return !string.IsNullOrEmpty(name) && ThemeRegistry.ContainsKey(name);
    }

    /// <summary>
    /// Removes a theme from the registry
    /// </summary>
    /// <param name="name">Theme name</param>
    /// <returns>True if theme was removed</returns>
    public static bool UnregisterTheme(string name)
    {
        return !string.IsNullOrEmpty(name) && ThemeRegistry.TryRemove(name, out _);
    }

    /// <summary>
    /// Registers all built-in themes
    /// </summary>
    private static void RegisterBuiltInThemes()
    {
        // Register default theme
        RegisterTheme("default", new DefaultTheme());

        // Try to register other built-in themes
        foreach (string themeName in BuiltInThemes)
        {
            var theme = TryLoadBuiltInTheme(themeName);
            if (theme != null)
            {
                RegisterTheme(themeName, theme);
            }
        }
    }

    /// <summary>
    /// Attempts to load a built-in theme from embedded resources
    /// </summary>
    /// <param name="themeName">Name of the theme to load</param>
    /// <returns>Loaded theme or null if not found</returns>
    private static ITheme? TryLoadBuiltInTheme(string themeName)
    {
        try
        {
            // Try to load from embedded resources
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"Lanterna.Bundle.Themes.{themeName}-theme.properties";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                return null;
            }

            // Load properties from stream
            var properties = LoadPropertiesFromStream(stream);
            if (properties.Count == 0)
            {
                return null;
            }

            // Create PropertyTheme from loaded properties
            return new PropertyTheme(properties, true);
        }
        catch
        {
            // Silently fail and return null for missing themes
            return null;
        }
    }

    /// <summary>
    /// Loads properties from a stream
    /// </summary>
    private static Dictionary<string, string> LoadPropertiesFromStream(Stream stream)
    {
        var properties = new Dictionary<string, string>();
        
        using var reader = new StreamReader(stream);
        string? line;
        
        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            
            // Skip empty lines and comments
            if (string.IsNullOrEmpty(line) || line.StartsWith("#") || line.StartsWith("!"))
            {
                continue;
            }

            // Find the separator (= or :)
            int separatorIndex = -1;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '=' || c == ':')
                {
                    // Check if it's escaped
                    if (i == 0 || line[i - 1] != '\\')
                    {
                        separatorIndex = i;
                        break;
                    }
                }
            }

            if (separatorIndex == -1)
            {
                continue; // No separator found
            }

            string key = line.Substring(0, separatorIndex).Trim();
            string value = line.Substring(separatorIndex + 1).Trim();

            properties[key] = value;
        }

        return properties;
    }

    /// <summary>
    /// Creates a theme from a properties file path
    /// </summary>
    /// <param name="filePath">Path to properties file</param>
    /// <param name="ignoreUnknownClasses">Whether to ignore unknown class references</param>
    /// <returns>Loaded theme</returns>
    public static ITheme CreateThemeFromFile(string filePath, bool ignoreUnknownClasses = true)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Theme file not found: {filePath}");
        }

        string content = File.ReadAllText(filePath);
        return new PropertyTheme(content, ignoreUnknownClasses);
    }

    /// <summary>
    /// Creates a theme from properties content string
    /// </summary>
    /// <param name="propertiesContent">Properties content</param>
    /// <param name="ignoreUnknownClasses">Whether to ignore unknown class references</param>
    /// <returns>Created theme</returns>
    public static ITheme CreateThemeFromString(string propertiesContent, bool ignoreUnknownClasses = true)
    {
        if (propertiesContent == null)
        {
            throw new ArgumentNullException(nameof(propertiesContent));
        }

        return new PropertyTheme(propertiesContent, ignoreUnknownClasses);
    }
}