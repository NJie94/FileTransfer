using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace FileTransfer.Page
{
    /// <summary>
    /// Interaction logic for Setting.xaml
    /// </summary>
    public partial class Setting : Window
    {
        public Setting()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            strHousekeepingPeriod.Text = MainWindow.iClearlogPeriod.ToString();
            strHousekeepingExpire.Text = MainWindow.iLogFileExpire.ToString();
            txtLastClearLogTime.Text = MainWindow.strLastHousekeepingDateTime;
            strLogEntries.Text = MainWindow.dClearLogNumber.ToString();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
           
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {


            int parsedValue;
            if (strHousekeepingPeriod.Text.Length != 0)
            {
                if (int.TryParse(strHousekeepingPeriod.Text, out parsedValue))
                {
                    MainWindow.iClearlogPeriod = Convert.ToInt32(strHousekeepingPeriod.Text);
                    MainWindow.ConfigFile.IniWriteValue("Log", "iClearlogPeriod", MainWindow.iClearlogPeriod.ToString());
                    Page.LogViewer.sLogMessage = "Clear Log Period Set: " + MainWindow.iClearlogPeriod.ToString();
                }
                else
                {
                    strHousekeepingPeriod.Text = "Invalid input, Please Enter a Number";
                }

            }
            else
            {
                strHousekeepingPeriod.Text = "Cannot Set Empty Value!!";
            }

            int parsedValue1;
            if (strHousekeepingExpire.Text.Length != 0)
            {
                if (int.TryParse(strHousekeepingExpire.Text, out parsedValue1))
                {
                    MainWindow.iLogFileExpire = Convert.ToInt32(strHousekeepingExpire.Text);
                    MainWindow.ConfigFile.IniWriteValue("Log", "iLogFileExpire", MainWindow.iLogFileExpire.ToString());
                    Page.LogViewer.sLogMessage = "Clear Log Period Set: " + MainWindow.iLogFileExpire.ToString();
                }
                else
                {
                    strHousekeepingExpire.Text = "Invalid input, Please Enter a Number";
                }

            }
            else
            {
                strHousekeepingExpire.Text = "Cannot Set Empty Value!!";
            }

            int parsedValue2;
            if (strLogEntries.Text.Length != 0)
            {
                if (int.TryParse(strLogEntries.Text, out parsedValue2))
                {
                    MainWindow.dClearLogNumber = Convert.ToInt32(strLogEntries.Text);
                    MainWindow.ConfigFile.IniWriteValue("Log", "dClearLogNumber", MainWindow.dClearLogNumber.ToString());
                    Page.LogViewer.sLogMessage = "Clear Log Entries number Set: " + MainWindow.dClearLogNumber.ToString();
                }
                else
                {
                    strLogEntries.Text = "Invalid input, Please Enter a Number";
                }

            }
            else
            {
                strLogEntries.Text = "Cannot Set Empty Value!!";
            }

            int parseSave1;
            int parseSave2;
            int parseSave3;
            if (int.TryParse(strHousekeepingPeriod.Text, out parseSave1) && int.TryParse(strHousekeepingExpire.Text, out parseSave2)
                && int.TryParse(strLogEntries.Text, out parseSave3))
            {
                this.Visibility = Visibility.Hidden;
            }
                
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            strHousekeepingPeriod.Text = MainWindow.iClearlogPeriod.ToString();
            strHousekeepingExpire.Text = MainWindow.iLogFileExpire.ToString();
            strLogEntries.Text = MainWindow.dClearLogNumber.ToString();

            this.Visibility = Visibility.Hidden;
        }
    }
    
}
