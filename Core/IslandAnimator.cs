using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WindowsDynamicHalo.Core
{
    // Centralized creation of fluid animations for expand/collapse.
    // Uses RenderTransform (Scale) to avoid layout thrashing and DoubleAnimation on Width for card growth.
    public static class IslandAnimator
    {
        public static Storyboard CreateExpandStoryboard(FrameworkElement target, double fromWidth, double toWidth, double durationMs = 220)
        {
            EnsureScaleTransform(target);
            var sb = new Storyboard();

            var widthAnim = new DoubleAnimation
            {
                From = fromWidth,
                To = toWidth,
                Duration = TimeSpanFromMs(durationMs),
                EasingFunction = new QuinticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(widthAnim, target);
            Storyboard.SetTargetProperty(widthAnim, new PropertyPath(FrameworkElement.WidthProperty));
            sb.Children.Add(widthAnim);

            var scaleX = new DoubleAnimation
            {
                From = 0.96,
                To = 1.0,
                Duration = TimeSpanFromMs(durationMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var scaleY = new DoubleAnimation
            {
                From = 0.96,
                To = 1.0,
                Duration = TimeSpanFromMs(durationMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(scaleX, target);
            Storyboard.SetTarget(scaleY, target);
            Storyboard.SetTargetProperty(scaleX, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            sb.Children.Add(scaleX);
            sb.Children.Add(scaleY);

            return sb;
        }

        public static Storyboard CreateCollapseStoryboard(FrameworkElement target, double fromWidth, double toWidth, double durationMs = 200)
        {
            EnsureScaleTransform(target);
            var sb = new Storyboard();

            var widthAnim = new DoubleAnimation
            {
                From = fromWidth,
                To = toWidth,
                Duration = TimeSpanFromMs(durationMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(widthAnim, target);
            Storyboard.SetTargetProperty(widthAnim, new PropertyPath(FrameworkElement.WidthProperty));
            sb.Children.Add(widthAnim);

            var scaleX = new DoubleAnimation
            {
                From = 1.0,
                To = 0.98,
                Duration = TimeSpanFromMs(durationMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var scaleY = new DoubleAnimation
            {
                From = 1.0,
                To = 0.98,
                Duration = TimeSpanFromMs(durationMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(scaleX, target);
            Storyboard.SetTarget(scaleY, target);
            Storyboard.SetTargetProperty(scaleX, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
            sb.Children.Add(scaleX);
            sb.Children.Add(scaleY);

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

