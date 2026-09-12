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

	[SerializeReference]
	public List<IGameActionWrapperBase> GameActions = new List<IGameActionWrapperBase>();

	[SerializeReference]
	public IAffectedEntitySelectorWrapperBase AffectedEntitySelectorWrapper;

	public override CardBattleEngine.Card CreateCard()
	{
		return null;
	}

	internal CardBattleEngine.HeroPower CreateHeroPower()
	{
		return new CardBattleEngine.HeroPower()
		{
			Name = HeroPowerName,
			ManaCost = Cost,
			ValidTargetSelector = ValidTargetSelector.Create(),
			CastRestriction = CastRestriction.Create(),
			GameActions = GameActions.Select(x => x.Create()),
			AffectedEntitySelector = AffectedEntitySelectorWrapper?.Create(),
			UsedThisTurn = false
		};
	}
	public static CardBattleEngine.HeroPower CreateHeroPowerFromHeroCard(MinionCardDefinition minionCard, CardBattleEngine.Player owner = null)
	{
		var leader = minionCard?.CreateCard() as MinionCard;
		if (leader != null) leader.Owner = owner;
		return CardBattleEngine.HeroPower.FromLeader(leader);
	}
}
