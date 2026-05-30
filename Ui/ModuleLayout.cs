using System.Windows;

namespace UnderwaterBaseClicker.Ui;

public static class ModuleLayout
{
    public const double SandTop = 410;
    public const double SandHeight = 70;

    public static readonly ModuleHostDef Algae = new("algae", 12, 352, 4, [
        new(0, 0, 0),
        new(68, -3, 1),
        new(136, 2, 2),
        new(204, -2, 3),
        new(272, 0, 4),
        new(340, -3, 5),
        new(408, 2, 6),
        new(476, 0, 7)
    ]);

    /// <summary>Подлодки плавают в воде над песком.</summary>
    public static readonly ModuleHostDef Dock = new("sub", 140, 248, 6, [
        new(0, 0, 0),
        new(120, -12, 1),
        new(240, 4, 2),
        new(360, -8, 3)
    ]);

    public static readonly Point ReactorCenter = new(320, 170);
    public static readonly Point DroneOrbitCenter = new(320, 210);

    public static ModuleHostDef GetHost(string upgradeId) => upgradeId switch
    {
        "algae" => Algae,
        "sub" => Dock,
        _ => Algae
    };

    public static Point GetAnchor(string upgradeId, int levelIndex)
    {
        if (upgradeId == "drone")
        {
            var angle = levelIndex * 2.1 + 0.5;
            return new Point(
                DroneOrbitCenter.X + Math.Cos(angle) * (120 + levelIndex * 14),
                DroneOrbitCenter.Y + Math.Sin(angle * 0.75) * (70 + levelIndex * 8));
        }

        var host = GetHost(upgradeId);
        var slot = host.Slots[Math.Min(levelIndex, host.Slots.Length - 1)];
        return new Point(host.SceneX + slot.X, host.SceneY + slot.Y);
    }

    public static bool HasVisual(string upgradeId) => upgradeId is "algae" or "sub" or "drone";

    public sealed record ModuleSlot(double X, double Y, int ZIndex);

    public sealed record ModuleHostDef(string Id, double SceneX, double SceneY, int LayerZ, ModuleSlot[] Slots);
}
