using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using AlarmServiceLibrary;

namespace Host.ViewModel
{
    public class MainWindowViewModel : BaseViewModel
    {
        public MainWindowViewModel()
        {
            // Step 1 Create a URI to serve as the base address.  
            Uri baseAddress = new Uri("http://localhost:9000/GettingStarted/");

            // Step 2 Create a ServiceHost instance  
            ServiceHost selfHost = new ServiceHost(typeof (AlarmService), baseAddress);

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
    }
}
