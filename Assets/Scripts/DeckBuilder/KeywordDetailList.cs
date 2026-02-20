using System.Linq;
using UnityEngine;

public class KeywordDetailList : MonoBehaviour
{
	public Transform Container;
	public KeywordDetail KeywordDetailPrefab;

	private void ClearItems()
	{
		foreach (Transform child in Container)
		{
			Destroy(child.gameObject);
		}
	}

	public void Setup(Card card)
	{
		Setup(card.Data);
	}

	public void Setup(CardBattleEngine.Card card)
	{
		ClearItems();

		//go through each keyword
		if (!(card is CardBattleEngine.MinionCard minion))
		{
			return;
		}

		foreach (var keyword in Keywords)
		{
			if (keyword.HasKeyword(minion))
			{
				AddDetails(keyword.Name, keyword.Description);
			}
		}
	}

	private static readonly KeywordInfo[] Keywords =
	{
		new("BattleCry", "Does Something when played", m => m.MinionTriggeredEffects.Any(x => x.EffectTrigger == CardBattleEngine.EffectTrigger.Battlecry)),
		new("DeathRattle", "Does Something when destroyed", m => m.MinionTriggeredEffects.Any(x => x.EffectTrigger == CardBattleEngine.EffectTrigger.Deathrattle)),
		new("Freeze", "Unable to Attack for 1 turn", 
			m => m.MinionTriggeredEffects.SelectMany(x => x.GameActions)
			.Any(x => x.GetType() == typeof(CardBattleEngine.FreezeAction))),
		new("Taunt", "Must be attacked first", m => m.HasTaunt),
		new("Reborn", "Returns with 1 Health after dying", m => m.HasReborn),
		new("Lifesteal", "Damage dealt heals your hero", m => m.HasLifeSteal),
		new("Windfury", "Can attack twice per turn", m => m.HasWindfury),
		new("Rush", "Can attack minions immediately", m => m.HasRush),
		new("Poisonous", "Destroys any minion damaged", m => m.HasPoisonous),
		new("Divine Shield", "Ignores the first damage taken", m => m.HasDivineShield),
		new("Charge", "Can attack immediately", m => m.HasCharge),
		new("Stealth", "Cannot be targeted until it attacks", m => m.IsStealth),
	}; 

	private struct KeywordInfo
	{
		public string Name;
		public string Description;
		public System.Func<CardBattleEngine.MinionCard, bool> HasKeyword;

		public KeywordInfo(string name, string description,
			System.Func<CardBattleEngine.MinionCard, bool> hasKeyword)
		{
			Name = name;
			Description = description;
			HasKeyword = hasKeyword;
		}
	}

	public void AddDetails(string keyword, string description)
	{
		var newItem = Instantiate(KeywordDetailPrefab, Container);
		newItem.Title.text = keyword;
		newItem.Description.text = description;
	}
}
