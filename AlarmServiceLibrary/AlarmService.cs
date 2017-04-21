using System;
using System.ServiceModel;
using AlarmServiceLibrary.EventArguments;

namespace AlarmServiceLibrary
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode =InstanceContextMode.Single)]
    public class AlarmService : IAlarmService
    {
        public EventHandler<AlarmEventArgs> Alarm;

        public void ActivateAlarm(int clientId, string name)
        {
            Alarm?.Invoke(name, new AlarmEventArgs() {ClientId = clientId, Name = name});
        }
    }
}
