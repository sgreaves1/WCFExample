namespace Client.Model
{
    public class ServiceModel : BaseModel
    {
        private string _name;
        private string _endpointAddress;
        private bool _connected;

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

        public bool Connected
        {
            get { return _connected; }
            set
            {
                _connected = value;
                OnPropertyChanged();
            }
        }
    }
}
