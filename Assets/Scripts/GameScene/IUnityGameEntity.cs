public interface IUnityGameEntity
{
	public CardBattleEngine.IGameEntity Entity { get; }
	public void SyncData(CardBattleEngine.IGameEntity entity);
}