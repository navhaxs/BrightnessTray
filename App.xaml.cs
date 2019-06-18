using System;
using System.Diagnostics;
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

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            DateTime now = DateTime.Now;
            string filename = "BrightnessTrayApp_ErrorReport_" + now.Day + now.Month + now.Year + "_" + now.Hour + now.Minute + now.Second + ".txt";
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string[] report = { "BrightnessTray", now.ToString(), fvi.FileVersion, System.Environment.OSVersion.ToString(), e.Exception.ToString() };
            string[] baseDirsToAttempt = { System.Environment.CurrentDirectory, System.Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) };
            
            bool success = false;
            foreach (string dir in baseDirsToAttempt)
            {
                try
                {
                    System.IO.File.WriteAllLines(dir + @"\" + filename, report);

                    // no exception thrown, assume log file written without error
                    MessageBox.Show("Crash report written to: " + dir + @"\" + filename);
                    success = true;
                    break;
                }
                catch
                { }
            }

            if (!success)
            {
                MessageBox.Show("Crash: " + string.Join("\r\n", report));
            }
        }
    }
}
