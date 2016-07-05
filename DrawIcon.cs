using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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
            if (notifyIcon == null || Config.isTextIcon == false)
            {
                return;
            }

            string drawMe = percentage.ToString();

            Font fontToUse;
            if (percentage == 100)
            {
                // reduce size to fit "100"
                fontToUse = new Font("Tahoma", 20, FontStyle.Regular, GraphicsUnit.Pixel);
            } else
            {
                fontToUse = new Font("Tahoma", 24, FontStyle.Regular, GraphicsUnit.Pixel);
            }

            Brush brushToUse = new SolidBrush(Color.White);
            Rectangle rect = new Rectangle(-6, 2, 42, 32);
            Bitmap bitmapText = new Bitmap(32, 32);
            Graphics g = Graphics.FromImage(bitmapText);
            IntPtr hIcon;
            g.Clear(System.Drawing.Color.Transparent);
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
