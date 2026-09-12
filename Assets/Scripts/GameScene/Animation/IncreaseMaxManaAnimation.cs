using CardBattleEngine;
using System.Collections;

public class IncreaseMaxManaAnimation : GameActionAnimation<IncreaseMaxManaAction>
{
    // The playback coordinator applies captured values and advances the game after the visuals.
    public override IEnumerator Play() { yield break; }
}
