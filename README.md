BrightnessTray
==========================

A simple Windows utility which lets you control your **laptop screen brightness**, right from your system tray.

Use your mouse/scroll wheel!

![screenshot of main screen](http://i.imgur.com/CQgrsb6.png)

### Extra features
- Power off the laptop screen, and any connected monitors if possible.
- Sleep mode shortcut.
- Start the screensaver... e.g. [Fliqlo](https://fliqlo.com/) flip clock on demand!
- Enable 'Caffiene' mode to keep the monitor from turning off during idle.
- Enable 'Autostart' to put a shortcut in the startup folder.

![screenshot of context menu](http://i.imgur.com/uD1eHCo.jpg)

### Optional Customisation!

- Show a static icon in the tray with the `/NoTextIcon` launch parameter
- Hide the brightness text label in the fly-out window with the `/NoPercentageText` launch parameter
- Customise the tray icon text colours using `BrightnessTray.ini`

```
[icon]
foreground = Red
background = 0,0,0,0
[lighticon]
foreground = Red
background = Black
```

- (The `lighticon` block is used if you are using the light taskbar theme of Windows 10 1903)

![screenshot of main screen](http://i.imgur.com/IISMa4g.jpg)

### Download

Binaries are available on the releases page for this repo on GitHub.

### Attributions

BrightnessTray is released under the GPLv3. See LICENSE.

- Thanks to David Warner's WPF Notification Area Icon sample code, from http://blog.quppa.net/2011/01/03/windows-7-style-notification-area-applications-in-wpf-recap-sample/

- Thanks to @Enichan's Ini.cs from https://github.com/Enichan/Ini/