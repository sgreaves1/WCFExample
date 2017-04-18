using MyLibrary.SelectPanel;

namespace Host.Model
{
    public class AlarmModel : BaseModel, IPanelItem
    {
        public int ClientId { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
