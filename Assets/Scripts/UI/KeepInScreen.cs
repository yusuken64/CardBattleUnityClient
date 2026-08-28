using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeepInScreen : MonoBehaviour
{
    public RectTransform rect;

	private Vector3 topLeft;
	private Vector3 bottomRight;
	private float halfWidth;
	private float halfHeight;
	private float minX;
	private float minY;
	private float maxX;
	private float maxY;

	private void Start()
	{
        rect = GetComponent<RectTransform>();
        RecalculateBounds();
    }

    void OnRectTransformDimensionsChange()
    {
        RecalculateBounds();
    }

	private void RecalculateBounds()
    {
        topLeft = Camera.main.ScreenToWorldPoint(new Vector2(0, 0));
        bottomRight = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));

        halfWidth = rect.rect.width / 2;
        halfHeight = rect.rect.height / 2;

        minX = topLeft.x + halfWidth;
        minY = topLeft.y + halfHeight;

        maxX = bottomRight.x - halfWidth;
        maxY = bottomRight.y - halfHeight;
    }

    internal void SetPosition(Vector3 world)
    {
        var world2 = new Vector3(Mathf.Clamp(world.x, minX, maxX),
            Mathf.Clamp(world.y, minY, maxY));

        this.transform.position = world2;
    }
}
