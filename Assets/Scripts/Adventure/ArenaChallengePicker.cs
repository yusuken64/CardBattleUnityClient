using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArenaChallengePicker : MonoBehaviour
{
	public ArenaItem ArenaItemPrefab;
	public Transform ArenaItemContainer;

	[SerializeReference]
	public List<StoryBattleModifier> BattleModifiers;

	public List<ArenaItem> ArenaItems = new();

	public void Setup()
	{
		ArenaItems.Clear();
		foreach (Transform child in ArenaItemContainer)
		{
			Destroy(child.gameObject);
		}

		foreach(var modifier in BattleModifiers)
		{
			var newItem = Instantiate(ArenaItemPrefab, ArenaItemContainer);
			newItem.Setup(modifier);
			ArenaItems.Add(newItem);
		}
	}

	internal List<string> GetActiveModifierIds()
	{
		return ArenaItems
			.Where(x => x.SwitchButton.IsOn)
			.Select(x => x.Modifier.Id)
			.ToList();
	}

	internal List<StoryBattleModifier> GetModifiersFromIds(List<string> ids)
	{
		var lookup = BattleModifiers.ToDictionary(x => x.Id);

		var result = new List<StoryBattleModifier>();

		foreach (var id in ids)
		{
			if (lookup.TryGetValue(id, out var modifier))
				result.Add(modifier);
		}

		return result;
	}
}
