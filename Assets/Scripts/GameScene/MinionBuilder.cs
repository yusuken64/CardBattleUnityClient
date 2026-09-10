using CardBattleEngine;

public static class MinionBuilder
{
    public static CardBattleEngine.Minion BuildMinion(MinionView mv, CardBattleEngine.Player owner)
    {
        MinionCard minionCard = null;
        var definition = Common.Instance.CardManager.GetCardByID(mv.CardId);
        if (definition != null)
        {
            minionCard = definition.CreateCard() as MinionCard;
        }

        if (minionCard == null)
        {
            // CardId didn't resolve (null today until the server populates it, an
            // unrecognized id, or a definition that isn't actually a minion) - fall back
            // to a minimal MinionCard so callers always get a usable Minion. Sprite will
            // fall back to CardManager's DefaultMissingSprite since SpriteID stays null.
            minionCard = new MinionCard(mv.Name, 0, mv.Attack, mv.Health);
            CardArtNetworkService.RequestArtIfNeeded(mv.CardId);
        }

        var minion = new CardBattleEngine.Minion(minionCard, owner)
        {
            Id = mv.Id,
            Attack = mv.Attack,
            Health = mv.Health,
            MaxHealth = mv.MaxHealth,
            Taunt = mv.Taunt,
            IsFrozen = mv.IsFrozen,
            IsStealth = mv.IsStealth,
            HasDivineShield = mv.HasDivineShield,
            HasPoisonous = mv.HasPoisonous,
            HasWindfury = mv.HasWindfury,
            HasLifeSteal = mv.HasLifeSteal,
            HasReborn = mv.HasReborn,
            HasSummoningSickness = mv.HasSummoningSickness,
        };

        return minion;
    }
}
