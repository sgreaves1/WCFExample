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

        private static TaskCompletionSource<int> tcs1 = new TaskCompletionSource<int>();
        private Task<int> t1 = tcs1.Task;

        public MainWindowViewModel()
        {
            Seconds = 1;

            InitCommands();

            ReadAllSettings();

            ProcessMessages();
        }

        private void InitCommands()
        {
            SendingButtonCommand = new RelayCommand(() =>
            {
                Sending = !Sending;
                Run();
            });
        }

        private void ProcessMessages()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await t1;

                    lock (_messages)
                    {
                        if (_messages.Count > 0 && CurrentService != null)
                        {
                            try
                            {
                                CurrentService.Client.ActivateAlarm(ClientID, _messages.First());
                                log.Trace("Alarm Sent, Name: " + _messages.First());
                                _messages.RemoveFromFront();
                            }
                            catch (Exception ex)
                            {
                                log.Trace("Not connected to WCF host. " + ex.Message);
                                if (CurrentService != null) CurrentService.ConnectionState = ConnectionStatus.Disconnected;
                                CurrentService = null;
                                cancellation.Cancel();
                            }
                        }

                        if (_messages.Count == 0)
                        {
                            tcs1 = null;
                            t1 = null;

                            tcs1 = new TaskCompletionSource<int>();
                            t1 = tcs1.Task;
                        }
                    }
                }
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
                msgNumber++;
                string msg = _names[rnd.Next(0, _names.Length)];
                _messages.AddToBack(msg + " " + msgNumber);

                if (CurrentService != null)
                    // Tell the task that we have something to process
                    tcs1.TrySetResult(10);
                
            }
            catch (Exception ex)
            {
               
            }
        }

        public async void FindWorkingHost()
        {
            while (CurrentService == null)
            {
                await Task.Run(() =>
                {
                    var task1 = DoWork(Services[0]);
                    var task2 = DoWork(Services[1]);
                    var task3 = DoWork(Services[2]);

                    Task.WhenAll(task1, task2, task3);
                });

                await Task.Delay(5000);
            }
        }

        public async Task DoWork(ServiceModel service)
        {
            await Task.Run(() =>
            { 
                if (service.TryConnect())
                {
                    CurrentService = service;
                }
            });
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
