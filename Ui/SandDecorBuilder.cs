using System.Windows;
using System.Windows.Controls;

namespace UnderwaterBaseClicker.Ui;

public static class SandDecorBuilder
{
    private static readonly (string File, double X, double Y, double Size)[] Stones =
    [
        ("Stone_1.png", 18, 28, 34),
        ("Stone_3.png", 78, 42, 22),
        ("Stone_2.png", 145, 24, 28),
        ("Stone_5.png", 210, 44, 20),
        ("Stone_4.png", 268, 30, 26),
        ("Stone_6.png", 330, 46, 18),
        ("Stone_1.png", 395, 32, 24),
        ("Stone_3.png", 458, 40, 22),
        ("Stone_2.png", 520, 26, 30),
        ("Stone_5.png", 575, 44, 20),
        ("Stone_4.png", 55, 18, 16),
        ("Stone_6.png", 610, 34, 18)
    ];

    public static void Populate(Canvas layer)
    {
        layer.Children.Clear();
        foreach (var (file, x, y, size) in Stones)
        {
            var stone = GameSprites.Create(file, size, pixelArt: true);
            Canvas.SetLeft(stone, x);
            Canvas.SetTop(stone, y);
            Panel.SetZIndex(stone, 1);
            layer.Children.Add(stone);
        }
    }
}
