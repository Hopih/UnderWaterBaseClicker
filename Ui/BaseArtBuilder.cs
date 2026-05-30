using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace UnderwaterBaseClicker.Ui;

public static class BaseArtBuilder
{
    public static FrameworkElement? CreateModule(string upgradeId, int level) => upgradeId switch
    {
        "algae" => GameSprites.Create(GameSprites.GetUpgradeSprite("algae", level), 72, 58),
        "sub" => GameSprites.Create("submarine.png", 88 + level * 6, 44),
        _ => null
    };

    public static Border WrapForSlot(UIElement content, double width, double height, bool anchorBottom = true) =>
        new()
        {
            Child = content,
            Width = width,
            Height = height,
            Background = Brushes.Transparent,
            RenderTransformOrigin = anchorBottom ? new Point(0.5, 1.0) : new Point(0.5, 0.5)
        };
}
