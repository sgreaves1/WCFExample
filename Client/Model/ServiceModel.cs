﻿using Client.AlarmServiceReference;
using Client.Enumerator;

namespace Client.Model
{
    public class ServiceModel : BaseModel
    {
        private string _name;
        private ConnectionStatus _connectionState = ConnectionStatus.Disconnected;

        public ServiceModel(string endpointAddress)
        {
            Client = new AlarmServiceClient("WSHttpBinding_IAlarmService", endpointAddress);
            ConnectionState = ConnectionStatus.Disconnected;
        }

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
        
        public ConnectionStatus ConnectionState
        {
            get { return _connectionState; }
            set
            {
                _connectionState = value;
                OnPropertyChanged();
            }
        }
    }
}
