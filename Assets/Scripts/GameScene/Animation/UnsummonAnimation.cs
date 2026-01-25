using CardBattleEngine;
using System.Collections;
using UnityEngine;

public class UnsummonAnimation : GameActionAnimation<ReturnMinionToCard>
{
	public override IEnumerator Play()
	{
		var player = GameManager.GetPlayerFor(Context.Target.Owner);

		if (Context.Target is CardBattleEngine.Minion minion)
		{
			var minionObject = GameManager.GetObjectFor(minion).GetComponent<Minion>();
			if (player.Board.Minions.Contains(minionObject))
			{
				player.Board.Minions.Remove(minionObject);
				var pendingAnimator = minionObject.GetComponent<Animator>();
				pendingAnimator.Play("MinionReturn");
			}
			player.Board.UpdateMinionPositions();
			Destroy(minionObject.gameObject, 1f); //the length of animation

			yield return new WaitForSecondsRealtime(1f);
		}
	}
}