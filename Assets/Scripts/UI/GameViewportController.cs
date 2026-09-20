using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-32000)]
public sealed class GameViewportController : MonoBehaviour
{
    private readonly Dictionary<Camera, Rect> originalRects = new Dictionary<Camera, Rect>();
    private readonly RectTransform[] bars = new RectTransform[4];
    private Vector2 screenSize;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        var root = new GameObject("Game Viewport", typeof(RectTransform));
        root.layer = 5;
        DontDestroyOnLoad(root);
        root.AddComponent<GameViewportController>();
    }

    private void Awake()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        gameObject.AddComponent<GraphicRaycaster>();
        for (int i = 0; i < bars.Length; i++)
        {
            var bar = new GameObject("Viewport Bar " + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bar.layer = 5;
            bar.transform.SetParent(transform, false);
            var image = bar.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = true;
            bars[i] = (RectTransform)bar.transform;
        }
        Camera.onPreCull += ApplyCamera;
        RenderPipelineManager.beginCameraRendering += BeforeCamera;
        SceneManager.sceneLoaded += SceneLoaded;
        Refresh();
    }

    private void OnDestroy()
    {
        Camera.onPreCull -= ApplyCamera;
        RenderPipelineManager.beginCameraRendering -= BeforeCamera;
        SceneManager.sceneLoaded -= SceneLoaded;
        foreach (var entry in originalRects)
            if (entry.Key != null) entry.Key.rect = entry.Value;
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode) => Refresh();
    private void BeforeCamera(ScriptableRenderContext context, Camera camera) => ApplyCamera(camera);

    private void Update()
    {
        if (screenSize != new Vector2(Screen.width, Screen.height)) Refresh();
    }

    private void Refresh()
    {
        screenSize = new Vector2(Screen.width, Screen.height);
        foreach (var camera in Camera.allCameras) ApplyCamera(camera);
        var v = GameViewport.Normalized;
        SetBar(0, Vector2.zero, new Vector2(v.xMin, 1));
        SetBar(1, new Vector2(v.xMax, 0), Vector2.one);
        SetBar(2, new Vector2(v.xMin, 0), new Vector2(v.xMax, v.yMin));
        SetBar(3, new Vector2(v.xMin, v.yMax), new Vector2(v.xMax, 1));
    }

    private void ApplyCamera(Camera camera)
    {
        if (camera == null || camera.cameraType != CameraType.Game || camera.targetTexture != null || camera.targetDisplay != 0) return;
        if (!originalRects.TryGetValue(camera, out var original))
        {
            original = camera.rect;
            originalRects.Add(camera, original);
        }
        var viewport = GameViewport.Normalized;
        camera.rect = new Rect(viewport.position + Vector2.Scale(original.position, viewport.size),
            Vector2.Scale(original.size, viewport.size));
    }

    private void SetBar(int index, Vector2 min, Vector2 max)
    {
        var bar = bars[index];
        bar.gameObject.SetActive(max.x - min.x > 0.00001f && max.y - min.y > 0.00001f);
        bar.anchorMin = min;
        bar.anchorMax = max;
        bar.offsetMin = bar.offsetMax = Vector2.zero;
    }
}
