using System;
using Client.AlarmServiceReference;
using Client.Enumerator;

namespace Client.Model
{
    public class ServiceModel : BaseModel
    {
        private string _name;
        private string _endpointAddress;
        private ConnectionStatus _connectionState = ConnectionStatus.Disconnected;

        public AlarmServiceClient Client;

        public string Name
        {
            get {  return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string EndpointAddress
        {
            get { return _endpointAddress; }
            set
            {
                _endpointAddress = value;
                OnPropertyChanged();
            }
        }

        public ConnectionStatus ConnectionState
        {
            get { return _connectionState; }
            set
            {
                _connectionState = value;
                OnPropertyChanged();
            }
        }

        public bool TryConnect()
        {
            try
            {
                ConnectionState = ConnectionStatus.Attempting;
                Client = new AlarmServiceClient("WSHttpBinding_IAlarmService", EndpointAddress);

                Client.Open();
            }
            catch (Exception)
            {

                ConnectionState = ConnectionStatus.Disconnected;
                return false;
            }
            
            ConnectionState = ConnectionStatus.Connected;
            return true;
        }
    }
}
