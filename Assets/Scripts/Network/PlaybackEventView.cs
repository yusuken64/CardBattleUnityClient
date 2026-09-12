using System;
using System.Collections.Generic;
using CardBattleEngine;

// Value-only presentation payload. No executable action, selectors, or unredacted GameState.
public class PlaybackEntityView
{
    public Guid Id;
    public Guid OwnerId;
    public CardView Card;
    public MinionView Minion;
}

public class PlaybackAmountView
{
    public PlaybackEntityView Target;
    public int Amount;
}

public class PlaybackStatusView
{
    public PlaybackEntityView Target;
    public StatusType Status;
    public bool Gained;
}

public class PlaybackEventView
{
    public long Sequence;
    public string ActionType;
    public Guid PlayerId;
    public PlaybackEntityView Source;
    public PlaybackEntityView Target;
    public PlaybackEntityView SourceCard;
    public PlaybackEntityView SummonedMinion;
    public PlaybackEntityView CardGained;
    public PlaybackEntityView TriggerSource;
    public List<PlaybackAmountView> AffectedEntities = new();
    public List<PlaybackStatusView> StatusChanges = new();
    public int DamageDealt;
    public int HealedAmount;
    public int ArmorGained;
    public int ManaSpent;
    public int PlayIndex;
    public int CardsLeftInDeck;
    public bool IsAttack;
    public bool HiddenCardPlayed;
    public string PresentationEffectId;
    public PlayerGameView After;
}
