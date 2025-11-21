using CardBattleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(
	fileName = "NewHeroPower",
	menuName = "Game/Cards/HeroPower Definition"
)]
public class HeroPowerDefinition : CardDefinition
{
	public string HeroPowerName;

	public TargetingType TargetingType;

	[SerializeReference]
	public List<IGameActionWrapperBase> GameActions = new List<IGameActionWrapperBase>();

	[SerializeReference]
	public IAffectedEntitySelectorWrapperBase AffectedEntitySelectorWrapper;

	internal override CardBattleEngine.Card CreateCard()
	{
		return null;
	}

	internal CardBattleEngine.HeroPower CreateHeroPower()
	{
		return new CardBattleEngine.HeroPower()
		{
			Name = HeroPowerName,
			ManaCost = Cost,
			TargetingType = TargetingType,
			GameActions = GameActions.Select(x => x.Create()),
			AffectedEntitySelector = AffectedEntitySelectorWrapper?.Create(),
			UsedThisTurn = false
		};
	}
}

