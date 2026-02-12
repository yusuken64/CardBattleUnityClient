using UnityEngine;
using UnityEditor;

public static class Cheats
{
	[MenuItem("Game/AutoPlayerTurn")]
	public static void AutoPlay()
	{
		var gameManager = Object.FindFirstObjectByType<GameManager>();
		//gameManager._opponentAgent = new RandomAI(gameManager.Opponent.Data, new UnityRNG());
		gameManager._opponentAgent = new AdvancedAI(gameManager.Opponent.Data, new UnityRNG());
		gameManager._playerAgent = new AdvancedAI(gameManager.Player.Data, new UnityRNG());
		//_playerAgent = new BasicAI(Player.Data, new UnityRNG());
		gameManager.ProcessPlayerMove();
	}
}
