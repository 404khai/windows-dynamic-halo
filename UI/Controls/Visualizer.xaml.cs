using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace WindowsDynamicHalo.UI.Controls
{
    // Timer-driven visualizer: animates bar heights when IsActive is true.
    // Uses DoubleAnimation with easing to keep motion fluid and GPU-friendly.
    public partial class Visualizer : UserControl
    {
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(Visualizer),
                new PropertyMetadata(false, OnIsActiveChanged));

        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(180) };
        private readonly Random _rnd = new Random();

        public Visualizer()
        {
            InitializeComponent();
            _timer.Tick += (_, __) => AnimateTick();
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viz = (Visualizer)d;
            if ((bool)e.NewValue) viz._timer.Start();
            else viz._timer.Stop();
        }

        private void AnimateTick()
        {
            AnimateBar(Bar1, 6, 20);
            AnimateBar(Bar2, 8, 22);
            AnimateBar(Bar3, 5, 18);
            AnimateBar(Bar4, 10, 24);
            AnimateBar(Bar5, 6, 20);
        }

        private void AnimateBar(FrameworkElement bar, double min, double max)
        {
            var target = min + _rnd.NextDouble() * (max - min);
            var anim = new DoubleAnimation
            {
                To = target,
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            bar.BeginAnimation(FrameworkElement.HeightProperty, anim, HandoffBehavior.SnapshotAndReplace);
        }
    }
}

