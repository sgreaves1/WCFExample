using Client.Enumerator;

namespace Client.Model
{
    public class ServiceModel : BaseModel
    {
        private string _name;
        private string _endpointAddress;
        private ConnectionStatus _connectionState = ConnectionStatus.Disconnected;

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
    }
}
