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
namespace BrightnessTray
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            
            // initialise flags
            isKeyDown = false;
            MouseClickToHideNotifyIcon = false;

            PreferenceEventHandler = null;
            PreferenceEvent = null;

            // set up listener for brightness changed events
            eventWatcher = new EventWatcherAsync();
            eventWatcher.BrightnessChanged += EventWatcher_BrightnessChanged;

            CreateNotifyIcon();

            this.Visibility = Visibility.Hidden;

        }

        /// <summary>
        /// Update the slider due to a brightness changed event.
        /// </summary>
        private void EventWatcher_BrightnessChanged(object sender, EventWatcherAsync.BrightnessChangedEventArgs e)
        {
            NotifyIcon.Text = "Brightness " + e.newBrightness.ToString() + "%";

            // Only update the slider if the event was generated from an external source 
            // - and NOT caused by BrightnessTray.
            // This helps keep the slider free of being jerky when the user is moving it.

            // e.g. ignore if the user is dragging the slider, or using the scroll wheel on the slider, or up/down keys on the slider.
            if (!this.IsMouseOver && !this.isKeyDown)
            {
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.BrightnessSlider.Value = int.Parse(e.newBrightness.ToString());
                }));
            }
        }

        /// <summary>
        /// Is the keyboard being pressed?
        /// </summary>
        bool isKeyDown;

        /// <summary>
        /// Listener to backlight change events from WMI.
        /// </summary>
        EventWatcherAsync eventWatcher;
        
        /// <summary>
        /// Delegate for handling user preference changes (namely desktop preference changes).
        /// </summary>
        /// <param name="sender">The source of the event. When this event is raised by the SystemEvents class, this object is always null.</param>
        /// <param name="e">A UserPreferenceChangedEventArgs that contains the event data.</param>
        private delegate void UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e);

        /// <summary>
        /// User preferences changed event handler. Set when the window is made visible and unset when the window is hidden.
        /// </summary>
        private event UserPreferenceChanged PreferenceEvent;

        /// <summary>
        /// Gets or sets a handler for user preference changes.
        /// </summary>
        private UserPreferenceChangedEventHandler PreferenceEventHandler { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not we are currently blocking sleep mode 'Caffiene'.
        /// </summary>
        private bool CaffieneEnabled { get; set; }

        /// <summary>
        /// Gets or sets the window's notify icon.
        /// </summary>
        private System.Windows.Forms.NotifyIcon NotifyIcon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user hid the window by clicking the notify icon a second time.
        /// </summary>
        private bool MouseClickToHideNotifyIcon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the location of the cursor when the window was last hidden by clicking the notify icon a second time.
        /// </summary>
        private Point MouseClickToHideNotifyIconPoint { get; set; }

        /// <summary>
        /// The text label in the application's tray icon context menu.
        /// </summary>
        private System.Windows.Forms.MenuItem mnuLabel;

        /// <summary>
        /// The autostart menu item (exists as a local variable so that the checked state can be toggled).
        /// </summary>
        private System.Windows.Forms.MenuItem mnuAutostart;

        /// <summary>
        /// Updates the display (position and appearance) of the window if it is currently visible.
        /// </summary>
        /// <param name="activatewindow">True if the window should be activated, false if not.</param>
        public void UpdateWindowDisplayIfOpen(bool activatewindow)
        {
            if (this.Visibility == Visibility.Visible)
                this.UpdateWindowDisplay(activatewindow);
        }

        /// <summary>
        /// Updates the display (position and appearance) of the window.
        /// </summary>
        /// <param name="activatewindow">True if the window should be activated, false if not.</param>
        public void UpdateWindowDisplay(bool activatewindow)
        {
            if (this.IsLoaded)
            {
                // set handlers if necessary
                this.SetHandlers();

                // get the handle of the window
                HwndSource windowhandlesource = PresentationSource.FromVisual(this) as HwndSource;

                bool glassenabled = Compatibility.IsDWMEnabled;

                //// update location

                Rect windowbounds = (glassenabled ? WindowPositioning.GetWindowSize(windowhandlesource.Handle) : WindowPositioning.GetWindowClientAreaSize(windowhandlesource.Handle));

                // work out the current screen's DPI
                Matrix screenmatrix = windowhandlesource.CompositionTarget.TransformToDevice;

                double dpiX = screenmatrix.M11; // 1.0 = 96 dpi
                double dpiY = screenmatrix.M22; // 1.25 = 120 dpi, etc.

                Point position = WindowPositioning.GetWindowPosition(this.NotifyIcon, windowbounds.Width, windowbounds.Height, dpiX);

                // translate wpf points to screen coordinates
                Point screenposition = new Point(position.X / dpiX, position.Y / dpiY);

                this.Left = screenposition.X;
                this.Top = screenposition.Y;

                // update borders
                if (glassenabled)
                    this.Style = (Style)FindResource("AeroBorderStyle");
                else
                    this.SetNonGlassBorder(this.IsActive);

                // fix aero border if necessary
                if (glassenabled)
                {
                    // set the root border element's margin to 1 pixel
                    WindowBorder.Margin = new Thickness(1 / dpiX);
                    this.BorderThickness = new Thickness(0);

                    // set the background of the window to transparent (otherwise the inner border colour won't be visible)
                    windowhandlesource.CompositionTarget.BackgroundColor = Colors.Transparent;

                    // get dpi-dependent aero border width
                    int xmargin = Convert.ToInt32(1); // 1 unit wide
                    int ymargin = Convert.ToInt32(1); // 1 unit tall

                    NativeMethods.MARGINS margins = new NativeMethods.MARGINS() { cxLeftWidth = xmargin, cxRightWidth = xmargin, cyBottomHeight = ymargin, cyTopHeight = ymargin };

                    NativeMethods.DwmExtendFrameIntoClientArea(windowhandlesource.Handle, ref margins);
                }
                else
                {
                    WindowBorder.Margin = new Thickness(0); // reset the margin if the DWM is disabled
                    this.BorderThickness = new Thickness(1 / dpiX); // set the window's border thickness to 1 pixel
                }

                if (activatewindow)
                {
                    this.Show();
                    this.Activate();
                }
            }
        }

        #region Event Handlers

        /// <summary>
        /// Adds hook to custom DefWindowProc function.
        /// </summary>
        /// <param name="e">OnSourceInitialized EventArgs.</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(this.WndProc);
        }

        /// <summary>
        /// Custom DefWindowProc function used to disable resize and update the window appearance when the window size is changed or the DWM is enabled/disabled.
        /// </summary>
        /// <param name="hWnd">A handle to the window procedure that received the message.</param>
        /// <param name="msg">The message.</param>
        /// <param name="wParam">Additional message information. The content of this parameter depends on the value of the Msg parameter (wParam).</param>
        /// <param name="lParam">Additional message information. The content of this parameter depends on the value of the Msg parameter (lParam).</param>
        /// <param name="handled">True if the message has been handled by the custom window procedure, false if not.</param>
        /// <returns>The return value is the result of the message processing and depends on the message.</returns>
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (this.IsLoaded && this.Visibility == Visibility.Visible)
            {
                switch (msg)
                {
                    case NativeMethods.WM_NCHITTEST:
                        // if the mouse pointer is not over the client area of the tab
                        // ignore it - this disables resize on the glass chrome
                        if (!NativeMethods.IsOverClientArea(hWnd, wParam, lParam))
                            handled = true;

                        break;

                    case NativeMethods.WM_SETCURSOR:
                        if (!NativeMethods.IsOverClientArea(hWnd, wParam, lParam))
                        {
                            // the high word of lParam specifies the mouse message identifier
                            // we only want to handle mouse down messages on the border
                            int hiword = (int)lParam >> 16;
                            if (hiword == NativeMethods.WM_LBUTTONDOWN
                                || hiword == NativeMethods.WM_RBUTTONDOWN
                                || hiword == NativeMethods.WM_MBUTTONDOWN
                                || hiword == NativeMethods.WM_XBUTTONDOWN)
                            {
                                handled = true;
                                this.Focus(); // focus the window
                            }
                        }

                        break;

                    case NativeMethods.WM_DWMCOMPOSITIONCHANGED:
                        // update window appearance accordingly
                        this.UpdateWindowDisplayIfOpen(false);

                        break;

                    case NativeMethods.WM_SIZE:
                        // update window appearance accordingly
                        this.UpdateWindowDisplayIfOpen(false);

                        break;
                }
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Sets handlers for notifying the application of desktop preference changes (taskbar movements, etc.).
        /// </summary>
        private void SetHandlers()
        {
            if (this.PreferenceEvent == null && this.PreferenceEventHandler == null)
            {
                this.PreferenceEvent = new UserPreferenceChanged(this.DesktopPreferenceChangedHandler);
                this.PreferenceEventHandler = new UserPreferenceChangedEventHandler(this.PreferenceEvent);
                SystemEvents.UserPreferenceChanged += this.PreferenceEventHandler;
            }
        }

        /// <summary>
        /// Releases handlers set by SetHandlers().
        /// </summary>
        private void ReleaseHandlers()
        {

            if (this.PreferenceEvent != null || this.PreferenceEventHandler != null)
            {
                SystemEvents.UserPreferenceChanged -= this.PreferenceEventHandler;
                this.PreferenceEvent = null;
                this.PreferenceEventHandler = null;
            }
        }

        /// <summary>
        /// Handler for UserPreferenceChangedEventArgs that updates the window display when the user modifies his or her desktop.
        /// Note: This does not detect taskbar changes when the taskbar is set to auto-hide.
        /// </summary>
        /// <param name="sender">The source of the event. When this event is raised by the SystemEvents class, this object is always null.</param>
        /// <param name="e">A UserPreferenceChangedEventArgs that contains the event data.</param>
        private void DesktopPreferenceChangedHandler(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.Desktop)
                this.UpdateWindowDisplayIfOpen(false);
        }

        #endregion

        /// <summary>
        /// Notify icon clicked or double-clicked method.
        /// </summary>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="args">System.Windows.Forms.MouseEventArgs (which mouse button was pressed, etc.).</param>
        private void NotifyIconClick(object sender, System.Windows.Forms.MouseEventArgs args)
        {

            // sorry if you swapped the primary mouse button
            if (args.Button == System.Windows.Forms.MouseButtons.Left &&
                (!this.MouseClickToHideNotifyIcon
                || (WindowPositioning.GetCursorPosition().X != this.MouseClickToHideNotifyIconPoint.X || WindowPositioning.GetCursorPosition().Y != this.MouseClickToHideNotifyIconPoint.Y)))
            {
                if (!this.IsLoaded)
                {
                    this.Show();
                }
                this.UpdateWindowDisplay(true);
            } else {
                this.MouseClickToHideNotifyIcon = false;
            }
        }

        /// <summary>
        /// Sets the border of the window when the DWM is not enabled. The colour of the border depends on whether the window is active or not.
        /// </summary>
        /// <param name="windowactivated">True if the window is active, false if not.</param>
        private void SetNonGlassBorder(bool windowactivated)
        {
            //if (windowactivated)
            //    this.Style = (Style)FindResource("ClassicBorderStyle");
            //else
            //    this.Style = (Style)FindResource("ClassicBorderStyleInactive");
        }

        /// <summary>
        /// Window deactivated method. Hides window if not pinned and sets deactivated border colour if DWM is disabled.
        /// </summary>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="e">Event arguments.</param>
        private void Window_Deactivated(object sender, EventArgs e)
        {
#if !DEBUG
            this.HideWindow();
#endif

            //if (!Compatibility.IsDWMEnabled)
            //    this.SetNonGlassBorder(false);
        }

        /// <summary>
        /// Hides the window
        /// </summary>
        private void HideWindow()
        {
            // note if mouse is over the notify icon when hiding the window
            // if it is, we will assume that the user clicked the icon to hide the window
            this.MouseClickToHideNotifyIcon = WindowPositioning.IsCursorOverNotifyIcon(this.NotifyIcon) && WindowPositioning.IsNotificationAreaActive;
            if (this.MouseClickToHideNotifyIcon)
                this.MouseClickToHideNotifyIconPoint = WindowPositioning.GetCursorPosition();

            this.ReleaseHandlers();
            this.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Window closing method. Disposes of the notify icon, removes the custom window procedure and releases user preference change handlers.
        /// </summary>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="e">Cancel event arguments.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            eventWatcher.stop();

            // remove the notify icon
            this.NotifyIcon.Visible = false;
            this.NotifyIcon.Dispose();

            if (this.IsLoaded)
            {
                HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
                source.RemoveHook(this.WndProc);
            }

            this.ReleaseHandlers();
        }

        /// <summary>
        /// Exit button clicked method.
        /// </summary>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="e">Routed event arguments.</param>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Exit();
        }

        /// <summary>
        /// Sleep menu button clicked method.
        /// </summary>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="e">Event arguments.</param>
        private void SleepMenuEventHandler(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Suspend, true, true);
        }

        /// <summary>
        /// Exit menu button (notify icon) clicked method.
        /// </summary>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="e">Event arguments.</param>
        private void ExitMenuEventHandler(object sender, EventArgs e)
        {
            this.Exit();
        }

        /// <summary>
        /// Close the application.
        /// </summary>
        private void Exit()
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Window activated method. Sets the window border colour if the DWM is disabled.
        /// </summary>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="e">Event arguments.</param>
        private void Window_Activated(object sender, EventArgs e)
        {

            // update the slider to the current brightness
            var currentBrightness = WmiFunctions.GetBrightnessLevel();
            BrightnessSlider.Value = currentBrightness;

            if (!Compatibility.IsDWMEnabled) {
                this.SetNonGlassBorder(true);
            }
        }

        /// <summary>
        /// Window loaded method. We update the window display before it is made visible, otherwise the user will see its position jump when the notify icon is first clicked.
        /// </summary>
        /// <param name="sender">Sender of the message.</param>
        /// <param name="e">Routed event arguments.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateWindowDisplay(false);

        }

        /// <summary>
        /// Creates notify icon and menu.
        /// </summary>
        private void CreateNotifyIcon()
        {
            System.Windows.Forms.NotifyIcon notifyicon;
    
            notifyicon = new System.Windows.Forms.NotifyIcon();
            var currentBrightness = WmiFunctions.GetBrightnessLevel();
            notifyicon.Text = "Brightness " + currentBrightness + "%";

            // set icon
            Stream iconstream = Application.GetResourceStream(new Uri("pack://application:,,,/BrightnessTray;component/res/sun.ico")).Stream;
            notifyicon.Icon = new System.Drawing.Icon(iconstream, System.Windows.Forms.SystemInformation.SmallIconSize);
            iconstream.Close();

            //System.Windows.Forms.MenuItem mnuPin = new System.Windows.Forms.MenuItem("Pin", new EventHandler(this.PinMenuEventHandler));
            System.Windows.Forms.MenuItem mnuMonitorOff = new System.Windows.Forms.MenuItem("Power off display", new EventHandler(this.MonitorOffMenuEventHandler));
            System.Windows.Forms.MenuItem mnuSleep = new System.Windows.Forms.MenuItem("Enter sleep mode", new EventHandler(this.SleepMenuEventHandler));
            System.Windows.Forms.MenuItem mnuCaffiene = new System.Windows.Forms.MenuItem("Caffiene", new EventHandler(this.CaffieneMenuEventHandler));
            mnuAutostart = new System.Windows.Forms.MenuItem("Autostart", new EventHandler(this.AutostartMenuEventHandler));
            mnuAutostart.Checked = Autostart.CheckStartupFolderShortcutsExists();

            System.Windows.Forms.MenuItem mnuExit = new System.Windows.Forms.MenuItem("Close", new EventHandler(this.ExitMenuEventHandler));
            mnuLabel = new System.Windows.Forms.MenuItem("");
            mnuLabel.Enabled = false; // greyed-out style label

            System.Windows.Forms.MenuItem[] menuitems = new System.Windows.Forms.MenuItem[]
            {
                mnuLabel, new System.Windows.Forms.MenuItem("-"), mnuMonitorOff, mnuSleep, new System.Windows.Forms.MenuItem("-"), mnuCaffiene, new System.Windows.Forms.MenuItem("-"), mnuAutostart, new System.Windows.Forms.MenuItem("-"), mnuExit
            };

            System.Windows.Forms.ContextMenu contextmenu = new System.Windows.Forms.ContextMenu(menuitems);
            contextmenu.Popup += this.ContextMenuPopup;

            notifyicon.ContextMenu = contextmenu;

            notifyicon.MouseClick += this.NotifyIconClick;
            notifyicon.MouseDoubleClick += this.NotifyIconClick;

            notifyicon.Visible = true;

            this.NotifyIcon = notifyicon;
        }

        private void AutostartMenuEventHandler(object sender, EventArgs e)
        {
            if (mnuAutostart.Checked)
            {
                // remove autostart entry
                Autostart.DeleteStartupFolderShortcut();
            } else
            {
                // create autostart entry
                Autostart.CreateStartupFolderShortcut();
            }
            mnuAutostart.Checked = Autostart.CheckStartupFolderShortcutsExists();
        }

        private void ContextMenuPopup(object sender, EventArgs e)
        {
            // update the slider to the current brightness
            var currentBrightness = WmiFunctions.GetBrightnessLevel();

            mnuLabel.Text = "Brightness: " + currentBrightness + "%";
        }

        private void CaffieneMenuEventHandler(object sender, EventArgs e)
        {
            System.Windows.Forms.MenuItem menuitem = sender as System.Windows.Forms.MenuItem;
            if (menuitem != null) {

                if (menuitem.Checked)
                {
                    // reverse-of-being-checked action
                    Caffeine.unlockSleepMode();
                } else
                {
                    Caffeine.lockSleepMode();
                }

                menuitem.Checked = !menuitem.Checked;
            }
        }

        private void MonitorOffMenuEventHandler(object sender, EventArgs e)
        {
            MonitorOff.TurnOffMonitor(this);

        }


        /// <summary>
        /// Hyperlink clicked method.
        /// </summary>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="e">Routed event arguments.</param>
        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            this.HideWindow(); // todo remove pinning
            MonitorOff.TurnOffMonitor(this);
        }

        /// <summary>
        /// Hyperlink clicked method.
        /// </summary>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="e">Routed event arguments.</param>
        private void SleepHyperlink_Click(object sender, RoutedEventArgs e)
        {
            this.HideWindow(); // todo remove pinning
            System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Suspend, true, true);
        }

        /// <summary>
        /// Change the monitor brightness when the slider value changes.
        /// </summary>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var newBrightness = (int) e.NewValue;

            // Change the brightness in a background thread to avoid UI blocking
            new Thread((data) =>
            {
                WmiFunctions.SetBrightnessLevel((int) newBrightness);

            }).Start(); 
        }

        private void Window_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            BrightnessSlider.Focus();
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            BrightnessSlider.Value += e.Delta / 120;
            e.Handled = true;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            isKeyDown = false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            isKeyDown = true;
        }
    }
}
