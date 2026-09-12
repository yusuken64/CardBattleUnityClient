using System;
using CardBattleEngine;
using System.Collections.Generic;

public class BoardStateSnapshot
{
    public Guid LocalPlayerId;
    public bool IsFromNetwork;
    public PlayerBoardSnapshot Self;
    public PlayerBoardSnapshot Opponent;

    public static BoardStateSnapshot FromGameState(CardBattleEngine.GameState state, Guid localPlayerId)
    {
        var p0 = state.Players[0];
        var p1 = state.Players[1];
        var selfData = p0.Id == localPlayerId ? p0 : p1;
        var opponentData = p0.Id == localPlayerId ? p1 : p0;

        return new BoardStateSnapshot
        {
            LocalPlayerId = localPlayerId,
            IsFromNetwork = false,
            Self = new PlayerBoardSnapshot { Data = selfData, HiddenHandCount = 0, DeckCount = selfData.Deck.Count, SourceMinionViews = null },
            Opponent = new PlayerBoardSnapshot { Data = opponentData, HiddenHandCount = 0, DeckCount = opponentData.Deck.Count, SourceMinionViews = null },
        };
    }

    public static BoardStateSnapshot FromPlayerGameView(PlayerGameView view)
    {
        var selfPlayer = BuildPlayer(view.Self);
        var opponentPlayer = BuildPlayer(view.Opponent);

        return new BoardStateSnapshot
        {
            LocalPlayerId = view.ViewerPlayerId,
            IsFromNetwork = true,
            Self = new PlayerBoardSnapshot
            {
                Data = selfPlayer,
                HiddenHandCount = 0,
                DeckCount = view.Self.DeckCount,
                SourceMinionViews = view.Self.Board,
            },
            Opponent = new PlayerBoardSnapshot
            {
                Data = opponentPlayer,
                HiddenHandCount = view.Opponent.HandCount,
                DeckCount = view.Opponent.DeckCount,
                SourceMinionViews = view.Opponent.Board,
            },
        };
    }

    private static CardBattleEngine.Player BuildPlayer(PublicPlayerView view)
    {
        var player = new CardBattleEngine.Player(view.Name)
        {
            Id = view.PlayerId,
            Health = view.Health,
            MaxHealth = view.MaxHealth,
            Armor = view.Armor,
            Mana = view.Mana,
            MaxMana = view.MaxMana,
            Attack = view.Attack,
            IsAlive = view.IsAlive,
            HasAttackedThisTurn = view.HasAttackedThisTurn,
            IsFrozen = view.IsFrozen,
            IsStealth = view.IsStealth,
        };

        if (view.Hand != null)
        {
            foreach (var cv in view.Hand)
            {
                player.Hand.Add(CardBuilder.BuildCard(cv, player));
            }
        }
        // view.Hand is null for the opponent's view (hidden by design) - Hand stays empty;
        // HiddenHandCount on the snapshot is what a caller uses to know how many placeholder
        // cards to render instead.

        foreach (var mv in view.Board)
        {
            player.Board.Add(MinionBuilder.BuildMinion(mv, player));
        }

        if (view.HeroPower != null)
        {
            player.HeroPower = new CardBattleEngine.HeroPower
            {
                LeaderCard = view.HeroPower.LeaderCard == null ? null : CardBuilder.BuildCard(view.HeroPower.LeaderCard, player) as MinionCard,
                Name = view.HeroPower.Name,
                ManaCost = view.HeroPower.ManaCost,
                UsedThisTurn = view.HeroPower.UsedThisTurn,
            };
        }

        if (view.EquippedWeapon != null)
        {
            player.EquippedWeapon = WeaponBuilder.BuildWeapon(view.EquippedWeapon, player);
        }

        return player;
    }
}

public class PlayerBoardSnapshot
{
    public CardBattleEngine.Player Data;
    public int HiddenHandCount;
    public int DeckCount;
    public List<MinionView> SourceMinionViews; // null when IsFromNetwork is false; otherwise aligned index-for-index with Data.Board - Data.Board[i] corresponds to SourceMinionViews[i]
}
