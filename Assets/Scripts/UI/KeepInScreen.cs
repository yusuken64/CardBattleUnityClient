using UnityEngine;

public class KeepInScreen : MonoBehaviour
{
    public RectTransform rect;
    private readonly Vector3[] corners = new Vector3[4];
    private bool positioned;

    private void Awake()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
    }

    private Camera RenderCamera()
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
        return (canvas != null ? canvas.rootCanvas.worldCamera : null) ?? Camera.main;
    }

    internal void SetPosition(Vector3 world)
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        var camera = RenderCamera();
        world.z = rect.position.z;
        if (camera == null && Camera.main != null)
        {
            var screen = Camera.main.WorldToScreenPoint(world);
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, screen, null, out var position))
                world = position;
        }
        rect.position = world;
        positioned = true;
        ClampToViewport(camera);
    }

    private void LateUpdate()
    {
        if (positioned && rect != null) ClampToViewport(RenderCamera());
    }

    private void ClampToViewport(Camera camera)
    {
        rect.GetWorldCorners(corners);
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        foreach (var corner in corners)
        {
            var screen = RectTransformUtility.WorldToScreenPoint(camera, corner);
            min = Vector2.Min(min, screen);
            max = Vector2.Max(max, screen);
        }
        var viewport = GameViewport.Pixels;
        var shift = new Vector2(Correction(min.x, max.x, viewport.xMin, viewport.xMax),
            Correction(min.y, max.y, viewport.yMin, viewport.yMax));
        if (shift.sqrMagnitude < 0.0001f) return;
        var pivot = RectTransformUtility.WorldToScreenPoint(camera, rect.position);
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, pivot + shift, camera, out var clamped))
            rect.position = clamped;
    }

    public static float Correction(float min, float max, float viewportMin, float viewportMax)
    {
        if (max - min > viewportMax - viewportMin) return (viewportMin + viewportMax - min - max) / 2f;
        if (min < viewportMin) return viewportMin - min;
        if (max > viewportMax) return viewportMax - max;
        return 0f;
    }
}
