using System.IO;
using System.Text.Json;

namespace UnderwaterBaseClicker;

public sealed class GameState
{
    private static readonly string SavePath = Path.Combine(
        AppContext.BaseDirectory,
        "underwater_save.json");

    public double Energy { get; set; }
    public double TotalEnergyEarned { get; set; }
    public List<Upgrade> Upgrades { get; } = CreateDefaultUpgrades();

    public double EnergyPerClick =>
        1 + Upgrades.Where(u => u.Kind == UpgradeKind.Click).Sum(u => u.TotalEffect);

    public double EnergyPerSecond =>
        Upgrades.Where(u => u.Kind == UpgradeKind.Passive).Sum(u => u.TotalEffect);

    public void AddClickEnergy()
    {
        Energy += EnergyPerClick;
        TotalEnergyEarned += EnergyPerClick;
    }

    public void AddPassiveEnergy(double seconds)
    {
        if (EnergyPerSecond <= 0 || seconds <= 0)
            return;

        var gained = EnergyPerSecond * seconds;
        Energy += gained;
        TotalEnergyEarned += gained;
    }

    public bool TryBuyUpgrade(Upgrade upgrade)
    {
        var cost = upgrade.CurrentCost;
        if (Energy < cost)
            return false;

        Energy -= cost;
        upgrade.Count++;
        return true;
    }

    public void Save()
    {
        var data = new SaveData
        {
            Energy = Energy,
            TotalEnergyEarned = TotalEnergyEarned,
            UpgradeCounts = Upgrades.ToDictionary(u => u.Id, u => u.Count)
        };

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SavePath, json);
    }

    public void Load()
    {
        if (!File.Exists(SavePath))
            return;

        try
        {
            var json = File.ReadAllText(SavePath);
            var data = JsonSerializer.Deserialize<SaveData>(json);
            if (data is null)
                return;

            Energy = data.Energy;
            TotalEnergyEarned = data.TotalEnergyEarned;

            foreach (var upgrade in Upgrades)
            {
                if (data.UpgradeCounts.TryGetValue(upgrade.Id, out var count))
                    upgrade.Count = count;
            }
        }
        catch
        {
            // Повреждённое сохранение
        }
    }

    public void Reset()
    {
        Energy = 0;
        TotalEnergyEarned = 0;
        foreach (var upgrade in Upgrades)
            upgrade.Count = 0;

        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    private static List<Upgrade> CreateDefaultUpgrades() =>
    [
        new("algae", "Ферма водорослей", "Пассивная биоэнергия", 15, 1.12, UpgradeKind.Passive, 0.5, "🌿"),
        new("drone", "Дрон-разведчик", "Сканирует рифы", 80, 1.14, UpgradeKind.Passive, 2, "🤖"),
        new("pressure", "Усилитель давления", "Больше энергии за клик", 40, 1.13, UpgradeKind.Click, 1, "🔧"),
        new("sub", "Мини-батискаф", "Доставка ресурсов", 250, 1.15, UpgradeKind.Passive, 8, "🚤"),
        new("sonar", "Глубинный сонар", "Находит геотермальные источники", 1200, 1.16, UpgradeKind.Passive, 35, "📡"),
        new("lab", "Исследовательская лаборатория", "Оптимизация систем базы", 5000, 1.17, UpgradeKind.Passive, 120, "🔬"),
        new("reactor", "Реактор базы", "Сердце подводной станции", 25000, 1.18, UpgradeKind.Passive, 500, "⚛️"),
        new("dome", "Расширение купола", "Новые модули для колонии", 100000, 1.19, UpgradeKind.Passive, 2000, "🫧"),
    ];

    private sealed class SaveData
    {
        public double Energy { get; set; }
        public double TotalEnergyEarned { get; set; }
        public Dictionary<string, int> UpgradeCounts { get; set; } = new();
    }
}
