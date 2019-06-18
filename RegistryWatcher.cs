/*
	This file is part of BrightnessTray.

    BrightnessTray is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    BrightnessTray is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with BrightnessTray.  If not, see <http://www.gnu.org/licenses/>.

*/
using Microsoft.Win32;
using System;
using System.Management;
using System.Security.Principal;
using System.Windows.Forms;

namespace BrightnessTray
{
    public class RegistryWatcher : IDisposable
    {
        const string KEY_PATH = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";
        const string KEY_NAME = "SystemUsesLightTheme";

        public delegate void StatusUpdateHandler(object sender, string e);
        public event StatusUpdateHandler OnUpdateStatus;

        private ManagementEventWatcher _watcher = null;

        public RegistryWatcher()
        {
            try
            {

                var currentUser = WindowsIdentity.GetCurrent();
                var query = new WqlEventQuery(string.Format("SELECT * FROM RegistryValueChangeEvent WHERE Hive='HKEY_USERS' AND KeyPath='{0}\\\\{1}' AND ValueName='{2}'",
                currentUser.User.Value, KEY_PATH.Replace("\\", "\\\\"), KEY_NAME));

                _watcher = new ManagementEventWatcher(query);
                _watcher.EventArrived += (sender, args) => { UpdateStatus(args.ToString()); };

                // Start listening for events
                _watcher.Start();
            }
            catch (ManagementException err)
            {
                MessageBox.Show("An error occurred while trying to receive an event: " +
                err.Message);
            }
        }

        public void Dispose()
        {
            _watcher?.Stop();
        }

        private void UpdateStatus(string status)
        {
            // Make sure someone is listening to event
            if (OnUpdateStatus == null) return;

            OnUpdateStatus(this, status);
        }

        public static bool getSystemUsesLightTheme()
        {
            bool result = false;

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KEY_PATH))
                {
                    if (key != null)
                    {
                        object keyValue = key.GetValue(KEY_NAME);

                        if (keyValue != null)
                            result = Convert.ToBoolean(keyValue);
                    }
                }
            }
            catch {}

            return result;
        }
    }
}