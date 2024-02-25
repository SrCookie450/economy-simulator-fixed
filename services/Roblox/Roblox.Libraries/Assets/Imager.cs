using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace Roblox.Libraries;

public enum ImagerFormat
{
    Undefined = 0,
    PNG = 1,
    JPEG,
    GIF,
    BMP,
}

public class UnsupportedImageFormatException : Exception
{
    
}

/// <summary>
/// Generic parse error, e.g. image format is not supported, image is corrupted, image is invalid
/// </summary>
public class InvalidImageException : Exception
{
    
}

public class Imager
{
    private Stream content { get; set; }

    public int height { get; private set; } = 0;
    public int width { get; private set; } = 0;
    public ImagerFormat imageFormat { get; private set; } = ImagerFormat.Undefined;
    
    private Image image { get; set; }
    private IImageFormat format { get; set; }
    
    private Imager(Stream content)
    {
        this.content = content;
    }

    private async Task InitializeAsync()
    {
        Image imageData;
        IImageFormat imageFormat;
        try
        {
            (imageData, imageFormat) = await SixLabors.ImageSharp.Image.LoadWithFormatAsync(content);
        }
        catch (Exception e) when (e is UnknownImageFormatException or InvalidImageContentException)
        {
            throw new InvalidImageException();
        }

        this.image = imageData;
        this.format = imageFormat;
        height = imageData.Height;
        width = imageData.Width;
        this.imageFormat = format.Name switch
        {
            "PNG" => ImagerFormat.PNG,
            "JPEG" => ImagerFormat.JPEG,
            "GIF" => ImagerFormat.GIF,
            "BMP" => ImagerFormat.BMP,
            _ => throw new UnsupportedImageFormatException()
        };
    }

    public static async Task<Imager> ReadAsync(Stream content)
    {
        var img = new Imager(content);
        await img.InitializeAsync();
        return img;
    }
}