using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

/*
 * Credit: NineBerry https://stackoverflow.com/q/36379547/
 */

namespace BrightnessTray
{
    class DrawIcon
    {
        const string INI_FILE = "BrightnessTray.ini";

        static public Color parseColor(string rawColorString)
        {
            if (rawColorString.ToLower().Equals("transparent"))
            {
                return Color.FromArgb(0, 0, 0, 0);
            }
            else if (rawColorString.Contains(","))
            {
                string[] s = rawColorString.Split(',');
                if (s.Length == 3)
                {
                    return Color.FromArgb(int.Parse(s[0]), int.Parse(s[1]), int.Parse(s[2]));
                }
                else if (s.Length == 4)
                {
                    return Color.FromArgb(int.Parse(s[0]), int.Parse(s[1]), int.Parse(s[2]), int.Parse(s[3]));
                }
                throw new Exception("Invalid RGB string");
            }
            else {
                return Color.FromName(rawColorString);
            }
        }

        // draw the brightness percentage to the tray icon, and update the tooltip label
        static public void updateNotifyIcon(NotifyIcon notifyIcon, int percentage)
        {
            if (notifyIcon == null)
            {
                return;
            }

            if (Config.showTextIcon == false)
            {
                // draw an icon instead of text
                Stream iconstream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/BrightnessTray;component/res/sun.ico")).Stream;
                notifyIcon.Icon = new System.Drawing.Icon(iconstream, System.Windows.Forms.SystemInformation.SmallIconSize);
                iconstream.Close();
                return;
            }

            Color _foreground;
            Color _foregroundLight;
            Color _background;
            Color _backgroundLight;
            if (File.Exists(INI_FILE))
            {
                IniFile inifile = new IniFile();
                inifile.Load(INI_FILE);

                _foreground = parseColor(inifile["icon"]["foreground"].GetString());
                _foregroundLight = parseColor(inifile["lighticon"]["foreground"].GetString());
                _background = parseColor(inifile["icon"]["background"].GetString());
                _backgroundLight = parseColor(inifile["lighticon"]["background"].GetString());
            }
            else
            {
                _foreground = Color.White;
                _foregroundLight = Color.Black;
                _background = Color.Transparent;
                _backgroundLight = Color.Transparent;
            }

            Color foreground = RegistryWatcher.getSystemUsesLightTheme() ? _foregroundLight : _foreground;
            Color background = RegistryWatcher.getSystemUsesLightTheme() ? _backgroundLight : _background;

            string drawMe = percentage.ToString();
            Font fontToUse;
            Brush brushToUse = new SolidBrush(foreground);
            Rectangle rect;
            Bitmap bitmapText;
            Graphics g;
            IntPtr hIcon;

            // draw correct icon size (prevents antialiasing due to dpi)
            int requestedSize = NativeMethods.GetSystemMetrics(NativeMethods.SystemMetric.SM_CXSMICON);

            if (requestedSize > 16)
            {
                //32x32

                if (percentage == 100)
                {
                    // reduce size to fit "100"
                    fontToUse = new Font("Tahoma", 20, FontStyle.Regular, GraphicsUnit.Pixel);
                }
                else
                {
                    fontToUse = new Font("Tahoma", 24, FontStyle.Regular, GraphicsUnit.Pixel);
                }

                rect = new Rectangle(-6, 2, 42, 32);
                bitmapText = new Bitmap(32, 32);

            } else
            {
                //16x16

                if (percentage == 100)
                {
                    // reduce size to fit "100"
                    fontToUse = new Font("Tahoma", 9, FontStyle.Regular, GraphicsUnit.Pixel);
                }
                else
                {
                    fontToUse = new Font("Tahoma", 12, FontStyle.Regular, GraphicsUnit.Pixel);
                }

                rect = new Rectangle(-2, 1, 20, 16);
                bitmapText = new Bitmap(16, 16);

            }

            g = Graphics.FromImage(bitmapText);
            using (SolidBrush brush = new SolidBrush(background))
            {
                g.FillRectangle(brush, 0, 0, 32, 32);
            }
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            g.DrawString(drawMe, fontToUse, brushToUse, rect, sf);

            hIcon = (bitmapText.GetHicon());

            notifyIcon.Icon = System.Drawing.Icon.FromHandle(hIcon);
            notifyIcon.Text = "Brightness " + percentage.ToString() + "%";

        }
    }
}
