using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Plays animated beats for networked history entries (attacks, damage/heal popups, minion summons).
/// Decoupled from GameManager's live engine pipeline — operates only on HistoryEntryView DTOs
/// and resolved GameObjects looked up via GameManager.GetObjectByID.
///
/// Usage: call Enqueue(PlayerGameView view) once per OnStateUpdated push.
/// A coroutine will automatically dequeue and play each batch in strict order.
/// </summary>
public class NetworkAnimationQueue : MonoBehaviour
{
	[SerializeField]
	public GameManager GameManager;

	private Queue<(PlayerGameView View, HistoryEntryView Entry)> _queue =
		new Queue<(PlayerGameView View, HistoryEntryView Entry)>();

	private bool _isProcessing = false;
	public bool IsProcessing => _isProcessing;
	private UI _ui;

	private void Start()
	{
		_ui = FindFirstObjectByType<UI>();
	}

	/// <summary>
	/// Appends all entries from view.NewHistory to the queue, paired with the view.
	/// If the queue wasn't running, starts the processing coroutine.
	/// </summary>
	public void Enqueue(PlayerGameView view)
	{
		if (view == null)
		{
			return;
		}

		// Pair each history entry with its source view so we can resnap at batch end
		if (view.NewHistory != null && view.NewHistory.Count > 0)
		{
			foreach (var entry in view.NewHistory)
			{
				_queue.Enqueue((view, entry));
			}
		}

		// Always queue a sentinel (null entry) at batch end to trigger ApplyBoardState,
		// even if NewHistory was empty or null
		_queue.Enqueue((view, null));

		if (!_isProcessing)
		{
			StartCoroutine(ProcessQueue());
		}
	}

	private IEnumerator ProcessQueue()
	{
		_isProcessing = true;

		while (_queue.Count > 0)
		{
			var (view, entry) = _queue.Dequeue();

			// Null entry signals end-of-batch board state snapshot
			if (entry == null)
			{
				GameManager.ApplyBoardState(BoardStateSnapshot.FromPlayerGameView(view));
				yield return null;
				continue;
			}

			// Play the visual beat for this entry
			yield return StartCoroutine(PlayEntryAnimation(view, entry));
		}

		_isProcessing = false;
	}

	private IEnumerator PlayEntryAnimation(PlayerGameView view, HistoryEntryView entry)
	{
		// Play attack animation if this is an attack action
		if (entry.ActionType == "AttackAction")
		{
			if (entry.SourceId.HasValue && entry.TargetId.HasValue)
			{
				yield return StartCoroutine(PlayAttackAnimation(entry.SourceId.Value, entry.TargetId.Value));
			}
		}

		// Play damage/heal popup if this action dealt damage or healed
		if (entry.DamageDealt.HasValue || entry.HealedAmount.HasValue)
		{
			if (entry.TargetId.HasValue)
			{
				yield return StartCoroutine(PlayDamageOrHealPopup(entry));
			}
		}

		// Play minion summon animation if a minion was summoned
		if (entry.SummonedMinionId.HasValue)
		{
			yield return StartCoroutine(PlayMinionSummon(view, entry.SummonedMinionId.Value));
		}

		// A cast spell has no persistent view of its own (unlike a minion landing on the board or
		// a weapon staying equipped) - briefly preview its art here instead, using the same
		// CardPreview slot PlayCardAnimation uses locally for the opponent's played cards. Only for
		// the opponent's own casts - our own spell casts are something we already saw when we
		// played them, no reveal needed.
		if (entry.ActionType == "CastSpellAction" && entry.PlayerId != GameManager.LocalPlayerId)
		{
			yield return StartCoroutine(PlayOpponentSpellCast(entry));
		}

		// EndTurnAction, StartTurnAction, and other types: no animation, just continue
	}

	private IEnumerator PlayOpponentSpellCast(HistoryEntryView entry)
	{
		if (_ui == null || string.IsNullOrEmpty(entry.SourceCardId) || GameManager.Opponent == null)
		{
			yield break;
		}

		var spellCard = SpellBuilder.BuildSpell(entry, GameManager.Opponent.Data);

		_ui.PreviewCard(spellCard);
		yield return new WaitForSecondsRealtime(1.5f);
		_ui.PreviewEnd();
	}

