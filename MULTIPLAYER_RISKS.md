# Multiplayer implementation risks

Tracks known risks and open gaps in the in-progress multiplayer work (`StartGameArgs`,
`GameManager.InitializeNetworkedGame`, `Assets/Scripts/Network/*`) against
`CardBattleEngine/GameServer`. Nothing below has been compiled or run against a live server -
this is reasoning from reading the code, not verified behavior.

## Untested / unverified

- **Nothing has been compiled in Unity or run against a live `GameServer` instance.** All of
  `MiniSignalRClient`, `InitializeNetworkedGame`, and the DTOs in `Network/NetworkContracts.cs`
  are unverified. Compile it first, then run one end-to-end match against a local `GameServer`
  before trusting any of this.
- `MiniSignalRClient.NegotiateAsync` polls `UnityWebRequestAsyncOperation.isDone` in a `while`
  loop with `await Task.Yield()` rather than `await request.SendWebRequest()`, specifically to
  avoid depending on an awaiter extension whose availability on this Unity version is unconfirmed.
  Works, but busy-polls (many iterations per frame) until the negotiate call returns.
- `System.Net.WebSockets.ClientWebSocket` behavior under Unity's Mono/IL2CPP runtimes is assumed,
  not confirmed. If a build target doesn't support it (WebGL in particular - browsers don't expose
  raw `ClientWebSocket`-style sockets the same way), the whole transport needs replacing for that
  platform.
- Newtonsoft's default handling of plain public fields (no `[DataContract]`) on all the DTOs in
  `NetworkContracts.cs` is relied on for both serialization and case-insensitive property matching
  against the server's camelCase JSON. Reasoned through, not tested against real server output.

## Deferred by design (known gaps, not oversights)

- **No rendering adapter.** `Card.cs`, `Minion.cs`, `Player.cs`, `Board.cs`, and every
  `Animation/*` class still bind to live `CardBattleEngine` objects and `(IGameAction,
  ActionContext)` playback. Networked mode never runs the engine client-side, so nothing renders
  from `PlayerGameView`/`NewHistory` yet - only the connect/submit plumbing exists.
- **No click-to-action mapping.** `GameManager.SubmitLocalAction(LegalActionView)` exists, but
  nothing yet matches a card/minion click against `_lastNetworkView.LegalActions` to produce that
  argument.
- **No matchmaking/lobby UI.** `StartGameArgs.MatchId` (host vs. joiner) has no UI path setting it
  - only reachable by hand-setting the static before `SceneManager.LoadScene("GameScene")`.
- **Weapon cards aren't sent.** `DeckNetworkExtensions.ToDecklistRequest` silently skips
  `WeaponCardDefinition` entries because the server's `DecklistRequest` has no `Weapons` slot.

## Structural risks

- **No reconnect/resume.** `MiniSignalRClient` makes one connection attempt with no retry. If the
  socket drops mid-match, the client has no way to resume the match. Worse, server-side
  `RemotePlayerAgent.GetNextAction` blocks its match's dedicated thread indefinitely waiting for a
  submission - a client that disconnects mid-turn and never reconnects leaves that thread parked
  forever with no timeout (this is a `GameServer` gap, not a client one, but it means a flaky
  client connection can wedge a match permanently on the server).
- **No timeout on `InvokeAsync`.** If a completion message is lost or the server never responds,
  the awaiting `Task` hangs forever - no cancellation token, no timeout wrapper.
- **Hand-maintained DTO mirror.** `NetworkContracts.cs` duplicates shapes owned by
  `GameServer/Contracts` and `CardBattleEngine/View/PlayerGameView.cs` in a separate repo. Because
  Newtonsoft ignores unknown/missing JSON fields by default, a shape change on the server (renamed
  or restructured field) won't throw on the client - it'll just silently stop populating that
  field. No shared schema/versioning between the two repos today.
- **`UnityRNG.Clone()` is broken** (`GameManager.cs`, `UnityRNG.Clone()` returns a new
  unseeded instance rather than a true clone) - irrelevant to networked mode since the server owns
  the only real RNG there, but still a live bug in `InitializeLocalTestGame`'s single-player path.

## Operational risks

- `MiniSignalRClient.On<T>` handler registration is documented as "must happen before
  `ConnectAsync`" but nothing enforces it - registering a handler after connecting is a silent bug
  (the receive loop just won't have it yet), not a thrown error.
- `GameManager.Update()` must call `_networkClient.PumpMainThread()` every frame for any server
  push (`OnStateUpdated`, `OnActionRejected`, `OnMatchEnded`) to ever actually invoke its callback.
  If `GameManager` is ever disabled/inactive while a match is live, pushes queue up silently and
  are never delivered until it re-enables.
