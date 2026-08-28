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
	public static CardBattleEngine.HeroPower CreateHeroPowerFromHeroCard(MinionCardDefinition minionCard)
	{
		if (minionCard == null) { return null; }

		List<TriggeredEffectWrapper> minionTriggeredEffects = minionCard.MinionTriggeredEffects;
		if (minionTriggeredEffects != null &&
			minionTriggeredEffects.Any() &&
			minionTriggeredEffects[0].EffectTrigger == EffectTrigger.Battlecry)
		{
			TriggeredEffectWrapper triggeredEffectWrapper = minionTriggeredEffects[0];
			IAffectedEntitySelector affectedEntitySelector = minionTriggeredEffects[0].AffectedEntitySelectorWrapper?.Create();
			if (affectedEntitySelector is ContextSelector contextSelector)
			{
				if (contextSelector.IncludeSummonedMinion)
				{
					contextSelector.IncludeSourcePlayer = true;
				}
			}

			return new CardBattleEngine.HeroPower()
			{
				Name = $"Invoke {minionCard.CardName}",
				ValidTargetSelector = minionCard.ValidTargetSelector?.Create(),
				CastRestriction = minionCard.CastRestriction?.Create(),
				AffectedEntitySelector = affectedEntitySelector,
				GameActions = minionTriggeredEffects[0].GameActions.Select(x => x.Create()).ToList(),
				ManaCost = minionCard.Cost,
				UsedThisTurn = false
			};
		}

		return null;
	}
}

