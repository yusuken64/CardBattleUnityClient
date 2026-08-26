using System;

public enum GameMode
{
	LocalTest,
	Networked
}

public class StartGameArgs
{
	public GameMode Mode = GameMode.LocalTest;

	// LocalTest mode: which CreateTestGame-created player (0 or 1) is "me". Guids don't exist yet at scene-load time.
	public int LocalSeat;

	// Networked mode: identity/match info supplied by the server/matchmaking layer.
	public Guid LocalPlayerId;
	public string MatchId;

	public static StartGameArgs LocalTestDefault()
	{
		return new StartGameArgs { Mode = GameMode.LocalTest };
	}
}
