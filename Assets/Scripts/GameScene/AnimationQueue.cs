using CardBattleEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationQueue : MonoBehaviour
{
    private Queue<IAnimation> queue = new();
    private bool isPlaying = false;

    [SerializeReference]
    public List<IAnimation> Animations;

    public void EnqueueAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
    {
        IAnimation newAnimation = CreateAnimationForAction(gameManager, state, current);
        if (newAnimation is null) { return; }
        queue.Enqueue(newAnimation);

        if (!isPlaying)
            StartCoroutine(ProcessQueue());
    }

    private IAnimation CreateAnimationForAction(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
    {
        return current.action switch
        {
            StartTurnAction => new StartTurnAnimation(gameManager, state, current),
            GainCardAction => new GainCardAnimation(gameManager, state, current),
            RefillManaAction => new RefillManaAnimation(gameManager, state, current),
            IncreaseMaxManaAction => new IncreaseMaxManaAnimation(gameManager, state, current),
            PlayCardAction => new PlayCardAnimation(gameManager, state, current),
            SpendManaAction => new SpendManaAnimation(gameManager, state, current),
            SummonMinionAction => new SummonMinionAnimation(gameManager, state, current),
            AttackAction => new AttackAnimation(gameManager, state, current),
            DamageAction => new TakeDamageAnimation(gameManager, state, current),
            HealAction => new TakeHealAnimation(gameManager, state, current),
            DeathAction => new DeathAnimation(gameManager, state, current),
            EquipWeaponAction => new EquipWeaponAnimation(gameManager, state, current),
            DestroyWeaponAction => new DestoryWeaponAnimation(gameManager, state, current),
            EndGameAction => new EndGameActionAnimation(gameManager, state, current),
            _ => null
        };
    }


    private IEnumerator ProcessQueue()
    {
        isPlaying = true;

        while (queue.Count > 0)
            yield return StartCoroutine(queue.Dequeue().Play());

        isPlaying = false;
    }
}

public abstract class GameActionAnimation<T> : IAnimation where T : IGameAction
{
	public abstract IEnumerator Play();
}

public interface IAnimation
{
	IEnumerator Play();
}