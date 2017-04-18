using System.Windows;
using Host.Model;

namespace Host.UserControl
{
    /// <summary>
    /// Interaction logic for AlarmDetails.xaml
    /// </summary>
    public partial class AlarmDetails
    {
        public static readonly DependencyProperty AlarmProperty =
            DependencyProperty.Register("Alarm", 
                typeof(AlarmModel), 
                typeof(AlarmDetails), 
                new PropertyMetadata(null));

        public AlarmDetails()
        {
            InitializeComponent();
        }

        public AlarmModel Alarm
        {
            get { return (AlarmModel)GetValue(AlarmProperty); }
            set { SetValue(AlarmProperty, value); }
        }
    }
}
