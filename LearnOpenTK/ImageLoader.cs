using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnOpenTK;

public class ImageLoader
{
    public byte[] Load(string name)
    {
        var image = Image.Load<Rgba32>(name);

        throw new NotImplementedException();
    }
}
