using UnityEngine;

public static class GameViewport
{
    public static readonly Vector2 ReferenceResolution = new Vector2(1920, 1080);

    public static Rect Fit(float width, float height)
    {
        if (width <= 0 || height <= 0) return new Rect(0, 0, 1, 1);
        float scale = Mathf.Min(width / ReferenceResolution.x, height / ReferenceResolution.y);
        var size = ReferenceResolution * scale;
        return new Rect((width - size.x) / 2f, (height - size.y) / 2f, size.x, size.y);
    }

    public static Rect Pixels => Fit(Screen.width, Screen.height);
    public static Rect Normalized
    {
        get
        {
            var rect = Pixels;
            return new Rect(rect.x / Mathf.Max(1, Screen.width), rect.y / Mathf.Max(1, Screen.height),
                rect.width / Mathf.Max(1, Screen.width), rect.height / Mathf.Max(1, Screen.height));
        }
    }

    public static bool Contains(Vector2 position) => Pixels.Contains(position);
}
