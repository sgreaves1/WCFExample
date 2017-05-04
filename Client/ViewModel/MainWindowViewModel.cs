using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Client.AlarmServiceReference;
using Client.Container;
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

        private ObservableCollection<ServiceModel> _services = new ObservableCollection<ServiceModel>();

        private ServiceModel CurrentService;

        private static ILog log = LogManager.GetLogger<MainWindowViewModel>();

        private string[] _names = new[] {"Sam", "Steve", "Ray", "Keeno", "Gob"};

        private bool _sending;

        private int _seconds;

        private Deque<string> _messages = new Deque<string>();

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

        async void Run()
        {
            FindWorkingHost();
            await IntervalMessageSending(SendMessage, TimeSpan.FromSeconds(Seconds), cancellation.Token);
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
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                log.Trace("Configuration Manager threw an exception: " + ex.Message);
            }
        }

        private int msgNumber = 0;

        private void SendMessage()
        {
            try
            {
                if (CurrentService != null)
                {
                    msgNumber++;
                    string msg = _names[rnd.Next(0, _names.Length)];
                    _messages.AddToBack(msg + " " + msgNumber);

                    CurrentService.Client.ActivateAlarm(ClientID, _messages.First());
                    _messages.RemoveFromFront();
                    log.Trace("Alarm Sent, Name: " + msg + " " + msgNumber);
                }

            }
            catch (Exception ex)
            {
                log.Trace("Not connected to WCF host. " + ex.Message);
                CurrentService.ConnectionState = ConnectionStatus.Disconnected;
                cancellation.Cancel();
            }
        }

        public void FindWorkingHost()
        {
            foreach (var service in Services)
            {
                Task.Run(() =>
                {
                    if (service.TryConnect())
                    {
                        CurrentService = service;
                    }
                });
            }
        }

        public async Task IntervalMessageSending(Action sendMessageAction, TimeSpan interval, CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        sendMessageAction();
                        await Task.Delay(interval, cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        cancellation.Dispose();
                        cancellation = new CancellationTokenSource();
                        cancellationToken = cancellation.Token;
                    }
                }
            }, cancellationToken);
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
