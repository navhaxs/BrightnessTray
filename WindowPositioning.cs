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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows;
    using System.Windows.Forms;

    /// <summary>
    /// Helper class for positioning the main window.
    /// </summary>
    public class WindowPositioning
    {
        /// <summary>
        /// Represents alignment of taskbar.
        /// </summary>
        public enum TaskBarAlignment
        {
            /// <summary>
            /// Bottom of screen.
            /// </summary>
            Bottom,

            /// <summary>
            /// Top of screen.
            /// </summary>
            Top,

            /// <summary>
            /// Left of screen.
            /// </summary>
            Left,

            /// <summary>
            /// Right of screen.
            /// </summary>
            Right
        }

        /// <summary>
        /// Gets a value indicating whether the notification area is active.
        /// </summary>
        public static bool IsNotificationAreaActive
        {
            get
            {
                IntPtr activewindowhandle = NativeMethods.GetForegroundWindow();

                IntPtr taskbarhandle = NativeMethods.FindWindow("Shell_TrayWnd", string.Empty);

                // Windows 7 notification area fly-out
                IntPtr notificationareaoverflowhandle = NativeMethods.FindWindow("NotifyIconOverflowWindow", string.Empty);

                return (activewindowhandle == taskbarhandle || activewindowhandle == notificationareaoverflowhandle);
            }
        }

        /// <summary>
        /// Returns the optimum window position in relation to the specified notify icon.
        /// </summary>
        /// <param name="notifyicon">The notify icon that the window should be aligned to.</param>
        /// <param name="windowwidth">The width of the window.</param>
        /// <param name="windowheight">The height of the window.</param>
        /// <param name="dpi">The system's DPI (in relative units: 1.0 = 96 DPI, 1.25 = 120 DPI, etc.).</param>
        /// <param name="pinned">Whether the window is pinned open or not. Affects positioning in Windows 7 only.</param>
        /// <returns>A Point specifying the suggested location of the window (top left point).</returns>
        public static Point GetWindowPosition(NotifyIcon notifyicon, double windowwidth, double windowheight, double dpi)
        {
            // retrieve taskbar information
            TaskBarInfo taskbarinfo = GetTaskBarInfo();

            // retrieve notify icon location
            Rect? nipositiontemp = GetNotifyIconRectangle(notifyicon);

            // if our functions can't find the rectangle, align it to a corner of the screen
            Rect niposition;
            if (nipositiontemp == null)
            {
                switch (taskbarinfo.Alignment)
                {
                    case TaskBarAlignment.Bottom: // bottom right corner
                        niposition = new Rect(taskbarinfo.Position.Right - 1, taskbarinfo.Position.Bottom - 1, 1, 1);
                        break;
                    case TaskBarAlignment.Top: // top right corner
                        niposition = new Rect(taskbarinfo.Position.Right - 1, taskbarinfo.Position.Top, 1, 1);
                        break;
                    case TaskBarAlignment.Right: // bottom right corner
                        niposition = new Rect(taskbarinfo.Position.Right - 1, taskbarinfo.Position.Bottom - 1, 1, 1);
                        break;
                    case TaskBarAlignment.Left: // bottom left corner
                        niposition = new Rect(taskbarinfo.Position.Left, taskbarinfo.Position.Bottom - 1, 1, 1);
                        break;
                    default:
                        goto case TaskBarAlignment.Bottom;
                }
            }
            else
                niposition = (Rect)nipositiontemp;

            // check if notify icon is in the fly-out
            bool inflyout = IsNotifyIconInFlyOut(niposition, taskbarinfo.Position);

            // if the window is pinned open and in the fly-out (Windows 7 only),
            // we should position the window above the 'show hidden icons' button
            // if (inflyout && pinned)
            //     niposition = (Rect)GetNotifyAreaButtonRectangle();

            // determine centre of notify icon
            Point nicentre = new Point(niposition.Left + (niposition.Width / 2), niposition.Top + (niposition.Height / 2));

            // get window offset from edge
            double edgeoffset = Compatibility.WindowEdgeOffset * dpi;

            // get working area bounds
            Rect workarea = GetWorkingArea(niposition);

            // calculate window position
            double windowleft = 0, windowtop = 0;

            switch (taskbarinfo.Alignment)
            {
                case TaskBarAlignment.Bottom:
                    // horizontally centre above icon
                    windowleft = nicentre.X - (windowwidth / 2);
                    if (inflyout)
                        windowtop = niposition.Top - windowheight - edgeoffset;
                    else
                        windowtop = taskbarinfo.Position.Top - windowheight - edgeoffset;

                    break;

                case TaskBarAlignment.Top:
                    // horizontally centre below icon
                    windowleft = nicentre.X - (windowwidth / 2);
                    if (inflyout)
                        windowtop = niposition.Bottom + edgeoffset;
                    else
                        windowtop = taskbarinfo.Position.Bottom + edgeoffset;

                    break;

                case TaskBarAlignment.Left:
                    // vertically centre to the right of icon (or above icon if in flyout and not pinned)
                    if (inflyout)
                    {
                        windowleft = nicentre.X - (windowwidth / 2);
                        windowtop = niposition.Top - windowheight - edgeoffset;
                    }
                    else
                    {
                        windowleft = taskbarinfo.Position.Right + edgeoffset;
                        windowtop = nicentre.Y - (windowheight / 2);
                    }

                    break;

                case TaskBarAlignment.Right:
                    // vertically centre to the left of icon (or above icon if in flyout and not pinned)
                    if (inflyout)
                    {
                        windowleft = nicentre.X - (windowwidth / 2);
                        windowtop = niposition.Top - windowheight - edgeoffset;
                    }
                    else
                    {
                        windowleft = taskbarinfo.Position.Left - windowwidth - edgeoffset;
                        windowtop = nicentre.Y - (windowheight / 2);
                    }

                    break;

                default:
                    goto case TaskBarAlignment.Bottom; // should be unreachable
            }

            //// check that the window is within the working area
            //// if not, put it next to the closest edge

            if (windowleft + windowwidth + edgeoffset > workarea.Right) // too far right
                windowleft = workarea.Right - windowwidth - edgeoffset;
            else if (windowleft < workarea.Left) // too far left
                windowleft = workarea.Left + edgeoffset;

            if (windowtop + windowheight + edgeoffset > workarea.Bottom) // too far down
                windowtop = workarea.Bottom - windowheight - edgeoffset;
            //// the window should never be too far up, so we can skip checking for that

            return new Point(windowleft, windowtop);
        }

        #region Window Size

        /// <summary>
        /// Returns a Rect containing the bounds of the specified window's client area (i.e. area excluding border).
        /// </summary>
        /// <param name="hWnd">Handle of the window.</param>
        /// <returns>Rect containing window client area bounds.</returns>
        public static Rect GetWindowClientAreaSize(IntPtr hWnd)
        {
            NativeMethods.RECT result = new NativeMethods.RECT();
            if (NativeMethods.GetClientRect(hWnd, out result))
                return result;
            else
                throw new Exception(String.Format("Could not find client area bounds for specified window (handle {0:X})", hWnd));
        }

        /// <summary>
        /// Returns a Rect containing the bounds of the specified window's area (i.e. area excluding border).
        /// </summary>
        /// <param name="hWnd">Handle of the window.</param>
        /// <returns>Rect containing window bounds.</returns>
        public static Rect GetWindowSize(IntPtr hWnd)
        {
            NativeMethods.RECT result = new NativeMethods.RECT();
            if (NativeMethods.GetWindowRect(hWnd, out result))
                return result;
            else
                throw new Exception(String.Format("Could not find window bounds for specified window (handle {0:X})", hWnd));
        }

        #endregion

        #region Notify Icon Methods

        /// <summary>
        /// Returns a Rect containing the location of the specified notify icon.
        /// </summary>
        /// <param name="notifyicon">The NotifyIcon whose location should be retrieved.</param>
        /// <returns>The location of the specified NotifyIcon.</returns>
        public static Rect? GetNotifyIconRectangle(NotifyIcon notifyicon)
        {
            if (Compatibility.CurrentWindowsVersion == Compatibility.WindowsVersion.Windows7Plus)
                return GetNotifyIconRectWindows7(notifyicon);
            else
                return GetNotifyIconRectLegacy(notifyicon);
        }

        /// <summary>
        /// Returns a rectangle representing the location of the specified NotifyIcon. (Windows 7+.)
        /// </summary>
        /// <param name="notifyicon">The NotifyIcon whose location should be returned.</param>
        /// <returns>The location of the specified NotifyIcon. Null if the location could not be found.</returns>
        public static Rect? GetNotifyIconRectWindows7(NotifyIcon notifyicon)
        {
            if (Compatibility.CurrentWindowsVersion != Compatibility.WindowsVersion.Windows7Plus)
                throw new PlatformNotSupportedException("This method can only be used under Windows 7 or later. Please use GetNotifyIconRectangleLegacy() if you use an earlier operating system.");

            // get notify icon id
            FieldInfo idFieldInfo = notifyicon.GetType().GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);
            int iconid = (int)idFieldInfo.GetValue(notifyicon);

            // get notify icon hwnd
            IntPtr iconhandle;
            try
            {
                FieldInfo windowFieldInfo = notifyicon.GetType().GetField("window", BindingFlags.NonPublic | BindingFlags.Instance);
                System.Windows.Forms.NativeWindow nativeWindow = (System.Windows.Forms.NativeWindow)windowFieldInfo.GetValue(notifyicon);
                iconhandle = nativeWindow.Handle;
                if (iconhandle == null || iconhandle == IntPtr.Zero)
                    return null;
            } catch {
                return null;
            }

            NativeMethods.RECT rect = new NativeMethods.RECT();
            NativeMethods.NOTIFYICONIDENTIFIER nid = new NativeMethods.NOTIFYICONIDENTIFIER()
            {
                hWnd = iconhandle,
                uID = (uint)iconid
            };
            nid.cbSize = (uint)Marshal.SizeOf(nid);

            int result = NativeMethods.Shell_NotifyIconGetRect(ref nid, out rect);

            // 0 means success, 1 means the notify icon is in the fly-out - either is fine
            if (result != 0 && result != 1)
                return null;

            // convert to System.Rect and return
            return rect;
        }

        /// <summary>
        /// Returns a rectangle representing the location of the specified NotifyIcon. (Windows Vista and earlier.)
        /// </summary>
        /// <param name="notifyicon">The NotifyIcon whose location should be returned.</param>
        /// <returns>The location of the specified NotifyIcon.</returns>
        public static Rect? GetNotifyIconRectLegacy(NotifyIcon notifyicon)
        {
            Rect? nirect = null;

            FieldInfo idFieldInfo = notifyicon.GetType().GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);
            int niid = (int)idFieldInfo.GetValue(notifyicon);

            FieldInfo windowFieldInfo = notifyicon.GetType().GetField("window", BindingFlags.NonPublic | BindingFlags.Instance);
            System.Windows.Forms.NativeWindow nativeWindow = (System.Windows.Forms.NativeWindow)windowFieldInfo.GetValue(notifyicon);
            IntPtr nihandle = nativeWindow.Handle;
            if (nihandle == null || nihandle == IntPtr.Zero)
                return null;

            // find the handle of the task bar
            IntPtr taskbarparenthandle = NativeMethods.FindWindow("Shell_TrayWnd", null);

            if (taskbarparenthandle == (IntPtr)null)
                return null;

            // find the handle of the notification area
            IntPtr naparenthandle = NativeMethods.FindWindowEx(taskbarparenthandle, IntPtr.Zero, "TrayNotifyWnd", null);

            if (naparenthandle == (IntPtr)null)
                return null;

            // make a list of toolbars in the notification area (one of them should contain the icon)
            List<IntPtr> natoolbarwindows = NativeMethods.GetChildToolbarWindows(naparenthandle);

            bool found = false;

            for (int i = 0; !found && i < natoolbarwindows.Count; i++)
            {
                IntPtr natoolbarhandle = natoolbarwindows[i];

                // retrieve the number of toolbar buttons (i.e. notify icons)
                int buttoncount = NativeMethods.SendMessage(natoolbarhandle, NativeMethods.TB_BUTTONCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();

                // get notification area's process id
                uint naprocessid;
                NativeMethods.GetWindowThreadProcessId(natoolbarhandle, out naprocessid);

                // get handle to notification area's process
                IntPtr naprocesshandle = NativeMethods.OpenProcess(NativeMethods.ProcessAccessFlags.All, false, naprocessid);

                if (naprocesshandle == IntPtr.Zero)
                    return null;

                // allocate enough memory within the notification area's process to store the button info we want
                IntPtr toolbarmemoryptr = NativeMethods.VirtualAllocEx(naprocesshandle, (IntPtr)null, (uint)Marshal.SizeOf(typeof(NativeMethods.TBBUTTON)), NativeMethods.AllocationType.Commit, NativeMethods.MemoryProtection.ReadWrite);

                if (toolbarmemoryptr == IntPtr.Zero)
                    return null;

                try
                {
                    // loop through the toolbar's buttons until we find our notify icon
                    for (int j = 0; !found && j < buttoncount; j++)
                    {
                        int bytesread = -1;

                        // ask the notification area to give us information about the current button
                        NativeMethods.SendMessage(natoolbarhandle, NativeMethods.TB_GETBUTTON, new IntPtr(j), toolbarmemoryptr);

                        // retrieve that information from the notification area's process
                        NativeMethods.TBBUTTON buttoninfo = new NativeMethods.TBBUTTON();
                        NativeMethods.ReadProcessMemory(naprocesshandle, toolbarmemoryptr, out buttoninfo, Marshal.SizeOf(buttoninfo), out bytesread);

                        if (bytesread != Marshal.SizeOf(buttoninfo))
                            return null;

                        if (buttoninfo.dwData == IntPtr.Zero)
                            return null;

                        // the dwData field contains a pointer to information about the notify icon:
                        // the handle of the notify icon (an 4/8 bytes) and the id of the notify icon (4 bytes)
                        IntPtr niinfopointer = buttoninfo.dwData;

                        // read the notify icon handle
                        IntPtr nihandlenew;
                        NativeMethods.ReadProcessMemory(naprocesshandle, niinfopointer, out nihandlenew, Marshal.SizeOf(typeof(IntPtr)), out bytesread);

                        if (bytesread != Marshal.SizeOf(typeof(IntPtr)))
                            return null;

                        // read the notify icon id
                        uint niidnew;
                        NativeMethods.ReadProcessMemory(naprocesshandle, niinfopointer + Marshal.SizeOf(typeof(IntPtr)), out niidnew, Marshal.SizeOf(typeof(uint)), out bytesread);

                        if (bytesread != Marshal.SizeOf(typeof(uint)))
                            return null;

                        // if we've found a match
                        if (nihandlenew == nihandle && niidnew == niid)
                        {
                            // check if the button is hidden: if it is, return the rectangle of the 'show hidden icons' button
                            if ((byte)(buttoninfo.fsState & NativeMethods.TBSTATE_HIDDEN) != 0)
                            {
                                nirect = GetNotifyAreaButtonRectangle();
                            }
                            else
                            {
                                NativeMethods.RECT result = new NativeMethods.RECT();

                                // get the relative rectangle of the toolbar button (notify icon)
                                NativeMethods.SendMessage(natoolbarhandle, NativeMethods.TB_GETITEMRECT, new IntPtr(j), toolbarmemoryptr);

                                NativeMethods.ReadProcessMemory(naprocesshandle, toolbarmemoryptr, out result, Marshal.SizeOf(result), out bytesread);

                                if (bytesread != Marshal.SizeOf(result))
                                    return null;

                                // find where the rectangle lies in relation to the screen
                                NativeMethods.MapWindowPoints(natoolbarhandle, (IntPtr)null, ref result, 2);

                                nirect = result;
                            }

                            found = true;
                        }
                    }
                }
                finally
                {
                    // free memory within process
                    NativeMethods.VirtualFreeEx(naprocesshandle, toolbarmemoryptr, 0, NativeMethods.FreeType.Release);

                    // close handle to process
                    NativeMethods.CloseHandle(naprocesshandle);
                }
            }

            return nirect;
        }

        /// <summary>
        /// Retrieves the rectangle of the 'Show Hidden Icons' button, or null if it can't be found.
        /// </summary>
        /// <returns>Rectangle containing bounds of 'Show Hidden Icons' button, or null if it can't be found.</returns>
        public static Rect? GetNotifyAreaButtonRectangle()
        {
            // find the handle of the taskbar
            IntPtr taskbarparenthandle = NativeMethods.FindWindow("Shell_TrayWnd", null);

            if (taskbarparenthandle == (IntPtr)null)
                return null;

            // find the handle of the notification area
            IntPtr naparenthandle = NativeMethods.FindWindowEx(taskbarparenthandle, IntPtr.Zero, "TrayNotifyWnd", null);

            if (naparenthandle == (IntPtr)null)
                return null;

            List<IntPtr> nabuttonwindows = NativeMethods.GetChildButtonWindows(naparenthandle);

            if (nabuttonwindows.Count == 0)
                return null; // found no buttons

            IntPtr buttonpointer = nabuttonwindows[0]; // just take the first button

            NativeMethods.RECT result;

            if (!NativeMethods.GetWindowRect(buttonpointer, out result))
                return null; // return null if we can't find the button

            return result;
        }

        /// <summary>
        /// Determines whether the specified System.Windows.Forms.NotifyIcon is contained within the Windows 7 notification area fly-out.
        /// Note that this function will return false if the fly-out is closed.
        /// </summary>
        /// <param name="notifyicon">Notify icon to test.</param>
        /// <param name="taskbarinfo">Taskbar information structure containing taskbar alignment (bottom/top/left/right).</param>
        /// <returns>True if the notify icon is in the fly-out, false if not.</returns>
        public static bool IsNotifyIconInFlyOut(NotifyIcon notifyicon, TaskBarInfo taskbarinfo)
        {
            if (Compatibility.CurrentWindowsVersion != Compatibility.WindowsVersion.Windows7Plus)
                return false; // nothing to worry about in earlier versions

            Rect? nirect = GetNotifyIconRectangle(notifyicon);

            if (nirect == null)
                return false; // if we can't find the notify icon, return false

            return IsNotifyIconInFlyOut((Rect)nirect, taskbarinfo.Position);
        }

        /// <summary>
        /// Determines whether the specified System.Windows.Forms.NotifyIcon is contained within the Windows 7 notification area fly-out.
        /// Note that this function will return false if the fly-out is closed.
        /// </summary>
        /// <param name="notifyiconrect">Rectangle of notify icon bounds.</param>
        /// <param name="taskbarrect">Rectangle of taskbar bounds.</param>
        /// <returns>True if the notify icon is in the fly-out, false if not.</returns>
        public static bool IsNotifyIconInFlyOut(Rect notifyiconrect, Rect taskbarrect)
        {
            if (Compatibility.CurrentWindowsVersion != Compatibility.WindowsVersion.Windows7Plus)
                return false; // nothing to worry about in earlier versions

            return (notifyiconrect.Left > taskbarrect.Right || notifyiconrect.Right < taskbarrect.Left
                 || notifyiconrect.Bottom < taskbarrect.Top || notifyiconrect.Top > taskbarrect.Bottom);
        }

        /// <summary>
        /// Checks whether a point is within the bounds of the specified notify icon.
        /// </summary>
        /// <param name="point">Point to check.</param>
        /// <param name="notifyicon">Notify icon to check.</param>
        /// <returns>True if the point is contained in the bounds, false otherwise.</returns>
        public static bool IsPointInNotifyIcon(Point point, NotifyIcon notifyicon)
        {
            Rect? nirect = GetNotifyIconRectangle(notifyicon);
            if (nirect == null)
                return false;
            return ((Rect)nirect).Contains(point);
        }

        #endregion

        #region Mouse Positioning

        /// <summary>
        /// Returns the cursor's current position as a System.Windows.Point.
        /// </summary>
        /// <returns>Cursor's current position.</returns>
        public static Point GetCursorPosition()
        {
            NativeMethods.POINT result;
            if (NativeMethods.GetCursorPos(out result))
                return result;
            else
                // Failed to retrieve mouse position
                // Simply return (0,0) to continue instead of crashing
                return new Point(0,0);
        }

        /// <summary>
        /// Returns true if the cursor is currently over the specified notify icon.
        /// </summary>
        /// <param name="notifyicon">The notify icon to test.</param>
        /// <returns>True if the cursor is over the notify icon, false if not.</returns>
        public static bool IsCursorOverNotifyIcon(NotifyIcon notifyicon)
        {
            return IsPointInNotifyIcon(GetCursorPosition(), notifyicon);
        }

        #endregion

        #region Taskbar Methods

        /// <summary>
        /// Retrieves taskbar position and alignment.
        /// </summary>
        /// <returns>Taskbar position and alignment.</returns>
        public static TaskBarInfo GetTaskBarInfo()
        {
            // allocate appbardata structure
            NativeMethods.APPBARDATA abdata = new NativeMethods.APPBARDATA() { hWnd = IntPtr.Zero };
            abdata.cbSize = (uint)Marshal.SizeOf(abdata);

            // get task bar info
            IntPtr result = NativeMethods.SHAppBarMessage(NativeMethods.ABMsg.ABM_GETTASKBARPOS, ref abdata);

            // return null if the call failed
            if (result == IntPtr.Zero)
                throw new Exception("Could not retrieve taskbar information.");

            Rect position = abdata.rc;

            TaskBarAlignment alignment;

            switch (abdata.uEdge)
            {
                case NativeMethods.ABEdge.ABE_BOTTOM:
                    alignment = TaskBarAlignment.Bottom;
                    break;
                case NativeMethods.ABEdge.ABE_TOP:
                    alignment = TaskBarAlignment.Top;
                    break;
                case NativeMethods.ABEdge.ABE_LEFT:
                    alignment = TaskBarAlignment.Left;
                    break;
                case NativeMethods.ABEdge.ABE_RIGHT:
                    alignment = TaskBarAlignment.Right;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Couldn't retrieve location of taskbar.");
            }

            return new TaskBarInfo() { Position = position, Alignment = alignment };
        }

        #endregion

        #region Monitor Bounds

        /// <summary>
        /// Returns the working area of the monitor that intersects most with the specified rectangle.
        /// If no monitor can be found, the closest monitor to the rectangle is returned.
        /// </summary>
        /// <param name="rectangle">The rectangle that is located on the monitor whose working area should be returned.</param>
        /// <returns>A rectangle defining the working area of the monitor closest to containing the specified rectangle.</returns>
        public static Rect GetWorkingArea(Rect rectangle)
        {
            NativeMethods.RECT rect = (NativeMethods.RECT)rectangle;
            IntPtr monitorhandle = NativeMethods.MonitorFromRect(ref rect, NativeMethods.MONITOR_DEFAULTTONEAREST);

            NativeMethods.MONITORINFO monitorinfo = new NativeMethods.MONITORINFO();
            monitorinfo.cbSize = (uint)Marshal.SizeOf(monitorinfo);

            bool result = NativeMethods.GetMonitorInfo(monitorhandle, ref monitorinfo);
            if (!result)
                throw new Exception("Failed to retrieve monitor information.");

            return monitorinfo.rcWork;
        }

        #endregion

        /// <summary>
        /// Managed structure representing taskbar position and alignment.
        /// </summary>
        public struct TaskBarInfo
        {
            /// <summary>
            /// Rectangle of taskbar bounds.
            /// </summary>
            public Rect Position;

            /// <summary>
            /// Alignment of taskbar.
            /// </summary>
            public TaskBarAlignment Alignment;
        }
    }
}
