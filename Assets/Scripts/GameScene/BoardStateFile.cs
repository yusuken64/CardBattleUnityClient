using CardBattleEngine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Exports/imports the full local board state (both players' hands/boards/hero stats) to/from a
// JSON file on disk, so a specific position can be captured and later restored exactly. Reuses
// CardView/MinionView (Assets/Scripts/Network/NetworkContracts.cs) as the on-disk shape - unlike
// a network PlayerGameView, both hands are always fully populated here (this is a local save
// file, not a redacted per-viewer wire message), so HiddenHandCount is always 0 on import.
public static class BoardStateFile
{
    public static void Export(GameState state, Guid localPlayerId, string filePath)
    {
        var data = Capture(state, localPlayerId);
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);

        string directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, json);
    }

    public static BoardStateSnapshot Import(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var data = JsonConvert.DeserializeObject<BoardStateFileData>(json);
        return ToSnapshot(data);
    }

    private static BoardStateFileData Capture(GameState state, Guid localPlayerId)
    {
        var p0 = state.Players[0];
        var p1 = state.Players[1];
        var selfData = p0.Id == localPlayerId ? p0 : p1;
        var opponentData = p0.Id == localPlayerId ? p1 : p0;

        return new BoardStateFileData
        {
            LocalPlayerId = localPlayerId,
            Self = CapturePlayer(selfData),
            Opponent = CapturePlayer(opponentData),
        };
    }

    private static PlayerStateData CapturePlayer(CardBattleEngine.Player player)
    {
        return new PlayerStateData
        {
            PlayerId = player.Id,
            Name = player.Name,
            Health = player.Health,
            MaxHealth = player.MaxHealth,
            Armor = player.Armor,
            Mana = player.Mana,
            MaxMana = player.MaxMana,
            Attack = player.Attack,
            IsAlive = player.IsAlive,
            HasAttackedThisTurn = player.HasAttackedThisTurn,
            IsFrozen = player.IsFrozen,
            IsStealth = player.IsStealth,
            Hand = player.Hand.Select(ToCardView).ToList(),
            Board = player.Board.Select(ToMinionView).ToList(),
        };
    }

    private static CardView ToCardView(CardBattleEngine.Card card)
    {
        var cv = new CardView
        {
            Id = card.Id,
            Name = card.Name,
            ManaCost = card.ManaCost,
            Type = card.Type,
            // CardId is deliberately left null: SpriteID looks like a definition-ID proxy (it's what
            // CardManager.GetSpriteByCardID resolves against) but it has a public setter unlike every
            // other identity-ish field on Card, i.e. it's designed to be overwritten by gameplay
            // effects (a transform/disguise-style effect could repoint it without this actually being
            // that other card). Reflecting CardBattleEngine.dll confirms there's no other client-visible
            // field that stably tracks the original CardDefinition. Leaving CardId null routes this
            // through CardBuilder.BuildCard's existing "unresolved" fallback (same path used before the
            // server populates CardView.CardId) instead of risking a wrong-definition reconstruction -
            // stats/type round-trip exactly since those are captured explicitly below; sprite and any
            // CastRestriction/TriggeredEffects tied to the original definition do not.
            CardId = null,
        };

        if (card is MinionCard minionCard)
        {
            cv.Attack = minionCard.Attack;
            cv.Health = minionCard.Health;
        }
        else if (card is WeaponCard weaponCard)
        {
            cv.Attack = weaponCard.Attack;
            cv.Health = weaponCard.Health;
        }

        return cv;
    }

    private static MinionView ToMinionView(CardBattleEngine.Minion minion)
    {
        return new MinionView
        {
            Id = minion.Id,
            // See ToCardView - same reasoning: OriginalCard.SpriteID is not a trustworthy definition-ID
            // proxy, so identity resolution is intentionally skipped here too. Every gameplay-relevant
            // flag below is captured explicitly instead, so board state still round-trips correctly.
            CardId = null,
            Name = minion.OriginalCard?.Name,
            Attack = minion.Attack,
            Health = minion.Health,
            MaxHealth = minion.MaxHealth,
            Taunt = minion.Taunt,
            IsFrozen = minion.IsFrozen,
            IsStealth = minion.IsStealth,
            HasDivineShield = minion.HasDivineShield,
            CanAttack = minion.CanAttack(),
            HasPoisonous = minion.HasPoisonous,
            HasWindfury = minion.HasWindfury,
            HasLifeSteal = minion.HasLifeSteal,
            HasReborn = minion.HasReborn,
            HasSummoningSickness = minion.HasSummoningSickness,
            // Mirrors Minion.RefreshData()'s derivation - TriggeredEffects only reflects the base
            // card, not anything granted/removed at runtime, but that's the same limitation the
            // live visual component already has, so it's the best available source here too.
            HasDeathRattle = minion.TriggeredEffects.Any(x => x.EffectTrigger == EffectTrigger.Deathrattle),
            HasTrigger = minion.TriggeredEffects.Any(x =>
                x.EffectTrigger != EffectTrigger.Deathrattle &&
                x.EffectTrigger != EffectTrigger.Battlecry),
        };
    }

    private static BoardStateSnapshot ToSnapshot(BoardStateFileData data)
    {
        var selfPlayer = BuildPlayer(data.Self);
        var opponentPlayer = BuildPlayer(data.Opponent);

        return new BoardStateSnapshot
        {
            LocalPlayerId = data.LocalPlayerId,
            // Treated the same as a network-sourced snapshot: MinionBuilder-reconstructed minions'
            // TriggeredEffects don't reflect runtime-granted/removed abilities either, so
            // GameManager.ApplyPlayerBoardState's HasDeathRattle/HasTrigger override (driven by
            // SourceMinionViews) is needed here too.
            IsFromNetwork = true,
            Self = new PlayerBoardSnapshot { Data = selfPlayer, HiddenHandCount = 0, SourceMinionViews = data.Self.Board },
            Opponent = new PlayerBoardSnapshot { Data = opponentPlayer, HiddenHandCount = 0, SourceMinionViews = data.Opponent.Board },
        };
    }

    private static CardBattleEngine.Player BuildPlayer(PlayerStateData data)
    {
        var player = new CardBattleEngine.Player(data.Name)
        {
            Id = data.PlayerId,
            Health = data.Health,
            MaxHealth = data.MaxHealth,
            Armor = data.Armor,
            Mana = data.Mana,
            MaxMana = data.MaxMana,
            Attack = data.Attack,
            IsAlive = data.IsAlive,
            HasAttackedThisTurn = data.HasAttackedThisTurn,
            IsFrozen = data.IsFrozen,
            IsStealth = data.IsStealth,
        };

        foreach (var cv in data.Hand)
        {
            player.Hand.Add(CardBuilder.BuildCard(cv, player));
        }

        foreach (var mv in data.Board)
        {
            player.Board.Add(MinionBuilder.BuildMinion(mv, player));
        }

        return player;
    }
}

public class BoardStateFileData
{
    public Guid LocalPlayerId;
    public PlayerStateData Self;
    public PlayerStateData Opponent;
}

public class PlayerStateData
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
    public List<CardView> Hand = new List<CardView>();
    public List<MinionView> Board = new List<MinionView>();
}
