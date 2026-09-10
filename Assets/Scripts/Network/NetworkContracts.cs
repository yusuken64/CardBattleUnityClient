using System;
using System.Collections.Generic;
using CardBattleEngine;

// Mirrors GameServer.Contracts / CardBattleEngine.View wire shapes. Newtonsoft ignores unknown
// incoming JSON properties by default. Kept in sync with the engine's actual PlayerGameView - no
// rendering adapter consumes Self/Opponent/PendingChoice/NewHistory yet, but the fields are here so
// the shape matches the server.
// CardView.CardId and the MinionView ability flags (HasPoisonous/HasWindfury/HasLifeSteal/HasReborn/
// HasSummoningSickness/HasDeathRattle/HasTrigger) are populated by the server once its view-builder
// is updated to send them; until then they deserialize as null/false and callers must treat that as
// "unknown identity" / "no special ability state", not an error.

public class DecklistRequest
{
	public string PlayerName = string.Empty;
	public List<CardCount> Minions = new List<CardCount>();
	public List<CardCount> Spells = new List<CardCount>();
	public List<CardCount> Weapons = new List<CardCount>();
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

public class CardView
{
	public Guid Id;
	public string Name;
	public int ManaCost;
	public CardType Type;
	public int? Attack;
	public int? Health;
	public string CardId;
}

public class MinionView
{
	public Guid Id;
	public string CardId;
	public bool HasPoisonous;
	public bool HasWindfury;
	public bool HasLifeSteal;
	public bool HasReborn;
	public bool HasSummoningSickness;
	public bool HasDeathRattle;
	public bool HasTrigger;
	public string Name;
	public int Attack;
	public int Health;
	public int MaxHealth;
	public bool Taunt;
	public bool IsFrozen;
	public bool IsStealth;
	public bool HasDivineShield;
	public bool CanAttack;
}

public class WeaponView
{
	public Guid Id;
	public string Name;
	public int Attack;
	public int Durability;
}

public class HeroPowerView
{
	public string Name;
	public int ManaCost;
	public bool UsedThisTurn;
}

public class SecretView
{
	public int Index;
	public string Name;
	public Guid? CardId;
}

public class PublicPlayerView
{
	public Guid PlayerId;
	public string Name;
	public int Health;
	public int MaxHealth;
	public int Armor;
	public int Mana;
	public int MaxMana;
	public int Attack;
	public bool IsAlive;
	public bool HasAttackedThisTurn;
	public bool IsFrozen;
	public bool IsStealth;

	public int HandCount;
	public List<CardView> Hand; // null unless this is the viewer's own player

	public int DeckCount;

	public List<MinionView> Board = new List<MinionView>();
	public List<MinionView> Graveyard = new List<MinionView>();

	public WeaponView EquippedWeapon;
	public HeroPowerView HeroPower;

	public int SecretCount;
	public List<SecretView> Secrets; // null unless this is the viewer's own player
}

public class PendingChoiceView
{
	public Guid SourcePlayerId;
	public string ChoiceKind;
	public List<LegalActionView> Options = new List<LegalActionView>();
}

public class HistoryEntryView
{
	public int Turn;
	public Guid PlayerId;
	public string ActionType;
	public Guid? SourceId;
	public Guid? TargetId;
	public int? DamageDealt;
	public int? HealedAmount;
	public Guid? SummonedMinionId;
	public string SummonedMinionName;
	public Guid? CardGainedId;
	public string CardGainedName;
}

public class PlayerGameView
{
	public Guid ViewerPlayerId;
	public int Turn;
	public Guid CurrentPlayerId;
	public bool IsGameOver;
	public Guid? WinnerPlayerId;
	public PublicPlayerView Self;
	public PublicPlayerView Opponent;
	public PendingChoiceView PendingChoice;
	public bool OpponentIsChoosing;
	public List<LegalActionView> LegalActions = new List<LegalActionView>();
	public int? PromptVersion;
	public List<HistoryEntryView> NewHistory = new List<HistoryEntryView>();
}
