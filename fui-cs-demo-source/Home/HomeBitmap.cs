using FuiCs;

namespace FuiCsDemo.Home;

/// <summary>The images panel's app-owned bitmap: a soft red disc crossed by a yellow diagonal, in premultiplied RGBA.</summary>
internal static class HomeBitmap
{
    private const int Size = 512;

    public static Bitmap Create()
    {
        var bitmap = new Bitmap(Size, Size);
        var pixels = bitmap.Pixels;
        var center = (Size - 1.0) * 0.5;
        // The drawing is laid out on a 96 px grid and scaled up.
        var scale = Size / 96.0;
        var radius = 34.0 * scale;
        var radiusSquared = radius * radius;
        var minSquare = (int)(20.0 * scale);
        var maxSquare = (int)(76.0 * scale);
        var diagonalMin = (int)(93.0 * scale);
        var diagonalMax = (int)(99.0 * scale);
        for (var y = 0; y < Size; ++y)
        {
            for (var x = 0; x < Size; ++x)
            {
                var dx = x - center;
                var dy = y - center;
                var distanceSquared = (dx * dx) + (dy * dy);
                if (distanceSquared <= radiusSquared)
                {
                    var falloff = 1.0 - (distanceSquared / radiusSquared);
                    SetPremultiplied(pixels, x, y, 0xff, 0x40, 0x40, (byte)(64.0 + (falloff * 144.0)));
                }
                if (x >= minSquare && x <= maxSquare && y >= minSquare && y <= maxSquare && x + y >= diagonalMin && x + y <= diagonalMax)
                {
                    SetPremultiplied(pixels, x, y, 0xff, 0xff, 0x22, 0xd8);
                }
            }
        }
        bitmap.Commit();
        return bitmap;
    }

    private static byte Premultiply(byte channel, byte alpha) => (byte)(((channel * alpha) + 127) / 255);

    private static void SetPremultiplied(byte[] pixels, int x, int y, byte red, byte green, byte blue, byte alpha)
    {
        var offset = (x + (y * Size)) * 4;
        pixels[offset] = Premultiply(red, alpha);
        pixels[offset + 1] = Premultiply(green, alpha);
        pixels[offset + 2] = Premultiply(blue, alpha);
        pixels[offset + 3] = alpha;
    }
}
