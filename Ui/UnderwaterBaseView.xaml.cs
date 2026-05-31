using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace UnderwaterBaseClicker.Ui;

public partial class UnderwaterBaseView : UserControl
{
    private readonly Dictionary<string, int> _syncedCounts = new();
    private readonly Dictionary<string, Canvas> _moduleHosts = new();
    private readonly List<AnimatedDrone> _drones = [];
    private readonly BubbleField _bubbles;
    private readonly DispatcherTimer _animTimer;
    private double _animTime;

    public event EventHandler? ReactorClicked;

    public UnderwaterBaseView()
    {
        InitializeComponent();

        _moduleHosts["algae"] = AlgaeLayer;
        _moduleHosts["sub"] = DockSlot;

        _bubbles = new BubbleField(BubbleCanvas, count: 36);

        _animTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        _animTimer.Tick += (_, _) => OnAnimTick();
        Loaded += (_, _) =>
        {
            SandDecorBuilder.Populate(SandDecorLayer);
            _bubbles.Start();
            _animTimer.Start();
            StartReactorBreathing();
        };
        Unloaded += (_, _) =>
        {
            _bubbles.Stop();
            _animTimer.Stop();
            ModuleIdleAnimator.Clear();
        };
    }

    public Point GetReactorCenter(UIElement relativeTo) =>
        ReactorButton.TranslatePoint(new Point(ReactorButton.ActualWidth / 2, ReactorButton.ActualHeight / 2), relativeTo);

    public Point GetModuleAnchor(string upgradeId, UIElement relativeTo)
    {
        if (!ModuleLayout.HasVisual(upgradeId))
            return GetReactorCenter(relativeTo);

        var levelIndex = Math.Max(0, _syncedCounts.GetValueOrDefault(upgradeId) - 1);
        return Scene.TranslatePoint(ModuleLayout.GetAnchor(upgradeId, levelIndex), relativeTo);
    }

    public void SyncFromUpgrades(IEnumerable<Upgrade> upgrades, bool animateNew)
    {
        ModuleIdleAnimator.Clear();
        AlgaeLayer.Children.Clear();
        DockSlot.Children.Clear();
        DroneLayer.Children.Clear();
        _drones.Clear();

        foreach (var upgrade in upgrades)
        {
            _syncedCounts[upgrade.Id] = 0;
            for (var i = 0; i < upgrade.Count; i++)
                AddModuleVisual(upgrade.Id, i, animateNew);
            _syncedCounts[upgrade.Id] = upgrade.Count;
        }
    }

    public void OnUpgradePurchased(Upgrade upgrade)
    {
        if (!ModuleLayout.HasVisual(upgrade.Id))
            return;
        AddModuleVisual(upgrade.Id, upgrade.Count - 1, animate: true);
        _syncedCounts[upgrade.Id] = upgrade.Count;
    }

    public void PulseReactor() => ClickFx.PulseScale(ReactorScale, 1.12);

    private void ReactorButton_Click(object sender, RoutedEventArgs e) =>
        ReactorClicked?.Invoke(this, EventArgs.Empty);

    private void AddModuleVisual(string upgradeId, int levelIndex, bool animate)
    {
        if (!ModuleLayout.HasVisual(upgradeId))
            return;

        if (upgradeId == "drone")
        {
            SpawnDrone(levelIndex, animate);
            return;
        }

        if (_moduleHosts.TryGetValue(upgradeId, out var host))
            PlaceInSlot(host, upgradeId, levelIndex, animate);
    }

    private void PlaceInSlot(Canvas host, string upgradeId, int levelIndex, bool animate)
    {
        var hostDef = ModuleLayout.GetHost(upgradeId);
        var slot = hostDef.Slots[Math.Min(levelIndex, hostDef.Slots.Length - 1)];

        var content = BaseArtBuilder.CreateModule(upgradeId, levelIndex);
        if (content is null)
            return;

        var (w, h) = GetModuleSize(upgradeId);
        var slotRoot = BaseArtBuilder.WrapForSlot(content, w, h, anchorBottom: upgradeId == "algae");

        host.Children.Add(slotRoot);
        Canvas.SetLeft(slotRoot, slot.X);
        Canvas.SetTop(slotRoot, slot.Y);
        Panel.SetZIndex(slotRoot, slot.ZIndex);

        var idle = upgradeId switch
        {
            "algae" => IdleAnimKind.Sway,
            "sub" => IdleAnimKind.Float,
            _ => IdleAnimKind.Float
        };
        ModuleIdleAnimator.Attach(content, idle, levelIndex);

        if (animate)
            ModuleSpawnAnimation.Play(slotRoot);
        else
            ModuleSpawnAnimation.SetVisible(slotRoot);
    }

    private void SpawnDrone(int index, bool animate)
    {
        var drone = new AnimatedDrone(index);
        DroneLayer.Children.Add(drone.Root);
        _drones.Add(drone);

        if (animate)
            ModuleSpawnAnimation.Play(drone.Root);
        else
            ModuleSpawnAnimation.SetVisible(drone.Root);
    }

    private static (double W, double H) GetModuleSize(string upgradeId) => upgradeId switch
    {
        "algae" => (72, 58),
        "sub" => (96, 48),
        _ => (48, 48)
    };

    private void OnAnimTick()
    {
        _animTime += 0.04;
        ModuleIdleAnimator.Tick(_animTime);

        var center = ModuleLayout.DroneOrbitCenter;
        foreach (var drone in _drones)
        {
            drone.Update(_animTime);
            var angle = _animTime * (0.85 + drone.Index * 0.12) + drone.Index * 2.2;
            var radiusX = 130 + drone.Index * 16;
            var radiusY = 75 + drone.Index * 10;
            Canvas.SetLeft(drone.Root, center.X + Math.Cos(angle) * radiusX - 18);
            Canvas.SetTop(drone.Root, center.Y + Math.Sin(angle * 0.78) * radiusY - 18);
        }
    }

    private sealed class AnimatedDrone
    {
        private readonly Image _image;
        private readonly BitmapSource _sheet;
        private readonly int _frames;
        private int _lastFrame = -1;

        public AnimatedDrone(int index)
        {
            Index = index;
            var file = "drone_fish.png";
            _sheet = GameSprites.Load(file);
            _frames = 4;
            _image = GameSprites.CreateSheetFrame(file, 0, _frames, 36);
            Root = new Border
            {
                Child = _image,
                Background = Brushes.Transparent,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
        }

        public int Index { get; }
        public Border Root { get; }

        public void Update(double time)
        {
            var frame = ((int)(time * 8) + Index) % _frames;
            if (frame == _lastFrame)
                return;
            _lastFrame = frame;
            var frameW = _sheet.PixelWidth / _frames;
            var cropped = new CroppedBitmap(_sheet, new Int32Rect(frame * frameW, 0, frameW, _sheet.PixelHeight));
            cropped.Freeze();
            _image.Source = cropped;
        }
    }

    private void StartReactorBreathing()
    {
        var anim = new DoubleAnimation(1, 1.06, TimeSpan.FromSeconds(2))
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase()
        };
        ReactorScale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
        ReactorScale.BeginAnimation(ScaleTransform.ScaleYProperty, anim.Clone());
    }
}
