using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
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

        private ServiceModel CurrentService = new ServiceModel() {ConnectionState = ConnectionStatus.Disconnected};

        private static ILog log = LogManager.GetLogger<MainWindowViewModel>();

        private string[] _names = new[] {"Sam", "Steve", "Ray", "Keeno", "Gob"};

        private bool _sending;

        private int _seconds;

        private Deque<string> _messages = new Deque<string>();

        // Set to completed when there are messages to process. Gets reset when the message deque is empty
        private static TaskCompletionSource<bool> _noMessagesCompletionSource;
        private Task<int> _noMessagesTask;

        // Flag used to indicate if the app is attempting to find a host. 
        private bool _findingHost = false;

        private string _singleMessageText;

        private bool _addNumberToSingleMessage;

        public MainWindowViewModel()
        {
            Seconds = 1;
            SingleMessageText = "";

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

            SendSingleButtonCommand = new RelayCommand(ExecuteSendSingleCommand, CanExecuteSendSingleCommand);
        }

        private bool CanExecuteSendSingleCommand()
        {
            if (String.IsNullOrEmpty(SingleMessageText))
                return false;

            return true;
        }

        private void ExecuteSendSingleCommand()
        {
            lock (_messages)
            {
                string message = SingleMessageText;

                if (AddNumberToSingleMessage)
                {
                    _messageNumber++;
                    message += " " + _messageNumber;
                }


                _messages.AddToBack(message);
                // Tell the task that we have something to process
                _noMessagesCompletionSource?.TrySetResult(true);
            }
        }

        private void ProcessMessages()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    Task awaitMessages = null;

                    if (_messages.Count == 0)
                    {
                        _noMessagesCompletionSource =
                            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                        awaitMessages = _noMessagesCompletionSource.Task;
                    }

                    if (awaitMessages != null)
                    {
                        // Wait for the no messages task to finish.
                        // which means wait on this line while the deque is empty.
                        await awaitMessages;
                    }

                    string message = "";

                    lock (_messages)
                    {
                        message = _messages.First();
                        log.Trace("Alarm Sent, Name: " + _messages.First());
                        _messages.RemoveFromFront();
                    }

                    while (true)
                    {
                        try
                        {
                            CurrentService.Client.ActivateAlarm(ClientID, message);
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (message.ToLower() == "close")
                            {
                                break;
                            }

                            log.Trace("Not connected to WCF host. " + ex.Message);

                            if (CurrentService != null)
                                CurrentService.ConnectionState = ConnectionStatus.Disconnected;


                            // If the service has gone for whatever reason, and we are not already finding a suitable host, 
                            // then find one.
                            if (!_findingHost)
                            {
                                FindWorkingHost();
                            }

                            await Task.Delay(1000);
                        }
                    }
                }
            });
        }

        async void Run()
        {
            if (!_findingHost)
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

        private int _messageNumber = 0;

        private void SendMessage()
        {
            lock (_messages)
            {
                _messageNumber++;
                string message = _names[rnd.Next(0, _names.Length)];
                _messages.AddToBack(message + " " + _messageNumber);
                // Tell the task that we have something to process
                _noMessagesCompletionSource?.TrySetResult(true);
            }
        }

        public async void FindWorkingHost()
        {
            _findingHost = true;

            await Task.Run(async () =>
            {

                while (CurrentService.ConnectionState == ConnectionStatus.Disconnected)
                {

                    bool connected1 = TryConnectToService(Services[0]);
                    bool connected2 = TryConnectToService(Services[1]);
                    bool connected3 = TryConnectToService(Services[2]);

                    if (connected1 || connected2 || connected3)
                        break;

                    await Task.Delay(5000);
                }
            });

            _findingHost = false;
        }

        /// <summary>
        /// Make an attempt to connect to the given service.
        /// </summary>
        /// <param name="service">Service object to attempt WCF connection on.</param>
        /// <returns></returns>
        public bool TryConnectToService(ServiceModel service)
        {
            if (service.TryConnect())
            {
                CurrentService = service;
                return true;
            }

            return false;
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
        
        /// <summary>
        /// 
        /// </summary>
        public string SingleMessageText
        {
            get { return _singleMessageText; }
            set
            {
                _singleMessageText = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public bool AddNumberToSingleMessage
        {
            get { return _addNumberToSingleMessage; }
            set
            {
                _addNumberToSingleMessage = value;
                OnPropertyChanged();
            }
        }


        public ICommand SendingButtonCommand { get; set; }
        public ICommand SendSingleButtonCommand { get; set; }
    }
}
