/*
 * This file is part of lanterna (https://github.com/mabe02/lanterna).
 *
 * lanterna is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Copyright (C) 2010-2020 Martin Berglund
 */
namespace Lanterna.Gui2;

/// <summary>
/// Set of predefined localized string.
/// All this strings are localized by using LocalizedUIBundle.
/// Changing the locale by calling Locale.setDefault(Locale).
/// </summary>
public sealed class LocalizedString
{
    /// <summary>
    /// "OK"
    /// </summary>
    public static readonly LocalizedString OK = new("short.label.ok", "OK");
    
    /// <summary>
    /// "Cancel"
    /// </summary>
    public static readonly LocalizedString Cancel = new("short.label.cancel", "Cancel");
    
    /// <summary>
    /// "Yes"
    /// </summary>
    public static readonly LocalizedString Yes = new("short.label.yes", "Yes");
    
    /// <summary>
    /// "No"
    /// </summary>
    public static readonly LocalizedString No = new("short.label.no", "No");
    
    /// <summary>
    /// "Close"
    /// </summary>
    public static readonly LocalizedString Close = new("short.label.close", "Close");
    
    /// <summary>
    /// "Abort"
    /// </summary>
    public static readonly LocalizedString Abort = new("short.label.abort", "Abort");
    
    /// <summary>
    /// "Ignore"
    /// </summary>
    public static readonly LocalizedString Ignore = new("short.label.ignore", "Ignore");
    
    /// <summary>
    /// "Retry"
    /// </summary>
    public static readonly LocalizedString Retry = new("short.label.retry", "Retry");
    
    /// <summary>
    /// "Continue"
    /// </summary>
    public static readonly LocalizedString Continue = new("short.label.continue", "Continue");
    
    /// <summary>
    /// "Open"
    /// </summary>
    public static readonly LocalizedString Open = new("short.label.open", "Open");
    
    /// <summary>
    /// "Save"
    /// </summary>
    public static readonly LocalizedString Save = new("short.label.save", "Save");

    private readonly string _defaultValue;
    private readonly string _bundleKey;

    private LocalizedString(string bundleKey, string defaultValue)
    {
        _bundleKey = bundleKey;
        _defaultValue = defaultValue;
    }

    public override string ToString()
    {
        // TODO: Implement proper localization bundle support
        // For now, just return the default English value
        return _defaultValue;
    }
}