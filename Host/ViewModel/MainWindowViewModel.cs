using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Windows.Input;
using AlarmServiceLibrary;
using AlarmServiceLibrary.EventArguments;
using Host.Model;
using Host.XMLReader;
using MyLibrary.Command;
using MyLibrary.SelectPanel;

namespace Host.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        private ObservableCollection<IPanelItem> _alarms = new ObservableCollection<IPanelItem>();

        private IPanelItem _selectedItem;

        private ServiceHost _selfHost;

        private bool _connected;

        public MainWindowViewModel()
        {
            InitCommand();
        }

        private void InitCommand()
        {
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
        }

        private void Connect()
        {
            // Step 1 Create a URI to serve as the base address.  
            Uri baseAddress = new Uri(new DataReader().GetSettings());

            // Step 2 Create a ServiceHost instance 
            AlarmService serv = new AlarmService();
            serv.Alarm += Alarm;
            _selfHost = new ServiceHost(serv, baseAddress);

            try
            {
                // Step 3 Add a service endpoint.  
                _selfHost.AddServiceEndpoint(typeof(IAlarmService), new WSHttpBinding(), "AlarmService");

                // Step 4 Enable metadata exchange.  
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                _selfHost.Description.Behaviors.Add(smb);

                // Step 5 Start the service.  
                _selfHost.Open();
                Connected = true;
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine(@"An exception occurred: {0}", ce.Message);
                _selfHost.Abort();
                Connected = false;
            }
        }

        private void Disconnect()
        {
            _selfHost?.Close();
            Connected = false;
        }

        private void Alarm(object sender, EventArgs eventArgs)
        {
            var alarmEventArgs = eventArgs as AlarmEventArgs;

            if (alarmEventArgs != null)
                Alarms.Add(new AlarmModel {ClientId = alarmEventArgs.ClientId, Name = alarmEventArgs.Name});
        }

        public ObservableCollection<IPanelItem> Alarms
        {
            get { return _alarms; }
            set { _alarms = value; }
        }

        public IPanelItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public bool Connected
        {
            get { return _connected; }
            set
            {
                _connected = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
    }
}
