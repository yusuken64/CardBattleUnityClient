using CardBattleEngine;

public static class WeaponBuilder
{
    public static CardBattleEngine.Weapon BuildWeapon(WeaponView wv, CardBattleEngine.Player owner)
    {
        WeaponCard weaponCard = null;
        var definition = Common.Instance.CardManager.GetCardByID(wv.CardId);
        if (definition != null)
        {
            weaponCard = definition.CreateCard() as WeaponCard;
        }

        if (weaponCard == null)
        {
            // CardId didn't resolve (null today until the server populates it, an
            // unrecognized id, or a definition that isn't actually a weapon) - fall back
            // to a minimal WeaponCard so callers always get a usable Weapon. SpriteID is set
            // to the requested CardId (matching how CardDefinition.CreateCard sets it for a
            // recognized card) so GetSpriteByCardID can find the art once it arrives.
            weaponCard = new WeaponCard(wv.Name, 0, wv.Attack, wv.Durability) { SpriteID = wv.CardId };
            CardArtNetworkService.RequestArtIfNeeded(wv.CardId, owner.Name);
        }

        // CreateWeapon() links OriginalCard back to weaponCard - required for Weapon.Setup() to
        // resolve a sprite at all (it reads Data.OriginalCard.SpriteID).
        var weapon = weaponCard.CreateWeapon();
        weapon.Id = wv.Id;
        weapon.Attack = wv.Attack;
        weapon.Durability = wv.Durability;
        weapon.Owner = owner;

        return weapon;
    }
}
