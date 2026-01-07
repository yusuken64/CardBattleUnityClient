using CardBattleEngine;
using System.Collections;

public class PromptMulliganGameActionAnimation : GameActionAnimation<PromptMulliganGameAction>
{
	public MulliganPrompt MulliganPrompt;
	public override IEnumerator Play()
	{
		if (Context.SourcePlayer == GameManager.Player.Data)
		{
			MulliganPrompt.gameObject.SetActive(true);
			MulliganPrompt.Setup(GameManager.Player.Hand.Cards);
		}

		yield return null;
	}
}
