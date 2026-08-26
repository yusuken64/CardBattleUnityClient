using System;
using System.Collections.Generic;

// Mirrors GameServer.Contracts / CardBattleEngine.View wire shapes. Newtonsoft ignores unknown
// incoming JSON properties by default, so PlayerGameView below only carries the fields the
// submission pathway needs today - extend it once a rendering adapter consumes Self/Opponent/
// PendingChoice/NewHistory too.

public class DecklistRequest
{
	public string PlayerName = string.Empty;
	public List<CardCount> Minions = new List<CardCount>();
	public List<CardCount> Spells = new List<CardCount>();
}

public class CardCount
{
	public string CardId = string.Empty;
	public int Count;
}

public class JoinResult
{
	public bool Success;
	public string Error;
}

public class ActionResult
{
	public bool Success;
	public string Error;
}

// Index is the only thing submitted back via MatchHub.SubmitAction - see PlayerViewBuilder on the
// server for why it must always be resolved against the most recent LegalActions/PromptVersion pair.
public class LegalActionView
{
	public int Index;
	public string ActionType;
	public Guid? SourceEntityId;
	public Guid? TargetEntityId;
	public string DisplayName;
}

public class PlayerGameView
{
	public Guid ViewerPlayerId;
	public int Turn;
	public Guid CurrentPlayerId;
	public bool IsGameOver;
	public Guid? WinnerPlayerId;
	public bool OpponentIsChoosing;
	public List<LegalActionView> LegalActions = new List<LegalActionView>();
	public int? PromptVersion;
}
