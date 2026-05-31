using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace UnderwaterBaseClicker.Ui;

public static class ClickFx
{
    public static void SpawnFloatingEnergy(Canvas layer, Point origin, string text, Brush? brush = null)
    {
        var driftX = (Random.Shared.NextDouble() - 0.5) * 40;

        var block = new TextBlock
        {
            Text = text,
            FontSize = 24 + Random.Shared.Next(0, 8),
            FontWeight = FontWeights.Bold,
            Foreground = brush ?? new SolidColorBrush(Color.FromRgb(255, 230, 100)),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = brush != null ? Colors.Red : Color.FromRgb(255, 200, 50),
                BlurRadius = 14,
                ShadowDepth = 0,
                Opacity = 0.9
            },
            RenderTransform = new TranslateTransform()
        };

        layer.Children.Add(block);
        block.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Canvas.SetLeft(block, origin.X - block.DesiredSize.Width / 2);
        Canvas.SetTop(block, origin.Y);

        var rise = new DoubleAnimation(0, -90 - Random.Shared.Next(20, 50), TimeSpan.FromMilliseconds(850))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(850));
        var slide = new DoubleAnimation(0, driftX, TimeSpan.FromMilliseconds(850));

        Storyboard.SetTarget(rise, block);
        Storyboard.SetTargetProperty(rise, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

        Storyboard.SetTarget(slide, block);
        Storyboard.SetTargetProperty(slide, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

        Storyboard.SetTarget(fade, block);
        Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));

        var sb = new Storyboard { Children = { rise, slide, fade } };
        sb.Completed += (_, _) => layer.Children.Remove(block);
        sb.Begin();
    }
    
    public static void SpawnRipple(Canvas layer, Point center, double maxRadius = 70)
    {
        var ripple = new Ellipse
        {
            Width = 20,
            Height = 20,
            Stroke = new SolidColorBrush(Color.FromArgb(180, 0, 220, 240)),
            StrokeThickness = 3,
            Fill = Brushes.Transparent,
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new ScaleTransform(0.2, 0.2)
        };

        layer.Children.Add(ripple);
        Canvas.SetLeft(ripple, center.X - 10);
        Canvas.SetTop(ripple, center.Y - 10);

        var scaleX = new DoubleAnimation(0.2, maxRadius / 10, TimeSpan.FromMilliseconds(500));
        var scaleY = new DoubleAnimation(0.2, maxRadius / 10, TimeSpan.FromMilliseconds(500));
        var fade = new DoubleAnimation(0.9, 0, TimeSpan.FromMilliseconds(500));

        Storyboard.SetTarget(scaleX, ripple);
        Storyboard.SetTargetProperty(scaleX, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
        Storyboard.SetTarget(scaleY, ripple);
        Storyboard.SetTargetProperty(scaleY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
        Storyboard.SetTarget(fade, ripple);
        Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));

        var sb = new Storyboard { Children = { scaleX, scaleY, fade } };
        sb.Completed += (_, _) => layer.Children.Remove(ripple);
        sb.Begin();
    }

    public static void PulseScale(ScaleTransform transform, double peak = 1.14)
    {
        var anim = new DoubleAnimationUsingKeyFrames();
        anim.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        anim.KeyFrames.Add(new LinearDoubleKeyFrame(peak, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(80))));
        anim.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(240))));

        transform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
        transform.BeginAnimation(ScaleTransform.ScaleYProperty, anim.Clone());
    }

    public static void FlashElement(FrameworkElement element)
    {
        var anim = new ColorAnimation(
            Color.FromRgb(80, 255, 220),
            Color.FromRgb(18, 52, 80),
            TimeSpan.FromMilliseconds(400));

        if (element is Border border)
        {
            var brush = border.Background as SolidColorBrush ?? new SolidColorBrush(Color.FromRgb(18, 52, 80));
            if (!brush.IsFrozen)
                border.Background = brush;
            else
            {
                brush = brush.Clone();
                border.Background = brush;
            }

            brush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
        }
    }

    public static void SpawnSparkles(Canvas layer, Point origin, int count)
    {
        for (var i = 0; i < count; i++)
        {
            var sparkle = new Ellipse
            {
                Width = 4 + Random.Shared.NextDouble() * 5,
                Height = 4 + Random.Shared.NextDouble() * 5,
                Fill = new SolidColorBrush(Color.FromRgb(255, 255, 200)),
                Opacity = 0.9,
                RenderTransform = new TranslateTransform()
            };
            layer.Children.Add(sparkle);
            Canvas.SetLeft(sparkle, origin.X);
            Canvas.SetTop(sparkle, origin.Y);

            var angle = Random.Shared.NextDouble() * Math.PI * 2;
            var dist = 30 + Random.Shared.NextDouble() * 50;
            var tx = Math.Cos(angle) * dist;
            var ty = Math.Sin(angle) * dist - 20;

            var moveX = new DoubleAnimation(0, tx, TimeSpan.FromMilliseconds(600));
            var moveY = new DoubleAnimation(0, ty, TimeSpan.FromMilliseconds(600))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(600));

            Storyboard.SetTarget(moveX, sparkle);
            Storyboard.SetTargetProperty(moveX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
            Storyboard.SetTarget(moveY, sparkle);
            Storyboard.SetTargetProperty(moveY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            Storyboard.SetTarget(fade, sparkle);
            Storyboard.SetTargetProperty(fade, new PropertyPath(UIElement.OpacityProperty));

            var sb = new Storyboard { Children = { moveX, moveY, fade } };
            sb.Completed += (_, _) => layer.Children.Remove(sparkle);
            sb.Begin();
        }
    }
}
