using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Client.AlarmServiceReference;
using Client.ViewModel;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Random rnd = new Random(DateTime.Now.Millisecond);
        private static int clientID = rnd.Next(1, 10000);

        static CancellationTokenSource cancellation = new CancellationTokenSource();

        static AlarmServiceClient client = new AlarmServiceClient();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindowViewModel();

            Console.WriteLine("Client id: " + clientID);
            Console.WriteLine("The client is running.");
            Console.WriteLine("The client will send random alarms ever second.");
            Console.WriteLine();
            Run();
        }

        static async void Run()
        {
            await RepeatActionEvery(Action, TimeSpan.FromSeconds(1), cancellation.Token);
        }

        private static void Action()
        {
            client.ActivateAlarm(clientID, "Sam");
            Console.WriteLine("Alarm Sent.");
        }

        public static async Task RepeatActionEvery(Action action, TimeSpan interval, CancellationToken cancellationToken)
        {
            while (true)
            {
                action();
                Task task = Task.Delay(interval, cancellationToken);

                try
                {
                    await task;
                }

                catch (TaskCanceledException)
                {
                    Console.WriteLine("Alarm Task Cancel");
                    return;
                }
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            client.Close();
        }
    }
}
