namespace UnderwaterBaseClicker;
public sealed class DownGrade
{
    public static void Down(IEnumerable<Upgrade> upgrades)
    {
        foreach (var upgrade in  upgrades )
        {
            if (upgrade.Count != 0)
            {
                upgrade.Count -= 1;
            }
        }
    }
}