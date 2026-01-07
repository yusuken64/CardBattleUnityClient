using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public List<Minion> Minions;
    public RectTransform MinionArea;

    [ContextMenu("Update Minion Positions")]
    public void UpdateMinionPositions()
	{
        UpdateMinionPositions(Minions);
    }

    public void UpdateMinionPositions(List<Minion> minions)
	{
        if (minions == null || minions.Count == 0 || minions == null)
            return;

        int count = minions.Count;
        float areaWidth = MinionArea.rect.width;
        float areaHeight = MinionArea.rect.height;

        // Calculate spacing
        float spacing = minions[0].GetComponent<RectTransform>().rect.width; // default spacing
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
            float y = 0;

			Minion minion = minions[i];
            minion.SetTargetPosition(new Vector2(x, y));
            minion.Moving = true;
			//RectTransform rt = minion.GetComponent<RectTransform>();
   //         rt.anchoredPosition = ;
        }
    }
}
