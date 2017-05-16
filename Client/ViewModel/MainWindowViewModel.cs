using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Client.Container;
using Client.Enumerator;
using Client.Model;
using Common.Logging;
using MyLibrary.Command;
using MyLibrary.SelectPanel;

namespace Client.ViewModel
{
    public class MainWindowViewModel : BaseModel
    {
        private static Random rnd = new Random(DateTime.Now.Millisecond);
        private int _clientID = rnd.Next(1, 10000);
        
        CancellationTokenSource cancellation = new CancellationTokenSource();

        private ObservableCollection<ServiceModel> _services = new ObservableCollection<ServiceModel>();

        private ServiceModel CurrentService = null;

        private static ILog log = LogManager.GetLogger<MainWindowViewModel>();

        private string[] _names = new[] {"Sam", "Steve", "Ray", "Keeno", "Gob"};

        private bool _sending;

        private int _seconds;

        private Deque<string> _messages = new Deque<string>();
        ObservableCollection<IPanelItem> _messagesView = new ObservableCollection<IPanelItem>();
        private IPanelItem _currentMessage; 

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
                Sending = true;
                Run();
            });

            StopingButtonCommand = new RelayCommand(() =>
            {
                Sending = false;
                cancellation.Cancel();
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
                UpdateMessagesOnView();
                // Tell the task that we have something to process
                _noMessagesCompletionSource?.TrySetResult(true);
            }
        }

        private void UpdateMessagesOnView()
        {
            MessagesOnView.Clear();

            foreach (var message in _messages)
            {
                MessagesOnView.Add(new MessageModel() {Name = message});
            }
        }

        private void ProcessMessages()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    Task awaitMessages = null;
                    string message = null;

                    lock (_messages)
                    {
                        App.Current.Dispatcher.Invoke(UpdateMessagesOnView);

                        if (_messages.Count == 0)
                        {
                            _noMessagesCompletionSource =
                                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                            awaitMessages = _noMessagesCompletionSource.Task;
                        }
                        else
                        {

                            message = _messages.First();
                            log.Trace("Alarm Sent, Name: " + _messages.First());
                            _messages.RemoveFromFront();
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                CurrentMessage = new MessageModel() { Name = message };
                            });

                            if (message.ToLower() == "close")
                            {
                                CurrentService.ConnectionState = ConnectionStatus.Disconnected;
                                CurrentService = null;
                                _messages.Clear();
                                App.Current.Dispatcher.Invoke(cancellation.Cancel);
                                continue;
                            }

                        }
                    }
                    if (awaitMessages != null)
                    {
                        // Wait for the no messages task to finish.
                        // which means wait on this line while the deque is empty.
                        await awaitMessages;
                        continue;
                    }

                    while (true)
                    {
                        try
                        {
                            CurrentService.ConnectionState = ConnectionStatus.Attempting;
                            CurrentService.Client.ActivateAlarm(ClientID, message);
                            CurrentService.ConnectionState = ConnectionStatus.Connected;
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                CurrentMessage = null;
                            });
                            break;
                        }
                        catch (Exception ex)
                        {
                            log.Trace("Not connected to WCF host. " + ex.Message);

                            if (CurrentService != null)
                            {
                                CurrentService.ConnectionState = ConnectionStatus.Disconnected;
                                CurrentService.Tested = true;
                            }

                            if (Services.All(x => x.Tested))
                            {
                                await Task.Delay(10000);

                                foreach (var serviceModel in Services)
                                {
                                    serviceModel.Tested = false;
                                }
                            }

                            // If the service has gone for whatever reason, get the next service in the list
                            GetNextUntestedHost();
                        }
                    }
                }
            });
        }

        async void Run()
        {
            await IntervalMessageSending(SendMessage, TimeSpan.FromSeconds(Seconds), cancellation.Token);
            Sending = false;
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

                        Services.Add(new ServiceModel(appSettings[key]) {Name = "Host"});
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

            App.Current.Dispatcher.Invoke(UpdateMessagesOnView);
        }

        private int index = -1;
        public void GetNextUntestedHost()
        {
            while (true)
            {
                index++;
                if (index > Services.Count - 1)
                    index = 0;

                if (!Services.ToList()[index].Tested)
                {
                    CurrentService = Services.ToList()[index];
                    break;
                }

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
                        break;
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

        public ObservableCollection<IPanelItem> MessagesOnView
        {
            get { return _messagesView; }
            set
            {
                _messagesView = value;
                OnPropertyChanged();
            }
        }

        public IPanelItem CurrentMessage
        {
            get { return _currentMessage; }
            set
            {
                _currentMessage = value;
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
        public ICommand StopingButtonCommand { get; set; }
        public ICommand SendSingleButtonCommand { get; set; }
    }
}
