using System;
using TMPro;
using UnityEngine;

public class ArenaItem : MonoBehaviour
{
	public TextMeshProUGUI Text;
	public SwitchButton SwitchButton;
	public StoryBattleModifier Modifier;

	internal void Setup(StoryBattleModifier modifier)
	{
		Text.text = modifier.ModifierText;
		Modifier = modifier;
	}
}
