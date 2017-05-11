using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyLibrary.SelectPanel;

namespace Client.Model
{
    /// <summary>
    /// Class used to represent message on the view
    /// </summary>
    public class MessageModel : BaseModel, IPanelItem
    {
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}
