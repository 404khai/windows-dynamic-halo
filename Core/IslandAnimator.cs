using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WindowsDynamicHalo.Core
{
    // Centralized creation of fluid animations for expand/collapse.
    // Uses RenderTransform (Scale) to avoid layout thrashing and DoubleAnimation on Width for card growth.
    public static class IslandAnimator
    {
        public static Storyboard CreateResizeStoryboard(FrameworkElement target, double fromWidth, double fromHeight, double toWidth, double toHeight, double durationMs = 300)
        {
            EnsureScaleTransform(target);
            var sb = new Storyboard();

            var wAnim = new DoubleAnimation
            {
                From = fromWidth,
                To = toWidth,
                Duration = TimeSpanFromMs(durationMs),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(wAnim, target);
            Storyboard.SetTargetProperty(wAnim, new PropertyPath(FrameworkElement.WidthProperty));
            sb.Children.Add(wAnim);

            var hAnim = new DoubleAnimation
            {
                From = fromHeight,
                To = toHeight,
                Duration = TimeSpanFromMs(durationMs),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(hAnim, target);
            Storyboard.SetTargetProperty(hAnim, new PropertyPath(FrameworkElement.HeightProperty));
            sb.Children.Add(hAnim);

            return sb;
        }

        public static Storyboard CreateFadeStoryboard(FrameworkElement target, double fromOpacity, double toOpacity, double durationMs = 200, double delayMs = 0)
        {
            var sb = new Storyboard();
            var anim = new DoubleAnimation
            {
                From = fromOpacity,
                To = toOpacity,
                Duration = TimeSpanFromMs(durationMs),
                BeginTime = TimeSpan.FromMilliseconds(delayMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(anim, target);
            Storyboard.SetTargetProperty(anim, new PropertyPath(UIElement.OpacityProperty));
            sb.Children.Add(anim);
            return sb;
        }

        private static Duration TimeSpanFromMs(double ms) => new Duration(System.TimeSpan.FromMilliseconds(ms));

        private static void EnsureScaleTransform(FrameworkElement target)
        {
            if (target.RenderTransform is not ScaleTransform)
            {
                target.RenderTransform = new ScaleTransform(1.0, 1.0);
                target.RenderTransformOrigin = new Point(0.5, 0.5);
            }
        }
    }
}
