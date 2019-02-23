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
using System;
using System.Management;

namespace BrightnessTray
{
    public class BrightnessWatcher : IDisposable
    {
        public event EventHandler<BrightnessChangedEventArgs> BrightnessChanged;

        public class BrightnessChangedEventArgs : EventArgs
        {
            public object newBrightness { get; set; } // new screen brightness value

            public BrightnessChangedEventArgs(object b)
            {
                this.newBrightness = b;
            }
        }

        private void WmiEventHandler(object sender, EventArrivedEventArgs e)
        {
            //Debug.WriteLine("Active :          " + e.NewEvent.Properties["Active"].Value.ToString());
            //Debug.WriteLine("Brightness :      " + e.NewEvent.Properties["Brightness"].Value.ToString());
            //Debug.WriteLine("InstanceName :    " + e.NewEvent.Properties["InstanceName"].Value.ToString());
            //Debug.Print("");
        
            if (BrightnessChanged != null)
            {
                BrightnessChanged(this, new BrightnessChangedEventArgs(e.NewEvent.Properties["Brightness"].Value));
            }
        }

        private readonly ManagementEventWatcher _watcher;

        public BrightnessWatcher()
        {
            try
            {
                //register WMI event listener
                var scope = @"root\wmi";
                var query = "SELECT * FROM WmiMonitorBrightnessEvent";
                _watcher = new ManagementEventWatcher(scope, query);
                _watcher.EventArrived += new EventArrivedEventHandler(WmiEventHandler);
                _watcher.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception {0} Trace {1}", e.Message, e.StackTrace);
            }

        }

        public void Dispose()
        {
            if (_watcher != null)
            {
                _watcher.Stop();
            }

            _watcher.Dispose();
        }
    }
}