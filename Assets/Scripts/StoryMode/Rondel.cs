using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Rondel : MonoBehaviour
{
	public DifficultyDot DotsPrefab;
	public Transform DotsContainer;
	public TextMeshProUGUI ModifiersText;

	public List<DifficultyDot> Dots;

	private int currentPage;
	private StoryModeDungeonEncounterDefinition _data;

	public void Setup(StoryModeDungeonEncounterDefinition data)
	{
		this._data = data;
		foreach (Transform child in DotsContainer)
		{
			Destroy(child.gameObject);
		}

		Dots.Clear();
		foreach (var modifier in data.StoryBattleModifiers)
		{
			var dot = Instantiate(DotsPrefab, DotsContainer);
			Dots.Add(dot);
		}
		Show();
	}

	public void Show()
	{
		this.gameObject.SetActive(true);
		currentPage = 0;
		UpdateShowingPage();
	}

	public void Next_Click()
	{
		if (currentPage == Dots.Count)
		{
			return;
		}

		currentPage++;
		currentPage = Mathf.Clamp(currentPage, 0, Dots.Count);
		UpdateShowingPage();
	}

	public void Prev_Click()
	{
		currentPage--;
		currentPage = Mathf.Clamp(currentPage, 0, Dots.Count);
		UpdateShowingPage();
	}

	private void UpdateShowingPage()
	{
		for (int i = 0; i < Dots.Count; i++)
		{
			var dot = Dots[i];
			if (i < currentPage)
			{
				dot.TurnOn();
			}
			else
			{
				dot.TurnOff();
			}
		}

		var modifiers = GetActiveEffects()
			.Select(x => x.ModifierText);
		ModifiersText.text = string.Join(Environment.NewLine, modifiers);
	}

	public IEnumerable<StoryBattleModifier> GetActiveEffects()
	{
		return _data.StoryBattleModifiers.Take(currentPage);
	}
}
