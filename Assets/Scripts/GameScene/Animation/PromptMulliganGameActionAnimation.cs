using CardBattleEngine;
using System.Collections;

public class PromptMulliganGameActionAnimation : GameActionAnimation<PromptMulliganGameAction>
{
	public MulliganPrompt MulliganPrompt;
    // The playback coordinator applies captured values and advances the game after the visuals.
    public override IEnumerator Play() { yield break; }
}
