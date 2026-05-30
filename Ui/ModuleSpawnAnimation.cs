using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace UnderwaterBaseClicker.Ui;

public static class ModuleSpawnAnimation
{
    public static void Play(FrameworkElement element)
    {
        element.RenderTransformOrigin = new Point(0.5, 0.5);
        var scale = new ScaleTransform(0.01, 0.01);
        element.RenderTransform = scale;
        element.Opacity = 0;

        var pop = new DoubleAnimationUsingKeyFrames();
        pop.KeyFrames.Add(new EasingDoubleKeyFrame(0.01, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        pop.KeyFrames.Add(new EasingDoubleKeyFrame(1.18, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(320)))
        {
            EasingFunction = new BackEase { Amplitude = 0.55 }
        });
        pop.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(480))));

        scale.BeginAnimation(ScaleTransform.ScaleXProperty, pop);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, pop.Clone());
        element.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(180)));
    }

    public static void SetVisible(FrameworkElement element)
    {
        element.RenderTransform = Transform.Identity;
        element.Opacity = 1;
    }
}
