using System.Windows;
using System.Windows.Media;

namespace UnderwaterBaseClicker.Ui;

public enum IdleAnimKind
{
    Sway,
    Bob,
    Float,
    Pulse,
    SlowSpin,
    Glow
}

public static class ModuleIdleAnimator
{
    private sealed record Entry(FrameworkElement Element, IdleAnimKind Kind, double Phase, Point Pivot);

    private static readonly List<Entry> Entries = [];

    public static void Attach(FrameworkElement element, IdleAnimKind kind, int index)
    {
        var pivot = kind switch
        {
            IdleAnimKind.Sway => new Point(0.5, 1.0),
            IdleAnimKind.Float or IdleAnimKind.Bob => new Point(0.5, 0.5),
            IdleAnimKind.SlowSpin => new Point(0.5, 0.5),
            _ => new Point(0.5, 0.5)
        };
        element.RenderTransformOrigin = pivot;
        element.RenderTransform = kind switch
        {
            IdleAnimKind.Sway => new RotateTransform(),
            IdleAnimKind.Bob or IdleAnimKind.Float => new TranslateTransform(),
            IdleAnimKind.SlowSpin => new RotateTransform(),
            IdleAnimKind.Pulse or IdleAnimKind.Glow => new ScaleTransform(1, 1),
            _ => Transform.Identity
        };

        Entries.Add(new Entry(element, kind, index * 1.37, pivot));
    }

    public static void Tick(double time)
    {
        foreach (var entry in Entries)
        {
            var t = time + entry.Phase;
            switch (entry.Kind)
            {
                case IdleAnimKind.Sway when entry.Element.RenderTransform is RotateTransform sway:
                    sway.Angle = Math.Sin(t * 1.6) * 5.5;
                    break;
                case IdleAnimKind.Bob when entry.Element.RenderTransform is TranslateTransform bob:
                    bob.X = Math.Sin(t * 2.1) * 2.5;
                    bob.Y = Math.Sin(t * 1.7) * 4;
                    break;
                case IdleAnimKind.Float when entry.Element.RenderTransform is TranslateTransform floatTf:
                    floatTf.X = Math.Sin(t * 1.3 + entry.Phase) * 4;
                    floatTf.Y = Math.Sin(t * 1.8 + entry.Phase) * 7;
                    break;
                case IdleAnimKind.Pulse when entry.Element.RenderTransform is ScaleTransform pulse:
                    var s = 1 + Math.Sin(t * 2.8) * 0.045;
                    pulse.ScaleX = s;
                    pulse.ScaleY = s;
                    break;
                case IdleAnimKind.SlowSpin when entry.Element.RenderTransform is RotateTransform spin:
                    spin.Angle = t * 18;
                    break;
                case IdleAnimKind.Glow:
                    entry.Element.Opacity = 0.82 + Math.Sin(t * 2.2) * 0.12;
                    break;
            }
        }
    }

    public static void Clear() => Entries.Clear();
}
