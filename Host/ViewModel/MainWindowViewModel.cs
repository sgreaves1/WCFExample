using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Description;
using AlarmServiceLibrary;
using AlarmServiceLibrary.EventArguments;
using Host.Model;
using MyLibrary.SelectPanel;

namespace Host.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        private ObservableCollection<IPanelItem> _alarms = new ObservableCollection<IPanelItem>();

        public MainWindowViewModel()
        {
            // Step 1 Create a URI to serve as the base address.  
            Uri baseAddress = new Uri("http://localhost:9000/AlarmService/");
            
            // Step 2 Create a ServiceHost instance 
            AlarmService serv = new AlarmService();
            serv.Alarm += Alarm; 
            ServiceHost selfHost = new ServiceHost(serv, baseAddress);       

            try
            {
                // Step 3 Add a service endpoint.  
                selfHost.AddServiceEndpoint(typeof (IAlarmService), new WSHttpBinding(), "AlarmService");

                // Step 4 Enable metadata exchange.  
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                selfHost.Description.Behaviors.Add(smb);

                // Step 5 Start the service.  
                selfHost.Open();
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine(@"An exception occurred: {0}", ce.Message);
                selfHost.Abort();
            }
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
    }
}
