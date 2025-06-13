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
using Lanterna.Core;

namespace Lanterna.Gui2.Dialogs;

/// <summary>
/// Dialog builder for the FileDialog class, use this to create instances of that class and to customize
/// them
/// </summary>
public class FileDialogBuilder : AbstractDialogBuilder<FileDialogBuilder, FileDialog>
{
    private string _actionLabel;
    private TerminalSize _suggestedSize;
    private FileSystemInfo? _selectedFile;
    private bool _showHiddenDirectories;

    /// <summary>
    /// Default constructor
    /// </summary>
    public FileDialogBuilder() : base("FileDialog")
    {
        _actionLabel = LocalizedString.OK.ToString();
        _suggestedSize = new TerminalSize(45, 10);
        _showHiddenDirectories = false;
        _selectedFile = null;
    }

    protected override FileDialog BuildDialog()
    {
        return new FileDialog(_title, _description, _actionLabel, _suggestedSize, _showHiddenDirectories, _selectedFile);
    }

    /// <summary>
    /// Defines the label to be but on the confirmation button (default: "ok"). You probably want to set this to
    /// LocalizedString.Save.ToString() or LocalizedString.Open.ToString()
    /// </summary>
    /// <param name="actionLabel">Label to put on the confirmation button</param>
    /// <returns>Itself</returns>
    public FileDialogBuilder SetActionLabel(string actionLabel)
    {
        _actionLabel = actionLabel;
        return this;
    }

    /// <summary>
    /// Returns the label on the confirmation button
    /// </summary>
    /// <returns>Label on the confirmation button</returns>
    public string GetActionLabel()
    {
        return _actionLabel;
    }

    /// <summary>
    /// Sets the suggested size for the file dialog, it won't have exactly this size but roughly. Default suggested size
    /// is 45x10.
    /// </summary>
    /// <param name="suggestedSize">Suggested size for the file dialog</param>
    /// <returns>Itself</returns>
    public FileDialogBuilder SetSuggestedSize(TerminalSize suggestedSize)
    {
        _suggestedSize = suggestedSize;
        return this;
    }

    /// <summary>
    /// Returns the suggested size for the file dialog
    /// </summary>
    /// <returns>Suggested size for the file dialog</returns>
    public TerminalSize GetSuggestedSize()
    {
        return _suggestedSize;
    }

    /// <summary>
    /// Sets the file that is initially selected in the dialog
    /// </summary>
    /// <param name="selectedFile">File that is initially selected in the dialog</param>
    /// <returns>Itself</returns>
    public FileDialogBuilder SetSelectedFile(FileSystemInfo? selectedFile)
    {
        _selectedFile = selectedFile;
        return this;
    }

    /// <summary>
    /// Returns the file that is initially selected in the dialog
    /// </summary>
    /// <returns>File that is initially selected in the dialog</returns>
    public FileSystemInfo? GetSelectedFile()
    {
        return _selectedFile;
    }

    /// <summary>
    /// Sets if hidden files and directories should be visible in the dialog (default: false)
    /// </summary>
    /// <param name="showHiddenDirectories">If true then hidden files and directories will be visible</param>
    /// <returns>Itself</returns>
    public FileDialogBuilder SetShowHiddenDirectories(bool showHiddenDirectories)
    {
        _showHiddenDirectories = showHiddenDirectories;
        return this;
    }

    /// <summary>
    /// Checks if hidden files and directories will be visible in the dialog
    /// </summary>
    /// <returns>If true then hidden files and directories will be visible</returns>
    public bool IsShowHiddenDirectories()
    {
        return _showHiddenDirectories;
    }

    protected override FileDialogBuilder Self()
    {
        return this;
    }
}