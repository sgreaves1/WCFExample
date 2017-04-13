using System;
using System.ServiceModel;
using Host.EventArguments;

namespace AlarmServiceLibrary
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode =InstanceContextMode.Single)]
    public class AlarmService : IAlarmService
    {
        public EventHandler Alarm;

        public void ActivateAlarm(string name)
        {
            Alarm?.Invoke(name, new AlarmEventArgs() {Name = name});
        }
    }
}
