namespace MauiYoloV5;

/// <summary>
/// Label of detected object.
/// </summary>
public record YoloLabel(int Id, string Name, Brush Color, YoloLabelKind Kind)
{
    public YoloLabel(int id, string name)
        : this(id, name, Brush.Yellow, YoloLabelKind.Generic) { }
}
