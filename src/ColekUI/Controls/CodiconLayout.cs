using Avalonia;
using Avalonia.Media;

namespace ColekUI.Controls;

internal static class CodiconLayout
{
    private const double GlyphSize = 16;

    public static Matrix Compute(Rect geometryBounds, Size box, Stretch stretch)
    {
        if (stretch == Stretch.None || geometryBounds.Width <= 0 && geometryBounds.Height <= 0)
        {
            double scale = Math.Min(box.Width, box.Height) / GlyphSize;
            var offset = new Vector((box.Width - GlyphSize * scale) / 2, (box.Height - GlyphSize * scale) / 2);
            return Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(offset);
        }

        double scaleX = geometryBounds.Width > 0 ? box.Width / geometryBounds.Width : double.PositiveInfinity;
        double scaleY = geometryBounds.Height > 0 ? box.Height / geometryBounds.Height : double.PositiveInfinity;
        double fit = Math.Min(scaleX, scaleY);

        double tx = (box.Width - geometryBounds.Width * fit) / 2 - geometryBounds.X * fit;
        double ty = (box.Height - geometryBounds.Height * fit) / 2 - geometryBounds.Y * fit;
        return Matrix.CreateScale(fit, fit) * Matrix.CreateTranslation(tx, ty);
    }
}
