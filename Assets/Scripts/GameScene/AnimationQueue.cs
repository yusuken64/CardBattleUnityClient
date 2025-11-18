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
            DrawCardFromDeckAction => new DrawCardAnimation(gameManager, state, current),
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