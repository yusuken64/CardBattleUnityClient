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
        if (!isActiveAndEnabled) return;
        Restore();
        CaptureChildren();
        Apply();
    }

    private void CaptureChildren()
    {
        children.Clear();
        positionedChildren.Clear();
        foreach (Transform child in transform)
            if (child is RectTransform rect)
                children.Add(new Anchors { Rect = rect, Min = rect.anchorMin, Max = rect.anchorMax });
            else
                positionedChildren.Add(child, child.localPosition);
    }

    private void Restore()
    {
        foreach (var child in children)
        {
            if (child.Rect == null) continue;
            child.Rect.anchorMin = child.Min;
            child.Rect.anchorMax = child.Max;
        }
        foreach (var child in positionedChildren)
            if (child.Key != null) child.Key.localPosition = child.Value;
    }

    private void Update()
    {
        if (screenSize != new Vector2(Screen.width, Screen.height)) Apply();
    }

    private void Apply()
    {
        screenSize = new Vector2(Screen.width, Screen.height);
        var viewport = GameViewport.Normalized;
        foreach (var child in children)
        {
            if (child.Rect == null) continue;
            child.Rect.anchorMin = viewport.position + Vector2.Scale(child.Min, viewport.size);
            child.Rect.anchorMax = viewport.position + Vector2.Scale(child.Max, viewport.size);
        }
        var root = (RectTransform)transform;
        float scale = Mathf.Max(0.0001f, Mathf.Min(Screen.width / 1920f, Screen.height / 1080f));
        Vector2 logicalScreen = screenSize / scale;
        Vector2 offset = Vector2.Scale(viewport.position + Vector2.Scale(viewport.size - Vector2.one, root.pivot), logicalScreen);
        foreach (var child in positionedChildren)
            if (child.Key != null) child.Key.localPosition = child.Value + (Vector3)offset;
    }
}
