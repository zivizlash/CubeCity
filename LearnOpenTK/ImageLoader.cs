using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LearnOpenTK;

public readonly record struct ByteImage(byte[] Data, int Width, int Height)
{
    public readonly int Count = Width * Height;
}

public static class ImageLoader
{
    public static ByteImage LoadImageBytes(string name)
    {
        using var image = Image.Load<Rgba32>(name);

        image.Mutate(m => m.Rotate(RotateMode.Rotate90));

        var pixels = new byte[image.Width * image.Height * 4];

        for (int w = 0, counter = 0; w < image.Width; w++)
        {
            for (int h = 0; h < image.Height; h++)
            {
                var pixel = image[w, h];

                pixels[counter + 0] = pixel.R;
                pixels[counter + 1] = pixel.G;
                pixels[counter + 2] = pixel.B;
                pixels[counter + 3] = pixel.A;

                counter += 4;
            }
        }

        return new ByteImage(pixels, image.Width, image.Height);
    }
}
