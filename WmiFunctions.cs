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
    /// <summary>
    ///     Functions to modify system values using WMI.
    /// </summary>
    internal static class WmiFunctions
    {

        /// <summary>
        ///     Query the brightness setting of the display.
        /// </summary>
        internal static int GetBrightnessLevel()
        {
            try
            {
                var s = new ManagementScope("root\\WMI");
                var q = new SelectQuery("WmiMonitorBrightness");
                var mos = new ManagementObjectSearcher(s, q);
                var moc = mos.Get();

                foreach (var managementBaseObject in moc)
                {
                    foreach (var o in managementBaseObject.Properties)
                    {
                        if (o.Name == "CurrentBrightness")
                            return Convert.ToInt32(o.Value);
                    }
                }

                moc.Dispose();
                mos.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return 0;
        }


        /// <summary>
        ///     Change the brightness setting of the display.
        /// </summary>
        /// <param name="brightnessLevel">
        ///     The brightness value to apply. Should
        ///     be between 0 and 100.
        /// </param>
        internal static void SetBrightnessLevel(int brightnessLevel)
        {
            if (brightnessLevel < 0 ||
                brightnessLevel > 100)
                throw new ArgumentOutOfRangeException("brightnessLevel");

            try
            {
                var s = new ManagementScope("root\\WMI");
                var q = new SelectQuery("WmiMonitorBrightnessMethods");
                var mos = new ManagementObjectSearcher(s, q);
                var moc = mos.Get();

                foreach (var managementBaseObject in moc)
                {
                    var o = (ManagementObject)managementBaseObject;
                    o.InvokeMethod("WmiSetBrightness", new object[]
                    {
                        UInt32.MaxValue,
                        brightnessLevel
                    });
                }

                moc.Dispose();
                mos.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}