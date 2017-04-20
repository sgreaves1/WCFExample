using System.Collections.ObjectModel;
using System.Windows;
using Client.Model;

namespace Client.UserControl
{
    /// <summary>
    /// Interaction logic for StatusBoxes.xaml
    /// </summary>
    public partial class StatusBoxes
    {
        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", 
                typeof(ObservableCollection<ServiceModel>), 
                typeof(StatusBoxes), 
                new PropertyMetadata(null));
        
        public StatusBoxes()
        {
            InitializeComponent();
        }

        public ObservableCollection<ServiceModel> ItemsSource
        {
            get { return (ObservableCollection<ServiceModel>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
    }
}
