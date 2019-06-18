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
            
            Color foreground = RegistryWatcher.getSystemUsesLightTheme() ? Config.ForegroundLight : Config.Foreground;
            Color background = RegistryWatcher.getSystemUsesLightTheme() ? Config.BackgroundLight : Config.Background;

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
