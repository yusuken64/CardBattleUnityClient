public static class CardDefinitionNetworkExtensions
{
	public static string ToWireDefinitionJson(this CardDefinition definition, CardBattleEngine.Card runtimeCard)
	{
		return runtimeCard switch
		{
			CardBattleEngine.MinionCard m => CardBattleEngine.CardDatabase.ToDefinitionJson(
				CardBattleEngine.CardDatabase.ToMinionCardDefinition(m, definition.CardName)),
			CardBattleEngine.SpellCard s => CardBattleEngine.CardDatabase.ToDefinitionJson(
				CardBattleEngine.CardDatabase.ToSpellCardDefinition(s, definition.CardName)),
			CardBattleEngine.WeaponCard w => CardBattleEngine.CardDatabase.ToDefinitionJson(
				CardBattleEngine.CardDatabase.ToWeaponCardDefinition(w, definition.CardName)),
			_ => null
		};
	}
}