	private IEnumerator PlayAttackAnimation(Guid sourceId, Guid targetId)
	{
		var sourceObj = GameManager.GetObjectByID(sourceId);
		var targetObj = GameManager.GetObjectByID(targetId);

		if (sourceObj == null || targetObj == null)
		{
			yield break;
		}

		Transform attacker = sourceObj.gameObject.transform;
		Transform target = targetObj.gameObject.transform;

		float duration = 0.15f;

		Vector3 startPos = attacker.position + new Vector3(0, 0, -0.5f);
		attacker.transform.position = startPos;
		Vector3 dir = (target.position - attacker.position).normalized;
		Vector3 bumpPos = target.position - (dir * 0.4f) + new Vector3(0, 0, -0.1f);

		// Forward bump
		Tween forward = attacker.DOMove(bumpPos, duration).SetEase(Ease.InOutQuad);
		yield return forward.WaitForCompletion();

		// Backward bump
		Tween back = attacker.DOMove(startPos, 0.15f).SetEase(Ease.Linear);
		yield return back.WaitForCompletion();
	}

	private IEnumerator PlayDamageOrHealPopup(HistoryEntryView entry)
	{
		if (_ui == null)
		{
			yield break;
		}

		var targetObj = GameManager.GetObjectByID(entry.TargetId.Value);
		if (targetObj == null)
		{
			yield break;
		}

		if (entry.DamageDealt.HasValue)
		{
			_ui.ShowDamage(entry.DamageDealt.Value, targetObj.gameObject.transform);
		}
		else if (entry.HealedAmount.HasValue)
		{
			_ui.ShowHeal(entry.HealedAmount.Value, targetObj.gameObject.transform);
		}

		// Wait for the popup animation (fade out typically takes ~0.6s, plus pop-in 0.15s)
		yield return new WaitForSeconds(0.75f);
	}

	private IEnumerator PlayMinionSummon(PlayerGameView view, Guid summonedMinionId)
	{
		// Find the minion in either Self.Board or Opponent.Board
		MinionView minionView = null;
		Player ownerPlayer = null;

		if (view.Self != null && view.Self.Board != null)
		{
			minionView = view.Self.Board.FirstOrDefault(m => m.Id == summonedMinionId);
			if (minionView != null)
			{
				ownerPlayer = GameManager.Player;
			}
		}

		if (minionView == null && view.Opponent != null && view.Opponent.Board != null)
		{
			minionView = view.Opponent.Board.FirstOrDefault(m => m.Id == summonedMinionId);
			if (minionView != null)
			{
				ownerPlayer = GameManager.Opponent;
			}
		}

		if (minionView == null || ownerPlayer == null)
		{
			yield break;
		}

		// Get the minionPrefab from GameInteractionHandler
		var interactionHandler = FindFirstObjectByType<GameInteractionHandler>();
		if (interactionHandler == null || interactionHandler.MinionPrefab == null)
		{
			yield break;
		}

		// Build the minion engine object (owner can be null for animation purposes)
		var minionData = MinionBuilder.BuildMinion(minionView, null);

		// Instantiate the UI prefab
		var minionUI = Instantiate(interactionHandler.MinionPrefab, ownerPlayer.Board.transform);
		if (minionUI == null)
		{
			yield break;
		}

		// Set it up with the data
		minionUI.Setup(minionData);

		// Look for a CanvasGroup for fade-in
		CanvasGroup canvasGroup = minionUI.GetComponent<CanvasGroup>();
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 0;
			Tween fadeIn = canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.InOutQuad);
			yield return fadeIn.WaitForCompletion();
		}
		else
		{
			// Fallback: try SpriteRenderer
			SpriteRenderer spriteRenderer = minionUI.GetComponent<SpriteRenderer>();
			if (spriteRenderer != null)
			{
				Color startColor = spriteRenderer.color;
				startColor.a = 0;
				spriteRenderer.color = startColor;

				Tween fadeIn = spriteRenderer.DOFade(1f, 0.3f).SetEase(Ease.InOutQuad);
				yield return fadeIn.WaitForCompletion();
			}
		}
	}
}
