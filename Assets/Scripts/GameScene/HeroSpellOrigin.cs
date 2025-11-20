using CardBattleEngine;
using UnityEngine;

public class HeroSpellOrigin : MonoBehaviour, ITargetOrigin
{
	public Player Player;
	public AimIntent AimIntent { get; set; } = AimIntent.CastSpell;

	public bool CanStartAiming()
	{
		return false;
	}

	public IGameEntity GetData()
	{
		return Player.Data;
	}

	public CardBattleEngine.Player GetPlayer()
	{
		return Player.Data;
	}
}
