using System;
using System.Windows;
using System.Windows.Controls;

namespace Interface.Controls
{
    public partial class Button : UserControl
    {
        public static readonly new DependencyProperty ContentProperty =
            DependencyProperty.Register("ControlContent", typeof(string), typeof(Button), new PropertyMetadata("Button"));

        public string ControlContent
        {
            get { return (string)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly new DependencyProperty BackgroundProperty =
            DependencyProperty.Register("ControlBackground", typeof(string), typeof(Button), new PropertyMetadata("#FF0078D7"));

        public string ControlBackground
        {
            get { return (string)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly new DependencyProperty ForegroundProperty =
            DependencyProperty.Register("ControlForeground", typeof(string), typeof(Button), new PropertyMetadata("#FF009CFF"));

        public string ControlForeground
        {
            get { return (string)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public static readonly new DependencyProperty FontSizeProperty =
            DependencyProperty.Register("ControlFontSize", typeof(string), typeof(Button), new PropertyMetadata("5"));

        public string ControlFontSize
        {
            get { return (string)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static readonly new DependencyProperty FontWeightProperty =
            DependencyProperty.Register("ControlFontWeight", typeof(string), typeof(Button), new PropertyMetadata("Bold"));

        public string ControlFontWeight
        {
            get { return (string)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("ControlCornerRadius", typeof(string), typeof(Button), new PropertyMetadata("#FF009CFF"));

        public string ControlCornerRadius
        {
            get { return (string)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty MouseOverColorProperty =
            DependencyProperty.Register("ControlMouseOverColor", typeof(string), typeof(Button), new PropertyMetadata("#FF009CFF"));

        public string ControlMouseOverColor
        {
            get { return (string)GetValue(MouseOverColorProperty); }
            set { SetValue(MouseOverColorProperty, value); }
        }

        public static readonly DependencyProperty MousePressedMarginProperty =
            DependencyProperty.Register("ControlMousePressedMargin", typeof(string), typeof(Button), new PropertyMetadata("2"));

        public string ControlMousePressedMargin
        {
            get { return (string)GetValue(MousePressedMarginProperty); }
            set { SetValue(MousePressedMarginProperty, value); }
        }

        public event EventHandler Click;

        public Button()
        {
            InitializeComponent();
            DataContext = this;

            CustomButton.Click += CustomButton_Click;
        }

        private void CustomButton_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, EventArgs.Empty);
        }
    }
}