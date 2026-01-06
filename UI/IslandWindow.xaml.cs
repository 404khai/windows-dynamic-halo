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

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            Logger.Log("IslandWindow: OnLoaded fired.");
            // Ensure initial position is correct
            UpdateLayout();
            UpdatePosition();

            if (DataContext is IslandViewModel vm)
            {
                Logger.Log($"IslandWindow: OnLoaded. DataContext is VM #{vm.InstanceId}. Initial State -> Width={vm.Width}, Height={vm.Height}, HasMedia={vm.HasMedia}");
                
                // CRITICAL FIX: Ensure we are subscribed to PropertyChanged events.
                // If DataContext was set in XAML, OnDataContextChanged might have fired before we subscribed to the event in the constructor.
                vm.PropertyChanged -= OnVmPropertyChanged; // Prevent double subscription
                vm.PropertyChanged += OnVmPropertyChanged;
                Logger.Log("IslandWindow: Manually subscribed to ViewModel PropertyChanged events.");

                // Force sync initial state
                this.Width = vm.Width;
                this.Height = vm.Height;
                UpdateVisualState(vm);

                Logger.Log("IslandWindow: Calling InitializeAsync...");
                await vm.InitializeAsync();
                Logger.Log("IslandWindow: InitializeAsync returned.");
            }
            else
            {
                Logger.Log($"IslandWindow: DataContext is NOT IslandViewModel. It is {DataContext?.GetType().Name ?? "null"}");
            }
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
            Logger.Log("IslandWindow: OnDataContextChanged fired.");
            if (e.OldValue is IslandViewModel oldVm)
            {
                oldVm.PropertyChanged -= OnVmPropertyChanged;
            }
            if (e.NewValue is IslandViewModel newVm)
            {
                Logger.Log("IslandWindow: Subscribing to new ViewModel.");
                newVm.PropertyChanged += OnVmPropertyChanged;
                // Sync initial state
                this.Width = newVm.Width;
                this.Height = newVm.Height;
                UpdateVisualState(newVm);
            }
        }

        private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (sender is IslandViewModel vm)
                {
                    if (e.PropertyName == nameof(IslandViewModel.Width) || e.PropertyName == nameof(IslandViewModel.Height))
                    {
                        Logger.Log($"IslandWindow: Resizing to {vm.Width}x{vm.Height}");
                        // Animate Window Size
                        var sb = IslandAnimator.CreateResizeStoryboard(this, this.ActualWidth, this.ActualHeight, vm.Width, vm.Height);
                        sb.Begin(this);
                    }
                    else if (e.PropertyName == nameof(IslandViewModel.IsExpanded) || e.PropertyName == nameof(IslandViewModel.HasMedia))
                    {
                        Logger.Log($"IslandWindow: Updating Visual State. HasMedia={vm.HasMedia}, Expanded={vm.IsExpanded}");
                        // Animate Content
                        UpdateVisualState(vm);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.Log($"IslandWindow: Error in OnVmPropertyChanged: {ex.Message}");
            }
        }

        private void UpdateVisualState(IslandViewModel vm)
        {
            if (vm.HasMedia)
            {
                if (vm.IsExpanded)
                {
                    // Go to Expanded
                    TransitionToState(ExpandedState, CompactState);
                }
                else
                {
                    // Go to Compact
                    TransitionToState(CompactState, ExpandedState);
                }
            }
            else
            {
                // Go to Idle
                TransitionToState(null, CompactState); // Hide Compact
                TransitionToState(null, ExpandedState); // Hide Expanded
            }
        }

        private void TransitionToState(FrameworkElement? show, FrameworkElement? hide)
        {
            if (hide != null && hide.Visibility == Visibility.Visible)
            {
                var sb = IslandAnimator.CreateFadeStoryboard(hide, 1, 0, 150);
                sb.Completed += (s, e) => hide.Visibility = Visibility.Collapsed;
                sb.Begin(hide);
            }

            if (show != null)
            {
                show.Visibility = Visibility.Visible;
                var sb = IslandAnimator.CreateFadeStoryboard(show, 0, 1, 250, 50); // Slight delay for pop-in
                sb.Begin(show);
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
