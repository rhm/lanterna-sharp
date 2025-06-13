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
 * Copyright (C) 2010-2016 Martin
 * Copyright (C) 2017 University of Waikato, Hamilton, NZ
 */
using Lanterna.Core;

namespace Lanterna.Gui2.Dialogs;

/// <summary>
/// Dialog that allows the user to iterate the file system and pick directory.
/// </summary>
public class DirectoryDialog : DialogWindow
{
    private readonly ActionListBox _dirListBox;
    private readonly TextBox _dirBox;
    private readonly bool _showHiddenDirs;
    private DirectoryInfo _directory;
    private DirectoryInfo? _selectedDir;

    /// <summary>
    /// Default constructor for DirectoryDialog
    /// </summary>
    /// <param name="title">Title of the dialog</param>
    /// <param name="description">Description of the dialog, is displayed at the top of the content area</param>
    /// <param name="actionLabel">Label to use on the "confirm" button, for example "open" or "save"</param>
    /// <param name="dialogSize">Rough estimation of how big you want the dialog to be</param>
    /// <param name="showHiddenDirs">If true, hidden directories will be visible</param>
    /// <param name="selectedObject">Initially selected directory node</param>
    public DirectoryDialog(
        string title,
        string? description,
        string actionLabel,
        TerminalSize dialogSize,
        bool showHiddenDirs,
        DirectoryInfo? selectedObject) : base(title)
    {
        _selectedDir = null;
        _showHiddenDirs = showHiddenDirs;

        if (selectedObject == null || !selectedObject.Exists)
        {
            selectedObject = new DirectoryInfo(Directory.GetCurrentDirectory());
        }

        var contentPane = new Panel();
        contentPane.SetLayoutManager(new BorderLayout());

        var dirsPane = new Panel();
        dirsPane.SetLayoutManager(new BorderLayout());
        contentPane.AddComponent(dirsPane, new BorderLayout.BorderLayoutData(BorderLayout.Location.Center));

        if (description != null)
            contentPane.AddComponent(new Label(description), new BorderLayout.BorderLayoutData(BorderLayout.Location.Top));

        int unitHeight = dialogSize.Rows;

        _dirListBox = new ActionListBox(new TerminalSize(dialogSize.Columns, unitHeight));
        dirsPane.AddComponent(_dirListBox.WithBorder(Borders.SingleLine()), new BorderLayout.BorderLayoutData(BorderLayout.Location.Center));

        _dirBox = new TextBox(new TerminalSize(dialogSize.Columns, 1));
        dirsPane.AddComponent(_dirBox.WithBorder(Borders.SingleLine()), new BorderLayout.BorderLayoutData(BorderLayout.Location.Bottom));

        var panelButtons = new Panel(new GridLayout(2));
        panelButtons.SetLayoutData(GridLayout.CreateLayoutData(GridLayout.Alignment.End, GridLayout.Alignment.Center, false, false, 2, 1));
        panelButtons.AddComponent(new Button(actionLabel, new OkHandler(this)));
        panelButtons.AddComponent(new Button(LocalizedString.Cancel.ToString(), new CancelHandler(this)));
        contentPane.AddComponent(panelButtons, new BorderLayout.BorderLayoutData(BorderLayout.Location.Bottom));

        if (File.Exists(selectedObject.FullName))
        {
            _directory = selectedObject.Parent ?? selectedObject;
        }
        else if (selectedObject.Exists)
        {
            _directory = selectedObject;
        }
        else
        {
            _directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        }

        ReloadViews(_directory);
        Component = contentPane;
    }

    /// <summary>
    /// Opens the dialog by showing it on the GUI and doesn't return until the dialog has been closed
    /// </summary>
    /// <param name="textGUI">Text GUI to add the dialog to</param>
    /// <returns>The directory which was selected in the dialog or null if the dialog was cancelled</returns>
    public override object? ShowDialog(IWindowBasedTextGUI textGUI)
    {
        _selectedDir = null;
        base.ShowDialog(textGUI);
        return _selectedDir;
    }

    private class OkHandler
    {
        private readonly DirectoryDialog _dialog;

        public OkHandler(DirectoryDialog dialog)
        {
            _dialog = dialog;
        }

        public void Invoke()
        {
            try
            {
                var dir = new DirectoryInfo(_dialog._dirBox.GetText());
                if (dir.Exists)
                {
                    _dialog._selectedDir = dir;
                    _dialog.Close();
                }
                else
                {
                    MessageDialog.ShowMessageDialog((IWindowBasedTextGUI)_dialog.TextGUI!, "Error", "Please select a valid directory name", MessageDialogButton.OK);
                }
            }
            catch (Exception)
            {
                MessageDialog.ShowMessageDialog((IWindowBasedTextGUI)_dialog.TextGUI!, "Error", "Please select a valid directory name", MessageDialogButton.OK);
            }
        }

        public static implicit operator Action(OkHandler handler)
        {
            return handler.Invoke;
        }

        public override string ToString()
        {
            return "OK";
        }
    }

    private class CancelHandler
    {
        private readonly DirectoryDialog _dialog;

        public CancelHandler(DirectoryDialog dialog)
        {
            _dialog = dialog;
        }

        public void Invoke()
        {
            _dialog._selectedDir = null;
            _dialog.Close();
        }

        public static implicit operator Action(CancelHandler handler)
        {
            return handler.Invoke;
        }

        public override string ToString()
        {
            return "Cancel";
        }
    }

    private class DoNothing
    {
        public void Invoke()
        {
            // Do nothing
        }

        public static implicit operator Action(DoNothing doNothing)
        {
            return doNothing.Invoke;
        }

        public override string ToString()
        {
            return "<empty>";
        }
    }

    private void ReloadViews(DirectoryInfo directory)
    {
        _dirBox.SetText(directory.FullName);
        _dirListBox.ClearItems();
        
        try
        {
            DirectoryInfo[] entries = directory.GetDirectories();
            Array.Sort(entries, (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            
            if (directory.Parent != null)
            {
                _dirListBox.AddItem("..", () =>
                {
                    _directory = directory.Parent!;
                    ReloadViews(directory.Parent!);
                });
            }
            else
            {
                // Show drive roots on Windows, filesystem roots on Unix
                DirectoryInfo[] roots = DriveInfo.GetDrives()
                    .Where(d => d.IsReady)
                    .Select(d => d.RootDirectory)
                    .ToArray();
                    
                foreach (var entry in roots)
                {
                    try
                    {
                        if (entry.Exists)
                        {
                            _dirListBox.AddItem($"[{entry.FullName}]", () =>
                            {
                                _directory = entry;
                                ReloadViews(entry);
                            });
                        }
                    }
                    catch (Exception)
                    {
                        // Skip drives that can't be accessed
                    }
                }
            }
            
            foreach (var entry in entries)
            {
                try
                {
                    if ((entry.Attributes & FileAttributes.Hidden) != 0 && !_showHiddenDirs)
                    {
                        continue;
                    }
                    
                    _dirListBox.AddItem(entry.Name, () =>
                    {
                        _directory = entry;
                        ReloadViews(entry);
                    });
                }
                catch (Exception)
                {
                    // Skip directories that can't be accessed
                }
            }
        }
        catch (Exception)
        {
            // Handle case where directory can't be read
        }
        
        if (_dirListBox.IsEmpty)
        {
            _dirListBox.AddItem("<empty>", new DoNothing());
        }
    }
}