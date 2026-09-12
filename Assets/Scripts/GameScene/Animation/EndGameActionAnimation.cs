using CardBattleEngine;
using System.Collections;
using UnityEngine;

public class EndGameActionAnimation : GameActionAnimation<EndGameAction>
{
    // The playback coordinator applies captured values and advances the game after the visuals.
    public override IEnumerator Play() { yield break; }
}
