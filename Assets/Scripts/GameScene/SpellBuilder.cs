using CardBattleEngine;

// A cast spell has no persistent view of its own (unlike a MinionView/WeaponView) - the only
// signal that one was played is a HistoryEntryView, so this builds a display-only SpellCard from
// that entry's SourceCardId/SourceName rather than from a dedicated view type.
public static class SpellBuilder
{
    public static SpellCard BuildSpell(HistoryEntryView entry, CardBattleEngine.Player owner)
    {
        SpellCard spellCard = null;
        var definition = Common.Instance.CardManager.GetCardByID(entry.SourceCardId);
        if (definition != null)
        {
            spellCard = definition.CreateCard() as SpellCard;
        }

        if (spellCard == null)
        {
            // SourceCardId didn't resolve (an unrecognized/custom id, or a definition that isn't
            // actually a spell) - fall back to a minimal SpellCard so callers always get a usable
            // instance. SpriteID is set to the requested CardId (matching how CardDefinition.
            // CreateCard sets it for a recognized card) so GetSpriteByCardID can find the art once
            // it arrives.
            spellCard = new SpellCard(entry.SourceName ?? entry.SourceCardId, 0) { SpriteID = entry.SourceCardId };
            CardArtNetworkService.RequestArtIfNeeded(entry.SourceCardId, owner.Name);
        }

        spellCard.Owner = owner;
        return spellCard;
    }
}
