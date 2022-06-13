using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Threading;
using System.Diagnostics;
using EventLog;


namespace FileTransfer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string currentDirectory;

        public string strLogMessage = "";

        //Interlock for Transfer in Progress
        public bool bTransfering = false;

        //Saved selected Directory
        public string strOriginFolder = "";
        public string strTempFolder = "";
        public string strDestFolder = "";

        //Time Interval to copy folder
        public string strTimeInterval = "";

        //Housekeeping Parameter
        public static int iClearlogPeriod = 1;
        public static int iLogFileExpire = 7;
        public static string strLastHousekeepingDateTime = "";
        public static int dClearLogNumber;

        //Config File
        public static IniHelper ConfigFile = new IniHelper(System.IO.Path.Combine(Environment.CurrentDirectory, "Config.ini"));

        //Error in Copy File
        public bool bErrorCopyFile = false;

        //New Windows initialize
        Page.LogViewer logviewer = new Page.LogViewer();
        Page.About About = new Page.About();
        Page.Setting Setting = new Page.Setting();

        //Timer
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer Refreshtimer = new DispatcherTimer();
        DispatcherTimer Housekeepingtimer = new DispatcherTimer();

        


        public MainWindow()
        {
            //Start Timer

            
            loadConfig();
            InitializeComponent();
            Setting.cbBufferFolderEnable.IsChecked = true;
            //var startTimeSpan = TimeSpan.Zero;
            //var periodTimeSpan = TimeSpan.FromMinutes(dTimeInterval);
            /*
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(Convert.ToDouble(strTimeInterval));

            var timer = new System.Threading.Timer((e) =>
            {

            }, null, startTimeSpan, periodTimeSpan);
            */


            timer.Interval = TimeSpan.FromMinutes(Convert.ToDouble(strTimeInterval));
            timer.Tick += Timer_tick;
            timer.Start();

            
            Refreshtimer.Interval = TimeSpan.FromSeconds(1);
            Refreshtimer.Tick += RefreshTimer_tick;
            Refreshtimer.Start();

            Housekeepingtimer.Interval = TimeSpan.FromDays(iClearlogPeriod);
            Housekeepingtimer.Tick += HousekeepingTimer_tick;
            Housekeepingtimer.Start();

        }

        private void Timer_tick(object sender, EventArgs e)
        {
            bTransfering = true;
            //Check if Buffer Folder is use or not
            if (Setting.cbBufferFolderEnable.IsChecked == true )
            {
                // Move file to Temporary Folder before copy to server
                Page.LogViewer.sLogMessage = "Start to Transfer File to Buffer Folder";
                App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eApp, ("Start to Transfer File to Buffer Folder"));
                //Task.Delay(2);
                if (!IsDirectoryEmpty(strOriginFolder))
                {
                    try
                    {
                        MoveFilesRecursively(strOriginFolder, strTempFolder);
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eLocaltoBuffer, "From: " + strOriginFolder.ToString()
                            + "To: " + strTempFolder.ToString());
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Error Moving file to Temp Folder");
                        Page.LogViewer.sLogMessage = "Error Moving file to Temp Folder -- Buffer use";
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eLocaltoBuffer, ("Error Moving file to Temp Folder"));
                    }
                }

                Page.LogViewer.sLogMessage = "Moving File to Buffer Folder Completed -- Buffer use";
                App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eLocaltoBuffer, ("Moving File to Buffer Folder Completed"));

                //Copy from Buffer to Server
                if (!IsDirectoryEmpty(strTempFolder))
                {
                    try
                    {
                        bErrorCopyFile = false;
                        CopyFilesRecursively(strTempFolder, strDestFolder);
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eBuffertoServer, "From: " + strTempFolder.ToString()
                            + "To: " + strDestFolder.ToString());
                    }
                    catch (IOException)
                    {
                        bErrorCopyFile = true;
                        Page.LogViewer.sLogMessage = "Error to move file to HP Server Drive";
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eBuffertoServer, ("Error to move file to HP Server Drive"));
                    }
                    Page.LogViewer.sLogMessage = "Attemp to Copy to HP Server";
                    App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eBuffertoServer, ("Attemp to Copy to HP Server"));

                    //Delete File if Copy successful 
                    if (bErrorCopyFile == false)
                    {
                        DeleteAllSubfolder(strTempFolder);
                        Page.LogViewer.sLogMessage = "Moving File to HP Server Drive Completed, Delete Buffer Folder File -- Buffer use";
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eBuffertoServer, ("Moving File to HP Server Drive Completed, Delete Buffer Folder File"));
                    }
                    else if (bErrorCopyFile == true)
                    {
                        Page.LogViewer.sLogMessage = "Moving File to HP Server Drive fail, Buffer Folder File will not be deleted -- Buffer use";
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eBuffertoServer,
                            ("Moving File to HP Server Drive fail, Buffer Folder File will not be deleted"));
                    }
                }
            }
            else if (Setting.cbBufferFolderEnable.IsChecked == false)
            {
                // Move file to Temporary Folder before copy to server
                Page.LogViewer.sLogMessage = "Start to Transfer File to Buffer Folder";
                App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eApp, ("Start to Transfer File to Buffer Folder"));
                //Task.Delay(2);
                if (!IsDirectoryEmpty(strOriginFolder))
                {
                    try
                    {
                        MoveFilesRecursively(strOriginFolder, strTempFolder);
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eLocaltoBuffer, "From: " + strOriginFolder.ToString()
                            + "To: " + strTempFolder.ToString());
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Error Moving file to Temp Folder");
                        Page.LogViewer.sLogMessage = "Error Moving file to Temp Folder - Buffer not use";
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eLocaltoBuffer, ("Error Moving file to Temp Folder"));
                    }
                }

                Page.LogViewer.sLogMessage = "Moving File to Buffer Folder Completed -- Buffer not use";
                App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eLocaltoBuffer, ("Moving File to Buffer Folder Completed"));

                // Copy from Buffer to Server
                if (!IsDirectoryEmpty(strTempFolder))
                {
                    try
                    {
                        bErrorCopyFile = false;
                        CopyFilesRecursively(strTempFolder, strDestFolder);
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eBuffertoServer, "From: " + strTempFolder.ToString()
                            + "To: " + strDestFolder.ToString());
                    }
                    catch (IOException)
                    {
                        bErrorCopyFile = true;
                        Page.LogViewer.sLogMessage = "Error to move file to HP Server Drive";
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eBuffertoServer, ("Error to move file to HP Server Drive"));
                    }
                    Page.LogViewer.sLogMessage = "Attemp to Copy to HP Server";
                    App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eBuffertoServer, ("Attemp to Copy to HP Server"));

                    //Delete buffer if copy is successful
                    if (bErrorCopyFile == false)
                    {
                        DeleteAllSubfolder(strTempFolder);
                        Page.LogViewer.sLogMessage = "Moving File to HP Server Drive Completed, Delete Buffer Folder File -- Buffer not use";
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eBuffertoServer, ("Moving File to HP Server Drive Completed, Delete Buffer Folder File"));
                    }
                    else if (bErrorCopyFile == true)
                    {
                        Page.LogViewer.sLogMessage = "Moving File to HP Server Drive fail, Buffer Folder File will not be deleted -- Buffer not use";
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eBuffertoServer,
                            ("Moving File to HP Server Drive fail, Buffer Folder File will not be deleted"));
                    }
                }
            }
            else
            {
                // Move file to Temporary Folder before copy to server
                Page.LogViewer.sLogMessage = "Start to Transfer File to Buffer Folder";
                App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eApp, ("Start to Transfer File to Buffer Folder"));
                //Task.Delay(2);
                if (!IsDirectoryEmpty(strOriginFolder))
                {
                    try
                    {
                        MoveFilesRecursively(strOriginFolder, strTempFolder);
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eLocaltoBuffer, "From: " + strOriginFolder.ToString()
                            + "To: " + strTempFolder.ToString());
                    }
                    catch (IOException)
                    {
                        MessageBox.Show("Error Moving file to Temp Folder");
                        Page.LogViewer.sLogMessage = "Error Moving file to Temp Folder";
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eLocaltoBuffer, ("Error Moving file to Temp Folder"));
                    }
                }

                Page.LogViewer.sLogMessage = "Moving File to Buffer Folder Completed";
                App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eLocaltoBuffer, ("Moving File to Buffer Folder Completed"));

                if (!IsDirectoryEmpty(strTempFolder))
                {
                    try
                    {
                        bErrorCopyFile = false;
                        CopyFilesRecursively(strTempFolder, strDestFolder);
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eBuffertoServer, "From: " + strTempFolder.ToString()
                            + "To: " + strDestFolder.ToString());
                    }
                    catch (IOException)
                    {
                        bErrorCopyFile = true;
                        Page.LogViewer.sLogMessage = "Error to move file to HP Server Drive";
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eBuffertoServer, ("Error to move file to HP Server Drive"));
                    }
                    Page.LogViewer.sLogMessage = "Attemp to Copy to HP Server";
                    App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eBuffertoServer, ("Attemp to Copy to HP Server"));

                    if (bErrorCopyFile == false)
                    {
                        DeleteAllSubfolder(strTempFolder);
                        Page.LogViewer.sLogMessage = "Moving File to HP Server Drive Completed, Delete Buffer Folder File";
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eBuffertoServer, ("Moving File to HP Server Drive Completed, Delete Buffer Folder File"));
                    }
                    else if (bErrorCopyFile == true)
                    {
                        Page.LogViewer.sLogMessage = "Moving File to HP Server Drive fail, Buffer Folder File will not be deleted";
                        App.pUILog.WriteLog(EventLog.UIEventLogData.eEventLogCategory.eBuffertoServer,
                            ("Moving File to HP Server Drive fail, Buffer Folder File will not be deleted"));
                    }
                }
            }

            bTransfering = false;
        }

        private void RefreshTimer_tick(object sender, EventArgs e)
        {
            DisplayTime.Text = DateTime.Now.ToString(" yyyy/MMMM/dd  HH:mm:ss:fff ");

            if(logviewer.LogEntries.Count >= dClearLogNumber)
            {
                logviewer.LogEntries.Clear();
            }
        }

        #region Log File Housekeeping
        private void HousekeepingTimer_tick(object sender, EventArgs e)
        {
            string strCurrentEventlogDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, "EventLog");
            string[] Directories = Directory.GetDirectories(strCurrentEventlogDirectory);
            Page.LogViewer.sLogMessage = "Time for Housekeeping";
            foreach (string dir in Directories)
            {
                DirectoryInfo dirinfo = new DirectoryInfo(dir);
                //if (dirinfo.LastAccessTime < DateTime.Now.AddDays(-7))
                if (dirinfo.CreationTime < DateTime.Now.AddDays(-iLogFileExpire))
                    dirinfo.Delete(true);
            }
            Page.LogViewer.sLogMessage = "Housekeeping Done";
            strLastHousekeepingDateTime = DateTime.Now.ToString(" yyyy/MMMM/dd  HH:mm:ss:fff ");
            Setting.txtLastClearLogTime.Text = strLastHousekeepingDateTime;
        }

        public void ClearLog()
        {
            string strCurrentEventlogDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, "EventLog");
            string[] Directories = Directory.GetDirectories(strCurrentEventlogDirectory);
            Page.LogViewer.sLogMessage = "Time for Housekeeping";
            foreach (string dir in Directories)
            {
                DirectoryInfo dirinfo = new DirectoryInfo(dir);
                //if (dirinfo.LastAccessTime < DateTime.Now.AddDays(-7))
                if (dirinfo.CreationTime < DateTime.Now.AddDays(-iLogFileExpire))
                    dirinfo.Delete(true);
            }
            Page.LogViewer.sLogMessage = "Housekeeping Done";
            strLastHousekeepingDateTime = DateTime.Now.ToString(" yyyy/MMMM/dd  HH:mm:ss:fff ");
            Setting.txtLastClearLogTime.Text = strLastHousekeepingDateTime;
        }

        #endregion

        #region Windows Loading and Closing
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Show/Update Config on Screen
            txtOriginFolder.Text = strOriginFolder;
            txtDestFolder.Text = strDestFolder;
            txtTemporaryFolder.Text = strTempFolder;
            txtCopyInterval.Text = strTimeInterval;
            Page.LogViewer.sLogMessage = strOriginFolder;
            Page.LogViewer.sLogMessage = strDestFolder;
            Page.LogViewer.sLogMessage = strTempFolder;
            Page.LogViewer.sLogMessage = strTimeInterval;

            ClearLog();

            //Not used as might conflct with windows 10 way of handling process
            //Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if(App.bDuplicateApp == false && bTransfering == false)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(" Warning!!! Close this Application will stop transfer data to server!!!!!\n " +
               "Are you sure you want to close the application?", "Close Application",
               MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);

                if (result == MessageBoxResult.Yes)
                {
                    logviewer.bMainWindowClosed = true;
                    logviewer.Show();
                    logviewer.Visibility = Visibility.Visible;
                    logviewer.Show();
                    logviewer.Close();
                    About.Show();
                    About.Visibility = Visibility.Visible;
                    About.Show();
                    About.Close();
                    Setting.Show();
                    Setting.Visibility = Visibility.Visible;
                    Setting.Show();
                    Setting.Close();



                    ConfigFile.IniWriteValue("Folder", "OriginPath", strOriginFolder);
                    ConfigFile.IniWriteValue("Folder", "DestinationPath", strDestFolder);
                    ConfigFile.IniWriteValue("Folder", "CopyInterval", strTimeInterval);
                    ConfigFile.IniWriteValue("Folder", "TemporaryPath", strTempFolder);
                    MainWindow.ConfigFile.IniWriteValue("Log", "iClearlogPeriod", iClearlogPeriod.ToString());
                    MainWindow.ConfigFile.IniWriteValue("Log", "iLogFileExpire", iLogFileExpire.ToString());
                    MainWindow.ConfigFile.IniWriteValue("Log", "dClearLogNumber", dClearLogNumber.ToString());//dClearLogNumber

                    App.Current.Shutdown();
                }
                else
                {
                    e.Cancel = true;
                }
            }
            if (App.bDuplicateApp == true)
            {
                e.Cancel = true;
            }




        }
        #endregion

        #region load config 
        public void loadConfig()
        {
            strOriginFolder = ConfigFile.IniReadValue("Folder", "OriginPath");
            strDestFolder = ConfigFile.IniReadValue("Folder", "DestinationPath");
            strTempFolder = ConfigFile.IniReadValue("Folder", "TemporaryPath");
            strTimeInterval = ConfigFile.IniReadValue("Folder", "CopyInterval");
            iClearlogPeriod = Convert.ToInt32(ConfigFile.IniReadValue("Log", "iClearlogPeriod"));
            iLogFileExpire = Convert.ToInt32(ConfigFile.IniReadValue("Log", "iLogFileExpire")); 
            dClearLogNumber = Convert.ToInt32(ConfigFile.IniReadValue("Log", "dClearLogNumber"));
        }
        #endregion

        #region Button to Set Folder & copy interval
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var Dialog = new CommonOpenFileDialog();
            Dialog.Title = "Origin Folder";
            Dialog.IsFolderPicker = true;
            Dialog.InitialDirectory = currentDirectory;

            Dialog.AddToMostRecentlyUsedList = false;
            Dialog.AllowNonFileSystemItems = false;
            Dialog.DefaultDirectory = currentDirectory;
            Dialog.EnsureFileExists = true;
            Dialog.EnsurePathExists = true;
            Dialog.EnsureReadOnly = false;
            Dialog.EnsureValidNames = true;
            Dialog.Multiselect = false;
            Dialog.ShowPlacesList = true;

            if (Dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                strOriginFolder = Dialog.FileName.ToString();
                var folder = Dialog.FileName;
                // Do something with selected folder string
                txtOriginFolder.Text = strOriginFolder;
                ConfigFile.IniWriteValue("Folder", "OriginPath", strOriginFolder);
                Page.LogViewer.sLogMessage = "Origin Folder Path Selected: " + strOriginFolder;
            }
        }

        private void btnDestinationFolder_Click(object sender, RoutedEventArgs e)
        {
            var Dialog = new CommonOpenFileDialog();
            Dialog.Title = "Destinition Folder";
            Dialog.IsFolderPicker = true;
            Dialog.InitialDirectory = currentDirectory;

            Dialog.AddToMostRecentlyUsedList = false;
            Dialog.AllowNonFileSystemItems = false;
            Dialog.DefaultDirectory = currentDirectory;
            Dialog.EnsureFileExists = true;
            Dialog.EnsurePathExists = true;
            Dialog.EnsureReadOnly = false;
            Dialog.EnsureValidNames = true;
            Dialog.Multiselect = false;
            Dialog.ShowPlacesList = true;

            if (Dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                strDestFolder = Dialog.FileName.ToString();
                var folder = Dialog.FileName;
                // Do something with selected folder string
                txtDestFolder.Text = strDestFolder;
                ConfigFile.IniWriteValue("Folder", "DestinationPath", strDestFolder);
                Page.LogViewer.sLogMessage = "Destination Folder Path Selected: " + strDestFolder;
            }
        }

        private void btnTemporaryFolder_Click(object sender, RoutedEventArgs e)
        {
            var Dialog = new CommonOpenFileDialog();
            Dialog.Title = "Temporary Folder";
            Dialog.IsFolderPicker = true;
            Dialog.InitialDirectory = currentDirectory;

            Dialog.AddToMostRecentlyUsedList = false;
            Dialog.AllowNonFileSystemItems = false;
            Dialog.DefaultDirectory = currentDirectory;
            Dialog.EnsureFileExists = true;
            Dialog.EnsurePathExists = true;
            Dialog.EnsureReadOnly = false;
            Dialog.EnsureValidNames = true;
            Dialog.Multiselect = false;
            Dialog.ShowPlacesList = true;

            if (Dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                strTempFolder = Dialog.FileName.ToString();
                var folder = Dialog.FileName;
                // Do something with selected folder string
                txtTemporaryFolder.Text = strTempFolder;
                ConfigFile.IniWriteValue("Folder", "TemporaryPath", strTempFolder);
                Page.LogViewer.sLogMessage = "Temporary Folder Path Selected: " + strTempFolder;
            }
        }

       

        private void btnSetInterval_Click(object sender, RoutedEventArgs e)
        {
            int parsedValue;
            if (txtCopyInterval.Text.Length != 0)
            {
                if(int.TryParse(txtCopyInterval.Text, out parsedValue))
                {
                    strTimeInterval = txtCopyInterval.Text;
                    ConfigFile.IniWriteValue("Folder", "CopyInterval", strTimeInterval);
                    Page.LogViewer.sLogMessage = "Time Interval Set: " + strTimeInterval;
                }
                else
                {
                    txtCopyInterval.Text = "Invalid input, Please Enter a Number";
                }
                
            }
            else
            {
                txtCopyInterval.Text = "Cannot Set Empty Interval!!";
            }
        }
        #endregion

        #region Move file function
        private static void MoveFilesRecursively(string sourcePath, string targetPath)
        {

            //Now Create all of the directories
            foreach (string dirPath in System.IO.Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                System.IO.Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in System.IO.Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Move(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {

            //Now Create all of the directories
            foreach (string dirPath in System.IO.Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                System.IO.Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in System.IO.Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private static void DeleteAllSubfolder(string sourcePath)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(sourcePath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public bool IsDirectoryEmpty(string path)
        {
            IEnumerable<string> items = Directory.EnumerateFileSystemEntries(path);
            using (IEnumerator<string> en = items.GetEnumerator())
            {
                return !en.MoveNext();
            }
        }

        #endregion
        

        #region Open new Window Function
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            
            About.Show();
        }
        private void LogViewer_Click(object sender, RoutedEventArgs e)
        {
            
            logviewer.Show();
        }
        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            logviewer.LogEntries.Clear();
        }
        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            Setting.Show();
            
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();
            
            

        }
        #endregion

        #region Debug function
        private void btnTransferNow_Click(object sender, RoutedEventArgs e)
        {
            if (!IsDirectoryEmpty(strOriginFolder))
            {
                try
                {
                    MoveFilesRecursively(strOriginFolder, strTempFolder);
                }
                catch (IOException)
                {
                    MessageBox.Show("Error Moving file to Temp Folder");
                }
            }


            if (!IsDirectoryEmpty(strTempFolder))
            {
                try
                {
                    bErrorCopyFile = false;
                    CopyFilesRecursively(strTempFolder, strDestFolder);
                }
                catch (IOException)
                {
                    bErrorCopyFile = true;
                }

                if (bErrorCopyFile == false)
                {
                    DeleteAllSubfolder(strTempFolder);
                }
            }
        }
        #endregion
    }
}
