using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace IVySoft.VDS.Client.UI.WPF.Common
{
    /// <summary>
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private CancellationTokenSource CancellationSource;
        private Task Task;

        public string Message { get; private set; }

        private DispatcherTimer Timer;

        public ProgressWindow()
        {
            InitializeComponent();

            this.Timer = new DispatcherTimer();
            this.Timer.Tick += check_task;
            this.Timer.Interval = TimeSpan.FromSeconds(3);
            this.Timer.Start();
        }

        private void check_task(object sender, EventArgs e)
        {
            if (this.Task.IsCompleted)
            {
                this.Timer.Stop();
                this.DialogResult = true;
            }
        }

        private static Queue<Action> task_queue_ = new Queue<Action>();
        private static Thread work_thread_;
        public static void Run(string message, Window parent, Func<CancellationToken, Task> action)
        {
            lock (task_queue_)
            {
                task_queue_.Enqueue(() => run_task(message, parent, action));
                if(null == work_thread_)
                {
                    work_thread_ = new Thread(new ThreadStart(work_thread));
                    work_thread_.Start();
                }
            }
        }

        private static void work_thread()
        {
            for(; ; )
            {
                Action action;
                lock (task_queue_)
                {
                    if(task_queue_.Count == 0)
                    {
                        work_thread_ = null;
                        return;
                    }
                    action = task_queue_.Dequeue();
                }

                try
                {
                    action();
                }
                catch
                {
                }
            }
        }

        private static void run_task(string message, Window parent, Func<CancellationToken, Task> action)
        {
            var source = new CancellationTokenSource();
            var task = action(source.Token);

            if (!task.Wait(TimeSpan.FromSeconds(10)))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(()=>
                {
                    var dlg = new ProgressWindow();
                    dlg.Owner = parent;
                    dlg.CancellationSource = source;
                    dlg.Task = task;
                    dlg.Message = message;
                    dlg.ShowDialog();
                });

                task.Wait();
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.CancelBtn.IsEnabled = false;
            this.CancellationSource.Cancel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.TaskMessage.Content = $"Please wait for complete operation {this.Message}...";
        }
    }
}
