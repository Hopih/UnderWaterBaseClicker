using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using UnderwaterBaseClicker.Ui;

namespace UnderwaterBaseClicker;

public partial class MainWindow : Window
{
    private readonly GameState _state = new();
    private readonly DispatcherTimer _gameTimer;
    private readonly Dictionary<Upgrade, Border> _upgradeCards = new();
    private readonly Dictionary<Upgrade, Button> _buyButtons = new();

    private double _displayedEnergy;

    public MainWindow()
    {
        InitializeComponent();
        _state.Load();
        _displayedEnergy = _state.Energy;

        BaseView.ReactorClicked += (_, _) => OnReactorClick();
        BaseView.SyncFromUpgrades(_state.Upgrades, animateNew: false);

        BuildUpgradeCards();

        _gameTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _gameTimer.Tick += GameTimer_Tick;
        _gameTimer.Start();

        Closed += (_, _) => _state.Save();

        RefreshUi();
    }

    private void BuildUpgradeCards()
    {
        foreach (var upgrade in _state.Upgrades)
        {
            var buy = new Button
            {
                Style = (Style)FindResource("BuyUpgradeButtonStyle"),
                VerticalAlignment = VerticalAlignment.Center
            };
            buy.Click += (_, _) => TryBuy(upgrade);

            var card = new Border { Style = (Style)FindResource("UpgradeCardStyle") };
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var iconBg = new Border
            {
                Width = 44,
                Height = 44,
                CornerRadius = new CornerRadius(12),
                Background = new LinearGradientBrush(
                    Color.FromRgb(0, 140, 180),
                    Color.FromRgb(0, 80, 120),
                    new Point(0, 0), new Point(1, 1)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                Child = new Image
                {
                    Source = GameSprites.Load(GameSprites.GetUpgradeSprite(upgrade.Id)),
                    Width = 32,
                    Height = 32,
                    Stretch = Stretch.Uniform
                }
            };

            var info = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            info.Children.Add(new TextBlock
            {
                Text = upgrade.Name,
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Foreground = (Brush)FindResource("TextLightBrush")
            });
            info.Children.Add(new TextBlock
            {
                Text = upgrade.EffectLabel,
                FontSize = 11,
                Foreground = (Brush)FindResource("GoldBrush"),
                Margin = new Thickness(0, 2, 0, 0)
            });
            info.Children.Add(new TextBlock
            {
                Text = upgrade.Description,
                FontSize = 10,
                Foreground = (Brush)FindResource("TextMutedBrush"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 0)
            });

            Grid.SetColumn(iconBg, 0);
            Grid.SetColumn(info, 1);
            Grid.SetColumn(buy, 2);
            grid.Children.Add(iconBg);
            grid.Children.Add(info);
            grid.Children.Add(buy);
            card.Child = grid;

            _upgradeCards[upgrade] = card;
            _buyButtons[upgrade] = buy;
            UpgradesPanel.Children.Add(card);
        }
    }

    private void OnReactorClick()
    {
        var gained = _state.EnergyPerClick;
        _state.AddClickEnergy();

        var clickPoint = BaseView.GetReactorCenter(FxCanvas);
        ClickFx.SpawnFloatingEnergy(FxCanvas, clickPoint, $"+{Upgrade.FormatNumber(gained)}");
        ClickFx.SpawnRipple(FxCanvas, clickPoint, 100);
        ClickFx.SpawnSparkles(FxCanvas, clickPoint, 6);
        BaseView.PulseReactor();

        RefreshUi();
    }

    private void TryBuy(Upgrade upgrade)
    {
        if (!_state.TryBuyUpgrade(upgrade))
            return;

        BaseView.OnUpgradePurchased(upgrade);

        if (_upgradeCards.TryGetValue(upgrade, out var card))
            ClickFx.FlashElement(card);

        var modulePoint = BaseView.GetModuleAnchor(upgrade.Id, FxCanvas);
        ClickFx.SpawnFloatingEnergy(FxCanvas, modulePoint, $"{upgrade.Icon} {upgrade.Name}");
        ClickFx.SpawnRipple(FxCanvas, modulePoint, 60);
        ClickFx.SpawnSparkles(FxCanvas, modulePoint, 10);

        BaseHintText.Text = upgrade.Count == 1
            ? $"✨ Построено: {upgrade.Name}"
            : $"⬆️ {upgrade.Name} — уровень {upgrade.Count}";

        RefreshUi();
    }

    private void GameTimer_Tick(object? sender, EventArgs e)
    {
        var prevEnergy = _state.Energy;
        _state.AddPassiveEnergy(_gameTimer.Interval.TotalSeconds);

        if (_state.EnergyPerSecond > 0 && _state.Energy > prevEnergy)
            PulseEnergyCounter();

        SmoothEnergyDisplay();
        RefreshUi();
    }

    private void PulseEnergyCounter()
    {
        var anim = new DoubleAnimation(1, 1.08, TimeSpan.FromMilliseconds(100)) { AutoReverse = true };
        EnergyPulse.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
        EnergyPulse.BeginAnimation(ScaleTransform.ScaleYProperty, anim.Clone());
    }

    private void SmoothEnergyDisplay()
    {
        var diff = _state.Energy - _displayedEnergy;
        if (Math.Abs(diff) < 0.5)
            _displayedEnergy = _state.Energy;
        else
            _displayedEnergy += diff * 0.22;
    }

    private void RefreshUi()
    {
        EnergyText.Text = Upgrade.FormatNumber(_displayedEnergy);
        RatesText.Text = $"+{Upgrade.FormatNumber(_state.EnergyPerClick)}/тап  ·  +{Upgrade.FormatNumber(_state.EnergyPerSecond)}/с";
        StatsText.Text = $"Всего: {Upgrade.FormatNumber(_state.TotalEnergyEarned)}  ·  🏗️ {_state.Upgrades.Sum(u => u.Count)}";

        foreach (var upgrade in _state.Upgrades)
        {
            if (!_buyButtons.TryGetValue(upgrade, out var buy))
                continue;

            var cost = upgrade.CurrentCost;
            buy.Content = $"{Upgrade.FormatNumber(cost)} ⚡\nLv.{upgrade.Count}";
            buy.IsEnabled = _state.Energy >= cost;
        }
    }
}
