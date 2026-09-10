using CardBattleEngine;

public static class CardBuilder
{
    public static CardBattleEngine.Card BuildCard(CardView cv, CardBattleEngine.Player owner)
    {
        CardBattleEngine.Card card = null;
        var definition = Common.Instance.CardManager.GetCardByID(cv.CardId);
        if (definition != null)
        {
            card = definition.CreateCard();
        }

        if (card == null)
        {
            // CardId didn't resolve (null today until the server populates it, or an
            // unrecognized id) - fall back to a minimal card of the right type so callers
            // always get a usable instance. Sprite will fall back to CardManager's
            // DefaultMissingSprite since SpriteID stays null on these.
            switch (cv.Type)
            {
                case CardType.Minion:
                    card = new MinionCard(cv.Name, cv.ManaCost, cv.Attack ?? 0, cv.Health ?? 0);
                    break;
                case CardType.Weapon:
                    card = new WeaponCard(cv.Name, cv.ManaCost, cv.Attack ?? 0, cv.Health ?? 0);
                    break;
                case CardType.Spell:
                default:
                    card = new SpellCard(cv.Name, cv.ManaCost);
                    break;
            }

            CardArtNetworkService.RequestArtIfNeeded(cv.CardId, owner.Name);
        }
        else
        {
            // Overwrite live stats from the view - the definition only knows base stats.
            card.ManaCost = cv.ManaCost;
            if (card is MinionCard minionCard)
            {
                if (cv.Attack.HasValue) minionCard.Attack = cv.Attack.Value;
                if (cv.Health.HasValue) { minionCard.Health = cv.Health.Value; minionCard.MaxHealth = cv.Health.Value; }
            }
            else if (card is WeaponCard weaponCard)
            {
                if (cv.Attack.HasValue) weaponCard.Attack = cv.Attack.Value;
                if (cv.Health.HasValue) weaponCard.Health = cv.Health.Value; // WeaponCard.Health writes through to Durability
            }
        }

        card.Id = cv.Id;
        card.Owner = owner;
        return card;
    }
}
