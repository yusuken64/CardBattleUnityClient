using System;
using TMPro;
using UnityEngine;

public class ArenaItem : MonoBehaviour
{
	public TextMeshProUGUI Text;
	public SwitchButton SwitchButton;
	public StoryBattleModifier Modifier;

	public Color ActiveColor;
	public Color InactiveColor;

    private void Awake()
    {
        SwitchButton.OnSwitch += HandleSwitchChanged;

        HandleSwitchChanged(SwitchButton.IsOn);
    }

    private void OnDestroy()
    {
        if (SwitchButton != null)
        {
            SwitchButton.OnSwitch -= HandleSwitchChanged;
        }
    }

    private void HandleSwitchChanged(bool isOn)
    {
        Text.color = isOn ? ActiveColor : InactiveColor;
    }

    internal void Setup(StoryBattleModifier modifier)
	{
		Text.text = modifier.ModifierText;
		Modifier = modifier;
	}
}
