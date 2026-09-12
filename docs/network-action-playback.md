# Action playback

The engine resolves rules. `ActionPresentation` captures the values needed to display one
resolved action. `AnimationQueue` plays the existing action prefabs in order, then applies
the captured state. Animations must not resolve actions, modify engine entities, request
agent moves, or enable player input.

`GameManager.Presentation` owns progression: local games request the next agent move only
after playback drains; network games enable the latest server prompt only when its state
revision has been displayed. Mulligan prompts and game results follow the same boundary.

Network frames are captured synchronously by `PlayerViewBuilder.BuildPlayback` at the
engine's per-action callback, with separate private-data filtering for each viewer. They
include intermediate snapshots, so a summon followed by a death in the same resolution
still has both visual stages. History remains independent of playback.

`NetworkAnimationQueue` only handles sequence numbers, revision boundaries, and
resynchronization. It forwards frames to `AnimationQueue`, ignores redelivery, and uses
`GetState` to recover a snapshot baseline after a gap or playback failure. `GetState`
reads the server's last published snapshot instead of mutable engine state.

Custom effects travel as asset GUIDs (`PresentationEffectId`). The Resources registry
maps those IDs to locally bundled `CustomSFX` assets; Unity objects never enter the wire
definition. Rebuild it with **Tools → Network → Rebuild Presentation Effects** after
adding assets. Player builds rebuild it automatically. Unknown IDs use default visuals.

## Deck leader powers

Deck submissions include the selected leader's minion definition separately from the
cards in the draw pile. Hosted matches and matchmaking both create each player's power
with `HeroPower.FromLeader`, which single player also uses. The first minion effect must
be a battlecry; its actions, targeting, restriction, and mana cost become the power.
Effects that refer to the summoned leader apply to the hero, matching single player.
Leaders without that battlecry have no hero power.

The server publishes legal targeted or untargeted power actions and resolves their mana
cost, once-per-turn use, and reset. Public snapshots carry the leader card for the power's
hover preview. Playback updates the displayed used state at action boundaries. Game-state
clones retain an independent power state.

## Validation

- Unity EditMode tests: `MonsterGirl.Tests.ActionPresentationTests`.
- Unity real-scene smoke checks: **Tools → Network → Validate Local Presentation** and
  **Validate Network Presentation Seat 1 / Seat 2**. These run small games with real
  prefabs and restore the previous editor scene. Network smoke checks inject serialized
  server frames; SignalR transport is covered separately by `GameServer.Test`.
- Engine tests: `PlaybackViewBuilderTests`, `PlayerViewBuilderTests`, history, cloning,
  and card-definition roundtrips.
- Server tests: full games with two SignalR clients, including increasing revisions,
  contiguous playback sequences, redaction of intermediate snapshots, and both clients
  submitting targeted and untargeted leader powers. `LeaderHeroPowerTest` additionally
  covers matchmaking, costs, turn reset, clones, and absent or invalid leaders.

Ship the updated server and Unity client together, including the rebuilt engine DLL.
This protocol uses full per-action snapshots for correctness; snapshot compression and
downloading other players' custom effect assets are not included.
