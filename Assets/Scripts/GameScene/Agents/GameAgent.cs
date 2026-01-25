using CardBattleEngine;
using System;
using UnityEngine;

public class GameAgent : MonoBehaviour
{
}

public interface IGameAgent
{
    public (IGameAction, ActionContext) GetNextAction(GameState game);

    public void OnGameEnd(GameState gamestate, bool win);
    void SetTarget((IGameAction, ActionContext) nextAction, GameState gameState);
}
