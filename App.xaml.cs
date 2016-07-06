using System;
using System.Windows;

namespace BrightnessTray
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            foreach (string i in e.Args) { 
                if (String.Equals(i, "/noTextIcon", StringComparison.OrdinalIgnoreCase)) {
                    Config.showTextIcon = false;
                } else if (String.Equals(i, "/noPercentageText", StringComparison.OrdinalIgnoreCase)) {
                    Config.showPercentageText = false;
                }   
            }
        }
    }
}
