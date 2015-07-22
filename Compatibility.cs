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

    This file originated from the WPF Notification Area Icon
    sample code written by David Warner, 2014.
    It is licensed under the Apache License Version 2.0, see LICENCE.
    <https://github.com/Quppa/NotificationAreaIconSampleAppWPF>

*/
namespace BrightnessTray
{
    using System.Windows;

    /// <summary>
    /// Provides helper fields for determining system settings, such as what version of Windows the user is running.
    /// </summary>
    public class Compatibility
    {
        /// <summary>
        /// Versions of Windows 
        /// </summary>
        public enum WindowsVersion
        {
            /// <summary>
            /// Windows 7 or Windows Server 2008 R2 or newer. Supported.
            /// </summary>
            /// <remarks>Any newer versions of Windows are currently treated the same as Windows 7/Server 2008 R2.</remarks>
            Windows7Plus,

            /// <summary>
            /// Windows Vista or Windows Server 2008. Supported.
            /// </summary>
            WindowsVista,

            /// <summary>
            /// Versions of Windows earlier than Windows Vista/Server 2008. Not supported.
            /// </summary>
            WindowsLegacy
        }

        /// <summary>
        /// Gets a value indicating whether the application is being run within a remote session.
        /// </summary>
        public static bool IsRemoteSession
        {
            get
            {
                return (SystemParameters.IsRemoteSession || SystemParameters.IsRemotelyControlled);
            }
        }

        /// <summary>
        /// Gets the distance from the edge of the screen/taskbar that the window should be drawn from.
        /// This is 8 under Windows 7 with the DWM enabled, and 0 in Windows Vista and in Windows 7 when the DWM is disabled.
        /// </summary>
        public static int WindowEdgeOffset
        {
            get
            {
                // check if the DWM is enabled
                if (IsDWMEnabled)
                    if (Compatibility.CurrentWindowsVersion == WindowsVersion.WindowsVista)
                        return 1; // dwm enabled in Vista = offset of 1
                    else
                        return 8; // dwm enabled in 7+ = offset of 8
                else
                    return 0; // dwm disabled = offset of 0
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should use normal window borders or not.
        /// Normal window borders should be used only when the DWM is enabled.
        /// </summary>
        /// <returns>True if borders should be drawn, false otherwise.</returns>
        public static bool BorderVisibility
        {
            get
            {
                return IsDWMEnabled;
            }
        }

        /// <summary>
        /// Gets the current Windows version (Windows 7 or newer, Windows Vista or Windows XP).
        /// </summary>
        public static WindowsVersion CurrentWindowsVersion
        {
            get
            {
                if (System.Environment.OSVersion.Version.Major < 6)
                    return WindowsVersion.WindowsLegacy; // Legacy (<6.0)
                else
                    if (System.Environment.OSVersion.Version.Minor == 0)
                        return WindowsVersion.WindowsVista; // Vista (6.0)
                    else
                        return WindowsVersion.Windows7Plus; // Windows 7+ (6.1+)
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Desktop Window Manager (thus Aero) is enabled.
        /// </summary>
        /// <returns>True if the DWM is enabled, false otherwise.</returns>
        public static bool IsDWMEnabled
        {
            get
            {
                if (CurrentWindowsVersion == WindowsVersion.WindowsLegacy)
                    return false;

                bool result;
                NativeMethods.DwmIsCompositionEnabled(out result);
                return result;
            }
        }
    }
}
