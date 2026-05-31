using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UnderwaterBaseClicker.Ui;

public static class GameSprites
{
    private const string Root = "Assets/Sprites/";

    public static readonly Dictionary<string, string> UpgradeSprites = new()
    {
        ["algae"] = "seaweed_1.png",
        ["drone"] = "drone_fish.png",
        ["pressure"] = "pressure_chain.png",
        ["sub"] = "submarine.png",
        ["sonar"] = "sonar_mast.png",
        ["lab"] = "lab_pearl.png",
        ["reactor"] = "reactor_core.png",
        ["dome"] = "dome_shield.png"
    };

    public static BitmapImage Load(string file)
    {
        var uri = new Uri($"pack://application:,,,/{Root}{file}", UriKind.Absolute);
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.UriSource = uri;
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    public static Image Create(string file, double width, double? height = null, bool pixelArt = false)
    {
        var img = new Image
        {
            Source = Load(file),
            Width = width,
            Height = height ?? width,
            Stretch = Stretch.Uniform,
        };
        RenderOptions.SetBitmapScalingMode(img, pixelArt ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.HighQuality);
        return img;
    }

    public static Image CreateSheetFrame(string file, int frameIndex, int totalFrames, double displayWidth, bool pixelArt = true)
    {
        var source = Load(file);
        var frameW = source.PixelWidth / totalFrames;
        var cropped = new CroppedBitmap(source, new Int32Rect(frameIndex * frameW, 0, frameW, source.PixelHeight));
        cropped.Freeze();

        var img = new Image
        {
            Source = cropped,
            Width = displayWidth,
            Height = displayWidth * source.PixelHeight / frameW,
            Stretch = Stretch.Uniform,
        };
        RenderOptions.SetBitmapScalingMode(img, pixelArt ? BitmapScalingMode.NearestNeighbor : BitmapScalingMode.HighQuality);
        return img;
    }

    public static string GetUpgradeSprite(string upgradeId, int level = 0) =>
        upgradeId switch
        {
            "algae" => level % 2 == 0 ? "seaweed_1.png" : "seaweed_2.png",
            _ => UpgradeSprites.GetValueOrDefault(upgradeId, "coin.png")
        };
}
