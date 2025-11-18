using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
	public List<GameObject> Cards;
	public RectTransform CardArea;

    public float radius = 500f; // how large the arc is (bigger radius = flatter fan)
    public float maxAngle = 30f; // total spread angle (left to right)

    [ContextMenu("Update Card Positions")]
    public void UpdateCardPositions()
    {
        if (Cards == null || Cards.Count == 0 || CardArea == null)
            return;

        int count = Cards.Count;
        float areaWidth = CardArea.rect.width;
        float areaHeight = CardArea.rect.height;

        // Calculate spacing
        float spacing = Cards[0].GetComponent<RectTransform>().rect.width; // default spacing
        float totalWidth = spacing * (count - 1);

        if (totalWidth > areaWidth)
        {
            // squeeze cards to fit inside container
            spacing = areaWidth / Mathf.Max(1, count - 1);
            totalWidth = spacing * (count - 1);
        }

        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            float x = startX + spacing * i;

            // Rotation: map x to angle
            float t = (count == 1) ? 0.5f : (float)i / (count - 1); // 0..1
            float angle = Mathf.Lerp(-maxAngle / 2f, maxAngle / 2f, t);

            // Calculate y for subtle arc using circle formula
            float rad = angle * Mathf.Deg2Rad;
            //float y = -Mathf.Cos(rad) * radius + radius;
            float y = 0;

            RectTransform rt = Cards[i].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(x, y);
            rt.localRotation = Quaternion.Euler(0, 0, -angle);
        }
    }

}
