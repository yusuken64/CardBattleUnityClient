using CardBattleEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(
	fileName = "NewWeaponCard",
	menuName = "Game/Cards/WeaponCard Definition"
)]
public class WeaponCardDefinition : CardDefinition
{
	public string WeaponName;
	public int Attack;
	public int Durability;

	public List<TriggeredEffectWrapper> TriggeredEffectWrappers;

	internal override CardBattleEngine.Card CreateCard()
	{
		var weapon = new WeaponCard(WeaponName, Cost, Attack, Durability);
		weapon.TriggeredEffects.AddRange(TriggeredEffectWrappers.Select(x => x.CreateEffect()));

		return weapon;
	}
}
