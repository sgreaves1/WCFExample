using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.CustomControl
{
    public class SendMessageControl : Control
    {
        /// <summary>
        /// Dependency Property used to back the <see cref="SendCommand"/>
        /// </summary>
        public static readonly DependencyProperty SendCommandProperty =
            DependencyProperty.Register("SendCommand", 
                typeof(ICommand), 
                typeof(SendMessageControl), 
                new PropertyMetadata(null));
        
        static SendMessageControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SendMessageControl), new FrameworkPropertyMetadata(typeof(SendMessageControl)));
        }
        
        /// <summary>
        /// 
        /// </summary>
        public ICommand SendCommand
        {
            get { return (ICommand)GetValue(SendCommandProperty); }
            set { SetValue(SendCommandProperty, value); }
        }
    }
}
