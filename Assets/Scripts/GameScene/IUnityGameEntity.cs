using UnityEngine;

public interface IUnityGameEntity
{
	public CardBattleEngine.IGameEntity Entity { get; }
	public GameObject gameObject { get; }
	public void SyncData(CardBattleEngine.IGameEntity entity);
}