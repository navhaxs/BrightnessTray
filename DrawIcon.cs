using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
            
            Font fontToUse = new Font("Tahoma", 11, FontStyle.Regular, GraphicsUnit.Pixel);
            Brush brushToUse = new SolidBrush(Color.White);
            Rectangle rect = new Rectangle(0, 0, 16, 16);
            Bitmap bitmapText = new Bitmap(16, 16);
            Graphics g = Graphics.FromImage(bitmapText);
            IntPtr hIcon;
            g.Clear(System.Drawing.Color.Transparent);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            g.DrawString(percentage.ToString(), fontToUse, brushToUse, rect, sf);

            hIcon = (bitmapText.GetHicon());

            notifyIcon.Icon = System.Drawing.Icon.FromHandle(hIcon);
            notifyIcon.Text = "Brightness " + percentage.ToString() + "%";

        }
    }
}
