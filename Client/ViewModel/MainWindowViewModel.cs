using System;
using System.Threading;
using System.Threading.Tasks;
using Client.AlarmServiceReference;

namespace Client.ViewModel
{
    public class MainWindowViewModel : BaseModel
    {
        private static Random rnd = new Random(DateTime.Now.Millisecond);
        private int _clientID = rnd.Next(1, 10000);
        
        CancellationTokenSource cancellation = new CancellationTokenSource();

        AlarmServiceClient client = new AlarmServiceClient();

        public MainWindowViewModel()
        {
            Run();
        }

        ~MainWindowViewModel()
        {
            client.Close();
        }

        async void Run()
        {
            await RepeatActionEvery(Action, TimeSpan.FromSeconds(1), cancellation.Token);
        }

        private void Action()
        {
            client.ActivateAlarm(ClientID, "Sam");
            Console.WriteLine("Alarm Sent.");
        }

        public async Task RepeatActionEvery(Action action, TimeSpan interval, CancellationToken cancellationToken)
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

        public int ClientID
        {
            get { return _clientID; }
            set
            {
                _clientID = value;
                OnPropertyChanged();
            }
        }
    }
}
