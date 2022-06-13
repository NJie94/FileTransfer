using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using EventLog;
using System.Globalization;
using System.Threading;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace FileTransfer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string LocalFileDirectory;
        public static string AdminPassword;
        public static bool bDuplicateApp = false;

        //For loging 
        public static bool m_bGetDetailLogAsResult = true;
        public static UIEventLogData pUILog = new UIEventLogData(Environment.CurrentDirectory);

        public void LogWin_Startup(object sender, StartupEventArgs e)
        {

        }


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string serr = "";
           
            pUILog.WriteLog(UIEventLogData.eEventLogCategory.eApp, "Application Starting");
            do
            {
                //Check App Running Status
                Process thisProc = Process.GetCurrentProcess();

                if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
                {
                    MessageBox.Show("Oppss... This application is running");
                    bDuplicateApp = true;
                    Current.Shutdown();
                    return;
                }
            } while (false);

            if (serr != "")
            {
                try
                {
                    MessageBox.Show(serr);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }
        }

    }
}
