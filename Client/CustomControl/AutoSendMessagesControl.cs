using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.CustomControl
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Client.CustomControls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Client.CustomControls;assembly=Client.CustomControls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:AutoSendMessagesControl/>
    ///
    /// </summary>
    public class AutoSendMessagesControl : Control
    {
        // Using a DependencyProperty as the backing store for StartButtonWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartButtonWidthProperty =
            DependencyProperty.Register("StartButtonWidth",
                typeof(double), 
                typeof(AutoSendMessagesControl), 
                new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for StopButtonWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StopButtonWidthProperty =
            DependencyProperty.Register("StopButtonWidth",
                typeof(double),
                typeof(AutoSendMessagesControl),
                new PropertyMetadata(0.0));

        // Using a DependencyProperty as the backing store for Sending.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SendingProperty =
            DependencyProperty.Register("Sending", 
                typeof(bool), 
                typeof(AutoSendMessagesControl), 
                new PropertyMetadata(false));

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SendClickedCommandProperty =
            DependencyProperty.Register("SendClickedCommand", 
                typeof(ICommand), 
                typeof(AutoSendMessagesControl), 
                new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StopClickedCommandProperty =
            DependencyProperty.Register("StopClickedCommand",
                typeof(ICommand),
                typeof(AutoSendMessagesControl),
                new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for NumericValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumericValueProperty =
            DependencyProperty.Register("NumericValue", 
                typeof(int), 
                typeof(AutoSendMessagesControl),
                new PropertyMetadata(0));



        static AutoSendMessagesControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoSendMessagesControl), new FrameworkPropertyMetadata(typeof(AutoSendMessagesControl)));
        }

        public double StartButtonWidth
        {
            get { return (double)GetValue(StartButtonWidthProperty); }
            set { SetValue(StartButtonWidthProperty, value); }
        }

        public double StopButtonWidth
        {
            get { return (double)GetValue(StopButtonWidthProperty); }
            set { SetValue(StopButtonWidthProperty, value); }
        }

        public bool Sending
        {
            get { return (bool)GetValue(SendingProperty); }
            set { SetValue(SendingProperty, value); }
        }

        public ICommand SendClickedCommand
        {
            get { return (ICommand)GetValue(SendClickedCommandProperty); }
            set { SetValue(SendClickedCommandProperty, value); }
        }

        public ICommand StopClickedCommand
        {
            get { return (ICommand)GetValue(StopClickedCommandProperty); }
            set { SetValue(StopClickedCommandProperty, value); }
        }

        public int NumericValue
        {
            get { return (int)GetValue(NumericValueProperty); }
            set { SetValue(NumericValueProperty, value); }
        }
    }
}
