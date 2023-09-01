using Interface.Pages;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Interface
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Home home = new();
        private static readonly Settings settings = new();

        public MainWindow()
        {
            InitializeComponent();
            FrameContainer.Navigate(home);
        }

        #region Menu
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button)
            {
                Popup.PlacementTarget = button;
                Popup.Placement = PlacementMode.Right;
                Popup.IsOpen = true;
                Header.PopupText.Text = button.Tag.ToString();
            }
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Popup.Visibility = Visibility.Collapsed;
            Popup.IsOpen = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string pageName = button.Tag.ToString();
                switch (pageName)
                {
                    case "Home":
                        FrameContainer.Navigate(home);
                        break;
                    case "Settings":
                        FrameContainer.Navigate(settings);
                        break;
                }
            }
        }
        #endregion

        #region WindowsTools
        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonRestore_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
    }
}