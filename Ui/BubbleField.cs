using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UnderwaterBaseClicker.Ui;

public sealed class BubbleField
{
    private readonly Canvas _canvas;
    private readonly DispatcherTimer _timer;
    private readonly List<Bubble> _bubbles = [];
    private readonly Random _rng = new();
    private readonly bool _riseFromBottom;

    public BubbleField(Canvas canvas, int count = 32, bool riseFromBottom = true)
    {
        _canvas = canvas;
        _canvas.IsHitTestVisible = false;
        _riseFromBottom = riseFromBottom;

        for (var i = 0; i < count; i++)
            _bubbles.Add(CreateBubble());

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
        _timer.Tick += (_, _) => Tick();
    }

    public void Start() => _timer.Start();

    public void Stop() => _timer.Stop();

    private Bubble CreateBubble()
    {
        var size = _rng.Next(5, 22);
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Fill = new SolidColorBrush(Color.FromArgb((byte)_rng.Next(30, 80), 140, 230, 255)),
            Stroke = new SolidColorBrush(Color.FromArgb(70, 200, 245, 255)),
            StrokeThickness = 1
        };

        _canvas.Children.Add(ellipse);

        var w = Math.Max(_canvas.ActualWidth, 640);
        var h = Math.Max(_canvas.ActualHeight, 480);

        var bubble = new Bubble(ellipse, size)
        {
            X = _rng.NextDouble() * w,
            Y = _riseFromBottom
                ? h * 0.45 + _rng.NextDouble() * h * 0.55
                : _rng.NextDouble() * h,
            SpeedY = 0.4 + _rng.NextDouble() * 1.4,
            WobbleSpeed = 0.6 + _rng.NextDouble() * 2.2,
            WobblePhase = _rng.NextDouble() * Math.PI * 2
        };

        Canvas.SetLeft(ellipse, bubble.X);
        Canvas.SetTop(ellipse, bubble.Y);
        return bubble;
    }

    private void Tick()
    {
        var w = Math.Max(_canvas.ActualWidth, 640);
        var h = Math.Max(_canvas.ActualHeight, 480);
        var time = Environment.TickCount64 / 1000.0;

        foreach (var b in _bubbles)
        {
            b.Y -= b.SpeedY;
            b.X += Math.Sin(time * b.WobbleSpeed + b.WobblePhase) * 0.5;

            if (b.Y < -b.Size)
            {
                b.Y = h + b.Size;
                b.X = _rng.NextDouble() * w;
                if (_riseFromBottom)
                    b.Y = h * 0.55 + _rng.NextDouble() * h * 0.4;
            }

            Canvas.SetLeft(b.Shape, b.X);
            Canvas.SetTop(b.Shape, b.Y);
        }
    }

    private sealed class Bubble(Ellipse shape, double size)
    {
        public Ellipse Shape { get; } = shape;
        public double Size { get; } = size;
        public double X { get; set; }
        public double Y { get; set; }
        public double SpeedY { get; set; }
        public double WobbleSpeed { get; set; }
        public double WobblePhase { get; set; }
    }
}
