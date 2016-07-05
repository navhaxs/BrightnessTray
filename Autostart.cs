/*
	This file is part of BrightnessTray.
	Copyright (C) 2015 Jeremy Wong

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License along
	with this program; if not, write to the Free Software Foundation, Inc.,
	51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/
using System;
using IWshRuntimeLibrary;
using Shell32;
using System.Windows.Forms;
using System.IO;

namespace BrightnessTray
{
    static class Autostart
    {
        public static void CreateStartupFolderShortcut()
        {
          WshShellClass wshShell = new WshShellClass();
          IWshRuntimeLibrary.IWshShortcut shortcut;
          string startUpFolderPath = 
            Environment.GetFolderPath(Environment.SpecialFolder.Startup);

          // Create the shortcut
          shortcut = 
            (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(
              startUpFolderPath + "\\" + 
              Application.ProductName + ".lnk");

          shortcut.TargetPath = Application.ExecutablePath;
          shortcut.WorkingDirectory = Application.StartupPath;
          shortcut.Description = "Launch My Application";
          // shortcut.IconLocation = Application.StartupPath + @"\App.ico";
          shortcut.Save();
        }

        public static string GetShortcutTargetFile(string shortcutFilename)
        {
            string pathOnly = Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = Path.GetFileName(shortcutFilename);

            Shell32.Shell shell = new Shell32.ShellClass();
            Shell32.Folder folder = shell.NameSpace(pathOnly);
            Shell32.FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                Shell32.ShellLinkObject link =
                  (Shell32.ShellLinkObject)folderItem.GetLink;
                return link.Path;
            }

            return string.Empty; // Not found
        }

        public static bool CheckStartupFolderShortcutsExists()
        {
            string searchFile = Application.ExecutablePath;
            string startUpFolderPath =
              Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            DirectoryInfo di = new DirectoryInfo(startUpFolderPath);
            FileInfo[] files = di.GetFiles("*.lnk");

            foreach (FileInfo fi in files)
            {
                string shortcutTargetFile = GetShortcutTargetFile(fi.FullName);

                if (shortcutTargetFile.Equals(searchFile,
                      StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static void DeleteStartupFolderShortcut()
        {
            string searchFile = Application.ExecutablePath;
            string startUpFolderPath =
              Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            DirectoryInfo di = new DirectoryInfo(startUpFolderPath);
            FileInfo[] files = di.GetFiles("*.lnk");

            foreach (FileInfo fi in files)
            {
                string shortcutTargetFile = GetShortcutTargetFile(fi.FullName);

                if (shortcutTargetFile.Equals(searchFile,
                      StringComparison.InvariantCultureIgnoreCase))
                {
                    System.IO.File.Delete(fi.FullName);
                }
            }
        }
    }
}
