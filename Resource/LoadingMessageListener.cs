using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;

namespace FileTransfer.Resource
{
    public class LoadingMessageListener : DependencyObject
    {
        private static LoadingMessageListener mInstance;

        private LoadingMessageListener() { }

        public static LoadingMessageListener Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new LoadingMessageListener();
                return mInstance;
            }
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public void ReceiveMessage(string message)
        {
            Message = message;
            Debug.WriteLine(Message);
            DispatcherHelper.DoEvents();
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(LoadingMessageListener), new UIPropertyMetadata(null));
    }
}
