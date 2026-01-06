using System.Windows;
using System.ComponentModel;
using System.Windows.Media.Animation;
using WindowsDynamicHalo.Core;
using WindowsDynamicHalo.ViewModels;

namespace WindowsDynamicHalo.UI
{
    public partial class IslandWindow : Window
    {
        public IslandWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Ensure initial position is correct
            UpdateLayout();
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

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is IslandViewModel oldVm)
            {
                oldVm.PropertyChanged -= OnVmPropertyChanged;
            }
            if (e.NewValue is IslandViewModel newVm)
            {
                newVm.PropertyChanged += OnVmPropertyChanged;
            }
        }

        private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is IslandViewModel vm && (e.PropertyName == nameof(IslandViewModel.IsExpanded) || e.PropertyName == nameof(IslandViewModel.HasMedia) || e.PropertyName == nameof(IslandViewModel.Width) || e.PropertyName == nameof(IslandViewModel.Height)))
            {
                var toW = vm.Width;
                var toH = vm.Height;
                var sb = IslandAnimator.CreateResizeStoryboard(this, this.ActualWidth, this.ActualHeight, toW, toH);
                sb.Begin(this);

                var borderSb = vm.IsExpanded || vm.HasMedia
                    ? IslandAnimator.CreateExpandStoryboard(RootBorder, RootBorder.ActualWidth, RootBorder.ActualWidth)
                    : IslandAnimator.CreateCollapseStoryboard(RootBorder, RootBorder.ActualWidth, RootBorder.ActualWidth);
                borderSb.Begin(RootBorder);
            }
        }

        private void OnSliderDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (DataContext is IslandViewModel vm)
            {
                vm.Media.IsDragging = true;
            }
        }

        private void OnSliderDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (DataContext is IslandViewModel vm)
            {
                vm.Media.IsDragging = false;
                if (sender is System.Windows.Controls.Slider slider)
                {
                    vm.Media.CurrentPositionSeconds = slider.Value;
                }
            }
        }
    }
}
