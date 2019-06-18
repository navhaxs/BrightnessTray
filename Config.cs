using System;
using System.Drawing;
using System.IO;

namespace BrightnessTray
{
    class Config
    {
        const string INI_FILE = "BrightnessTray.ini";

        static public Color Foreground = Color.White;
        static public Color ForegroundLight = Color.Black;
        static public Color Background = Color.Transparent;
        static public Color BackgroundLight = Color.Transparent;

        static Config()
        {
            if (File.Exists(INI_FILE))
            {
                IniFile inifile = new IniFile();
                inifile.Load(INI_FILE);

                tryParseColor(inifile["icon"]["foreground"].GetString(), ref Foreground);
                tryParseColor(inifile["lighticon"]["foreground"].GetString(), ref ForegroundLight);
                tryParseColor(inifile["icon"]["background"].GetString(), ref Background);
                tryParseColor(inifile["lighticon"]["background"].GetString(), ref BackgroundLight);
            }
        }

        /// <summary>
        /// Tray icon: Show % text instead of the pretty icon (TODO: set by a startup parameter)
        /// </summary>
        static public bool showTextIcon = true;

        /// <summary>
        /// Pop-up panel: Show % text label (TODO: set by a startup parameter)
        /// </summary>
        static public bool showPercentageText = true;

        static private void tryParseColor(string rawColorString, ref Color target)
        {
            if (rawColorString == null || string.IsNullOrEmpty(rawColorString))
                return;

            if (rawColorString.ToLower().Equals("transparent"))
            {
                target = Color.FromArgb(0, 0, 0, 0);
            }
            else if (rawColorString.Contains(","))
            {
                string[] s = rawColorString.Split(',');
                if (s.Length == 3)
                {
                    target = Color.FromArgb(int.Parse(s[0]), int.Parse(s[1]), int.Parse(s[2]));
                    return;
                }
                else if (s.Length == 4)
                {
                    target = Color.FromArgb(int.Parse(s[0]), int.Parse(s[1]), int.Parse(s[2]), int.Parse(s[3]));
                    return;
                }
                throw new Exception($"Invalid ARGB string: [{rawColorString}]");
            }
            else
            {
                target = Color.FromName(rawColorString);
            }
        }

    }
}
