using UnityEngine;
using UnityEngine.UI;

// Keeps sprite-specific geometry and layout metrics independent of Transform scale.
// Used by StoryMode's baked, unit-scale UI (including its mirrored dungeon image).
[AddComponentMenu("UI/Image Metrics")]
[RequireComponent(typeof(Image))]
public sealed class UIImageMetrics : BaseMeshEffect, ILayoutElement
{
    [SerializeField] private bool mirrorHorizontally;
    [SerializeField] private float horizontalBorderScale = 1f;
    [SerializeField] private bool overridePreferredSize;
    [SerializeField] private Vector2 preferredSizeScale = Vector2.one;

    private Image TargetImage => (Image)graphic;

    public float minWidth => -1f;
    public float minHeight => -1f;
    public float preferredWidth => overridePreferredSize ? TargetImage.preferredWidth * preferredSizeScale.x : -1f;
    public float preferredHeight => overridePreferredSize ? TargetImage.preferredHeight * preferredSizeScale.y : -1f;
    public float flexibleWidth => -1f;
    public float flexibleHeight => -1f;
    public int layoutPriority => 1;
    public void CalculateLayoutInputHorizontal() { }
    public void CalculateLayoutInputVertical() { }

    public override void ModifyMesh(VertexHelper vertices)
    {
        if (!IsActive()) return;

        var image = TargetImage;
        var rect = image.GetPixelAdjustedRect();
        var sprite = image.overrideSprite;
        bool adjustBorder = image.type == Image.Type.Sliced && sprite != null
            && !Mathf.Approximately(horizontalBorderScale, 1f);
        if (!adjustBorder && !mirrorHorizontally) return;
        float left = 0f, right = 0f, targetLeft = 0f, targetRight = 0f;
        if (adjustBorder)
        {
            float pixelsPerUnit = image.pixelsPerUnit * image.pixelsPerUnitMultiplier;
            left = sprite.border.x / pixelsPerUnit;
            right = sprite.border.z / pixelsPerUnit;
            targetLeft = left * horizontalBorderScale;
            targetRight = right * horizontalBorderScale;
            FitBorders(ref left, ref right, rect.width);
            FitBorders(ref targetLeft, ref targetRight, rect.width);
        }

        var vertex = new UIVertex();
        for (int i = 0; i < vertices.currentVertCount; i++)
        {
            vertices.PopulateUIVertex(ref vertex, i);
            float x = vertex.position.x - rect.xMin;
            if (adjustBorder)
            {
                if (x <= left && left > 0f)
                    x *= targetLeft / left;
                else if (x >= rect.width - right && right > 0f)
                    x = rect.width - (rect.width - x) * targetRight / right;
                else if (rect.width > left + right)
                    x = targetLeft + (x - left) * (rect.width - targetLeft - targetRight)
                        / (rect.width - left - right);
            }
            if (mirrorHorizontally) x = rect.width - x;
            vertex.position.x = rect.xMin + x;
            vertices.SetUIVertex(vertex, i);
        }
    }

    private static void FitBorders(ref float left, ref float right, float width)
    {
        float total = left + right;
        if (total <= width || total <= 0f) return;
        float ratio = Mathf.Max(0f, width) / total;
        left *= ratio;
        right *= ratio;
    }
}
