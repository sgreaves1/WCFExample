using System.ServiceModel;

namespace AlarmServiceLibrary
{
    [ServiceContract]
    public interface IAlarmService
    {
        [OperationContract]
        void ActivateAlarm(int clientId, string name);
    }
}
