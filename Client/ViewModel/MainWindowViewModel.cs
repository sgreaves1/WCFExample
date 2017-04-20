using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Client.AlarmServiceReference;
using Client.Model;
using TextLoggingPackage;

namespace Client.ViewModel
{
    public class MainWindowViewModel : BaseModel
    {
        private static Random rnd = new Random(DateTime.Now.Millisecond);
        private int _clientID = rnd.Next(1, 10000);
        
        CancellationTokenSource cancellation = new CancellationTokenSource();

        AlarmServiceClient client = new AlarmServiceClient();

        private ObservableCollection<ServiceModel> _services = new ObservableCollection<ServiceModel>();

        public MainWindowViewModel()
        {
            Logger.ApplicationLoggingLevel = LoggingLevel.Trace;
            ReadAllSettings();
            Run();
        }

        ~MainWindowViewModel()
        {
            try
            {
                client.Close();
            }
            catch (Exception ex)
            {
                Logger.Log("Service threw an exception: " + ex.Message, "WCF Client App", LoggingLevel.Trace);
            }
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
                        
                        Services.Add(new ServiceModel() {Name = "Host", EndpointAddress = appSettings[key], Connected = false});
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
            try
            {
                client.ActivateAlarm(ClientID, "Sam");
                Logger.Log("Alarm Sent, Name: " + "Sam", "WCF Client App", LoggingLevel.Trace);
                Services.First().Connected = true;
            }
            catch (Exception)
            {
                Logger.Log("Not connected to WCF host." , "WCF Client App", LoggingLevel.Trace);
                Services.First().Connected = false;
                cancellation.Cancel();
            }
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

        public ObservableCollection<ServiceModel> Services
        {
            get { return _services; }
            set
            {
                _services = value;
                OnPropertyChanged();
            }
        } 
    }
}
