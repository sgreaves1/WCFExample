using System;
using System.Threading;
using System.Threading.Tasks;
using Client.AlarmServiceReference;

namespace Client.ViewModel
{
    public class MainWindowViewModel : BaseModel
    {
        private static Random rnd = new Random(DateTime.Now.Millisecond);
        private static int clientID = rnd.Next(1, 10000);
        
        static CancellationTokenSource cancellation = new CancellationTokenSource();

        static AlarmServiceClient client = new AlarmServiceClient();

        public MainWindowViewModel()
        {
            Run();
        }

        ~MainWindowViewModel()
        {
            client.Close();
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
    }
}
