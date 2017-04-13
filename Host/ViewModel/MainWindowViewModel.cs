using System;
using System.ServiceModel;
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
            ServiceHost selfHost = new ServiceHost(typeof(AlarmService), baseAddress);
        }
    }
}
