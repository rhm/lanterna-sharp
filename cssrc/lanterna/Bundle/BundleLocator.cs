using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace Lanterna.Bundle;

/// <summary>
/// Abstract base class for handling resource bundles with UTF-8 encoding support.
/// Provides localization functionality similar to Java's ResourceBundle system.
/// </summary>
public abstract class BundleLocator
{
    private readonly string baseName;
    private readonly Dictionary<CultureInfo, Dictionary<string, string>> bundleCache;

    protected BundleLocator(string baseName)
    {
        this.baseName = baseName ?? throw new ArgumentNullException(nameof(baseName));
        this.bundleCache = new Dictionary<CultureInfo, Dictionary<string, string>>();
    }

    /// <summary>
    /// Gets a localized message for the specified key using the current culture
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="parameters">Optional parameters for message formatting</param>
    /// <returns>The localized message</returns>
    public string Get(string key, params object[] parameters)
    {
        return Get(CultureInfo.CurrentCulture, key, parameters);
    }

    /// <summary>
    /// Gets a localized message for the specified key using the specified culture
    /// </summary>
    /// <param name="culture">The culture to use for localization</param>
    /// <param name="key">The resource key</param>
    /// <param name="parameters">Optional parameters for message formatting</param>
    /// <returns>The localized message</returns>
    public string Get(CultureInfo culture, string key, params object[] parameters)
    {
        var bundle = GetBundle(culture);
        if (bundle.TryGetValue(key, out string? message))
        {
            if (parameters != null && parameters.Length > 0)
            {
                return string.Format(message, parameters);
            }
            return message;
        }
        
        // Fallback to key name if not found, similar to Java ResourceBundle
        return key;
    }

    /// <summary>
    /// Gets the resource bundle for the specified culture, loading it if necessary
    /// </summary>
    private Dictionary<string, string> GetBundle(CultureInfo culture)
    {
        if (bundleCache.TryGetValue(culture, out Dictionary<string, string>? bundle))
        {
            return bundle;
        }

        bundle = LoadBundle(culture);
        bundleCache[culture] = bundle;
        return bundle;
    }

    /// <summary>
    /// Loads the resource bundle for the specified culture.
    /// Implements culture fallback similar to Java's ResourceBundle.
    /// </summary>
    private Dictionary<string, string> LoadBundle(CultureInfo culture)
    {
        // Try culture-specific variants first, then fallback to invariant culture
        var culturesToTry = new List<CultureInfo>();
        
        // Add specific culture (e.g., "en-US")
        if (!culture.IsNeutralCulture)
        {
            culturesToTry.Add(culture);
        }
        
        // Add neutral culture (e.g., "en")
        if (culture.Parent != CultureInfo.InvariantCulture)
        {
            culturesToTry.Add(culture.Parent);
        }
        
        // Add invariant culture as final fallback
        culturesToTry.Add(CultureInfo.InvariantCulture);

        foreach (var cultureToTry in culturesToTry)
        {
            var bundle = TryLoadBundleForCulture(cultureToTry);
            if (bundle != null)
            {
                return bundle;
            }
        }

        // Return empty bundle if no resources found
        return new Dictionary<string, string>();
    }

    /// <summary>
    /// Attempts to load a resource bundle for a specific culture
    /// </summary>
    private Dictionary<string, string>? TryLoadBundleForCulture(CultureInfo culture)
    {
        string resourceName = GetResourceName(culture);
        
        // Try to load from embedded resources
        var assembly = GetType().Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        
        if (stream == null)
        {
            return null;
        }

        return LoadPropertiesFromStream(stream);
    }

    /// <summary>
    /// Gets the resource name for the specified culture
    /// </summary>
    private string GetResourceName(CultureInfo culture)
    {
        if (culture == CultureInfo.InvariantCulture)
        {
            return $"{baseName}.properties";
        }
        
        return $"{baseName}_{culture.Name}.properties";
    }

    /// <summary>
    /// Loads properties from a stream using UTF-8 encoding.
    /// This handles Java .properties file format.
    /// </summary>
    private static Dictionary<string, string> LoadPropertiesFromStream(Stream stream)
    {
        var properties = new Dictionary<string, string>();
        
        using var reader = new StreamReader(stream, Encoding.UTF8);
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