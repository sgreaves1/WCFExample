using System;

namespace AlarmServiceLibrary.EventArguments
{
    public class AlarmEventArgs : EventArgs
    {
        public int ClientId;
        public string Name;
    }
}
