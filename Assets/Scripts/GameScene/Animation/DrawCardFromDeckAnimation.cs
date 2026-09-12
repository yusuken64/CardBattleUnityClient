using CardBattleEngine;
using System.Collections;

public class DrawCardFromDeckAnimation : GameActionAnimation<DrawCardFromDeckAction>
{
    // The playback coordinator applies captured values and advances the game after the visuals.
    public override IEnumerator Play() { yield break; }
}
