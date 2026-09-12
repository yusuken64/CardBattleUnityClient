using CardBattleEngine;
using System.Collections;

public class SpendManaAnimation : GameActionAnimation<SpendManaAction>
{
    // The playback coordinator applies captured values and advances the game after the visuals.
    public override IEnumerator Play() { yield break; }
}
