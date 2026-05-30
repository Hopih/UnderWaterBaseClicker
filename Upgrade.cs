namespace UnderwaterBaseClicker;

public enum UpgradeKind
{
    Passive,
    Click
}

public sealed class Upgrade(
    string id,
    string name,
    string description,
    double baseCost,
    double costGrowth,
    UpgradeKind kind,
    double effectPerLevel,
    string icon = "⚡")
{
    public string Id { get; } = id;
    public string Icon { get; } = icon;
    public string Name { get; } = name;
    public string Description { get; } = description;
    public double BaseCost { get; } = baseCost;
    public double CostGrowth { get; } = costGrowth;
    public UpgradeKind Kind { get; } = kind;
    public double EffectPerLevel { get; } = effectPerLevel;

    public int Count { get; set; }

    public double TotalEffect => EffectPerLevel * Count;

    public double CurrentCost => BaseCost * Math.Pow(CostGrowth, Count);

    public string EffectLabel => Kind switch
    {
        UpgradeKind.Passive => $"+{FormatNumber(EffectPerLevel)}/с",
        UpgradeKind.Click => $"+{FormatNumber(EffectPerLevel)} за клик",
        _ => ""
    };

    public static string FormatNumber(double value)
    {
        if (value >= 1_000_000_000)
            return $"{value / 1_000_000_000:0.##} млрд";
        if (value >= 1_000_000)
            return $"{value / 1_000_000:0.##}M";
        if (value >= 1_000)
            return $"{value / 1_000:0.##}K";
        if (value >= 10)
            return $"{value:0}";
        return $"{value:0.#}";
    }
}
