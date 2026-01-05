using System;
using System.Windows;

namespace WinDynamicIsland.UI
{
    public partial class IslandWindow : Window
    {
        public IslandWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdatePosition();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            // Center the window horizontally on the primary screen
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            this.Left = (screenWidth - this.ActualWidth) / 2;
            this.Top = 10; // 10px margin from top
        }
    }
}
