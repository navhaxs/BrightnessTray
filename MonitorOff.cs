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
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace BrightnessTray
{
    static class MonitorOff
    {

        private static int WM_SYSCOMMAND = 0x0112;
        private static int SC_MONITORPOWER = 0xF170;

        [DllImport("user32.dll")]
        private static extern int SendMessage(int hWnd, int hMsg, int wParam, int lParam);

        public static void TurnOffMonitor(Window wnd)
        {
            WindowInteropHelper windowHwnd = new WindowInteropHelper(wnd);
            SendMessage(windowHwnd.Handle.ToInt32(), WM_SYSCOMMAND, SC_MONITORPOWER, 2);
        }
    }
}
