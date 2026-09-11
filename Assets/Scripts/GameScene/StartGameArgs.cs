using System;

public enum GameMode
{
	LocalTest,
	Networked
}

// Networked mode: how InitializeNetworkedGameAsync should establish the match - mirrors the
// Quick match (Q) / create (C) / join (J) choice in CardBattleEngine.GamePlayer's RemoteGameClient.
public enum NetworkJoinMode
{
	Host,
	Join,
	QuickMatch
}

public class StartGameArgs
{
	public GameMode Mode = GameMode.LocalTest;

	// LocalTest mode: which CreateTestGame-created player (0 or 1) is "me". Guids don't exist yet at scene-load time.
	public int LocalSeat;

	// Networked mode: identity/match info supplied by the server/matchmaking layer.
	public Guid LocalPlayerId;
	public NetworkJoinMode JoinMode = NetworkJoinMode.Host;

	// Only meaningful when JoinMode == Join.
	public string MatchId;

	public static StartGameArgs LocalTestDefault()
	{
		return new StartGameArgs { Mode = GameMode.LocalTest };
	}
}
