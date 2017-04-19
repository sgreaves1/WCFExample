using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Client.AlarmServiceReference;
using TextLoggingPackage;

namespace Client.ViewModel
{
    public class MainWindowViewModel : BaseModel
    {
        private static Random rnd = new Random(DateTime.Now.Millisecond);
        private int _clientID = rnd.Next(1, 10000);
        
        CancellationTokenSource cancellation = new CancellationTokenSource();

        AlarmServiceClient client = new AlarmServiceClient();

        private ObservableCollection<string> endpointAddress = new ObservableCollection<string>(); 

        public MainWindowViewModel()
        {
            Logger.ApplicationLoggingLevel = LoggingLevel.Trace;
            ReadAllSettings();
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

        void ReadAllSettings()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if (appSettings.Count == 0)
                {
                    Logger.Log("No App settings.", "WCF Client App", LoggingLevel.Trace);
                }
                else
                {
                    foreach (var key in appSettings.AllKeys)
                    {
                        Logger.Log("Key: "+ key +" Value: " + appSettings[key], "WCF Client App", LoggingLevel.Trace);
                        
                        endpointAddress.Add(appSettings[key]);
                    }
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                Logger.Log("Configuration Manager threw an exception: " + ex.Message, "WCF Client App", LoggingLevel.Trace);
            }
        }

        private void Action()
        {
            client.ActivateAlarm(ClientID, "Sam");
            Logger.Log("Alarm Sent, Name: " + "Sam", "WCF Client App", LoggingLevel.Trace);
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
