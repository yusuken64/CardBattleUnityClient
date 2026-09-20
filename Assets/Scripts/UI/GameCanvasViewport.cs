using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Maps the authored anchors into the game area without adding a scaled layout parent.
[DefaultExecutionOrder(-31000)]
[RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
public sealed class GameCanvasViewport : MonoBehaviour
{
    private struct Anchors
    {
        public RectTransform Rect;
        public Vector2 Min, Max;
    }

    private readonly List<Anchors> children = new List<Anchors>();
    private readonly Dictionary<Transform, Vector3> positionedChildren = new Dictionary<Transform, Vector3>();
    private Vector2 screenSize;
    private bool childrenDirty;

    private void OnEnable()
    {
        var scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = GameViewport.ReferenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        CaptureChildren();
        Apply();
    }

    private void OnDisable()
    {
        Restore();
        children.Clear();
        positionedChildren.Clear();
    }

    private void OnTransformChildrenChanged()
    {
        // Unity can invoke this while TMP is destroying its dropdown/blocker.
        // Mutating anchors inside that native hierarchy operation can crash the player.
        childrenDirty = true;
    }

    private void CaptureChildren()
    {
        children.Clear();
        positionedChildren.Clear();
        foreach (Transform child in transform)
        {
            // Dropdowns position their temporary overlay canvases in screen space.
            if (child.GetComponent<Canvas>() != null) continue;
            if (child is RectTransform rect)
                children.Add(new Anchors { Rect = rect, Min = rect.anchorMin, Max = rect.anchorMax });
            else
                positionedChildren.Add(child, child.localPosition);
        }
        childrenDirty = false;
    }

    private void Restore()
    {
        foreach (var child in children)
        {
            if (child.Rect == null || child.Rect.parent != transform) continue;
            child.Rect.anchorMin = child.Min;
            child.Rect.anchorMax = child.Max;
        }
        foreach (var child in positionedChildren)
            if (child.Key != null && child.Key.parent == transform) child.Key.localPosition = child.Value;
    }

    private void Update()
    {
        if (childrenDirty)
        {
            Restore();
            CaptureChildren();
            Apply();
            return;
        }
        if (screenSize != new Vector2(Screen.width, Screen.height)) Apply();
    }

    private void Apply()
    {
        screenSize = new Vector2(Screen.width, Screen.height);
        var viewport = GameViewport.Normalized;
        foreach (var child in children)
        {
            if (child.Rect == null || child.Rect.parent != transform) continue;
            child.Rect.anchorMin = viewport.position + Vector2.Scale(child.Min, viewport.size);
            child.Rect.anchorMax = viewport.position + Vector2.Scale(child.Max, viewport.size);
        }
        var root = (RectTransform)transform;
        float scale = Mathf.Max(0.0001f, Mathf.Min(Screen.width / 1920f, Screen.height / 1080f));
        Vector2 logicalScreen = screenSize / scale;
        Vector2 offset = Vector2.Scale(viewport.position + Vector2.Scale(viewport.size - Vector2.one, root.pivot), logicalScreen);
        foreach (var child in positionedChildren)
            if (child.Key != null && child.Key.parent == transform) child.Key.localPosition = child.Value + (Vector3)offset;
    }
}
