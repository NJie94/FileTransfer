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
//using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;
using System.Globalization;

namespace FileTransfer.Page
{
    /// <summary>
    /// Interaction logic for LogViewer.xaml
    /// </summary>
    /// <summary>
    /// Interaction logic for LogViewer.xaml
    /// </summary>
    /// 
    public class LogEntry : PropertyChangedBase
    {
        //public DateTime DateTime { get; set; }
        public string DateTime { get; set; }

        public int Index { get; set; }

        public string Message { get; set; }
    }

    public class CollapsibleLogEntry : LogEntry
    {
        public List<LogEntry> Contents { get; set; }
    }

    public class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs(propertyName));
            }));
        }
    }






    public partial class LogViewer : Window
    {
        private string data;
        private List<string> words;
        private int maxword;
        private int index;
        private string PrevMessage = "";
        private bool AutoScroll = true;
        public System.Timers.Timer DisplayTimer;
        private System.Threading.Timer Timer;
        public static string sLogMessage = "";

        //Set Main Window Closed
        public bool bMainWindowClosed = false;

        public ObservableCollection<LogEntry> LogEntries { get; set; }

        public LogViewer()
        {
            InitializeComponent();
            DataContext = LogEntries = new ObservableCollection<LogEntry>();


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            DisplayTimer = new System.Timers.Timer();
            DisplayTimer.Interval = 1;
            DisplayTimer.Elapsed += new System.Timers.ElapsedEventHandler(DisplayTimer_Elapsed);
            DisplayTimer.Enabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(bMainWindowClosed == false)
            {
                e.Cancel = true;
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                DisplayTimer.Enabled = false;
                DisplayTimer.Stop();
                DisplayTimer.Close();
                
            }
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
            //DisplayTimer.Enabled = false;
            //DisplayTimer.Stop();
            //DisplayTimer.Close();
            
            
        }

        void DisplayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(
                    delegate () {
                        if (sLogMessage != PrevMessage)
                        {
                            LogEntries.Add(GetLogEntry());
                            //AddLogEntry();
                            sLogMessage = PrevMessage;
                        }

                    }));

        }

        private void AddLogEntry()
        {
            Dispatcher.BeginInvoke((Action)(() => LogEntries.Add(GetLogEntry())));
        }

        private LogEntry GetLogEntry()
        {

            return new LogEntry
            {
                Index = index++,
                DateTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss:fff", CultureInfo.InvariantCulture),
                Message = sLogMessage,
            };
        }


        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset autoscroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if ((e.Source as ScrollViewer).VerticalOffset == (e.Source as ScrollViewer).ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set autoscroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset autoscroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : autoscroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and autoscroll mode set
                // Autoscroll
                (e.Source as ScrollViewer).ScrollToVerticalOffset((e.Source as ScrollViewer).ExtentHeight);
            }
        }

        
    }

}
