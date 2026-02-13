using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

public static class Cheats
{
    [MenuItem("Game/Auto/AutoPlayerTurn")]
    public static void AutoPlay()
    {
        var gameManager = Object.FindFirstObjectByType<GameManager>();
        //gameManager._opponentAgent = new RandomAI(gameManager.Opponent.Data, new UnityRNG());
        gameManager._opponentAgent = new AdvancedAI(gameManager.Opponent.Data, new UnityRNG());
        gameManager._playerAgent = new AdvancedAI(gameManager.Player.Data, new UnityRNG());
        //_playerAgent = new BasicAI(Player.Data, new UnityRNG());
        gameManager.ProcessPlayerMove();
    }

    [MenuItem("Game/Auto/AutoPlayerTurn_Stop")]
    public static void AutoPlay_Stop()
    {
        var gameManager = Object.FindFirstObjectByType<GameManager>();
        gameManager._playerAgent = null;
    }

    [MenuItem("Game/AI/EvaluatePosition_Player")]
    public static void EvaluatePosition_Player()
    {
        var gameManager = Object.FindFirstObjectByType<GameManager>();
        PrintTopActions(gameManager._gameState, gameManager.Player.Data);
    }


    public static void PrintTopActions(GameState state, CardBattleEngine.Player me, int topN = 10)
    {
        var ai = new AdvancedAI(me, new UnityRNG());
        var top = ai.GetTopActions(state);

        Debug.Log("==== ACTION RANKING ====");

        int i = 1;
        foreach (var entry in top)
        {
            string actionName = entry.Action.GetType().Name;
            if (entry.Action is PlayCardAction playCardAction)
			{
                actionName = $"Play {playCardAction.Card.Name}";
			}

            string context = entry.Context?.ToString() ?? "";

            Debug.Log($"{i}. {actionName} {entry.Context.Source} -> {entry.Context.Target} : {entry.Score:F1}");
            i++;
        }
    }
}
