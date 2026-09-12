using CardBattleEngine;
using System.Collections;
using System.Linq;

public class UpdateStatsActionAnimation : GameActionAnimation<AddStatModifierAction>
{
    // The playback coordinator applies captured values and advances the game after the visuals.
    public override IEnumerator Play() { yield break; }
}
