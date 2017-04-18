using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Client.AlarmServiceReference;

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
