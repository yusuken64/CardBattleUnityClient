using CardBattleEngine;
using UnityEngine;

public class HeroPortrait : MonoBehaviour, ITargetOrigin
{
	public Player Player;
    public AimIntent AimIntent { get; set; } = AimIntent.Attack;

	public bool CanStartAiming()
	{
		return Player.Data.CanAttack();
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
