using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lanterna.Bundle;

namespace Lanterna.Graphics;

/// <summary>
/// Theme implementation that loads theme definitions from property collections.
/// This class extends AbstractTheme and provides a way to define UI themes using 
/// property files with a hierarchical key-value format.
/// </summary>
public class PropertyTheme : AbstractTheme
{
    /// <summary>
    /// Creates a new PropertyTheme from a property dictionary.
    /// Throws exceptions on invalid class references.
    /// </summary>
    /// <param name="properties">Properties containing theme definitions</param>
    public PropertyTheme(Dictionary<string, string> properties) 
        : this(properties, false)
    {
    }

    /// <summary>
    /// Creates a new PropertyTheme from a property dictionary with option to ignore unknown classes.
    /// </summary>
    /// <param name="properties">Properties containing theme definitions</param>
    /// <param name="ignoreUnknownClasses">If true, ignores invalid class references instead of throwing exceptions</param>
    public PropertyTheme(Dictionary<string, string> properties, bool ignoreUnknownClasses)
    {
        if (properties == null)
        {
            throw new ArgumentNullException(nameof(properties));
        }

        LoadFromProperties(properties, ignoreUnknownClasses);
    }

    /// <summary>
    /// Creates a PropertyTheme from a properties string content
    /// </summary>
    /// <param name="propertiesContent">String content in Java properties format</param>
    /// <param name="ignoreUnknownClasses">If true, ignores invalid class references</param>
    public PropertyTheme(string propertiesContent, bool ignoreUnknownClasses = false)
    {
        if (propertiesContent == null)
        {
            throw new ArgumentNullException(nameof(propertiesContent));
        }

        var properties = ParsePropertiesString(propertiesContent);
        LoadFromProperties(properties, ignoreUnknownClasses);
    }

    /// <summary>
    /// Loads theme definitions from properties
    /// </summary>
    private void LoadFromProperties(Dictionary<string, string> properties, bool ignoreUnknownClasses)
    {
        foreach (var entry in properties)
        {
            string propertyName = entry.Key;
            string propertyValue = entry.Value;

            try
            {
                ProcessProperty(propertyName, propertyValue);
            }
            catch (Exception ex)
            {
                if (!ignoreUnknownClasses)
                {
                    throw new InvalidOperationException($"Failed to process property '{propertyName}': {ex.Message}", ex);
                }
                // Silently ignore invalid properties if ignoreUnknownClasses is true
            }
        }
    }

    /// <summary>
    /// Processes a single property entry
    /// </summary>
    private void ProcessProperty(string propertyName, string propertyValue)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return;
        }

        // Extract the definition (class name) from property name
        string? definition = GetDefinition(propertyName);
        if (definition == null)
        {
            return;
        }

        // Extract the style attribute from property name
        string style = GetStyle(propertyName);

        // Handle special renderer properties
        if (style.Equals("postrenderer", StringComparison.OrdinalIgnoreCase))
        {
            HandlePostRenderer(definition, propertyValue);
            return;
        }

        if (style.Equals("windowdecoration", StringComparison.OrdinalIgnoreCase))
        {
            HandleWindowDecoration(definition, propertyValue);
            return;
        }

        // Parse the class from definition string
        Type? clazz = ParseClassFromDefinition(definition);
        if (clazz == null)
        {
            throw new InvalidOperationException($"Unable to resolve class '{definition}'");
        }

        // Register the property with AbstractTheme
        AddStyle(definition, style, propertyValue);
    }

    /// <summary>
    /// Extracts the class definition from a property name
    /// </summary>
    private static string? GetDefinition(string propertyName)
    {
        // Find the last dot to separate class from property
        int lastDotIndex = propertyName.LastIndexOf('.');
        if (lastDotIndex == -1)
        {
            return null; // Global property, no class definition
        }

        return propertyName.Substring(0, lastDotIndex);
    }

    /// <summary>
    /// Extracts the style attribute from a property name
    /// </summary>
    private static string GetStyle(string propertyName)
    {
        int lastDotIndex = propertyName.LastIndexOf('.');
        if (lastDotIndex == -1)
        {
            return propertyName; // Global property
        }

        return propertyName.Substring(lastDotIndex + 1);
    }

    /// <summary>
    /// Parses a class type from a definition string
    /// </summary>
    private static Type? ParseClassFromDefinition(string definition)
    {
        // Try to resolve the type from loaded assemblies
        return Type.GetType(definition) ?? FindTypeByName(definition);
    }

    /// <summary>
    /// Attempts to find a type by name across all loaded assemblies
    /// </summary>
    private static Type? FindTypeByName(string typeName)
    {
        // Try common Lanterna namespaces
        string[] namespaces = {
            "Lanterna.Gui2",
            "Lanterna.Graphics", 
            "Lanterna.Core",
            "Lanterna.Terminal",
            "Lanterna.Screen"
        };

        foreach (string ns in namespaces)
        {
            string fullTypeName = $"{ns}.{typeName}";
            Type? type = Type.GetType(fullTypeName);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    /// <summary>
    /// Handles post renderer property
    /// </summary>
    private void HandlePostRenderer(string definition, string propertyValue)
    {
        // Implementation would create post renderer from class name
        // For now, this is a placeholder
    }

    /// <summary>
    /// Handles window decoration property
    /// </summary>
    private void HandleWindowDecoration(string definition, string propertyValue)
    {
        // Implementation would create window decoration renderer from class name
        // For now, this is a placeholder
    }

    /// <summary>
    /// Parses a properties string into a dictionary
    /// </summary>
    private static Dictionary<string, string> ParsePropertiesString(string propertiesContent)
    {
        var properties = new Dictionary<string, string>();
        
        using var reader = new StringReader(propertiesContent);
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

            // Unescape common escape sequences
            key = UnescapePropertyString(key);
            value = UnescapePropertyString(value);

            properties[key] = value;
        }

        return properties;
    }

    /// <summary>
    /// Unescapes common escape sequences in property strings
    /// </summary>
    private static string UnescapePropertyString(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var result = new StringBuilder(input.Length);
        
        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            
            if (c == '\\' && i + 1 < input.Length)
            {
                char nextChar = input[i + 1];
                switch (nextChar)
                {
                    case 'n':
                        result.Append('\n');
                        i++; // Skip next character
                        break;
                    case 'r':
                        result.Append('\r');
                        i++;
                        break;
                    case 't':
                        result.Append('\t');
                        i++;
                        break;
                    case '\\':
                        result.Append('\\');
                        i++;
                        break;
                    case '=':
                        result.Append('=');
                        i++;
                        break;
                    case ':':
                        result.Append(':');
                        i++;
                        break;
                    default:
                        result.Append(c);
                        break;
                }
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }
}