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
/// Dialog that allows the user to iterate the file system and pick file to open/save
/// </summary>
public class FileDialog : DialogWindow
{
    private readonly ActionListBox _fileListBox;
    private readonly ActionListBox _directoryListBox;
    private readonly TextBox _fileBox;
    private readonly Button _okButton;
    private readonly bool _showHiddenFilesAndDirs;

    private DirectoryInfo _directory;
    private FileInfo? _selectedFile;

    /// <summary>
    /// Default constructor for FileDialog
    /// </summary>
    /// <param name="title">Title of the dialog</param>
    /// <param name="description">Description of the dialog, is displayed at the top of the content area</param>
    /// <param name="actionLabel">Label to use on the "confirm" button, for example "open" or "save"</param>
    /// <param name="dialogSize">Rough estimation of how big you want the dialog to be</param>
    /// <param name="showHiddenFilesAndDirs">If true, hidden files and directories will be visible</param>
    /// <param name="selectedObject">Initially selected file node</param>
    public FileDialog(
        string title,
        string? description,
        string actionLabel,
        TerminalSize dialogSize,
        bool showHiddenFilesAndDirs,
        FileSystemInfo? selectedObject) : base(title)
    {
        _selectedFile = null;
        _showHiddenFilesAndDirs = showHiddenFilesAndDirs;

        if (selectedObject == null || !selectedObject.Exists)
        {
            selectedObject = new DirectoryInfo(Directory.GetCurrentDirectory());
        }

        var contentPane = new Panel();
        contentPane.SetLayoutManager(new GridLayout(2));

        if (description != null)
        {
            new Label(description)
                .SetLayoutData(
                    GridLayout.CreateLayoutData(
                        GridLayout.Alignment.Beginning,
                        GridLayout.Alignment.Center,
                        false,
                        false,
                        2,
                        1))
                .AddTo(contentPane);
        }

        int unitWidth = dialogSize.Columns / 3;
        int unitHeight = dialogSize.Rows;

        new FileSystemLocationLabel(this)
            .SetLayoutData(GridLayout.CreateLayoutData(
                GridLayout.Alignment.Fill,
                GridLayout.Alignment.Center,
                true,
                false,
                2,
                1))
            .AddTo(contentPane);

        _fileListBox = new ActionListBox(new TerminalSize(unitWidth * 2, unitHeight));
        _fileListBox.WithBorder(Borders.SingleLine())
            .SetLayoutData(GridLayout.CreateLayoutData(
                GridLayout.Alignment.Beginning,
                GridLayout.Alignment.Center,
                false,
                false))
            .AddTo(contentPane);
            
        _directoryListBox = new ActionListBox(new TerminalSize(unitWidth, unitHeight));
        _directoryListBox.WithBorder(Borders.SingleLine())
            .AddTo(contentPane);

        _fileBox = new TextBox();
        _fileBox.SetLayoutData(GridLayout.CreateLayoutData(
                GridLayout.Alignment.Fill,
                GridLayout.Alignment.Center,
                true,
                false,
                2,
                1));
        contentPane.AddComponent(_fileBox);

        new Separator(Direction.Horizontal)
            .SetLayoutData(
                GridLayout.CreateLayoutData(
                    GridLayout.Alignment.Fill,
                    GridLayout.Alignment.Center,
                    true,
                    false,
                    2,
                    1))
            .AddTo(contentPane);

        _okButton = new Button(actionLabel, new OkHandler(this));
        Panels.Grid(2,
                _okButton,
                new Button(LocalizedString.Cancel.ToString(), new CancelHandler(this)))
            .SetLayoutData(GridLayout.CreateLayoutData(GridLayout.Alignment.End, GridLayout.Alignment.Center, false, false, 2, 1))
            .AddTo(contentPane);

        if (selectedObject is FileInfo fileInfo)
        {
            _directory = fileInfo.Directory ?? new DirectoryInfo(Directory.GetCurrentDirectory());
            _fileBox.SetText(fileInfo.Name);
        }
        else if (selectedObject is DirectoryInfo dirInfo)
        {
            _directory = dirInfo;
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
    /// <returns>The file which was selected in the dialog or null if the dialog was cancelled</returns>
    public override object? ShowDialog(IWindowBasedTextGUI textGUI)
    {
        _selectedFile = null;
        base.ShowDialog(textGUI);
        return _selectedFile;
    }

    private class OkHandler
    {
        private readonly FileDialog _dialog;

        public OkHandler(FileDialog dialog)
        {
            _dialog = dialog;
        }

        public void Invoke()
        {
            if (!string.IsNullOrEmpty(_dialog._fileBox.GetText()))
            {
                try
                {
                    string fileName = _dialog._fileBox.GetText();
                    FileInfo file;
                    
                    if (Path.IsPathRooted(fileName))
                    {
                        file = new FileInfo(fileName);
                    }
                    else
                    {
                        file = new FileInfo(Path.Combine(_dialog._directory.FullName, fileName));
                    }
                    
                    _dialog._selectedFile = file;
                    _dialog.Close();
                }
                catch (Exception)
                {
                    MessageDialog.ShowMessageDialog((IWindowBasedTextGUI)_dialog.TextGUI!, "Error", "Please select a valid file name", MessageDialogButton.OK);
                }
            }
            else
            {
                MessageDialog.ShowMessageDialog((IWindowBasedTextGUI)_dialog.TextGUI!, "Error", "Please select a valid file name", MessageDialogButton.OK);
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
        private readonly FileDialog _dialog;

        public CancelHandler(FileDialog dialog)
        {
            _dialog = dialog;
        }

        public void Invoke()
        {
            _dialog._selectedFile = null;
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
        _directoryListBox.ClearItems();
        _fileListBox.ClearItems();
        
        try
        {
            var entries = directory.GetFileSystemInfos();
            Array.Sort(entries, (a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            
            if (directory.Parent != null)
            {
                _directoryListBox.AddItem("..", () =>
                {
                    _directory = directory.Parent!;
                    ReloadViews(directory.Parent!);
                });
            }
            else
            {
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
                            _directoryListBox.AddItem($"[{entry.FullName}]", () =>
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
                    if ((entry.Attributes & FileAttributes.Hidden) != 0 && !_showHiddenFilesAndDirs)
                    {
                        continue;
                    }
                    
                    if (entry is DirectoryInfo)
                    {
                        _directoryListBox.AddItem(entry.Name, () =>
                        {
                            _directory = (DirectoryInfo)entry;
                            ReloadViews((DirectoryInfo)entry);
                        });
                    }
                    else
                    {
                        _fileListBox.AddItem(entry.Name, () =>
                        {
                            _fileBox.SetText(entry.Name);
                            SetFocusedInteractable(_okButton);
                        });
                    }
                }
                catch (Exception)
                {
                    // Skip entries that can't be accessed
                }
            }
        }
        catch (Exception)
        {
            // Handle case where directory can't be read
        }
        
        if (_fileListBox.IsEmpty)
        {
            _fileListBox.AddItem("<empty>", new DoNothing());
        }
    }

    private class FileSystemLocationLabel : Label
    {
        private readonly FileDialog _dialog;

        public FileSystemLocationLabel(FileDialog dialog) : base("")
        {
            _dialog = dialog;
            SetPreferredSize(TerminalSize.One);
        }

        protected override void OnBeforeDrawing()
        {
            var area = Size;
            string absolutePath = _dialog._directory.FullName;
            int absolutePathLengthInColumns = TerminalTextUtils.GetColumnWidth(absolutePath);
            if (area.Columns < absolutePathLengthInColumns)
            {
                absolutePath = absolutePath.Substring(absolutePathLengthInColumns - area.Columns);
                absolutePath = "..." + absolutePath.Substring(Math.Min(absolutePathLengthInColumns, 3));
            }
            Text = absolutePath;
        }
    }
}