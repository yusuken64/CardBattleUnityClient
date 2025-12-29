using CardBattleEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationQueue : MonoBehaviour
{
    public List<GameActionAnimationBase> GameActionAnimations;

    private Dictionary<Type, GameActionAnimationBase> animationMap;
    private Queue<IAnimation> queue = new();
    private bool isPlaying = false;

    private void Awake()
    {
        animationMap = new Dictionary<Type, GameActionAnimationBase>();

        foreach (var anim in GameActionAnimations)
        {
            if (anim == null)
                continue;

            var actionType = anim.ActionType;

            if (!animationMap.ContainsKey(actionType))
            {
                animationMap.Add(actionType, anim);
            }
            else
            {
                Debug.LogWarning($"Duplicate animation for {actionType.Name}. Keeping first one.");
            }
        }
    }

    public void EnqueueAnimation(GameManager gameManager, GameState state, (IGameAction action, ActionContext context) current)
    {
        IAnimation newAnimation = CreateAnimationForAction(gameManager, state, current);
        if (newAnimation is null)
        {
            Debug.Log($"No animation for {current.action}");
            return;
        }
        queue.Enqueue(newAnimation);

        if (!isPlaying)
            StartCoroutine(ProcessQueue());
    }

    private IAnimation CreateAnimationForAction(
        GameManager gm,
        GameState state,
        (IGameAction action, ActionContext context) current)
    {
        var actionType = current.action.GetType();

        if (!animationMap.TryGetValue(actionType, out var prefab))
        {
            return null;
        }

        var instance = Instantiate(prefab, transform);
        instance.Init(gm, state.Clone(), current);
        return instance;
    }

    private IEnumerator ProcessQueue()
    {
        isPlaying = true;

        while (queue.Count > 0)
        {
            IAnimation anim = queue.Dequeue();
            var sfxRoutine = anim.CustomSFX();
            if (sfxRoutine != null)
            {
                yield return StartCoroutine(sfxRoutine);
            }

            yield return StartCoroutine(anim.ApplyStatus());
            yield return StartCoroutine(anim.Play());
            anim.SyncData();
            if (anim != null) Destroy(anim.GameObject);
        }

        isPlaying = false;
    }
}

public abstract class GameActionAnimationBase : MonoBehaviour, IAnimation
{
    protected GameManager GameManager { get; private set; }
    protected GameState State { get; private set; }
    protected IGameAction Action { get; set; }
    protected ActionContext Context { get; private set; }
    public abstract Type ActionType { get; }

    public GameObject GameObject => this.gameObject;

    public abstract IEnumerator Play();

    public virtual void Init(GameManager gm, GameState state, (IGameAction action, ActionContext context) current)
    {
        GameManager = gm;
        State = state;
        Action = current.action;
        Context = current.context;
    }

    public abstract IEnumerator CustomSFX();
    public abstract IEnumerator ApplyStatus();
    public abstract void SyncData();
}

public abstract class GameActionAnimation<T> : GameActionAnimationBase where T : IGameAction
{
    public override Type ActionType => typeof(T);
	protected new T Action { get; private set; }

	public override void Init(GameManager gm, GameState state, (IGameAction action, ActionContext context) current)
    {
        base.Init(gm, state, current);
        Action = (T)current.action;
    }

	public override IEnumerator CustomSFX()
    {
		if (Action.CustomSFX == null)
			yield break;

        CustomSFX SFXScriptableObject = Action.CustomSFX as CustomSFX;
        if (SFXScriptableObject == null)
            yield break;

        yield return SFXScriptableObject.Routine(Action, Context);
	}

    public override IEnumerator ApplyStatus()
    {
        foreach (var change in Context.ResolvedStatusChanges)
		{
            var target = GameManager.GetObjectFor(change.Target);
            if (target == null) { continue; }

            //TODO: Have each IGameEntity implement a view interface like IStatusView
            //and call ApplyStatusDelta on it
            Minion minion = target.GetComponent<Minion>();
			switch (change.Status)
			{
				case StatusType.Freeze:
					minion.IsFrozen = change.Gained;
					break;
				case StatusType.Stealth:
					minion.HasStealth = change.Gained;
                    break;
			}
            minion.UpdateUI();
		}

		yield return null;
	}

	public override void SyncData()
	{
        var allEnitites = State.GetAllEntities();
        foreach (var entity in allEnitites)
        {
            var unityEnity = GameManager.GetObjectByID(entity.Id);
            if (unityEnity?.Entity == null) { continue; }
            unityEnity?.SyncData(entity);
        }
	}
}

public interface IAnimation
{
	IEnumerator Play();
    IEnumerator CustomSFX();
    IEnumerator ApplyStatus();
    void SyncData();

    GameObject GameObject { get; }
}