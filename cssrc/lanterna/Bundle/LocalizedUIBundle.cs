using System;
using System.Globalization;

namespace Lanterna.Bundle;

/// <summary>
/// Singleton for accessing localized UI strings.
/// Provides a static facade over BundleLocator for "multilang.lanterna-ui" bundle.
/// Supports both default culture and specific culture lookups.
/// </summary>
public static class LocalizedUIBundle
{
    private static readonly Lazy<UIBundleLocator> BundleInstance = 
        new(() => new UIBundleLocator());

    /// <summary>
    /// Gets a localized UI string for the specified key using the current culture
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="parameters">Optional parameters for message formatting</param>
    /// <returns>The localized message</returns>
    public static string Get(string key, params object[] parameters)
    {
        return BundleInstance.Value.Get(key, parameters);
    }

    /// <summary>
    /// Gets a localized UI string for the specified key using the specified culture
    /// </summary>
    /// <param name="culture">The culture to use for localization</param>
    /// <param name="key">The resource key</param>
    /// <param name="parameters">Optional parameters for message formatting</param>
    /// <returns>The localized message</returns>
    public static string Get(CultureInfo culture, string key, params object[] parameters)
    {
        return BundleInstance.Value.Get(culture, key, parameters);
    }

    /// <summary>
    /// Internal BundleLocator implementation for UI localization
    /// </summary>
    private class UIBundleLocator : BundleLocator
    {
        public UIBundleLocator() : base("multilang.lanterna-ui")
        {
        }
    }
}

/// <summary>
/// Common UI string keys used throughout Lanterna
/// </summary>
public static class UIStringKeys
{
    // Dialog buttons
    public const string OK = "button.ok";
    public const string Cancel = "button.cancel";
    public const string Yes = "button.yes";
    public const string No = "button.no";
    public const string Close = "button.close";
    public const string Abort = "button.abort";
    public const string Ignore = "button.ignore";
    public const string Retry = "button.retry";
    public const string Continue = "button.continue";
    
    // File dialog
    public const string FileDialogTitle = "filedialog.title";
    public const string DirectoryDialogTitle = "directorydialog.title";
    public const string FileNotFound = "file.notfound";
    public const string DirectoryNotFound = "directory.notfound";
    
    // List operations
    public const string SelectItem = "list.select";
    public const string NoItemsAvailable = "list.empty";
    
    // General messages
    public const string Loading = "general.loading";
    public const string Saving = "general.saving";
    public const string Error = "general.error";
    public const string Warning = "general.warning";
    public const string Information = "general.information";
}

/// <summary>
/// Extension methods for easier UI string access
/// </summary>
public static class UIStringExtensions
{
    /// <summary>
    /// Gets a localized UI string using the key as extension method
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="parameters">Optional parameters for message formatting</param>
    /// <returns>The localized message</returns>
    public static string Localized(this string key, params object[] parameters)
    {
        return LocalizedUIBundle.Get(key, parameters);
    }

    /// <summary>
    /// Gets a localized UI string using the key and specific culture as extension method
    /// </summary>
    /// <param name="key">The resource key</param>
    /// <param name="culture">The culture to use for localization</param>
    /// <param name="parameters">Optional parameters for message formatting</param>
    /// <returns>The localized message</returns>
    public static string Localized(this string key, CultureInfo culture, params object[] parameters)
    {
        return LocalizedUIBundle.Get(culture, key, parameters);
    }
}