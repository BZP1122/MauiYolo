using System.Drawing;

namespace MauiYoloV5.Extensions;

public static class RectangleExtensions
{
    /// <summary>
    /// Calculates rectangle area.
    /// </summary>
    public static float Area(this RectangleF source)
    {
        return source.Width * source.Height;
    }

    public static Rectangle RectF2Rect(this RectangleF source)
    {
        return new Rectangle(
            (int)source.Left,
            (int)source.Top,
            (int)source.Width,
            (int)source.Height
        );
    }
}
