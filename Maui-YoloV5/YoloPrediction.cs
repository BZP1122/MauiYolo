using System.Drawing;

namespace MauiYoloV5;

/// <summary>
/// Object prediction.
/// </summary>
public class YoloPrediction
{
    public YoloPrediction(YoloLabel label, float score, RectangleF rectangle, float[] Mask = null)
    {
        Label = label;
        Score = score;
        Rectangle = rectangle;

        OutMask = Mask;
    }

    public int Area { get; set; }

    public YoloLabel Label { get; protected set; }
    public float Score { get; protected set; }
    public RectangleF Rectangle { get; protected set; }

    public float[] OutMask { get; internal set; }
}
