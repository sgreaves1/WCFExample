using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Client.AlarmServiceReference;
using Client.Enumerator;
using Client.Model;
using Common.Logging;
using MyLibrary.Command;

namespace Client.ViewModel
{
    public class MainWindowViewModel : BaseModel
    {
        private static Random rnd = new Random(DateTime.Now.Millisecond);
        private int _clientID = rnd.Next(1, 10000);
        
        CancellationTokenSource cancellation = new CancellationTokenSource();

        private AlarmServiceClient client;

        private ObservableCollection<ServiceModel> _services = new ObservableCollection<ServiceModel>();

        private IEnumerator<ServiceModel> CurrentService;

        private static ILog log = LogManager.GetLogger<MainWindowViewModel>();

        private string[] _names = new[] {"Sam", "Steve", "Ray", "Keeno", "Gob"};

        private bool _sending;

        private int _seconds;

        public MainWindowViewModel()
        {
            Seconds = 10;

            InitCommands();

            ReadAllSettings();
        }

        private void InitCommands()
        {
            SendingButtonCommand = new RelayCommand(() =>
            {
                Sending = !Sending;
                Run();
            });
        }

        ~MainWindowViewModel()
        {
            try
            {
                client.Close();
            }
            catch (Exception ex)
            {
                log.Trace("Service threw an exception: " + ex.Message);
            }
        }

        async void Run()
        {
            await RepeatActionEvery(Action, TimeSpan.FromSeconds(Seconds), cancellation.Token);
        }

        void ReadAllSettings()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if (appSettings.Count == 0)
                {
                    log.Trace("No App settings.");
                }
                else
                {
                    foreach (var key in appSettings.AllKeys)
                    {
                        log.Trace("Key: " + key + " Value: " + appSettings[key]);

                        Services.Add(new ServiceModel() {Name = "Host", EndpointAddress = appSettings[key]});
                    }

                    CurrentService = Services.GetEnumerator();
                    CurrentService.MoveNext();
                    client = new AlarmServiceClient("WSHttpBinding_IAlarmService", CurrentService.Current.EndpointAddress);
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                log.Trace("Configuration Manager threw an exception: " + ex.Message);
            }
        }

        private int msgNumber = 0;

        private void Action()
        {
            try
            {
                msgNumber++;
                string msg = _names[rnd.Next(0, _names.Length)];

                client.ActivateAlarm(ClientID, msg + " " + msgNumber);
                log.Trace("Alarm Sent, Name: " + msg + " " + msgNumber);
                CurrentService.Current.ConnectionState = ConnectionStatus.Connected;

            }
            catch (Exception ex)
            {
                log.Trace("Not connected to WCF host.");
                CurrentService.Current.ConnectionState = ConnectionStatus.Disconnected;
                cancellation.Cancel();
            }
        }

        public async Task RepeatActionEvery(Action action, TimeSpan interval, CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(async () =>
            {
                CurrentService.Current.ConnectionState = ConnectionStatus.Attempting;

                while (true)
                {
                    try
                    {
                        action();
                        await Task.Delay(interval, cancellationToken);

                    }
                    catch (TaskCanceledException)
                    {
                        Console.WriteLine("Alarm Task Cancel");

                        GetNextService();
                        cancellation.Dispose();
                        cancellation = new CancellationTokenSource();
                        cancellationToken = cancellation.Token;

                        await Task.Delay(interval);
                    }
                }
            }, cancellationToken);
        }

        private void GetNextService()
        {
            if (!CurrentService.MoveNext())
            {
                CurrentService.Reset();
                CurrentService.MoveNext();
            }

            CurrentService.Current.ConnectionState = ConnectionStatus.Attempting;
            client = new AlarmServiceClient("WSHttpBinding_IAlarmService", CurrentService.Current.EndpointAddress);
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

        public bool Sending
        {
            get { return _sending; }
            set
            {
                _sending = value;
                OnPropertyChanged();
            }
        }

        public int Seconds
        {
            get { return _seconds;}
            set
            {
                _seconds = value;
                OnPropertyChanged();
            }
        }

        public ICommand SendingButtonCommand { get; set; }
    }
}
